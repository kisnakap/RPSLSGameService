using RPSLSGameService.Utilities;

namespace RPSLSGameService.Domain.Models.Response
{
    public class PlayResult
    {
        public string Result { get; set; }
        public RPSLSEnum PlayerChoice { get; set; }
        public RPSLSEnum ComputerChoice { get; set; }
    }
}
