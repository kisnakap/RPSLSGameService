using RPSLSGameService.Utilities;

namespace RPSLSGameService.Domain.Models.Request
{
    public class MultiPlayerRequest
    {
        public string Name { get; set; }
        public RPSLSEnum? Choice { get; set; }
    }
}
