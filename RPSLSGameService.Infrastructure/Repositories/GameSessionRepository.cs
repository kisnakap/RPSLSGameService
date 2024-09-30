using Microsoft.EntityFrameworkCore;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Repositories
{
    public class GameSessionRepository : IGameSessionRepository
    {
        private readonly RPSLSDbContext _context;

        public GameSessionRepository(RPSLSDbContext context)
        {
            _context = context;
        }

        public async Task<GameSession> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            return await _context.GameSessions.Include(gsm => gsm.MatchResults).Include(gsp => gsp.Players).FirstOrDefaultAsync(gs => gs.SessionId == sessionId, cancellationToken);
        }

        public async Task AddSessionAsync(GameSession session, CancellationToken cancellationToken)
        {
            await _context.GameSessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateSessionAsync(GameSession session, CancellationToken cancellationToken)
        {
            _context.GameSessions.Update(session);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
