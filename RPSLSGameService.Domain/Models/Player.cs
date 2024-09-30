using RPSLSGameService.Utilities;
using System;

namespace RPSLSGameService.Domain.Models
{
    public class Player
    {
        public string Name { get; set; }
        public RPSLSEnum? Choice { get; set; }
        public Guid GameSessionId { get; set; }
    }
}
