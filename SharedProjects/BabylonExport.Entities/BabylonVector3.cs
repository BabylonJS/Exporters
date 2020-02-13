using System;

namespace BabylonExport.Entities
{
    public class BabylonVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public BabylonVector3() { }

        /**
         * Creates a new Vector3 object from the passed x, y, z (floats) coordinates.  
         * A Vector3 is the main object used in 3D geometry.  
         * It can represent etiher the coordinates of a point the space, either a direction.  
         */
        public BabylonVector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public float[] ToArray()
        {
            return new [] {X, Y, Z};
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public static BabylonVector3 operator +(BabylonVector3 a, BabylonVector3 b)
        {
            return new BabylonVector3 {X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z};
        }

        public static BabylonVector3 operator -(BabylonVector3 a, BabylonVector3 b)
        {
            return new BabylonVector3 { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        }

        public static BabylonVector3 operator /(BabylonVector3 a, float b)
        {
            return new BabylonVector3 { X = a.X / b, Y = a.Y / b, Z = a.Z / b };
        }

        public static BabylonVector3 operator *(BabylonVector3 a, float b)
        {
            return new BabylonVector3 { X = a.X * b, Y = a.Y * b, Z = a.Z * b };
        }

        public enum EulerRotationOrder
        {
            XYZ,
            YZX,
            ZXY,
            XZY,
            YXZ,
            ZYX
        }

        // https://www.mathworks.com/matlabcentral/fileexchange/20696-function-to-convert-between-dcm-euler-angles-quaternions-and-euler-vectors?s_tid=mwa_osa_a
        // https://github.com/mrdoob/three.js/blob/09cfc67a3f52aeb4dd0009921d82396fd5dc5172/src/math/Quaternion.js#L199-L272

        public BabylonQuaternion toQuaternion(EulerRotationOrder rotationOrder = EulerRotationOrder.XYZ)
        {
            BabylonQuaternion quaternion = new BabylonQuaternion();

            var c1 = Math.Cos(0.5 * this.X);
            var c2 = Math.Cos(0.5 * this.Y);
            var c3 = Math.Cos(0.5 * this.Z);

            var s1 = Math.Sin(0.5 * this.X);
            var s2 = Math.Sin(0.5 * this.Y);
            var s3 = Math.Sin(0.5 * this.Z);

            switch (rotationOrder)
            {
                case EulerRotationOrder.XYZ:
                    quaternion.X = (float)(s1 * c2 * c3 + c1 * s2 * s3);
                    quaternion.Y = (float)(c1 * s2 * c3 - s1 * c2 * s3);
                    quaternion.Z = (float)(c1 * c2 * s3 + s1 * s2 * c3);
                    quaternion.W = (float)(c1 * c2 * c3 - s1 * s2 * s3);
                    break;
                case EulerRotationOrder.YZX:
                    quaternion.X = (float)(s1 * c2 * c3 + c1 * s2 * s3);
                    quaternion.Y = (float)(c1 * s2 * c3 + s1 * c2 * s3);
                    quaternion.Z = (float)(c1 * c2 * s3 - s1 * s2 * c3);
                    quaternion.W = (float)(c1 * c2 * c3 - s1 * s2 * s3);
                    break;
                case EulerRotationOrder.ZXY:
                    quaternion.X = (float)(s1 * c2 * c3 - c1 * s2 * s3);
                    quaternion.Y = (float)(c1 * s2 * c3 + s1 * c2 * s3);
                    quaternion.Z = (float)(c1 * c2 * s3 + s1 * s2 * c3);
                    quaternion.W = (float)(c1 * c2 * c3 - s1 * s2 * s3);
                    break;
                case EulerRotationOrder.XZY:
                    quaternion.X = (float)(s1 * c2 * c3 - c1 * s2 * s3);
                    quaternion.Y = (float)(c1 * s2 * c3 - s1 * c2 * s3);
                    quaternion.Z = (float)(c1 * c2 * s3 + s1 * s2 * c3);
                    quaternion.W = (float)(c1 * c2 * c3 + s1 * s2 * s3);
                    break;
                case EulerRotationOrder.YXZ:
                    quaternion.X = (float)(s1 * c2 * c3 + c1 * s2 * s3);
                    quaternion.Y = (float)(c1 * s2 * c3 - s1 * c2 * s3);
                    quaternion.Z = (float)(c1 * c2 * s3 - s1 * s2 * c3);
                    quaternion.W = (float)(c1 * c2 * c3 + s1 * s2 * s3);
                    break;
                case EulerRotationOrder.ZYX:
                    quaternion.X = (float)(s1 * c2 * c3 - c1 * s2 * s3);
                    quaternion.Y = (float)(c1 * s2 * c3 + s1 * c2 * s3);
                    quaternion.Z = (float)(c1 * c2 * s3 - s1 * s2 * c3);
                    quaternion.W = (float)(c1 * c2 * c3 + s1 * s2 * s3);
                    break;
            }
            return quaternion;
        }

        /**
         * Returns a new Vector3 set from the index "countOffset" x 3 of the passed array.
         */
        public static BabylonVector3 FromArray(float[] array, int countOffset = 0)
        {
            var offset = countOffset * 3;
            return new BabylonVector3(array[offset], array[offset + 1], array[offset + 2]);
        }
        
        public override string ToString()
        {
            return "{ X=" + X + ", Y=" + Y + ", Z=" + Z + " }";
        }
    }
}
