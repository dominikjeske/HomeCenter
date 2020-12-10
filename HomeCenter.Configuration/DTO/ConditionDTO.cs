using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ConditionDTO : BaseDTO
    {
        public ConditionDTO(string uid, string type, Dictionary<string, object> properties) : base(uid, type, properties)
        {
        }
    }
}