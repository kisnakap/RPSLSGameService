using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading;
using RPSLSGameService.Utilities;
using System.Net.Http.Json;
using RPSLSGameService.Domain.Models.Response;

namespace RPSLSGameService.Services
{
    public class RandomChoiceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RandomChoiceService> _logger;
        private readonly string _apiUrl;

        public RandomChoiceService(IHttpClientFactory httpClientFactory, ILogger<RandomChoiceService> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiUrl = configuration["RandomChoiceService:ApiUrl"]; // Read the URL from configuration
        }

        
        public virtual async Task<RPSLSEnum> GetRandomChoiceAsync(CancellationToken cancellationToken)
        {
            // Check for cancellation before starting the HTTP request
            cancellationToken.ThrowIfCancellationRequested();

            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.GetFromJsonAsync<RandomResponse>(_apiUrl, cancellationToken);

                // Check for cancellation again, especially if there's a noticeable delay or before processing a subsequent step
                cancellationToken.ThrowIfCancellationRequested();

                if (response == null || response.RandomNumber < 0)
                {
                    throw new Exception("Invalid response from the random number service.");
                }

                return (RPSLSEnum)(response.RandomNumber % 5 + 1);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching random choice from external service.Request URL: {ApiUrl}.", _apiUrl);
                throw new Exception("Could not fetch random choice from external service.", ex);
            }
        }
    }
}
