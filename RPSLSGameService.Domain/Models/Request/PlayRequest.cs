using RPSLSGameService.Utilities;

namespace RPSLSGameService.Domain.Models.Request
{
    public class PlayRequest
    {
        public string Name { get; set; }
        public RPSLSEnum Choice { get; set; }
    }
}
