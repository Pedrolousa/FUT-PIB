using System.ComponentModel.DataAnnotations;

namespace FutPib.Models;

public enum UserRole
{
    Player = 0,
    Admin = 1
}

public enum UserStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Blocked = 3
}

public enum PlayerPosition
{
    Goleiro = 0,
    Defesa = 1,
    MeioCampo = 2,
    Ataque = 3
}

public class AppUser
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = "";

    [Required, MaxLength(40)]
    public string Nickname { get; set; } = "";

    [Required, MaxLength(40)]
    public string Username { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    public PlayerPosition PrimaryPosition { get; set; }

    public PlayerPosition? SecondaryPosition { get; set; }

    public UserRole Role { get; set; } = UserRole.Player;

    public UserStatus Status { get; set; } = UserStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
