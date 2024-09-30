using RPSLSGameService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPSLSGameService.Domain.Models
{
    public class GameSession
    {
        public Guid SessionId { get; set; } // Unique identifier for the game session
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp of when the session was created
        public List<Player> Players { get; set; } = new List<Player>();
        public GameState CurrentState { get; private set; } = GameState.WaitingForPlayers;

        public List<MatchResult> MatchResults { get; set; } = new List<MatchResult>(); // List of match results

        // Adds a player to the session, checking game rules and state
        public Player AddPlayer(Player player)
        {
            if (CurrentState != GameState.WaitingForPlayers)
            {
                throw new InvalidOperationException("Cannot add players after the game has started.");
            }

            if (Players.Count < 2 && !Players.Any(p => p.Name == player.Name))
            {
                Players.Add(player);
                if (Players.Count == 2)
                {
                    TransitionTo(GameState.ChoicesSubmitted); // Transition to ChoicesSubmitted when both players are present
                }
            }
            else
            {
                throw new InvalidOperationException("Session already has two players or the player is already in the session.");
            }

            return player;
        }

        // Store the player's choice and transition the state if both players have made their choices
        public Player SubmitChoice(string playerName, RPSLSEnum choice)
        {
            if (CurrentState != GameState.ChoicesSubmitted)
            {
                throw new InvalidOperationException("Invalid state for submitting choices.");
            }

            var player = Players.SingleOrDefault(p => p.Name == playerName);
            if (player == null)
            {
                throw new InvalidOperationException("Player not found in the session.");
            }

            player.Choice = choice; // Assign the choice to the player

            // Check if both players have made their choices
            if (Players.All(p => p.Choice.HasValue))
            {
                TransitionTo(GameState.GameInProgress); // Move to GameInProgress if both players made their choices
            }

            return player;
        }

        // Finalizes the game and determines the results
        public MatchResult FinalizeGame()
        {
            if (CurrentState != GameState.GameInProgress)
            {
                throw new InvalidOperationException("Game must be in progress to finalize.");
            }

            // Create match result and determine winner
            var matchResult = new MatchResult
            {
                Id = Guid.NewGuid(),
                SessionId = SessionId,
                WinnerName = DetermineWinner(), // Method to determine the winner based on player choices
                ResultDate = DateTime.UtcNow,
                Player1Choice = Players[0].Choice.Value,
                Player2Choice = Players[1].Choice.Value
            };

            MatchResults.Add(matchResult); // Store the result of the match
            TransitionTo(GameState.GameResults); // Transition to GameResults after finalizing

            return matchResult;
        }

        // Transition state method
        private void TransitionTo(GameState newState)
        {
            CurrentState = newState; // Change the state
        }

        // Sample winner determination logic
        private string DetermineWinner()
        {
            var playerChoices = Players.ToDictionary(p => p.Name, p => p.Choice.Value);
            if (playerChoices.Values.ElementAt(0) == playerChoices.Values.ElementAt(1)) return "None";
            else return GameLogicUtils.DetermineWinner(playerChoices.Values.ElementAt(0), playerChoices.Values.ElementAt(1)) == "win" ? playerChoices.Keys.ElementAt(0) : playerChoices.Keys.ElementAt(1);
        }
    }
}
