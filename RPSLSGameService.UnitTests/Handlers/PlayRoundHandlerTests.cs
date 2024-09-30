using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RPSLSGameService.Application.Handlers;
using RPSLSGameService.Domain.Interfaces;
using RPSLSGameService.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using RPSLSGameService.Utilities;
using RPSLSGameService.Application.RPSLSCommands.Requests;
using RPSLSGameService.Domain.Models.Response;

namespace RPSLSGameService.UnitTests.Handlers
{
    public class PlayRoundHandlerTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly TestLogger<RandomChoiceService> _testLoggerForService; // Use the custom logger
        private readonly TestLogger<PlayRoundHandler> _testLogger; // Use the custom logger
        private readonly Mock<IValidator<RPSLSEnum>> _mockValidator;
        private readonly Mock<RandomChoiceService> _mockRandomChoiceService;
        private readonly PlayRoundHandler _handler;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public PlayRoundHandlerTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _testLoggerForService = new TestLogger<RandomChoiceService>();
            _testLogger = new TestLogger<PlayRoundHandler>();
            _mockValidator = new Mock<IValidator<RPSLSEnum>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["RandomChoiceService:ApiUrl"]).Returns("https://codechallenge.boohma.com/random");

            // Mock RandomChoiceService directly since it's going to be used to control behavior
            _mockRandomChoiceService = new Mock<RandomChoiceService>(_mockHttpClientFactory.Object, _testLoggerForService, _mockConfiguration.Object);

            // Initialize the PlayRoundHandler with mocked dependencies
            _handler = new PlayRoundHandler(_mockRandomChoiceService.Object, _testLogger, _mockValidator.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnBadRequestIfValidationFails()
        {
            // Arrange
            _mockValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(false);

            var command = new PlayRoundCommand { PlayerChoice = RPSLSEnum.Rock };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            //Assert.Contains("Invalid choice.", errorResponse.Message);
        }

        [Fact]
        public async Task Handle_ShouldPlayRoundSuccessfully()
        {
            // Arrange
            var command = new PlayRoundCommand { PlayerChoice = RPSLSEnum.Rock };

            // Set up the validator to return true
            _mockValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(true);
            _mockRandomChoiceService.Setup(service => service.GetRandomChoiceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(RPSLSEnum.Scissors); // Expected random choice

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var playResult = Assert.IsType<PlayResult>(okResult.Value);

            // Check expected outcomes
            Assert.Equal("win", playResult.Result); // Check that Rock beats Scissors
            Assert.Equal(RPSLSEnum.Rock, playResult.PlayerChoice); // Verify player's choice
            Assert.Equal(RPSLSEnum.Scissors, playResult.ComputerChoice); // Verify computer's choice
        }

        [Fact]
        public async Task Handle_ShouldLogInformationWhenPlayingRound()
        {
            // Arrange
            var command = new PlayRoundCommand { PlayerChoice = RPSLSEnum.Rock };
            _mockValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(true);
            _mockRandomChoiceService.Setup(service => service.GetRandomChoiceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(RPSLSEnum.Scissors);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            string expectedLogMessage = "Executing game round for player choice: Rock.";
            Assert.Contains(expectedLogMessage, _testLogger.LogMessages);
        }

        [Fact]
        public async Task Handle_ShouldLogWarningIfValidationFails()
        {
            // Arrange
            var command = new PlayRoundCommand { PlayerChoice = RPSLSEnum.Rock };
            _mockValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(false);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            string expectedLogMessage = "Validation failed for player choice: Rock. Error: (null).";
            Assert.Contains(expectedLogMessage, _testLogger.LogMessages);
        }

        [Fact]
        public async Task Handle_ShouldReturnErrorIfServiceThrowsException()
        {
            // Arrange
            var command = new PlayRoundCommand { PlayerChoice = RPSLSEnum.Rock };
            _mockValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny)).Returns(true);
            _mockRandomChoiceService.Setup(service => service.GetRandomChoiceAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new System.Exception("Service error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Contains("Service error", errorResponse.Message);
        }

        [Fact]
        public async Task Handle_ShouldHandleCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token before the operation
            var command = new PlayRoundCommand { PlayerChoice = RPSLSEnum.Rock };

            // Set up the validator to return true
            _mockValidator.Setup(validator => validator.Validate(It.IsAny<RPSLSEnum>(), out It.Ref<string>.IsAny))
                .Returns(true);

            // Act
            var result = await _handler.Handle(command, cts.Token);

            // Check if it returns BadRequest object with cancellation message
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("Operation was canceled.", errorResponse.Message);
        }

    }
}