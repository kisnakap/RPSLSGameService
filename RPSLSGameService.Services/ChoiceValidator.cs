using Microsoft.Extensions.Logging;
using RPSLSGameService.Domain.Interfaces;
using RPSLSGameService.Utilities;
using System;

namespace RPSLSGameService.Services
{
    public class ChoiceValidator : IValidator<RPSLSEnum>
    {
        private readonly ILogger<ChoiceValidator> _logger;

        public ChoiceValidator(ILogger<ChoiceValidator> logger)
        {
            _logger = logger;
        }

        public bool Validate(RPSLSEnum choice, out string error)
        {

            // Check if the choice is a valid enum value
            if (!Enum.IsDefined(typeof(RPSLSEnum), choice))
            {
                error = "Invalid choice. Must be one of the following options: Rock, Paper, Scissors, Lizard, Spock.";
                _logger.LogWarning("Validation failed: {Error}", error);
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
