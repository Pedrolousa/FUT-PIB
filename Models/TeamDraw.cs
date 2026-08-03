namespace FutPib.Models;

public class TeamDraw
{
    public int Id { get; set; }
    public DateTime WeekReference { get; set; }
    public int TeamCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<TeamDrawPlayer> Players { get; set; } = new();
}

public class TeamDrawPlayer
{
    public int Id { get; set; }
    public int TeamDrawId { get; set; }
    public TeamDraw? TeamDraw { get; set; }

    public int UserId { get; set; }
    public AppUser? User { get; set; }

    public int TeamNumber { get; set; }
    public double PlayerScore { get; set; }
}
