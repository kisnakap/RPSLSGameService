using System;
using System.Collections.Generic;
using System.Linq;

namespace RPSLSGameService.Utilities
{
    public static class GameLogicUtils
    {
        public static string DetermineWinner(RPSLSEnum player, RPSLSEnum opponent)
        {
            if (player == opponent) return "tie";

            var wins = new Dictionary<RPSLSEnum, RPSLSEnum[]>
            {
                { RPSLSEnum.Rock, new[] { RPSLSEnum.Scissors, RPSLSEnum.Lizard } },
                { RPSLSEnum.Paper, new[] { RPSLSEnum.Rock, RPSLSEnum.Spock } },
                { RPSLSEnum.Scissors, new[] { RPSLSEnum.Paper, RPSLSEnum.Lizard } },
                { RPSLSEnum.Lizard, new[] { RPSLSEnum.Spock, RPSLSEnum.Paper } },
                { RPSLSEnum.Spock, new[] { RPSLSEnum.Scissors, RPSLSEnum.Rock } }
            };

            return wins[player].Contains(opponent) ? "win" : "lose";
        }
    }
}
