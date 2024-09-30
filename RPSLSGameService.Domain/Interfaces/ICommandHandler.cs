using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Domain.Interfaces
{
    public interface ICommandHandler<TCommand>
    {
        Task<IActionResult> Handle(TCommand command, CancellationToken cancellationToken);
    }
}
