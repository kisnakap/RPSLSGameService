using RPSLSGameService.Services;
using RPSLSGameService.Utilities;
using Xunit;

namespace RPSLSGameService.UnitTests.Services
{
    public class ChoiceValidatorTests
    {
        private readonly TestLogger<ChoiceValidator> _testLogger;
        private readonly ChoiceValidator _validator;

        public ChoiceValidatorTests()
        {
            _testLogger = new TestLogger<ChoiceValidator>(); // Initialize custom logger
            _validator = new ChoiceValidator(_testLogger);
        }

        [Theory]
        [InlineData(RPSLSEnum.Rock)]
        [InlineData(RPSLSEnum.Paper)]
        [InlineData(RPSLSEnum.Scissors)]
        [InlineData(RPSLSEnum.Lizard)]
        [InlineData(RPSLSEnum.Spock)]
        public void Validate_ShouldReturnTrue_ForValidEnumValues(RPSLSEnum validChoice)
        {
            // Arrange
            string error;

            // Act
            var result = _validator.Validate(validChoice, out error);

            // Assert
            Assert.True(result);
            Assert.Equal(string.Empty, error);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_ForInvalidEnumValue()
        {
            // Arrange
            string error;
            var invalidChoice = (RPSLSEnum)99; // An invalid enum value

            // Act
            var result = _validator.Validate(invalidChoice, out error);

            // Assert
            Assert.False(result); // Validate that the result is false
            Assert.NotEqual(string.Empty, error); // Ensure error message is not empty
            Assert.Contains("Invalid choice.", error); // Specifically check that the message is present

            // Check that the exact log message has been recorded
            string expectedLogMessage = "Validation failed: Invalid choice. Must be one of the following options: Rock, Paper, Scissors, Lizard, Spock.";
            Assert.Contains(expectedLogMessage, _testLogger.LogMessages);
        }

        [Fact]
        public void Validate_ShouldReturnFalse_ForZeroValue()
        {
            // Arrange
            string error;

            // Act
            var result = _validator.Validate((RPSLSEnum)0, out error); // Invalid enum value

            // Assert
            Assert.False(result);
            Assert.NotEqual(string.Empty, error);
            Assert.Contains("Invalid choice.", error);

            // Check that the exact log message has been recorded
            string expectedLogMessage = "Validation failed: Invalid choice. Must be one of the following options: Rock, Paper, Scissors, Lizard, Spock.";
            Assert.Contains(expectedLogMessage, _testLogger.LogMessages);
        }
    }
}
