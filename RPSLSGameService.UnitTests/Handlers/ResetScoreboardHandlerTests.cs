using Microsoft.AspNetCore.Mvc;
using Moq;
using RPSLSGameService.Application.Handlers;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Infrastructure.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RPSLSGameService.UnitTests.Handlers
{
    public class ResetScoreboardHandlerTests
    {
        private readonly Mock<IMatchResultRepository> _mockMatchResultRepository;
        private readonly ResetScoreboardHandler _handler;
        private readonly TestLogger<ResetScoreboardHandler> _testLogger; // Use the custom logger

        public ResetScoreboardHandlerTests()
        {
            _testLogger = new TestLogger<ResetScoreboardHandler>();
            _mockMatchResultRepository = new Mock<IMatchResultRepository>();
            _handler = new ResetScoreboardHandler(_mockMatchResultRepository.Object, _testLogger);
        }

        [Fact]
        public async Task Handle_ShouldReturnOk_WhenResetIsSuccessful()
        {
            // Arrange
            var command = new ResetScoreboardCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockMatchResultRepository.Verify(repo => repo.ResetResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _mockMatchResultRepository.Setup(repo => repo.ResetResultsAsync(It.IsAny<CancellationToken>()))
                                      .ThrowsAsync(new System.Exception("Reset failed.")); // Simulate an exception

            var command = new ResetScoreboardCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Contains("Reset failed.", errorResponse.Message);
        }

        [Fact]
        public async Task Handle_ShouldCallResetResultsAsync()
        {
            // Arrange
            var command = new ResetScoreboardCommand();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockMatchResultRepository.Verify(repo => repo.ResetResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldHandleCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token before the operation
            var command = new ResetScoreboardCommand();

            // Act & Assert
            var result = await _handler.Handle(command, cts.Token);

            // Check if it returns BadRequest object with cancellation message
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("Operation was canceled.", errorResponse.Message);
        }
    }
}
