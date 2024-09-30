using RPSLSGameService.Models;
using RPSLSGameService.RPSLSInfrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Repositories
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        /*private readonly List<PlayerStats> _leaderboard = new();

        public void UpdatePlayerStats(string playerName, bool isWin)
        {
            var playerStats = _leaderboard.FirstOrDefault(p => p.PlayerName == playerName) ?? new PlayerStats { PlayerName = playerName };
            if (!_leaderboard.Contains(playerStats))
                _leaderboard.Add(playerStats);

            if (isWin) playerStats.Wins++;
            else playerStats.Losses++;
        }

        public IEnumerable<PlayerStats> GetLeaderboard() => _leaderboard.OrderByDescending(p => p.Wins);*/

        private readonly RPSLSDbContext _context;

        public LeaderboardRepository(RPSLSDbContext context)
        {
            _context = context;
        }

        public async Task UpdatePlayerStatsAsync(string playerName, bool isWin, CancellationToken cancellationToken)
        {
            var playerStats = _context.PlayerStats.FirstOrDefault(p => p.PlayerName == playerName);
            if (playerStats == null)
            {
                playerStats = new PlayerStats { PlayerName = playerName };
                _context.PlayerStats.Add(playerStats);
            }

            if (isWin)
            {
                playerStats.Wins++;
            }
            else
            {
                playerStats.Losses++;
            }

            _context.SaveChanges(); // Save the changes after modifying stats
        }

        public IEnumerable<PlayerStats> GetLeaderboard()
            => _context.PlayerStats.OrderByDescending(p => p.Wins).ToList();
    }
}
