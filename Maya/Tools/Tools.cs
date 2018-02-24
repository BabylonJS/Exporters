using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Maya2Babylon
{
    static class Tools
    {
        public const float Epsilon = 0.001f;


        // -------------------------
        // --------- Array ---------
        // -------------------------

        public static T[] SubArray<T>(T[] array, int startIndex, int count)
        {
            var result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = array[startIndex + i];
            }
            return result;
        }

        public static T[] SubArrayFromEntity<T>(T[] array, int startEntityIndex, int count)
        {
            return SubArray(array, startEntityIndex * count, count);
        }

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

        // -------------------------
        // ----------- UI ----------
        // -------------------------

        public static void UpdateCheckBox(CheckBox checkBox, MPxNode node, string propertyName)
        {
            if (checkBox.CheckState != CheckState.Indeterminate)
            {
                // TODO find function
                //node.setUserPropBool(propertyName, checkBox.CheckState == CheckState.Checked);
            }
        }

        // -------------------------
        // ----------- Math ----------
        // -------------------------

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
