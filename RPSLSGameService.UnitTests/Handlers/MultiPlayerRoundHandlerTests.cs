using Microsoft.AspNetCore.Mvc;
using Moq;
using RPSLSGameService.Application.Handlers;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Domain.Interfaces;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Domain.Models.Request;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Infrastructure.Interfaces;
using RPSLSGameService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RPSLSGameService.UnitTests.Handlers
{
    public class MultiplayerRoundHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPlayerRepository> _mockPlayerRepository;
        private readonly Mock<IGameSessionRepository> _mockGameSessionRepository;
        private readonly Mock<IMatchResultRepository> _mockMatchResultRepository;
        private readonly Mock<IValidator<RPSLSEnum>> _mockChoiceValidator;
        private readonly TestLogger<MultiplayerRoundHandler> _testLogger; // Use the custom logger
        private readonly MultiplayerRoundHandler _handler;

        public MultiplayerRoundHandlerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPlayerRepository = new Mock<IPlayerRepository>();
            _mockGameSessionRepository = new Mock<IGameSessionRepository>();
            _mockMatchResultRepository = new Mock<IMatchResultRepository>();
            _mockChoiceValidator = new Mock<IValidator<RPSLSEnum>>();
            _testLogger = new TestLogger<MultiplayerRoundHandler>();

            _mockUnitOfWork.Setup(uow => uow.Players).Returns(_mockPlayerRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.GameSessions).Returns(_mockGameSessionRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.MatchResults).Returns(_mockMatchResultRepository.Object);

            _handler = new MultiplayerRoundHandler(_mockUnitOfWork.Object, _testLogger, _mockChoiceValidator.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnBadRequestIfChoiceIsInvalid()
        {
            // Arrange
            var invalidChoice = (RPSLSEnum)99; // Invalid choice
            var command = new PlayMultiplayerCommand
            {
                SessionId = Guid.NewGuid(),
                Request = new MultiPlayerRequest { Name = "Player1", Choice = invalidChoice }
            };
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            //Assert.Contains("Invalid choice.", errorResponse.Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFoundIfSessionIsNull()
        {
            // Arrange
            var command = new PlayMultiplayerCommand
            {
                SessionId = Guid.NewGuid(),
                Request = new MultiPlayerRequest { Name = "Player1" }
            };

            _mockUnitOfWork.Setup(uow => uow.GameSessions.GetSessionAsync(command.SessionId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((GameSession)null); // Simulate a nonexistent session

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Handle_ShouldAddPlayer_WhenChoiceIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var command = new PlayMultiplayerCommand
            {
                SessionId = sessionId,
                Request = new MultiPlayerRequest { Name = "Player1", Choice = null } // No initial choice
            };

            var session = new GameSession { SessionId = sessionId, Players = new List<Player>() };
            _mockGameSessionRepository.Setup(repo => repo.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(session); // Mock the session retrieval

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            // Ensure that the player was added to the session.
            Assert.Single(session.Players); // Check that exactly one player is added
            Assert.Equal("Player1", session.Players.First().Name); // Verify the player's name
            Assert.IsType<ObjectResult>(result); // Check that a valid ObjectResult is returned
            Assert.Equal(GameState.WaitingForPlayers, session.CurrentState);
            _mockPlayerRepository.Verify(repo => repo.AddPlayerAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Once); // Verify that the player was added
        }


        [Fact]
        public async Task Handle_ShouldSubmitPlayerChoice_WhenChoiceIsProvided()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var session = new GameSession { SessionId = sessionId, Players = new List<Player>() };
            _mockUnitOfWork.Setup(uow => uow.GameSessions.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(session);

            // Pre-populate the session with players
            session.AddPlayer(new Player { Name = "Player1" });
            session.AddPlayer(new Player { Name = "Player2" });

            _mockChoiceValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(true);

            // Act
            var result = await _handler.Handle(new PlayMultiplayerCommand
            {
                SessionId = sessionId,
                Request = new MultiPlayerRequest { Name = "Player1", Choice = RPSLSEnum.Rock }
            }, CancellationToken.None);

            // Assert
            Assert.Equal(GameState.ChoicesSubmitted, session.CurrentState);
            Assert.IsType<ObjectResult>(result);
            _mockUnitOfWork.Verify(uow => uow.Players.UpdatePlayerAsync(It.IsAny<Player>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldLogInformationWhenPlayerChoiceIsSubmitted()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var session = new GameSession { SessionId = sessionId, Players = new List<Player>() };
            _mockUnitOfWork.Setup(uow => uow.GameSessions.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(session);

            // Pre-populate the session with players
            session.AddPlayer(new Player { Name = "Player1" });
            session.AddPlayer(new Player { Name = "Player2" });

            _mockChoiceValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(true);

            var command = new PlayMultiplayerCommand
            {
                SessionId = sessionId,
                Request = new MultiPlayerRequest { Name = "Player1", Choice = RPSLSEnum.Rock }
            };

            _mockUnitOfWork.Setup(uow => uow.GameSessions.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(session);

            _mockChoiceValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            string expectedLogMessage = "Executing game round for player Player1 choice: Rock.";
            Assert.Contains(expectedLogMessage, _testLogger.LogMessages);
        }

        [Fact]
        public async Task Handle_ShouldFinalizeGame_WhenBothPlayersHaveMadeChoices()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var session = new GameSession { SessionId = sessionId, Players = new List<Player>() };
            _mockUnitOfWork.Setup(uow => uow.GameSessions.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(session);

            // Pre-populate the session with players
            session.AddPlayer(new Player { Name = "Player1" });
            session.AddPlayer(new Player { Name = "Player2" });

            _mockChoiceValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(true);
            
            // Act
            await _handler.Handle(new PlayMultiplayerCommand
            {
                SessionId = sessionId,
                Request = new MultiPlayerRequest { Name = "Player1", Choice = RPSLSEnum.Rock }
            }, CancellationToken.None);

            var result = await _handler.Handle(new PlayMultiplayerCommand
            {
                SessionId = sessionId,
                Request = new MultiPlayerRequest { Name = "Player2", Choice = RPSLSEnum.Scissors }
            }, CancellationToken.None);

            // Assert
            Assert.Equal(GameState.GameResults, session.CurrentState); // Verify state transition
            var okResult = Assert.IsType<OkObjectResult>(result);
            var playResult = Assert.IsType<MultiplayerResult>(okResult.Value);

            // Check expected outcomes
            Assert.Equal("win", playResult.Result); // Check that Rock beats Scissors
            Assert.Equal("Player1", playResult.WinnerName); // Verify player's choice
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFound_WhenPlayerNotFound()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var session = new GameSession { SessionId = sessionId, Players = new List<Player>() };
            _mockUnitOfWork.Setup(uow => uow.GameSessions.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(session);

            // Pre-populate the session with players
            session.AddPlayer(new Player { Name = "Player1" });
            session.AddPlayer(new Player { Name = "Player2" });
            _mockUnitOfWork.Setup(uow => uow.GameSessions.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(session);

            _mockChoiceValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(true);

            var command = new PlayMultiplayerCommand
            {
                SessionId = sessionId,
                Request = new MultiPlayerRequest { Name = "NonExistentPlayer", Choice = RPSLSEnum.Rock }
            };

            // Act & Assert
            var result = await _handler.Handle(command, CancellationToken.None);
            
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);

            Assert.Equal("Player not found in the session.", errorResponse.Message);
        }

        [Fact]
        public async Task Handle_ShouldHandleCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token before the operation
            var sessionId = Guid.NewGuid();
            var command = new PlayMultiplayerCommand
            {
                SessionId = sessionId,
                Request = new MultiPlayerRequest { Name = "Player1", Choice = null } // No initial choice
            };

            var session = new GameSession { SessionId = sessionId, Players = new List<Player>() };
            _mockGameSessionRepository.Setup(repo => repo.GetSessionAsync(sessionId, It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(session); // Mock the session retrieval

            // Act & Assert
            var result = await _handler.Handle(command, cts.Token);

            // Check if it returns BadRequest object with cancellation message
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("Operation was canceled.", errorResponse.Message);
        }
    }
}
