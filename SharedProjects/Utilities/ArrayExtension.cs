using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    static class ArrayExtension
    {
        public static bool IsAlmostEqualTo(this float[] current, float[] other, float epsilon)
        {
            if (current == null && other == null)
            {
                return true;
            }
            if (current == null || other == null)
            {
                return false;
            }
            if (current.Length != other.Length)
            {
                return false;
            }
            for (var i = 0; i < current.Length; ++i)
            {
                if (Math.Abs(current[i] - other[i]) > epsilon)
                {
                    return false;
                }
            }
            return true;
        }

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

        public static string ToString<T>(this T[] array, bool withBrackets = true)
        {
            if (array == null)
            {
                return "";
            }

            var result = "";
            if (array.Length > 0)
            {
                result += array[0];
                for (int i = 1; i < array.Length; i++)
                {
                    result += ", " + array[i];
                }
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
    }
}
