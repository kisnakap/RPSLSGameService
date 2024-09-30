using Microsoft.EntityFrameworkCore;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Repositories
{
    public class MatchResultRepository : IMatchResultRepository
    {
        private readonly RPSLSDbContext _context;

        public MatchResultRepository(RPSLSDbContext context)
        {
            _context = context;
        }

        public async Task AddMatchResultAsync(MatchResult matchResult, CancellationToken cancellationToken)
        {
            await _context.MatchResults.AddAsync(matchResult, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<MatchResult>> GetRecentResultsAsync(int count, CancellationToken cancellationToken)
        {
            return await _context.MatchResults
                                 .OrderByDescending(mr => mr.ResultDate)
                                 .Take(count)
                                 .ToListAsync(cancellationToken);
        }

        public async Task ResetResultsAsync(CancellationToken cancellationToken)
        {
            foreach (var match in _context.MatchResults)
            {
                _context.MatchResults.Remove(match);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
