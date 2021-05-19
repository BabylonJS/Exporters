using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BabylonExport.Entities;

namespace Maya2Babylon
{
    static class Tools
    {
        public const float Epsilon = 0.00001f;

        // -------------------------
        // --------- Math ----------
        // -------------------------

        public static float Lerp(float min, float max, float t)
        {
            return min + (max - min) * t;
        }

        public static BabylonVector3.EulerRotationOrder ConvertMayaRotationOrder(MEulerRotation.RotationOrder mayaRotationOrder)
        {
            // http://download.autodesk.com/us/maya/2010help/api/class_m_transformation_matrix.html#adbf54177dae3a2015e51cd6bde8941e
            switch (mayaRotationOrder)
            {
                case MEulerRotation.RotationOrder.kXYZ:
                default:
                    return BabylonVector3.EulerRotationOrder.XYZ;
                case MEulerRotation.RotationOrder.kYZX:
                    return BabylonVector3.EulerRotationOrder.YZX;
                case MEulerRotation.RotationOrder.kZXY:
                    return BabylonVector3.EulerRotationOrder.ZXY;
                case MEulerRotation.RotationOrder.kXZY:
                    return BabylonVector3.EulerRotationOrder.XZY;
                case MEulerRotation.RotationOrder.kYXZ:
                    return BabylonVector3.EulerRotationOrder.YXZ;
                case MEulerRotation.RotationOrder.kZYX:
                    return BabylonVector3.EulerRotationOrder.ZYX;
            }
        }

        public static BabylonVector3.EulerRotationOrder InvertRotationOrder(BabylonVector3.EulerRotationOrder rotationOrder)
        {
            switch (rotationOrder)
            {
                case BabylonVector3.EulerRotationOrder.XYZ:
                default:
                    return BabylonVector3.EulerRotationOrder.ZYX;
                case BabylonVector3.EulerRotationOrder.YZX:
                    return BabylonVector3.EulerRotationOrder.XZY;
                case BabylonVector3.EulerRotationOrder.ZXY:
                    return BabylonVector3.EulerRotationOrder.YXZ;
                case BabylonVector3.EulerRotationOrder.XZY:
                    return BabylonVector3.EulerRotationOrder.YZX;
                case BabylonVector3.EulerRotationOrder.YXZ:
                    return BabylonVector3.EulerRotationOrder.ZXY;
                case BabylonVector3.EulerRotationOrder.ZYX:
                    return BabylonVector3.EulerRotationOrder.XYZ;
            }
        }

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

        public static int RoundToInt(float f)
        {
            return Convert.ToInt32(Math.Round(f, MidpointRounding.AwayFromZero));
        }

        public static T[] SubArrayFromEntity<T>(T[] array, int startEntityIndex, int count)
        {
            return SubArray(array, startEntityIndex * count, count);
        }

        /// <summary>
        /// Check for almost equality of two float[] arrays. Note this function return true if the both array are NULL.
        /// </summary>
        /// <param name="current"> the first array</param>
        /// <param name="other">the second array</param>
        /// <param name="epsilon">threshold</param>
        /// <returns>true if ALL the difference betwen indice related items into arrays are smaller than epsilon OR if both array are NULL.
        /// false otherwise</returns>
        public static bool IsAlmostEqualTo(this float[] current, float[] other, float epsilon)
        {
            if (current == null)
            {
                return other == null;
            }
            if (other == null)
            {
                return false;
            }

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

        public static string toString<T>(this T[] array, bool withBrackets = true)
        {
            if (array == null)
            {
                return "";
            }

            var result = "";
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

            if (withBrackets)
            {
                result = "[" + result + "]";
            }
            return result;
        }

        public static float[] Multiply(this float[] array, float[] array2)
        {
            float[] res = new float[array.Length];
            for (int index = 0; index < array.Length; index++)
            {
                res[index] = array[index] * array2[index];
            }
            return res;
        }

        public static float[] Multiply(this float[] array, float value)
        {
            float[] res = new float[array.Length];
            for (int index = 0; index < array.Length; index++)
            {
                res[index] = array[index] * value;
            }
            return res;
        }

        public static bool IsEqualTo(this float[] value, float[] other, float Epsilon = Epsilon)
        {
            if (value.Length != other.Length)
            {
                return false;
            }

            return !value.Where((t, i) => Math.Abs(t - other[i]) > Epsilon).Any();
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

        public static void GetProductVersion(out string product, out string version)
        {
            // The easy going solution is to relay on c# API MGlobal.mayaVersion, however, with miss the minor of the version
            // and still have to set the name by hand.
            string[] versionParts = null;
            try
            {
                versionParts = MGlobal.executeCommandStringResult("about -iv").Split();
            }
            catch
            {
                // we anticipate possible error.
            }
            versionParts = versionParts ?? new string[] { "Maya", MGlobal.mayaVersion ?? string.Empty };
            var l = versionParts.Length - 1;

            product = String.Join(" ", versionParts, 0, l);
            version = versionParts[l];
        }

        // -------------------------
        // --------- Math ----------
        // -------------------------

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }


        // -------------------------
        // --------- UUID ----------
        // -------------------------

        public static string GenerateUUID()
        {
            MUuid mUuid = new MUuid();
            mUuid.generate();
            return mUuid.asString();
        }
    }
}
