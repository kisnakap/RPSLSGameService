using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Services;
using RPSLSGameService.Utilities;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RPSLSGameService.UnitTests.Services
{
    public class RandomChoiceServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly TestLogger<RandomChoiceService> _testLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly RandomChoiceService _randomChoiceService;

        public RandomChoiceServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _testLogger = new TestLogger<RandomChoiceService>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Set some dummy configuration for the API URL
            _mockConfiguration.Setup(c => c["RandomChoiceService:ApiUrl"]).Returns("https://codechallenge.boohma.com/random");
            _randomChoiceService = new RandomChoiceService(
                _mockHttpClientFactory.Object,
                _testLogger,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetRandomChoiceAsync_ShouldReturnValidRPSLSEnum_WhenApiCallIsSuccessful()
        {
            // Arrange
            var randomResponse = new RandomResponse { RandomNumber = 7 }; // This should give "Scissors"
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(randomResponse))
            };

            // Setup the HttpMessageHandler to return the response
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", // Name of the protected method to set up
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get), 
                    ItExpr.IsAny<CancellationToken>() // Accept any CancellationToken
                )
                .ReturnsAsync(httpResponse); // Returns our pre-defined response

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(client);

            // Act
            var result = await _randomChoiceService.GetRandomChoiceAsync(CancellationToken.None);

            // Assert
            Assert.Equal(RPSLSEnum.Scissors, result); // 7 % 5 + 1 = 3 (this maps to the enum for Scissors)

            // Verify that SendAsync was called
            mockHttpMessageHandler
                .Protected()
                .Verify("SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get), // Ensure the method is GET
                    ItExpr.IsAny<CancellationToken>()); // Accepts any cancellation token
        }

        [Fact]
        public async Task GetRandomChoiceAsync_ShouldThrowException_WhenApiReturnsNull()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("null")
            };

            var mockClient = new Mock<HttpMessageHandler>();
            mockClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(mockClient.Object);
            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(client);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _randomChoiceService.GetRandomChoiceAsync(CancellationToken.None));
            Assert.Equal("Invalid response from the random number service.", ex.Message);
        }

        [Fact]
        public async Task GetRandomChoiceAsync_ShouldThrowException_WhenRandomNumberIsInvalid()
        {
            // Arrange
            var randomResponse = new RandomResponse { RandomNumber = -1 }; // Invalid number
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(randomResponse))
            };

            var mockClient = new Mock<HttpMessageHandler>();
            mockClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var client = new HttpClient(mockClient.Object);
            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(client);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _randomChoiceService.GetRandomChoiceAsync(CancellationToken.None));
            Assert.Equal("Invalid response from the random number service.", ex.Message);
        }

        [Fact]
        public async Task GetRandomChoiceAsync_ShouldLogError_WhenHttpRequestFails()
        {
            // Arrange
            var mockClient = new Mock<HttpMessageHandler>();
            mockClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get), 
                    ItExpr.IsAny<CancellationToken>())   // Match any CancellationToken
                .ThrowsAsync(new HttpRequestException("Network error")); // Simulate network error

            var client = new HttpClient(mockClient.Object);
            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(client);

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _randomChoiceService.GetRandomChoiceAsync(CancellationToken.None));

            // Assert
            Assert.Equal("Could not fetch random choice from external service.", ex.Message);

            // Verify that the appropriate error message was logged
            string expectedLogMessage = "Error fetching random choice from external service.Request URL: https://codechallenge.boohma.com/random.";
            Assert.Contains(expectedLogMessage, _testLogger.LogMessages);
        }

        [Fact]
        public async Task GetRandomChoiceAsync_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token before the operation

            // Set up a mock that simulates a successful call.
            var randomResponse = new RandomResponse { RandomNumber = 1 };
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(randomResponse))
            };

            var mockClient = new Mock<HttpMessageHandler>();
            mockClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get), 
                    ItExpr.IsAny<CancellationToken>()) // Accept any CancellationToken
                .ReturnsAsync(httpResponse); // Returns our pre-defined response

            var client = new HttpClient(mockClient.Object);
            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(client);

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                _randomChoiceService.GetRandomChoiceAsync(cts.Token));
        }
    }
}

