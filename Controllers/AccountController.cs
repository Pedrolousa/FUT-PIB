using System.Security.Claims;
using FutPib.Data;
using FutPib.Models;
using FutPib.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutPib.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly IConfiguration _configuration;

    public AccountController(AppDbContext db, IPasswordHasher<AppUser> hasher, IConfiguration configuration)
    {
        _db = db;
        _hasher = hasher;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.Username = model.Username.Trim().ToLowerInvariant();

        var expectedGroupCode = Environment.GetEnvironmentVariable("GROUP_CODE") ?? _configuration["FutPib:GroupCode"];

        if (model.GroupCode != expectedGroupCode)
            ModelState.AddModelError(nameof(model.GroupCode), "Código do grupo inválido.");

        if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            ModelState.AddModelError(nameof(model.Username), "Este usuário já está em uso.");

        if (!ModelState.IsValid)
            return View(model);

        var user = new AppUser
        {
            FullName = model.FullName.Trim(),
            Nickname = model.Nickname.Trim(),
            Username = model.Username,
            PrimaryPosition = model.PrimaryPosition,
            SecondaryPosition = model.SecondaryPosition,
            Role = UserRole.Player,
            Status = UserStatus.Pending,
            CreatedAt = DateTime.Now
        };

        user.PasswordHash = _hasher.HashPassword(user, model.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Cadastro enviado. Aguarde a aprovação do administrador.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var username = model.Username.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user is null ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Usuário ou senha inválidos.");
            return View(model);
        }

        if (user.Status == UserStatus.Pending)
        {
            ModelState.AddModelError("", "Seu cadastro ainda está aguardando aprovação.");
            return View(model);
        }

        if (user.Status != UserStatus.Approved)
        {
            ModelState.AddModelError("", "Sua conta não está liberada.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nickname),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return user.Role == UserRole.Admin
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Player");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
