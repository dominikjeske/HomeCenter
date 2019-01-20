using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ServiceDTO : BaseDTO
    {
        public List<AttachedPropertyDTO> ComponentsAttachedProperties = new List<AttachedPropertyDTO>();
        public List<AttachedPropertyDTO> AreasAttachedProperties = new List<AttachedPropertyDTO>();
    }
}