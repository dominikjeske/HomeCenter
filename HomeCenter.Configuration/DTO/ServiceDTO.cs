using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ServiceDTO : DeviceActorDTO
    {
        public ServiceDTO(string uid, string type, Dictionary<string, object> properties, IDictionary<string, string> tags, bool isEnabled, List<AttachedPropertyDTO> componentsAttachedProperties, List<AttachedPropertyDTO> areasAttachedProperties) : base(uid, type, properties, tags, isEnabled)
        {
            ComponentsAttachedProperties = componentsAttachedProperties;
            AreasAttachedProperties = areasAttachedProperties;
        }

        public List<AttachedPropertyDTO> ComponentsAttachedProperties { get; set; } = new List<AttachedPropertyDTO>();

        public List<AttachedPropertyDTO> AreasAttachedProperties { get; set; } = new List<AttachedPropertyDTO>();
    }
}