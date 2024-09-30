using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RPSLSGameService.Services;
using RPSLSGameService.Utilities;
using System.Threading;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Domain.Interfaces;
using System;
using RPSLSGameService.Domain.Models.Response;

namespace RPSLSGameService.Application.Handlers
{
    public class PlayRoundHandler : ICommandHandler<PlayRoundCommand>
    {
        private readonly RandomChoiceService _randomChoiceService;
        private readonly ILogger<PlayRoundHandler> _logger;
        private readonly IValidator<RPSLSEnum> _choiceValidator;

        public PlayRoundHandler(RandomChoiceService randomChoiceService, ILogger<PlayRoundHandler> logger, IValidator<RPSLSEnum> choiceValidator)
        {
            _randomChoiceService = randomChoiceService;
            _logger = logger;
            _choiceValidator = choiceValidator;
        }

        public async Task<IActionResult> Handle(PlayRoundCommand command, CancellationToken cancellationToken)
        {
            if (!_choiceValidator.Validate(command.PlayerChoice, out string error))
            {
                _logger.LogWarning("Validation failed for player choice: {PlayerChoice}. Error: {Error}.", command.PlayerChoice, error);
                return new BadRequestObjectResult(new ErrorResponse { Message = error });
            }

            // Check for cancellation before proceeding to the game logic
            if (cancellationToken.IsCancellationRequested) // Checking cancellation state
            {
                _logger.LogWarning("Play round operation was canceled before execution.");
                return new BadRequestObjectResult(new ErrorResponse { Message = "Operation was canceled." });
            }

            try
            {
                _logger.LogInformation("Executing game round for player choice: {PlayerChoice}.", command.PlayerChoice);
                RPSLSEnum computerChoice = await _randomChoiceService.GetRandomChoiceAsync(cancellationToken);
                string result = GameLogicUtils.DetermineWinner(command.PlayerChoice, computerChoice);
                _logger.LogInformation("Computer choice: {ComputerChoice}.", computerChoice);
                var playResult = new PlayResult { Result = result, PlayerChoice = command.PlayerChoice, ComputerChoice = computerChoice };
                return new OkObjectResult(playResult);
            }
            catch (OperationCanceledException)
            {
                // Log that the operation was canceled
                _logger.LogWarning("Play round operation was canceled.");
                // Return a bad request response or some indication of cancellation if desired
                return new BadRequestObjectResult(new ErrorResponse { Message = "Operation was canceled." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while playing round.");
                return new BadRequestObjectResult(new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
