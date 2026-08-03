using FutPib.Data;
using FutPib.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var configuredConnection = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(NormalizePostgresConnection(databaseUrl)));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(configuredConnection));
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "FutPib.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<FutPib.Services.TeamBalancerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Cria as tabelas automaticamente no SQL Express ou Supabase.
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var connection = db.Database.GetDbConnection();

        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT to_regclass('public.\"Users\"') IS NOT NULL;";

        var result = await command.ExecuteScalarAsync();
        var usersTableExists = result is bool exists && exists;

        await connection.CloseAsync();

        if (!usersTableExists)
        {
            var databaseCreator =
                db.GetService<IRelationalDatabaseCreator>();

            await databaseCreator.CreateTablesAsync();
        }
    }
    else
    {
        db.Database.EnsureCreated();
    }

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
    var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
    var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        if (app.Environment.IsDevelopment())
            adminPassword = "FutPib@2026";
        else
            throw new InvalidOperationException(
                "A variável secreta ADMIN_PASSWORD precisa ser configurada na hospedagem.");
    }

    var admin = db.Users.FirstOrDefault(u => u.Username == adminUsername);

    if (admin is null)
    {
        admin = new AppUser
        {
            FullName = "Administrador FUT PIB",
            Nickname = "Admin",
            Username = adminUsername,
            Role = UserRole.Admin,
            Status = UserStatus.Approved,
            PrimaryPosition = PlayerPosition.MeioCampo,
            CreatedAt = DateTime.Now
        };

        admin.PasswordHash = hasher.HashPassword(admin, adminPassword);
        db.Users.Add(admin);
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string NormalizePostgresConnection(string value)
{
    // Aceita tanto o formato do Supabase:
    // postgresql://usuario:senha@servidor:porta/banco
    // quanto o formato:
    // Host=...;Port=...;Database=...;Username=...;Password=...
    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return value;
    }

    var uri = new Uri(value);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = username,
        Password = password,
        SslMode = SslMode.Require,
        TrustServerCertificate = true,
        Pooling = true,
        Timeout = 30,
        CommandTimeout = 60
    };

    return builder.ConnectionString;
}
