using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RPSLSGameService.Application.RPSLSQueries.Interfaces;
using RPSLSGameService.Application.RPSLSQueries.Requests;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Application.Handlers
{
    public class GetScoreboardHandler : IQueryHandler<GetScoreboardQuery, IActionResult>
    {
        private readonly IMatchResultRepository _repository;
        private readonly ILogger<GetScoreboardHandler> _logger;

        public GetScoreboardHandler(IMatchResultRepository repository, ILogger<GetScoreboardHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IActionResult> Handle(GetScoreboardQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // Check for cancellation before proceeding to get scoreboard results
                cancellationToken.ThrowIfCancellationRequested();

                // Call the method with expected parameters and await the result
                var scoreboard = await _repository.GetRecentResultsAsync(10, cancellationToken);
                var results = scoreboard?.ToList() ?? new List<MatchResult>();
                return new OkObjectResult(results);
            }
            catch (OperationCanceledException)
            {
                // Log that the operation was canceled
                _logger.LogWarning("Get scoreboard operation was canceled.");
                // Return a bad request response or some indication of cancellation if desired
                return new BadRequestObjectResult(new ErrorResponse { Message = "Operation was canceled." });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error occurred while fetching scoreboard.");

                // Return an appropriate BadRequest response
                return new BadRequestObjectResult(new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
