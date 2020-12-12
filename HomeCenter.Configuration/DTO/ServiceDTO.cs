using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ServiceDTO : DeviceActorDTO
    {
        public ServiceDTO(string uid, string type, IDictionary<string, object> properties, IDictionary<string, string> tags, bool isEnabled, IEnumerable<AttachedPropertyDTO> componentsAttachedProperties, IEnumerable<AttachedPropertyDTO> areasAttachedProperties) : base(uid, type, properties, tags, isEnabled)
        {
            ComponentsAttachedProperties.AddRange(componentsAttachedProperties);
            AreasAttachedProperties.AddRange(areasAttachedProperties);
        }

        public List<AttachedPropertyDTO> ComponentsAttachedProperties { get; set; } = new List<AttachedPropertyDTO>();

        public List<AttachedPropertyDTO> AreasAttachedProperties { get; set; } = new List<AttachedPropertyDTO>();
    }
}