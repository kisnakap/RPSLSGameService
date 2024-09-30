using Microsoft.AspNetCore.Mvc;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Application.RPSLSQueries.Interfaces;
using RPSLSGameService.Application.RPSLSQueries.Requests;
using RPSLSGameService.Domain.Interfaces;
using RPSLSGameService.Domain.Models.Request;
using RPSLSGameService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RPSLSGameService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly ICommandHandler<PlayRoundCommand> _playRoundHandler;
        private readonly ICommandHandler<PlayMultiplayerCommand> _multiplayerRoundHandler;
        private readonly ICommandHandler<CreateSessionCommand> _createSessionHandler;
        private readonly IQueryHandler<GetScoreboardQuery, IActionResult> _getScoreboardHandler;
        private readonly ICommandHandler<ResetScoreboardCommand> _resetScoreboardHandler;
        private readonly IQueryHandler<GetRandomChoiceQuery, IActionResult> _getRandomChoiceHandler;

        public GameController(
            ICommandHandler<PlayRoundCommand> playRoundHandler,
            ICommandHandler<PlayMultiplayerCommand> multiplayerRoundHandler,
            ICommandHandler<CreateSessionCommand> createSessionHandler,
            IQueryHandler<GetScoreboardQuery, IActionResult> getScoreboardHandler,
            ICommandHandler<ResetScoreboardCommand> resetScoreboardHandler,
            IQueryHandler<GetRandomChoiceQuery, IActionResult> getRandomChoiceHandler)
        {
            _playRoundHandler = playRoundHandler;
            _multiplayerRoundHandler = multiplayerRoundHandler;
            _createSessionHandler = createSessionHandler;
            _getScoreboardHandler = getScoreboardHandler;
            _resetScoreboardHandler = resetScoreboardHandler;
            _getRandomChoiceHandler = getRandomChoiceHandler;

        }

        /// <summary>
        /// Gets the available choices for the game.
        /// </summary>
        /// <returns>A list of valid game choices.</returns>
        [HttpGet("choices")]
        public IActionResult GetChoices()
        {
            var choices = CreateChoices();
            return Ok(choices);
        }

        /// <summary>
        /// Gets a random choice for the game.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A random choice from the available options.</returns>
        [HttpGet("choice")]
        public async Task<IActionResult> GetRandomChoice(CancellationToken cancellationToken)
        {
            var query = new GetRandomChoiceQuery();
            return await _getRandomChoiceHandler.Handle(query, cancellationToken);
        }

        /// <summary>
        /// Plays a round of the game with the specified player choice.
        /// </summary>
        /// <param name="request">The player's choice and other relevant data.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>The result of the game round.</returns>
        [HttpPost("play")]
        public async Task<IActionResult> PlayRound([FromBody] PlayRequest request, CancellationToken cancellationToken)
        {
            var command = new PlayRoundCommand { PlayerChoice = request.Choice };
            return await _playRoundHandler.Handle(command, cancellationToken);
        }

        /// <summary>
        /// Creates a new game session.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A response indicating the creation of the session.</returns>
        [HttpPost("createSession")]
        public async Task<IActionResult> CreateSession(CancellationToken cancellationToken)
        {
            var command = new CreateSessionCommand();
            return await _createSessionHandler.Handle(command, cancellationToken);
        }

        /// <summary>
        /// Plays a multiplayer game round (2 players only) with the specified session and player data.
        /// </summary>
        /// <param name="sessionId">The unique identifier for the game session.</param>
        /// <param name="request">The player's choice and other relevant data.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>The result of the multiplayer game round.</returns>
        [HttpPost("playMulti")]
        public async Task<IActionResult> PlayMultiplayer(Guid sessionId, [FromBody] MultiPlayerRequest request, CancellationToken cancellationToken)
        {
            var command = new PlayMultiplayerCommand
            {
                SessionId = sessionId,
                Request = request
            };
            return await _multiplayerRoundHandler.Handle(command, cancellationToken);
        }

        /// <summary>
        /// Gets the current scoreboard, which displays the most recent match results.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A list of recent match results.</returns>
        [HttpGet("scoreboard")]
        public async Task<IActionResult> GetScoreboard(CancellationToken cancellationToken)
        {
            var query = new GetScoreboardQuery();
            return await _getScoreboardHandler.Handle(query, cancellationToken);
        }

        /// <summary>
        /// Resets the scoreboard, clearing all recorded match results.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A response indicating the outcome of the reset operation.</returns>
        [HttpPost("resetScoreboard")]
        public async Task<IActionResult> ResetScoreboard(CancellationToken cancellationToken)
        {
            var command = new ResetScoreboardCommand();
            return await _resetScoreboardHandler.Handle(command, cancellationToken);
        }

        private static IEnumerable<Choice> CreateChoices()
        {
            return Enum.GetValues(typeof(RPSLSEnum)).Cast<RPSLSEnum>().Select(value => new Choice(value));
        }
    }
}
