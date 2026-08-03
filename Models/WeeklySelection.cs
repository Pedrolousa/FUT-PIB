namespace FutPib.Models;

public class WeeklySelection
{
    public int Id { get; set; }
    public DateTime WeekReference { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public bool IsSelected { get; set; }
}
