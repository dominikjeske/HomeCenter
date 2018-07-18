using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Core
{
    //https://stackoverflow.com/questions/6590939/how-to-set-display-settings-to-extend-mode-in-windows-7-using-c
    //https://stackoverflow.com/questions/16790287/programmatically-changing-the-presentation-display-mode
    public class DisplayService : IDisplayService
    {
        public void SetDisplayMode(DisplayMode mode)
        {
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "DisplaySwitch.exe";
            switch (mode)
            {
                case DisplayMode.External:
                    proc.StartInfo.Arguments = "/external";
                    break;
                case DisplayMode.Internal:
                    proc.StartInfo.Arguments = "/internal";
                    break;
                case DisplayMode.Extend:
                    proc.StartInfo.Arguments = "/extend";
                    break;
                case DisplayMode.Duplicate:
                    proc.StartInfo.Arguments = "/clone";
                    break;
            }
            proc.Start();
        }

        public IEnumerable<IDisplay> GetActiveMonitors()
        {
            var pathWraps = QueryDisplay(QueryDisplayFlags.OnlyActivePaths, out DisplayConfigTopologyId topologyId);

            return pathWraps.Select(CreateDisplay);
        }

        private IEnumerable<DisplayConfigPathWrap> QueryDisplay(QueryDisplayFlags pathType, out DisplayConfigTopologyId topologyId)
        {
            topologyId = DisplayConfigTopologyId.Zero;

            var status = Win32Api.GetDisplayConfigBufferSizes(
                pathType,
                out int numPathArrayElements,
                out int numModeInfoArrayElements);

            if (status != StatusCode.Success)
            {
                var reason = string.Format("GetDisplayConfigBufferSizesFailed() failed. Status: {0}", status);
                throw new Exception(reason);
            }

            var pathInfoArray = new DisplayConfigPathInfo[numPathArrayElements];
            var modeInfoArray = new DisplayConfigModeInfo[numModeInfoArrayElements];

            var queryDisplayStatus = pathType == QueryDisplayFlags.DatabaseCurrent ?
                Win32Api.QueryDisplayConfig(pathType, ref numPathArrayElements, pathInfoArray, ref numModeInfoArrayElements, modeInfoArray, out topologyId) :
                Win32Api.QueryDisplayConfig(pathType, ref numPathArrayElements, pathInfoArray, ref numModeInfoArrayElements, modeInfoArray);

            if (queryDisplayStatus != StatusCode.Success)
            {
                var reason = string.Format("QueryDisplayConfig() failed. Status: {0}", queryDisplayStatus);
                throw new Exception(reason);
            }

            var list = new List<DisplayConfigPathWrap>();
            foreach (var path in pathInfoArray)
            {
                var outputModes = new List<DisplayConfigModeInfo>();
                foreach (var modeIndex in new[]
                                          {
                                              path.sourceInfo.modeInfoIdx,
                                              path.targetInfo.modeInfoIdx
                                          })
                {
                    if (modeIndex >= 0 && modeIndex < modeInfoArray.Length)
                        outputModes.Add(modeInfoArray[modeIndex]);
                }

                list.Add(new DisplayConfigPathWrap(path, outputModes));
            }
            return list;
        }

        private IDisplay CreateDisplay(DisplayConfigPathWrap pathWrap)
        {
            var path = pathWrap.Path;
            var sourceModeInfo = pathWrap.Modes.First(x => x.infoType == DisplayConfigModeInfoType.Source);
            var origin = new Point
            {
                X = sourceModeInfo.sourceMode.position.x,
                Y = sourceModeInfo.sourceMode.position.y
            };

            var resolution = new Size
            {
                Width = sourceModeInfo.sourceMode.width,
                Height = sourceModeInfo.sourceMode.height
            };

            var refreshRate = (int)Math.Round((double)path.targetInfo.refreshRate.numerator / path.targetInfo.refreshRate.denominator);
            var rotationOriginal = path.targetInfo.rotation;
            var isPrimary = IsPrimaryDisplay(origin);
            
            var displayName = "<unidentified>"; 
            var nameStatus = GetDisplayConfigSourceDeviceName(sourceModeInfo, out DisplayConfigSourceDeviceName displayConfigSourceDeviceName);
            if (nameStatus == StatusCode.Success)
                displayName = displayConfigSourceDeviceName.viewGdiDeviceName;

            return new Display(new DisplaySettings(resolution, origin, rotationOriginal.ToScreenRotation(), refreshRate, isPrimary, displayName));
        }

        private bool IsPrimaryDisplay(Point displayStart)
        {
            return displayStart.X == 0 && displayStart.Y == 0;
        }

        private StatusCode GetDisplayConfigSourceDeviceName(DisplayConfigModeInfo sourceModeInfo, out DisplayConfigSourceDeviceName displayConfigSourceDeviceName)
        {
            displayConfigSourceDeviceName = new DisplayConfigSourceDeviceName
            {
                header = new DisplayConfigDeviceInfoHeader
                {
                    adapterId = sourceModeInfo.adapterId,
                    id = sourceModeInfo.id,
                    size = Marshal.SizeOf(typeof(DisplayConfigSourceDeviceName)),
                    type = DisplayConfigDeviceInfoType.GetSourceName,
                }
            };

            return Win32Api.DisplayConfigGetDeviceInfo(ref displayConfigSourceDeviceName);
        }

        private static StatusCode GetDisplayConfigTargetDeviceName(DisplayConfigModeInfo targetModeInfo, out DisplayConfigTargetDeviceName displayConfigTargetDeviceName)
        {
            displayConfigTargetDeviceName = new DisplayConfigTargetDeviceName
            {
                header = new DisplayConfigDeviceInfoHeader
                {
                    adapterId = targetModeInfo.adapterId,
                    id = targetModeInfo.id,
                    size = Marshal.SizeOf(typeof(DisplayConfigTargetDeviceName)),
                    type = DisplayConfigDeviceInfoType.GetTargetName,
                }
            };

            return Win32Api.DisplayConfigGetDeviceInfo(ref displayConfigTargetDeviceName);
        }
    }
}
