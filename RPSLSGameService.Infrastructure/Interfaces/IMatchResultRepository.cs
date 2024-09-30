using RPSLSGameService.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Infrastructure.Interfaces
{
    public interface IMatchResultRepository
    {
        Task AddMatchResultAsync(MatchResult matchResult, CancellationToken cancellationToken);
        Task<List<MatchResult>> GetRecentResultsAsync(int count, CancellationToken cancellationToken);
        Task ResetResultsAsync(CancellationToken cancellationToken);
    }
}
