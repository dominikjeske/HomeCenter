using System.ComponentModel;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AttachedPropertyDTO : BaseDTO
    {
        [DefaultValue("Service")]
        public string Service { get; set; }

        public string AttachedActor { get; set; }

        public string AttachedArea { get; set; }
    }
}