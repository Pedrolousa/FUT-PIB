using FutPib.Data;
using FutPib.Models;
using FutPib.Services;
using FutPib.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutPib.Controllers;

[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly TeamBalancerService _balancer;

    public AdminController(AppDbContext db, TeamBalancerService balancer)
    {
        _db = db;
        _balancer = balancer;
    }

    public async Task<IActionResult> Index(DateTime? week)
    {
        var reference = StartOfWeek(week ?? DateTime.Today);

        var model = new AdminDashboardViewModel
        {
            WeekReference = reference,
            PendingUsers = await _db.Users
                .Where(u => u.Role == UserRole.Player && u.Status == UserStatus.Pending)
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(),
            ApprovedPlayers = await _db.Users
                .Where(u => u.Role == UserRole.Player && u.Status == UserStatus.Approved)
                .OrderBy(u => u.Nickname)
                .ToListAsync(),
            SelectedPlayerIds = (await _db.WeeklySelections
                .Where(s => s.WeekReference == reference && s.IsSelected)
                .Select(s => s.UserId)
                .ToListAsync())
                .ToHashSet()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is not null && user.Role == UserRole.Player)
        {
            user.Status = UserStatus.Approved;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is not null && user.Role == UserRole.Player)
        {
            user.Status = UserStatus.Rejected;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSelection(DateTime weekReference, int[] selectedIds)
    {
        var reference = StartOfWeek(weekReference);
        var current = await _db.WeeklySelections
            .Where(s => s.WeekReference == reference)
            .ToListAsync();

        _db.WeeklySelections.RemoveRange(current);

        foreach (var id in selectedIds.Distinct())
        {
            _db.WeeklySelections.Add(new WeeklySelection
            {
                WeekReference = reference,
                UserId = id,
                IsSelected = true
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Jogadores da semana atualizados.";
        return RedirectToAction(nameof(Index), new { week = reference.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DrawTeams(DateTime weekReference, int teamCount)
    {
        var reference = StartOfWeek(weekReference);

        if (teamCount is < 2 or > 6)
        {
            TempData["Error"] = "Escolha entre 2 e 6 times.";
            return RedirectToAction(nameof(Index), new { week = reference.ToString("yyyy-MM-dd") });
        }

        var selectedIds = await _db.WeeklySelections
            .Where(s => s.WeekReference == reference && s.IsSelected)
            .Select(s => s.UserId)
            .ToListAsync();

        var players = await _db.Users
            .Where(u => selectedIds.Contains(u.Id) && u.Status == UserStatus.Approved)
            .ToListAsync();

        if (players.Count < teamCount * 2)
        {
            TempData["Error"] = "Selecione pelo menos dois jogadores por time.";
            return RedirectToAction(nameof(Index), new { week = reference.ToString("yyyy-MM-dd") });
        }

        var ratings = await _db.Ratings
            .Where(r => selectedIds.Contains(r.RatedUserId))
            .ToListAsync();

        var scores = players.Select(player =>
        {
            var playerRatings = ratings.Where(r => r.RatedUserId == player.Id).ToList();
            var score = playerRatings.Count == 0
                ? 3.0
                : playerRatings.Average(r => r.Average);

            return new PlayerScoreViewModel
            {
                User = player,
                Score = Math.Round(score, 2),
                RatingCount = playerRatings.Count
            };
        }).ToList();

        var teams = _balancer.Balance(scores, teamCount);

        var oldDraws = await _db.TeamDraws
            .Where(d => d.WeekReference == reference)
            .ToListAsync();
        _db.TeamDraws.RemoveRange(oldDraws);

        var draw = new TeamDraw
        {
            WeekReference = reference,
            TeamCount = teamCount,
            CreatedAt = DateTime.Now
        };

        foreach (var team in teams)
        {
            foreach (var player in team.Players)
            {
                draw.Players.Add(new TeamDrawPlayer
                {
                    UserId = player.User.Id,
                    TeamNumber = team.TeamNumber,
                    PlayerScore = player.Score
                });
            }
        }

        _db.TeamDraws.Add(draw);
        await _db.SaveChangesAsync();

        return View("DrawResult", teams);
    }

    public async Task<IActionResult> Ratings()
    {
        var players = await _db.Users
            .Where(u => u.Role == UserRole.Player && u.Status == UserStatus.Approved)
            .OrderBy(u => u.Nickname)
            .ToListAsync();

        var ratings = await _db.Ratings.ToListAsync();

        var model = players.Select(p => new PlayerScoreViewModel
        {
            User = p,
            RatingCount = ratings.Count(r => r.RatedUserId == p.Id),
            Score = ratings.Any(r => r.RatedUserId == p.Id)
                ? Math.Round(ratings.Where(r => r.RatedUserId == p.Id).Average(r => r.Average), 2)
                : 3.0
        }).ToList();

        return View(model);
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
