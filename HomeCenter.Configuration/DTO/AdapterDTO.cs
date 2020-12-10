using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AdapterDTO : DeviceActorDTO
    {
        public AdapterDTO(string uid, string type, Dictionary<string, object> properties, IDictionary<string, string> tags, bool isEnabled) : base(uid, type, properties, tags, isEnabled)
        {
        }
    }
}