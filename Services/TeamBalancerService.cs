using FutPib.Models;
using FutPib.ViewModels;

namespace FutPib.Services;

public class TeamBalancerService
{
    public List<DrawResultViewModel> Balance(List<PlayerScoreViewModel> players, int teamCount)
    {
        if (teamCount < 2) teamCount = 2;
        if (players.Count < teamCount)
            throw new InvalidOperationException("Não há jogadores suficientes para a quantidade de times.");

        var teams = Enumerable.Range(1, teamCount)
            .Select(n => new DrawResultViewModel { TeamNumber = n })
            .ToList();

        var ordered = players
            .OrderByDescending(p => p.User.PrimaryPosition == PlayerPosition.Goleiro)
            .ThenByDescending(p => p.Score)
            .ToList();

        foreach (var player in ordered)
        {
            DrawResultViewModel chosen;

            if (player.User.PrimaryPosition == PlayerPosition.Goleiro)
            {
                chosen = teams
                    .OrderBy(t => t.Players.Count(p => p.User.PrimaryPosition == PlayerPosition.Goleiro))
                    .ThenBy(t => t.TotalScore)
                    .ThenBy(t => t.Players.Count)
                    .First();
            }
            else
            {
                chosen = teams
                    .OrderBy(t => t.Players.Count)
                    .ThenBy(t => t.TotalScore)
                    .First();
            }

            chosen.Players.Add(player);
        }

        ImproveBalance(teams);
        return teams;
    }

    private static void ImproveBalance(List<DrawResultViewModel> teams)
    {
        for (var iteration = 0; iteration < 150; iteration++)
        {
            var strongest = teams.OrderByDescending(t => t.TotalScore).First();
            var weakest = teams.OrderBy(t => t.TotalScore).First();
            var currentDifference = strongest.TotalScore - weakest.TotalScore;

            if (currentDifference < 0.15) break;

            var bestSwap = (
                Strong: (PlayerScoreViewModel?)null,
                Weak: (PlayerScoreViewModel?)null,
                Difference: currentDifference
            );

            foreach (var a in strongest.Players)
            {
                foreach (var b in weakest.Players)
                {
                    if (a.User.PrimaryPosition == PlayerPosition.Goleiro &&
                        b.User.PrimaryPosition != PlayerPosition.Goleiro)
                        continue;

                    if (b.User.PrimaryPosition == PlayerPosition.Goleiro &&
                        a.User.PrimaryPosition != PlayerPosition.Goleiro)
                        continue;

                    var newStrong = strongest.TotalScore - a.Score + b.Score;
                    var newWeak = weakest.TotalScore - b.Score + a.Score;
                    var diff = Math.Abs(newStrong - newWeak);

                    if (diff < bestSwap.Difference)
                        bestSwap = (a, b, diff);
                }
            }

            if (bestSwap.Strong is null || bestSwap.Weak is null)
                break;

            strongest.Players.Remove(bestSwap.Strong);
            weakest.Players.Remove(bestSwap.Weak);
            strongest.Players.Add(bestSwap.Weak);
            weakest.Players.Add(bestSwap.Strong);
        }
    }
}
