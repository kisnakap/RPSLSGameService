using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Domain.Interfaces;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Infrastructure.Interfaces;
using RPSLSGameService.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Application.Handlers
{

    public class MultiplayerRoundHandler : ICommandHandler<PlayMultiplayerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MultiplayerRoundHandler> _logger;
        private readonly IValidator<RPSLSEnum> _choiceValidator;

        public MultiplayerRoundHandler(IUnitOfWork unitOfWork, ILogger<MultiplayerRoundHandler> logger, IValidator<RPSLSEnum> choiceValidator)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _choiceValidator = choiceValidator;
        }

        public async Task<IActionResult> Handle(PlayMultiplayerCommand command, CancellationToken cancellationToken)
        {
            // Validate the player's choice
            if ((command.Request.Choice != null) && (!_choiceValidator.Validate(command.Request.Choice.Value, out string error)))
            {
                return new BadRequestObjectResult(new ErrorResponse { Message = error });
            }

            // Retrieve the session asynchronously
            var session = await _unitOfWork.GameSessions.GetSessionAsync(command.SessionId, cancellationToken);
            if (session == null)
            {
                return new NotFoundObjectResult("Session not found.");
            }

            try
            {
                // Check for cancellation before proceeding to multiplayer round
                cancellationToken.ThrowIfCancellationRequested();

                // Add Player to the Session
                if (command.Request.Choice == null)
                {
                    _logger.LogInformation("Adding player {PlayerName} to the session: {SessionId}.", command.Request.Name, command.SessionId);
                    Player newPlayer = new Player { Name = command.Request.Name, GameSessionId = session.SessionId};
                    // Add player to the session using the method in GameSession
                    session.AddPlayer(newPlayer);
                    await _unitOfWork.Players.AddPlayerAsync(newPlayer, cancellationToken);
                }

                // Submit Player's Choice
                if (command.Request.Choice != null)
                {
                    _logger.LogInformation("Executing game round for player {PlayerName} choice: {PlayerChoice}.", command.Request.Name, command.Request.Choice);
                    RPSLSEnum choice = command.Request.Choice.Value; // Use .Value to extract the non-nullable value
                    Player player = session.SubmitChoice(command.Request.Name, choice);
                    await _unitOfWork.Players.UpdatePlayerAsync(player, cancellationToken);
                }

                // Persist the session state to the database
                await _unitOfWork.GameSessions.UpdateSessionAsync(session, cancellationToken);

                // Finalize the game if both players are present and have made their choices
                if (session.Players.Count == 2 && session.CurrentState == GameState.GameInProgress)
                {
                    MatchResult matchResult = session.FinalizeGame();
                    await _unitOfWork.MatchResults.AddMatchResultAsync(matchResult, cancellationToken);
                    await _unitOfWork.GameSessions.UpdateSessionAsync(session, cancellationToken); // Update saved game session state
                    string result = (matchResult.WinnerName == "None") ? "tie" : "win";
                    var playResult = new MultiplayerResult { WinnerName = matchResult.WinnerName, Result = result };
                    return new OkObjectResult(playResult);
                }
            }
            catch (InvalidOperationException ex)
            {
                return new BadRequestObjectResult(new ErrorResponse { Message = ex.Message });
            }
            catch (OperationCanceledException)
            {
                // Log that the operation was canceled
                _logger.LogWarning("Multi player round operation was canceled.");
                // Return a bad request response or some indication of cancellation if desired
                return new BadRequestObjectResult(new ErrorResponse { Message = "Operation was canceled." });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "An error occurred while playing multi player round.");

                // Return a BadRequestObjectResult with the error message
                return new BadRequestObjectResult(new ErrorResponse { Message = ex.Message });
            }

            return new ObjectResult(new { state = session.CurrentState.ToString() })
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
