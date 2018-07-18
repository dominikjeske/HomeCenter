using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public static class DenonSurroundModes
    {
        private static Dictionary<string, string> _surroundApiMap;
        private static Dictionary<string, string> _surroundResultMap;

        public static string MapApiCommand(string surroundMode)
        {
            return _surroundApiMap[surroundMode];
        }

        public static string GetCommandResult(string surroundMode)
        {
            return _surroundResultMap[surroundMode];
        }

        static DenonSurroundModes()
        {
            _surroundApiMap = new Dictionary<string, string>()
            {
                {"Movie", "MSMOVIE"},
                {"Music", "MSMUSIC"},
                {"Game", "MSGAME"},
                {"Direct", "MSDIRECT"},
                {"Stereo", "MSSTEREO"},
                {"Pure direct", "MSPURE%20DIRECT"},
                {"Standard", "MSSTANDARD"},
                {"DOLBY Surrounds", "MSDOLBY%20DIGITAL"},
                {"DTS Surrounds", "MSDTS%20SURROUND"},
                {"Multichannel Stereo", "MSMCH%20STEREO"},
                {"Rock Arena", "MSROCK%20ARENA"},
                {"Jazz Club", "MSJAZZ%20CLUB"},
                {"Mono Movie", "MSMONO%20MOVIE"},
                {"VideoGame", "MSVIDEO%20GAME"},
                {"Matrix", "MSMATRIX"},
                {"Virtual", "MSVIRTUAL"},
                {"Auto", "MSAUTO"}
            };

            _surroundResultMap = new Dictionary<string, string>()
            {
                {"Movie", "PLII Cinema"},
                {"Music", "MULTI CH STEREO"},
                {"Game", "PLII Game"},
                {"Direct", "DIRECT"},
                {"Stereo", "STEREO"},
                {"Pure direct", "PURE DIRECT"},
                {"Standard", "PLII Game"},
                {"DOLBY Surrounds", "PLII Game"},
                {"DTS Surrounds", "DTS NEO:6 cinema"},
                {"Multichannel Stereo", "MULTI CH STEREO"},
                {"Rock Arena", "ROCK ARENA"},
                {"Jazz Club", "JAZZ CLUB"},
                {"Mono Movie", "MONO MOVIE"},
                {"VideoGame", "VIDEO GAME"},
                {"Matrix", "MATRIX"},
                {"Virtual", "VIRTUAL"},
                {"Auto", "VIRTUAL"}
            };
        }
    }
}