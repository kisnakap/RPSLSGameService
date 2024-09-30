using RPSLSGameService.Utilities;
using System;

namespace RPSLSGameService.Domain.Models
{
    public class MatchResult
    {
        public Guid Id { get; set; } // Unique identifier for the match result
        public Guid SessionId { get; set; } // Foreign key to the GameSession
        public string WinnerName { get; set; } // Name of the winner
        public DateTime ResultDate { get; set; } = DateTime.UtcNow; // Date and time of the match
        public RPSLSEnum Player1Choice { get; set; } // Choice made by player 1
        public RPSLSEnum Player2Choice { get; set; } // Choice made by player 2
    }
}
