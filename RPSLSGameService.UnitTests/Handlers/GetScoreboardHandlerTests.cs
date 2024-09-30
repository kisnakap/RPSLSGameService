using Microsoft.AspNetCore.Mvc;
using Moq;
using RPSLSGameService.Application.Handlers;
using RPSLSGameService.Application.RPSLSQueries.Requests;
using RPSLSGameService.Domain.Models;
using RPSLSGameService.Domain.Models.Response;
using RPSLSGameService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RPSLSGameService.UnitTests.Handlers
{
    public class GetScoreboardHandlerTests
    {
        private readonly Mock<IMatchResultRepository> _mockRepository;
        private readonly TestLogger<GetScoreboardHandler> _testLogger; // Use the custom logger
        private readonly GetScoreboardHandler _handler;

        public GetScoreboardHandlerTests()
        {
            _mockRepository = new Mock<IMatchResultRepository>();
            _testLogger = new TestLogger<GetScoreboardHandler>();
            _handler = new GetScoreboardHandler(_mockRepository.Object, _testLogger);
        }

        [Fact]
        public async Task Handle_ShouldReturnScoreboard_WhenResultsExist()
        {
            // Arrange
            var mockResults = new List<MatchResult>
            {
                new MatchResult { Id = Guid.NewGuid(), WinnerName = "Player1", SessionId = Guid.NewGuid() },
                new MatchResult { Id = Guid.NewGuid(), WinnerName = "Player2", SessionId = Guid.NewGuid() }
            };

            _mockRepository.Setup(repo => repo.GetRecentResultsAsync(10, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(mockResults);

            var query = new GetScoreboardQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResults = Assert.IsType<List<MatchResult>>(okResult.Value);
            Assert.Equal(mockResults.Count, returnedResults.Count);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoResultsExists()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetRecentResultsAsync(10, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new List<MatchResult>());

            var query = new GetScoreboardQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResults = Assert.IsType<List<MatchResult>>(okResult.Value);
            Assert.Empty(returnedResults);
        }

        [Fact]
        public async Task Handle_ShouldReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetRecentResultsAsync(10, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new System.Exception("Database error.")); // Simulate an error

            var query = new GetScoreboardQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);

            Assert.Equal("Database error.", errorResponse.Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnRecentResults_LimitedToCount()
        {
            // Arrange
            var mockResults = new List<MatchResult>
            {
                new MatchResult { Id = Guid.NewGuid(), WinnerName = "Player1", SessionId = Guid.NewGuid() },
                new MatchResult { Id = Guid.NewGuid(), WinnerName = "Player2", SessionId = Guid.NewGuid() },
                new MatchResult { Id = Guid.NewGuid(), WinnerName = "Player3", SessionId = Guid.NewGuid() }
            };

            // Set up to return only the first two results
            _mockRepository.Setup(repo => repo.GetRecentResultsAsync(10, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(mockResults.Take(2).ToList()); // return only first 2
            var query = new GetScoreboardQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResults = Assert.IsType<List<MatchResult>>(okResult.Value);
            Assert.Equal(2, returnedResults.Count); // Ensure it only returns 2
        }

        [Fact]
        public async Task Handle_ShouldHandleCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token before the operation

            // Act & Assert
            var result = await _handler.Handle(new GetScoreboardQuery(), cts.Token);

            // Check if it returns BadRequest object with cancellation message
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("Operation was canceled.", errorResponse.Message);
        }
    }
}
