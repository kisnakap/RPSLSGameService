using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Domain.Interfaces;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Application.Handlers
{
    public class ResetScoreboardHandler : ICommandHandler<ResetScoreboardCommand>
    {
        private readonly IMatchResultRepository _matchResultRepository;
        private readonly ILogger<ResetScoreboardHandler> _logger;

        public ResetScoreboardHandler(IMatchResultRepository matchResultRepository, ILogger<ResetScoreboardHandler> logger)
        {
            _matchResultRepository = matchResultRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Handle(ResetScoreboardCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Check for cancellation before proceeding to reset match results
                cancellationToken.ThrowIfCancellationRequested();

                // Reset the match results
                await _matchResultRepository.ResetResultsAsync(cancellationToken);
                // Indicate success
                return new OkResult();
            }
            catch (OperationCanceledException)
            {
                // Log that the operation was canceled
                _logger.LogWarning("Reset scoreboard operation was canceled.");
                // Return a bad request response or some indication of cancellation if desired
                return new BadRequestObjectResult(new ErrorResponse { Message = "Operation was canceled." });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while resetting the scoreboard.");

                // Return a BadRequestObjectResult with the error message
                return new BadRequestObjectResult(new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
