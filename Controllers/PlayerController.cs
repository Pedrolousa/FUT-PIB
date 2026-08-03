using System.Security.Claims;
using FutPib.Data;
using FutPib.Models;
using FutPib.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutPib.Controllers;

[Authorize(Roles = nameof(UserRole.Player))]
public class PlayerController : Controller
{
    private readonly AppDbContext _db;

    public PlayerController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var userId = CurrentUserId;
        var players = await _db.Users
            .Where(u => u.Role == UserRole.Player &&
                        u.Status == UserStatus.Approved &&
                        u.Id != userId)
            .OrderBy(u => u.Nickname)
            .ToListAsync();

        var ratedIds = await _db.Ratings
            .Where(r => r.RaterUserId == userId)
            .Select(r => r.RatedUserId)
            .ToListAsync();

        ViewBag.RatedIds = ratedIds.ToHashSet();

        var latestDraw = await _db.TeamDraws
            .Include(d => d.Players)
            .ThenInclude(p => p.User)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefaultAsync();

        ViewBag.LatestDraw = latestDraw;

        return View(players);
    }

    [HttpGet]
    public async Task<IActionResult> Rate(int id)
    {
        if (id == CurrentUserId) return BadRequest();

        var player = await _db.Users.FindAsync(id);
        if (player is null || player.Status != UserStatus.Approved)
            return NotFound();

        var existing = await _db.Ratings.FirstOrDefaultAsync(r =>
            r.RaterUserId == CurrentUserId && r.RatedUserId == id);

        var model = new RatingViewModel
        {
            RatedUserId = id,
            RatedUserName = player.Nickname,
            Defense = existing?.Defense ?? 3,
            Passing = existing?.Passing ?? 3,
            Speed = existing?.Speed ?? 3,
            Stamina = existing?.Stamina ?? 3,
            Dribbling = existing?.Dribbling ?? 3,
            Finishing = existing?.Finishing ?? 3,
            Teamwork = existing?.Teamwork ?? 3,
            Goalkeeping = existing?.Goalkeeping ?? 3
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(RatingViewModel model)
    {
        if (model.RatedUserId == CurrentUserId)
            ModelState.AddModelError("", "Você não pode avaliar a si mesmo.");

        if (!await _db.Users.AnyAsync(u =>
                u.Id == model.RatedUserId &&
                u.Status == UserStatus.Approved &&
                u.Role == UserRole.Player))
            ModelState.AddModelError("", "Jogador inválido.");

        if (!ModelState.IsValid)
            return View(model);

        var rating = await _db.Ratings.FirstOrDefaultAsync(r =>
            r.RaterUserId == CurrentUserId &&
            r.RatedUserId == model.RatedUserId);

        if (rating is null)
        {
            rating = new PlayerRating
            {
                RaterUserId = CurrentUserId,
                RatedUserId = model.RatedUserId
            };
            _db.Ratings.Add(rating);
        }

        rating.Defense = model.Defense;
        rating.Passing = model.Passing;
        rating.Speed = model.Speed;
        rating.Stamina = model.Stamina;
        rating.Dribbling = model.Dribbling;
        rating.Finishing = model.Finishing;
        rating.Teamwork = model.Teamwork;
        rating.Goalkeeping = model.Goalkeeping;
        rating.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Avaliação salva.";
        return RedirectToAction(nameof(Index));
    }
}
