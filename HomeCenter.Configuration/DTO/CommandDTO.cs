using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class CommandDTO : ActorMessageDTO
    {
        public CommandDTO(string uid, string type, Dictionary<string, object> properties) : base(uid, type, properties)
        {
        }
    }
}