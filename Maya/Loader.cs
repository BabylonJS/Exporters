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




        /// <summary>
        /// Mthodes to load/save data from/in the maya file
        /// </summary>
        /// 
        private static char separator = ';';
        public static string[] GetStringArrayProperty(string property)
        {
            string value = "";
            string[] values = { };

            if(GetUserPropString(property, ref value))
            {
                values = value.Split(separator);
            }

            return values;
        }

        public static void SetStringArrayProperty(string property, List<string> values)
        {
            string value = string.Join(separator.ToString(), values);

            SetStringProperty(property, value);
        }

        internal static bool GetUserPropString(string property, ref string value)
        {
            MCommandResult result = new MCommandResult();
            MGlobal.executeCommand($"fileInfo -q \"{property}\"", result);
            if (result.resultType == MCommandResult.Type.kStringArray)
            {
                MStringArray stringArray = new MStringArray();
                result.getResult(stringArray);
                value = string.Join("", stringArray.ToArray());
            }
            else
            {
                value = null;
            }

            return !string.IsNullOrEmpty(value);
        }

        internal static void SetStringProperty(string property, string value)
        {
            MGlobal.executeCommand($"fileInfo \"{property}\" \"{value}\"");
        }

        public static void DeleteProperty(string property)
        {
            MGlobal.executeCommand($"fileInfo -remove \"{property}\"");
        }

        internal static bool GetBoolProperty(string property)
        {
            bool value = false;
            MCommandResult result = new MCommandResult();
            MGlobal.executeCommand($"fileInfo -q \"{property}\"", result);
            if (result.resultType == MCommandResult.Type.kStringArray)
            {
                MStringArray stringArray = new MStringArray();
                result.getResult(stringArray);
                value = string.Join("", stringArray.ToArray()).Equals(true.ToString());
            }

            return value;
        }

        internal static void SetBoolProperty(string property, bool value)
        {
            SetStringProperty(property, value.ToString());
        }
    }
}
