using FutPib.Models;

namespace FutPib.ViewModels;

public class DrawResultViewModel
{
    public int TeamNumber { get; set; }
    public List<PlayerScoreViewModel> Players { get; set; } = new();
    public double TotalScore => Players.Sum(x => x.Score);
}
