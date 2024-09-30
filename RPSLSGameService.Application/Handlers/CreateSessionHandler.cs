using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Domain.Interfaces;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Application.Handlers
{
    public class CreateSessionHandler : ICommandHandler<CreateSessionCommand>
    {
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly ILogger<CreateSessionHandler> _logger;

        public CreateSessionHandler(IGameSessionRepository gameSessionRepository, ILogger<CreateSessionHandler> logger)
        {
            _gameSessionRepository = gameSessionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Handle(CreateSessionCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Check for cancellation before proceeding to create session
                cancellationToken.ThrowIfCancellationRequested();

                var session = new GameSession { SessionId = Guid.NewGuid() };
                // Use the asynchronous add method from your repository
                await _gameSessionRepository.AddSessionAsync(session, cancellationToken);
                var response = new CreateSessionResponse { SessionId = session.SessionId };
                // Return the session ID as an OkObjectResult directly
                return new OkObjectResult(response);
            }
            catch (OperationCanceledException)
            {
                // Log that the operation was canceled
                _logger.LogWarning("Create session operation was canceled.");
                // Return a bad request response or some indication of cancellation if desired
                return new BadRequestObjectResult(new ErrorResponse { Message = "Operation was canceled." });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while creating session.");

                // Return a BadRequestObjectResult with the error message
                return new BadRequestObjectResult(new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
