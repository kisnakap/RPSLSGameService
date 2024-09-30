using RPSLSGameService.Utilities;
using Xunit;

namespace RPSLSGameService.UnitTests.Utils
{
    public class GameLogicUtilsTests
    {
        [Theory]
        [InlineData(RPSLSEnum.Rock, RPSLSEnum.Scissors, "win")]
        [InlineData(RPSLSEnum.Rock, RPSLSEnum.Lizard, "win")]
        [InlineData(RPSLSEnum.Paper, RPSLSEnum.Rock, "win")]
        [InlineData(RPSLSEnum.Paper, RPSLSEnum.Spock, "win")]
        [InlineData(RPSLSEnum.Scissors, RPSLSEnum.Paper, "win")]
        [InlineData(RPSLSEnum.Scissors, RPSLSEnum.Lizard, "win")]
        [InlineData(RPSLSEnum.Lizard, RPSLSEnum.Paper, "win")]
        [InlineData(RPSLSEnum.Lizard, RPSLSEnum.Spock, "win")]
        [InlineData(RPSLSEnum.Spock, RPSLSEnum.Scissors, "win")]
        [InlineData(RPSLSEnum.Spock, RPSLSEnum.Rock, "win")]
        public void DetermineWinner_ShouldReturnWin_WhenPlayer1Wins(RPSLSEnum player1, RPSLSEnum player2, string expectedResult)
        {
            // Act
            var result = GameLogicUtils.DetermineWinner(player1, player2);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(RPSLSEnum.Scissors, RPSLSEnum.Rock, "lose")]
        [InlineData(RPSLSEnum.Lizard, RPSLSEnum.Rock, "lose")]
        [InlineData(RPSLSEnum.Rock, RPSLSEnum.Paper, "lose")]
        [InlineData(RPSLSEnum.Spock, RPSLSEnum.Paper, "lose")]
        [InlineData(RPSLSEnum.Paper, RPSLSEnum.Scissors, "lose")]
        [InlineData(RPSLSEnum.Lizard, RPSLSEnum.Scissors, "lose")]
        [InlineData(RPSLSEnum.Paper, RPSLSEnum.Lizard, "lose")]
        [InlineData(RPSLSEnum.Spock, RPSLSEnum.Lizard, "lose")]
        [InlineData(RPSLSEnum.Scissors, RPSLSEnum.Spock, "lose")]
        [InlineData(RPSLSEnum.Rock, RPSLSEnum.Spock, "lose")]
        public void DetermineWinner_ShouldReturnLose_WhenPlayer1Loses(RPSLSEnum player1, RPSLSEnum player2, string expectedResult)
        {
            // Act
            var result = GameLogicUtils.DetermineWinner(player1, player2);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(RPSLSEnum.Rock, RPSLSEnum.Rock)]
        [InlineData(RPSLSEnum.Paper, RPSLSEnum.Paper)]
        [InlineData(RPSLSEnum.Scissors, RPSLSEnum.Scissors)]
        [InlineData(RPSLSEnum.Lizard, RPSLSEnum.Lizard)]
        [InlineData(RPSLSEnum.Spock, RPSLSEnum.Spock)]
        public void DetermineWinner_ShouldReturnTie_WhenBothChoicesAreEqual(RPSLSEnum player1, RPSLSEnum player2)
        {
            // Act
            var result = GameLogicUtils.DetermineWinner(player1, player2);

            // Assert
            Assert.Equal("tie", result);
        }
    }
}
