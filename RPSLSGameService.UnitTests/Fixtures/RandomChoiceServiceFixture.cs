using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RPSLSGameService.Services;
using System.Net.Http;

namespace RPSLSGameService.UnitTests.Fixtures
{
    public class RandomChoiceServiceFixture
    {
        public Mock<IHttpClientFactory> MockHttpClientFactory { get; private set; }
        public Mock<ILogger<RandomChoiceService>> MockLogger { get; private set; }
        public Mock<IConfiguration> MockConfiguration { get; private set; }
        public RandomChoiceService RandomChoiceService { get; private set; }

        public RandomChoiceServiceFixture()
        {
            MockHttpClientFactory = new Mock<IHttpClientFactory>();
            MockLogger = new Mock<ILogger<RandomChoiceService>>();
            MockConfiguration = new Mock<IConfiguration>();

            // Create the RandomChoiceService instance with mocks
            RandomChoiceService = new RandomChoiceService(
                MockHttpClientFactory.Object,
                MockLogger.Object,
                MockConfiguration.Object
            );
        }
    }
}
