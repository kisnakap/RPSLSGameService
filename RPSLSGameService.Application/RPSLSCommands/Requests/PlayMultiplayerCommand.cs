using RPSLSGameService.Domain.Models.Request;
using System;

namespace RPSLSGameService.Application.RPSLSCommands.Requests
{
    public class PlayMultiplayerCommand
    {
        public Guid SessionId { get; set; }
        public MultiPlayerRequest Request { get; set; }
    }
}
