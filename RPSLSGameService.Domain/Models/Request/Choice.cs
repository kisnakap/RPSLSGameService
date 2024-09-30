using RPSLSGameService.Utilities;
using System;

namespace RPSLSGameService.Domain.Models.Request
{
    public class Choice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Choice() { }

        public Choice(int value) : this((RPSLSEnum)value) { }
        public Choice(RPSLSEnum value)
        {
            this.Id = (int)value;
            this.Name = Enum.GetName(typeof(RPSLSEnum), value);
        }
    }
}
