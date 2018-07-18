using System.Drawing;

namespace Wirehome.WindowsService.Core
{
    public struct DisplaySettings
    {
        public string Name { get; private set; }

        public Size Resolution { get; private set; }

        public Point Origin { get; private set; }

        public DisplayRotation Rotation { get; private set; }

        public int RefreshRate { get; private set; }

        public bool IsPrimary { get; private set; }

        public DisplaySettings(Size resolution, Point origin, DisplayRotation rotation, int refreshRate, bool isPrimary, string name) : this()
        {
            Resolution = resolution;
            Origin = origin;
            Rotation = rotation;
            RefreshRate = refreshRate;
            IsPrimary = isPrimary;
            Name = name;
        }
    }
}
