using FutPib.Models;

namespace FutPib.ViewModels;

public class PlayerScoreViewModel
{
    public AppUser User { get; set; } = null!;
    public double Score { get; set; }
    public int RatingCount { get; set; }
}
