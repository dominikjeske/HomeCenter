using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AdapterReferenceDTO : BaseDTO
    {
        public AdapterReferenceDTO(string uid, string type, Dictionary<string, object> properties, bool isMainAdapter) : base(uid, type, properties)
        {
            IsMainAdapter = isMainAdapter;
        }

        public bool IsMainAdapter { get; set; }
    }
}