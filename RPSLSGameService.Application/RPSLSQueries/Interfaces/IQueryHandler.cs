using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Application.RPSLSQueries.Interfaces
{
    public interface IQueryHandler<TQuery, TResult>
    {
        Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
    }
}
