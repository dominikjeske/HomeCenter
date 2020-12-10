using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class EventDTO : ActorMessageDTO
    {
        public EventDTO(string uid, string type, Dictionary<string, object> properties) : base(uid, type, properties)
        {
        }
    }
}