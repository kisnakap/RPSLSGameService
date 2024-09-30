using Microsoft.AspNetCore.Mvc;
using Moq;
using RPSLSGameService.Application.Handlers;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RPSLSGameService.UnitTests.Handlers
{
    public class CreateSessionHandlerTests
    {
        private readonly Mock<IGameSessionRepository> _mockRepository;
        private readonly TestLogger<CreateSessionHandler> _testLogger; // Use the custom logger
        private readonly CreateSessionHandler _handler;

        public CreateSessionHandlerTests()
        {
            _mockRepository = new Mock<IGameSessionRepository>();
            _testLogger = new TestLogger<CreateSessionHandler>();
            _handler = new CreateSessionHandler(_mockRepository.Object, _testLogger);
        }

        [Fact]
        public async Task Handle_ShouldReturnOkWithSessionId()
        {
            // Arrange
            // Assume that the AddSessionAsync method is called and the session is created
            var sessionId = Guid.NewGuid();
            _mockRepository.Setup(x => x.AddSessionAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()))
                          .Callback<GameSession, CancellationToken>((session, ct) =>
                          {
                              session.SessionId = sessionId; // Set the SessionId after creation
                      });

            // Act
            var result = await _handler.Handle(new CreateSessionCommand(), CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CreateSessionResponse>(okResult.Value);
            Assert.Equal(sessionId, response.SessionId);
            _mockRepository.Verify(x => x.AddSessionAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCallRepositoryAddSessionAsyncOnce()
        {
            // Arrange
            // Act
            await _handler.Handle(new CreateSessionCommand(), CancellationToken.None);

            // Assert
            _mockRepository.Verify(x => x.AddSessionAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldHandleCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token before the operation
            var command = new CreateSessionCommand();

            // Act & Assert
            var result = await _handler.Handle(command, cts.Token);

            // Check if it returns BadRequest object with cancellation message
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("Operation was canceled.", errorResponse.Message);
        }
    }
}
