using Microsoft.EntityFrameworkCore;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly RPSLSDbContext _context;

        public PlayerRepository(RPSLSDbContext context)
        {
            _context = context;
        }

        public async Task<Player> GetPlayerBySessionAsync(Guid sessionId, string playerName, CancellationToken cancellationToken)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.GameSessionId == sessionId && p.Name == playerName, cancellationToken);
        }

        public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken)
        {
            await _context.Players.AddAsync(player, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken); // Ensure changes are saved
        }

        public async Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken)
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemovePlayerAsync(string playerName, CancellationToken cancellationToken)
        {
            var player = await _context.Players.SingleOrDefaultAsync(p => p.Name == playerName, cancellationToken);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
