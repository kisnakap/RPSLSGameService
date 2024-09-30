using RPSLSGameService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Interfaces
{
    public interface ILeaderboardRepository
    {
        Task UpdatePlayerStatsAsync(string playerName, bool isWin, CancellationToken cancellationToken);
        IEnumerable<PlayerStats> GetLeaderboard();
    }
}
