using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RPSLSGameService.Application.RPSLSQueries.Interfaces;
using RPSLSGameService.Application.RPSLSQueries.Requests;
using RPSLSGameService.Domain.Models.Request;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Services;
using RPSLSGameService.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Application.Handlers
{
    public class GetRandomChoiceHandler : IQueryHandler<GetRandomChoiceQuery, IActionResult>
    {
        private readonly RandomChoiceService _randomChoiceService;
        private readonly ILogger<GetRandomChoiceHandler> _logger;

        public GetRandomChoiceHandler(RandomChoiceService randomChoiceService, ILogger<GetRandomChoiceHandler> logger)
        {
            _randomChoiceService = randomChoiceService;
            _logger = logger;
        }

        public async Task<IActionResult> Handle(GetRandomChoiceQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // Obtain random choice
                RPSLSEnum randomChoiceEnum = await _randomChoiceService.GetRandomChoiceAsync(cancellationToken);
                // Return a new Choice object
                return new OkObjectResult(new Choice(randomChoiceEnum));
            }
            catch (OperationCanceledException)
            {
                // Log that the operation was canceled
                _logger.LogWarning("Get random choice operation was canceled.");
                // Return a bad request response or some indication of cancellation if desired
                return new BadRequestObjectResult(new ErrorResponse { Message = "Operation was canceled." });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while getting random choice.");

                // Return a BadRequestObjectResult with the error message
                return new BadRequestObjectResult(new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
