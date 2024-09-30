using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using RPSLSGameServiceAPI;
using RPSLSGameService.Domain.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using RPSLSGameService.Utilities;
using RPSLSGameService.IntegrationTests.Utilities;
using Newtonsoft.Json.Linq;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Domain.Models.Request;

namespace RPSLSGameService.IntegrationTests
{
    public class GameControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public GameControllerIntegrationTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task CreateSession_Returns_CreatedSessionId()
        {
            // Act
            var response = await _client.PostAsync("/Game/createSession", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadAsStringAsync();
            var sessionInfo = JsonConvert.DeserializeObject<CreateSessionResponse>(responseBody);
            Assert.NotEqual(Guid.Empty, (Guid)sessionInfo.SessionId);
        }

        [Fact]
        public async Task GetChoices_Returns_AllChoices()
        {
            // Act
            var response = await _client.GetAsync("/Game/choices");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadAsStringAsync();
            var choices = JsonConvert.DeserializeObject<Choice[]>(responseBody);

            choices.Should().NotBeEmpty();
            choices.Should().HaveCount(5); // Assuming 5 choices: Rock, Paper, Scissors, Lizard, Spock
        }

        [Fact]
        public async Task GetRandomChoice_Returns_ValidChoice()
        {
            // Act
            var response = await _client.GetAsync("/Game/choice");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Deserialize into a JObject
            var choice = JObject.Parse(responseBody);

            // Access the properties directly
            int id = choice["id"].Value<int>();
            string name = choice["name"].Value<string>();

            // Perform assertions
            id.Should().BeGreaterOrEqualTo(1).And.BeLessOrEqualTo(5);
            name.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task PlayRound_Returns_ResultForValidChoice()
        {
            // Arrange
            var playRequest = new PlayRequest { Name = "Player1", Choice = RPSLSEnum.Rock };
            var playContent = new StringContent(JsonConvert.SerializeObject(playRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/Game/play", playContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Deserialize using PlayResult
            var playResponse = JsonConvert.DeserializeObject<PlayResult>(responseBody);

            Assert.NotNull(playResponse);
            Assert.NotNull(playResponse.Result);
            playResponse.Result.Should().Be("tie");
            playResponse.PlayerChoice.Should().Be(RPSLSEnum.Rock);
            playResponse.ComputerChoice.Should().Be(RPSLSEnum.Rock);
}
        [Fact]
        public async Task PlayMultiplayer_ReturnsValidResponse()
        {
            // Arrange
            // Step 1: Create a game session
            var createSessionResponse = await _client.PostAsync("/Game/createSession", null);
            createSessionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var sessionBody = await createSessionResponse.Content.ReadAsStringAsync();
            dynamic sessionInfo = JsonConvert.DeserializeObject(sessionBody);
            var sessionId = sessionInfo.sessionId;

            // Step 2: Add Player1 with no choice
            var player1Request = new MultiPlayerRequest { Name = "Player1", Choice = null };
            var player1Content = new StringContent(JsonConvert.SerializeObject(player1Request), Encoding.UTF8, "application/json");
            var player1Response = await _client.PostAsync($"/Game/playMulti?sessionId={sessionId}", player1Content);
            player1Response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 3: Add Player2 with no choice
            var player2Request = new MultiPlayerRequest { Name = "Player2", Choice = null };
            var player2Content = new StringContent(JsonConvert.SerializeObject(player2Request), Encoding.UTF8, "application/json");
            var player2Response = await _client.PostAsync($"/Game/playMulti?sessionId={sessionId}", player2Content);
            player2Response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 4: Player1 makes a choice
            player1Request = new MultiPlayerRequest { Name = "Player1", Choice = RPSLSEnum.Rock };
            player1Content = new StringContent(JsonConvert.SerializeObject(player1Request), Encoding.UTF8, "application/json");
            player1Response = await _client.PostAsync($"/Game/playMulti?sessionId={sessionId}", player1Content);
            player1Response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 5: Player2 makes a choice
            player2Request = new MultiPlayerRequest { Name = "Player2", Choice = RPSLSEnum.Paper };
            player2Content = new StringContent(JsonConvert.SerializeObject(player2Request), Encoding.UTF8, "application/json");
            player2Response = await _client.PostAsync($"/Game/playMulti?sessionId={sessionId}", player2Content);
            player2Response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert
            var responseBody = await player2Response.Content.ReadAsStringAsync();

            // Deserialize using MultiPlayResult
            var playResponse = JsonConvert.DeserializeObject<MultiplayerResult>(responseBody);

            Assert.NotNull(playResponse);
            Assert.NotNull(playResponse.Result);
            playResponse.Result.Should().Be("win");
            playResponse.WinnerName.Should().Be("Player2");
        }


        [Fact]
        public async Task GetScoreboard_Returns_RecentResults()
        {
            // Act
            var response = await _client.GetAsync("/Game/scoreboard");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadAsStringAsync();
            var matchResults = JsonConvert.DeserializeObject<MatchResult[]>(responseBody);

            matchResults.Should().NotBeNull();
        }

        [Fact]
        public async Task ResetScoreboard_Clears_AllResults()
        {
            // Act
            var response = await _client.PostAsync("/Game/resetScoreboard", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify if the scoreboard is clear
            var scoreboardResponse = await _client.GetAsync("/Game/scoreboard");
            var scoreboardBody = await scoreboardResponse.Content.ReadAsStringAsync();
            var matchResults = JsonConvert.DeserializeObject<MatchResult[]>(scoreboardBody);

            matchResults.Should().BeEmpty();
        }
    }
}
