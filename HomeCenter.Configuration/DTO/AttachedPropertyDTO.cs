using System.Collections.Generic;
using System.ComponentModel;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AttachedPropertyDTO : BaseDTO
    {
        public AttachedPropertyDTO(string uid, string type, Dictionary<string, object> properties, string service, string attachedActor, string attachedArea) : base(uid, type, properties)
        {
            Service = service;
            AttachedActor = attachedActor;
            AttachedArea = attachedArea;
        }

        [DefaultValue("Service")]
        public string Service { get; set; }

        public string AttachedActor { get; set; }

        public string AttachedArea { get; set; }
    }
}