using RPSLSGameService.Domain.Models;
using RPSLSGameService.Utilities;
using System;
using Xunit;

namespace RPSLSGameService.UnitTests.Domain
{
    public class GameSessionTests
    {
        private GameSession _gameSession;

        public GameSessionTests()
        {
            _gameSession = new GameSession { SessionId = Guid.NewGuid() };
        }

        [Fact]
        public void AddPlayer_ShouldAddPlayer_WhenSessionIsInWaitingForPlayersState()
        {
            // Arrange
            var player = new Player { Name = "Player1" };

            // Act
            var result = _gameSession.AddPlayer(player);

            // Assert
            Assert.Contains(player, _gameSession.Players);
            Assert.Equal(player, result);
            Assert.Equal(GameState.WaitingForPlayers, _gameSession.CurrentState);
        }

        [Fact]
        public void AddPlayer_ShouldThrowInvalidOperationException_WhenGameHasStarted()
        {
            // Arrange
            _gameSession.AddPlayer(new Player { Name = "Player1" });
            _gameSession.AddPlayer(new Player { Name = "Player2" }); // Now game state should be ChoicesSubmitted

            // Act & Assert
            var player = new Player { Name = "Player3" };
            var ex = Assert.Throws<InvalidOperationException>(() => _gameSession.AddPlayer(player));
            Assert.Equal("Cannot add players after the game has started.", ex.Message);
        }

        [Fact]
        public void AddPlayer_ShouldThrowInvalidOperationException_WhenPlayerAlreadyExists()
        {
            // Arrange
            var player = new Player { Name = "Player1" };
            _gameSession.AddPlayer(player);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _gameSession.AddPlayer(player));
            Assert.Equal("Session already has two players or the player is already in the session.", ex.Message);
        }

        [Fact]
        public void SubmitChoice_ShouldSubmitChoice_WhenCurrentStateIsChoicesSubmitted()
        {
            // Arrange
            var player1 = new Player { Name = "Player1" };
            var player2 = new Player { Name = "Player2" };
            _gameSession.AddPlayer(player1);
            _gameSession.AddPlayer(player2);
            _gameSession.SubmitChoice("Player1", RPSLSEnum.Rock);

            // Act
            var result = _gameSession.SubmitChoice("Player2", RPSLSEnum.Scissors);

            // Assert
            Assert.Equal(RPSLSEnum.Scissors, player2.Choice);
            Assert.Equal(GameState.GameInProgress, _gameSession.CurrentState);
        }

        [Fact]
        public void SubmitChoice_ShouldThrowInvalidOperationException_WhenStateIsNotChoicesSubmitted()
        {
            // Arrange
            var player1 = new Player { Name = "Player1" };
            var player2 = new Player { Name = "Player2" };
            _gameSession.AddPlayer(player1);
            _gameSession.AddPlayer(player2);
            _gameSession.SubmitChoice("Player1", RPSLSEnum.Rock);
            _gameSession.SubmitChoice("Player2", RPSLSEnum.Scissors);
            _gameSession.FinalizeGame();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _gameSession.SubmitChoice("Player1", RPSLSEnum.Rock));
            Assert.Equal("Invalid state for submitting choices.", ex.Message);
        }

        [Fact]
        public void SubmitChoice_ShouldThrowInvalidOperationException_WhenPlayerNotFound()
        {
            // Arrange
            var player1 = new Player { Name = "Player1" };
            var player2 = new Player { Name = "Player2" };
            _gameSession.AddPlayer(player1);
            _gameSession.AddPlayer(player2);
            _gameSession.SubmitChoice("Player1", RPSLSEnum.Rock);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _gameSession.SubmitChoice("Player3", RPSLSEnum.Lizard));
            Assert.Equal("Player not found in the session.", ex.Message);
        }

        [Fact]
        public void FinalizeGame_ShouldThrowInvalidOperationException_WhenStateIsNotGameInProgress()
        {
            // Arrange
            var player1 = new Player { Name = "Player1" };
            var player2 = new Player { Name = "Player2" };

            // Add players while the state is waiting for players, which is the default state
            _gameSession.AddPlayer(player1);
            _gameSession.AddPlayer(player2);

            // Transition to a different state (for instance, after both players join)
            _gameSession.SubmitChoice("Player1", RPSLSEnum.Rock);
            _gameSession.SubmitChoice("Player2", RPSLSEnum.Scissors); // This should transition to GameInProgress

            // Finalize the game first to move to the GameResults state
            _gameSession.FinalizeGame();

            // At this point, the state has transitioned to GameResults, and we need to attempt finalization
            // So, let's forcefully transition to an invalid state by directly accessing the private state

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _gameSession.FinalizeGame());
            Assert.Equal("Game must be in progress to finalize.", ex.Message);
        }
    }
}
