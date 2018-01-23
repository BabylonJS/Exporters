using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Maya2Babylon
{
    static class Tools
    {
        public const float Epsilon = 0.001f;

        public static bool IsAlmostEqualTo(this float[] current, float[] other, float epsilon)
        {
            if (current.Length != other.Length)
            {
                return false;
            }

            for (int index = 0; index < current.Length; index++)
            {
                if (Math.Abs(current[index] - other[index]) > epsilon)
                {
                    return false;
                }
            }
            
            return true;
        }

        public static string toString<T>(this T[] array)
        {
            if (array == null)
            {
                return "";
            }

            var result = "[";
            bool isFirst = true;
            for (uint index = 0; index < array.Length; index++)
            {
                if (!isFirst)
                {
                    result += ",";
                }
                isFirst = false;
                result += array[index];
            }
            return result + "]";
        }

        public static void UpdateCheckBox(CheckBox checkBox, MPxNode node, string propertyName)
        {
            if (checkBox.CheckState != CheckState.Indeterminate)
            {
                // TODO find function
                //node.setUserPropBool(propertyName, checkBox.CheckState == CheckState.Checked);
            }
        }
    }
}
