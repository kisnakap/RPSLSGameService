using RPSLSGameService.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Interfaces
{
    public interface IGameSessionRepository
    {
        Task<GameSession> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken);
        Task AddSessionAsync(GameSession session, CancellationToken cancellationToken);
        Task UpdateSessionAsync(GameSession session, CancellationToken cancellationToken);
    }
}
