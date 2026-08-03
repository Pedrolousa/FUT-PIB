using System.ComponentModel.DataAnnotations;

namespace FutPib.Models;

public class PlayerRating
{
    public int Id { get; set; }

    public int RaterUserId { get; set; }
    public AppUser? RaterUser { get; set; }

    public int RatedUserId { get; set; }
    public AppUser? RatedUser { get; set; }

    [Range(1, 5)] public int Defense { get; set; }
    [Range(1, 5)] public int Passing { get; set; }
    [Range(1, 5)] public int Speed { get; set; }
    [Range(1, 5)] public int Stamina { get; set; }
    [Range(1, 5)] public int Dribbling { get; set; }
    [Range(1, 5)] public int Finishing { get; set; }
    [Range(1, 5)] public int Teamwork { get; set; }
    [Range(1, 5)] public int Goalkeeping { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public double Average =>
        (Defense + Passing + Speed + Stamina + Dribbling + Finishing + Teamwork + Goalkeeping) / 8.0;
}
