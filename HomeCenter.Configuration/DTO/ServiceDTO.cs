using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ServiceDTO : DeviceActorDTO
    {
        public List<AttachedPropertyDTO> ComponentsAttachedProperties { get; set; } = new List<AttachedPropertyDTO>();

        public List<AttachedPropertyDTO> AreasAttachedProperties { get; set; } = new List<AttachedPropertyDTO>();
    }
}