using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maya2Babylon
{
    class Loader
    {
        static MGlobal global;


        public static MGlobal Global
        {
            get
            {
                return global;
            }
        }


        // TODO use an int or a float instead of MInArray
        public static int GetMinTime()
        {
            MGlobal.executeCommand("playbackOptions -q -animationStartTime", out double minTime);
            return (int)minTime;
        }

        public static int GetMaxTime()
        {
            MGlobal.executeCommand("playbackOptions -q -animationEndTime", out double maxTime);
            return (int)maxTime;
        }

        public static int GetFPS()
        {
            MGlobal.executeCommand("currentTimeUnitToFPS", out double framePerSecond);
            return (int)framePerSecond;
        }

        public string GetStringArrayProperty(string property)
        {
            MGlobal.executeCommand("fileInfo -q", out string result);
            return result;
        }
    }
}
