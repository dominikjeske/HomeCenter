using System;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    public struct PropertyKey
    {
        public Guid formatId;
        public int propertyId;

        public PropertyKey(Guid formatId, int propertyId)
        {
            this.formatId = formatId;
            this.propertyId = propertyId;
        }
    }
}