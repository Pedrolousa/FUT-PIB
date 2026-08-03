using FutPib.Models;

namespace FutPib.ViewModels;

public class AdminDashboardViewModel
{
    public List<AppUser> PendingUsers { get; set; } = new();
    public List<AppUser> ApprovedPlayers { get; set; } = new();
    public HashSet<int> SelectedPlayerIds { get; set; } = new();
    public DateTime WeekReference { get; set; }
}
