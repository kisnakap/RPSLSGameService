using Microsoft.AspNetCore.Mvc;
using Moq;
using RPSLSGameService.Application.Handlers;
using RPSLSGameService.Application.RPSLSQueries.Requests;
using RPSLSGameService.Domain.Models.Request;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Services;
using RPSLSGameService.UnitTests.Fixtures;
using RPSLSGameService.Utilities;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RPSLSGameService.UnitTests.Handlers
{
    public class GetRandomChoiceHandlerTests : IClassFixture<RandomChoiceServiceFixture>
    {
        private readonly RandomChoiceServiceFixture _fixture;
        private readonly TestLogger<GetRandomChoiceHandler> _testLogger; // Use the custom logger

        public GetRandomChoiceHandlerTests(RandomChoiceServiceFixture fixture)
        {
            _fixture = fixture;
            _testLogger = new TestLogger<GetRandomChoiceHandler>();
        }

        [Fact]
        public async Task Handle_ShouldReturnRandomChoice()
        {
            // Arrange
            // Setup the HttpClient with a BaseAddress
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://codechallenge.boohma.com/random")  
            };

            _fixture.MockHttpClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var expectedChoice = RPSLSEnum.Rock;

            // Now create handler dependency; mock the method
            var mockService = new Mock<RandomChoiceService>(
                _fixture.MockHttpClientFactory.Object,
                _fixture.MockLogger.Object,
                _fixture.MockConfiguration.Object);
            mockService.Setup(service => service.GetRandomChoiceAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedChoice);

            var handler = new GetRandomChoiceHandler(mockService.Object, _testLogger);

            // Act
            var result = await handler.Handle(new GetRandomChoiceQuery(), CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var choice = Assert.IsType<Choice>(okResult.Value);

            Assert.Equal((int)expectedChoice, choice.Id);
            Assert.Equal("Rock", choice.Name);
        }

        [Fact]
        public async Task Handle_ShouldCallRandomChoiceService()
        {
            // Arrange
            var handler = new GetRandomChoiceHandler(_fixture.RandomChoiceService, _testLogger);

            // Act
            await handler.Handle(new GetRandomChoiceQuery(), CancellationToken.None);

            // Assert
            _fixture.MockHttpClientFactory.Verify(s => s.CreateClient(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnRPSLSEnum_IfServiceReturnsValidChoice()
        {
            // Arrange
            var expectedChoice = RPSLSEnum.Scissors;
            var mockService = new Mock<RandomChoiceService>(
                _fixture.MockHttpClientFactory.Object,
                _fixture.MockLogger.Object,
                _fixture.MockConfiguration.Object
            );

            mockService.Setup(service => service.GetRandomChoiceAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedChoice);

            var handler = new GetRandomChoiceHandler(mockService.Object, _testLogger);

            // Act
            var result = await handler.Handle(new GetRandomChoiceQuery(), CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var choice = Assert.IsType<Choice>(okResult.Value);
            Assert.Equal((int)expectedChoice, choice.Id);
            Assert.Equal("Scissors", choice.Name);
        }

        [Fact]
        public async Task Handle_ShouldHandleCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token before the operation
            var _handler = new GetRandomChoiceHandler(_fixture.RandomChoiceService, _testLogger);

            // Act & Assert
            var result = await _handler.Handle(new GetRandomChoiceQuery(), cts.Token);

            // Check if it returns BadRequest object with cancellation message
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("Operation was canceled.", errorResponse.Message);
        }
    }
}
