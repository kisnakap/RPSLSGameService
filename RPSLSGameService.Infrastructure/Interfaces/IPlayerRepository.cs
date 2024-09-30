using RPSLSGameService.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player> GetPlayerBySessionAsync(Guid sessionId, string playerName, CancellationToken cancellationToken);
        Task AddPlayerAsync(Player player, CancellationToken cancellationToken);
        Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken);
        Task RemovePlayerAsync(string playerName, CancellationToken cancellationToken);
    }
}
