using RPSLSGameService.Infrastructure.Interfaces;
using RPSLSGameService.Infrastructure.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RPSLSDbContext _context;

        public UnitOfWork(RPSLSDbContext context)
        {
            _context = context;
            GameSessions = new GameSessionRepository(_context);
            MatchResults = new MatchResultRepository(_context);
            Players = new PlayerRepository(_context);
        }

        public IGameSessionRepository GameSessions { get; }
        public IMatchResultRepository MatchResults { get; }
        public IPlayerRepository Players { get; }

        public async Task<int> CompleteAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
