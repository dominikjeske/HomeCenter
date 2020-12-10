using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ActorMessageDTO : BaseDTO
    {
        public ActorMessageDTO(string uid, string type, Dictionary<string, object> properties) : base(uid, type, properties)
        {
        }
    }
}