using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGameSessionRepository GameSessions { get; }
        IMatchResultRepository MatchResults { get; }
        IPlayerRepository Players { get; }
        Task<int> CompleteAsync(CancellationToken cancellationToken);
    }
}
