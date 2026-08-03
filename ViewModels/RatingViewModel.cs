using System.ComponentModel.DataAnnotations;

namespace FutPib.ViewModels;

public class RatingViewModel
{
    public int RatedUserId { get; set; }
    public string RatedUserName { get; set; } = "";

    [Range(1,5)] public int Defense { get; set; } = 3;
    [Range(1,5)] public int Passing { get; set; } = 3;
    [Range(1,5)] public int Speed { get; set; } = 3;
    [Range(1,5)] public int Stamina { get; set; } = 3;
    [Range(1,5)] public int Dribbling { get; set; } = 3;
    [Range(1,5)] public int Finishing { get; set; } = 3;
    [Range(1,5)] public int Teamwork { get; set; } = 3;
    [Range(1,5)] public int Goalkeeping { get; set; } = 3;
}
