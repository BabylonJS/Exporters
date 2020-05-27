using System;

namespace BabylonExport.Entities
{
    public class BabylonQuaternion
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }


        public BabylonQuaternion() { }

        /**
         * Creates a new Quaternion from the passed floats.  
         */
        public BabylonQuaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public float[] ToArray()
        {
            return new[] { X, Y, Z, W };
        }

        /**
         * Copy / pasted from babylon 
         */
        public BabylonVector3 toEulerAngles()
        {
            var result = new BabylonVector3();

            var qz = this.Z;
            var qx = this.X;
            var qy = this.Y;
            var qw = this.W;

            var sqw = qw * qw;
            var sqz = qz * qz;
            var sqx = qx * qx;
            var sqy = qy * qy;

            var zAxisY = qy * qz - qx * qw;
            var limit = .4999999;

            if (zAxisY < -limit)
            {
                result.Y = (float)(2 * Math.Atan2(qy, qw));
                result.X = (float)Math.PI / 2;
                result.Z = 0;
            }
            else if (zAxisY > limit)
            {
                result.Y = (float)(2 * Math.Atan2(qy, qw));
                result.X = (float)-Math.PI / 2;
                result.Z = 0;
            }
            else
            {
                result.Z = (float)Math.Atan2(2.0 * (qx * qy + qz * qw), (-sqz - sqx + sqy + sqw));
                result.X = (float)Math.Asin(-2.0 * (qz * qy - qx * qw));
                result.Y = (float)Math.Atan2(2.0 * (qz * qx + qy * qw), (sqz - sqx - sqy + sqw));
            }

            return result;
        }

        public static BabylonQuaternion FromEulerAngles(float x, float y, float z)
        {
            var q = new BabylonQuaternion();
            BabylonQuaternion.RotationYawPitchRollToRef(y, x, z, q);
            return q;
        }

        /**
         * Creates a new rotation from the given Euler float angles (y, x, z) and stores it in the target quaternion
         * @param yaw defines the rotation around Y axis
         * @param pitch defines the rotation around X axis
         * @param roll defines the rotation around Z axis
         * @param result defines the target quaternion
         */
        public static void RotationYawPitchRollToRef(float yaw, float pitch, float roll, BabylonQuaternion result)
        {
            // Produces a quaternion from Euler angles in the z-y-x orientation (Tait-Bryan angles)
            var halfRoll = roll * 0.5;
            var halfPitch = pitch * 0.5;
            var halfYaw = yaw * 0.5;

            var sinRoll = Math.Sin(halfRoll);
            var cosRoll = Math.Cos(halfRoll);
            var sinPitch = Math.Sin(halfPitch);
            var cosPitch = Math.Cos(halfPitch);
            var sinYaw = Math.Sin(halfYaw);
            var cosYaw = Math.Cos(halfYaw);

            result.X = (float)((cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll));
            result.Y = (float)((sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll));
            result.Z = (float)((cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll));
            result.W = (float)((cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll));
        }

        public override string ToString()
        {
            return "{ X=" + X + ", Y=" + Y + ", Z=" + Z + ", W=" + W + " }";
        }

        /**
         * Updates the passed quaternion "result" with the passed rotation matrix values.  
         */
        public static void FromRotationMatrixToRef(BabylonMatrix matrix, BabylonQuaternion result)
        {
            var data = matrix.m;
            float m11 = data[0], m12 = data[4], m13 = data[8];
            float m21 = data[1], m22 = data[5], m23 = data[9];
            float m31 = data[2], m32 = data[6], m33 = data[10];
            var trace = m11 + m22 + m33;
            float s;

            if (trace > 0)
            {

                s = (float)(0.5 / Math.Sqrt(trace + 1.0));

                result.W = 0.25f / s;
                result.X = (m32 - m23) * s;
                result.Y = (m13 - m31) * s;
                result.Z = (m21 - m12) * s;
            }
            else if (m11 > m22 && m11 > m33)
            {

                s = (float)(2.0 * Math.Sqrt(1.0 + m11 - m22 - m33));

                result.W = (m32 - m23) / s;
                result.X = 0.25f * s;
                result.Y = (m12 + m21) / s;
                result.Z = (m13 + m31) / s;
            }
            else if (m22 > m33)
            {

                s = (float)(2.0 * Math.Sqrt(1.0 + m22 - m11 - m33));

                result.W = (m13 - m31) / s;
                result.X = (m12 + m21) / s;
                result.Y = 0.25f * s;
                result.Z = (m23 + m32) / s;
            }
            else
            {

                s = (float)(2.0 * Math.Sqrt(1.0 + m33 - m11 - m22));

                result.W = (m21 - m12) / s;
                result.X = (m13 + m31) / s;
                result.Y = (m23 + m32) / s;
                result.Z = 0.25f * s;
            }
        }

        /**
         * Updates the passed rotation matrix with the current Quaternion values.  
         * Returns the current Quaternion.  
         */
        public BabylonQuaternion toRotationMatrix(BabylonMatrix result)
        {
            var xx = this.X * this.X;
            var yy = this.Y * this.Y;
            var zz = this.Z * this.Z;
            var xy = this.X * this.Y;
            var zw = this.Z * this.W;
            var zx = this.Z * this.X;
            var yw = this.Y * this.W;
            var yz = this.Y * this.Z;
            var xw = this.X * this.W;

            result.m[0] = 1.0f - (2.0f * (yy + zz));
            result.m[1] = 2.0f * (xy + zw);
            result.m[2] = 2.0f * (zx - yw);
            result.m[3] = 0;
            result.m[4] = 2.0f * (xy - zw);
            result.m[5] = 1.0f - (2.0f * (zz + xx));
            result.m[6] = 2.0f * (yz + xw);
            result.m[7] = 0;
            result.m[8] = 2.0f * (zx + yw);
            result.m[9] = 2.0f * (yz - xw);
            result.m[10] = 1.0f - (2.0f * (yy + xx));
            result.m[11] = 0;
            result.m[12] = 0;
            result.m[13] = 0;
            result.m[14] = 0;
            result.m[15] = 1.0f;

            return this;
        }

        /**
         * Retuns a new Quaternion set from the starting index of the passed array.
         */
        public static BabylonQuaternion FromArray(float[] array, int countOffset = 0)
        {
            var offset = countOffset * 4;
            return new BabylonQuaternion(array[offset], array[offset + 1], array[offset + 2], array[offset + 3]);
        }


        public BabylonQuaternion MultiplyWith(BabylonQuaternion quaternion)
        {
            BabylonQuaternion result = new BabylonQuaternion();
            // (a + i b + j c + k d)*(e + i f + j g + k h) = a*e - b*f - c*g- d*h + i (b*e + a*f + c*h - d*g) + j (a*g - b*h + c*e + d*f) + k (a*h + b*g - c*f + d*e)
            // W*q.W - X*q.X - Y*q.Y- Z*q.Z + i (X*q.W + W*q.X + Y*q.Z - Z*q.Y) + j (W*q.Y - X*q.Z + Y*q.W + Z*q.X) + k (W*q.Z + X*q.Y - Y*q.X + Z*q.W)
            result.W = W * quaternion.W - X * quaternion.X - Y * quaternion.Y - Z * quaternion.Z;
            result.X = X * quaternion.W + W * quaternion.X + Y * quaternion.Z - Z * quaternion.Y;
            result.Y = W * quaternion.Y - X * quaternion.Z + Y * quaternion.W + Z * quaternion.X;
            result.Z = W * quaternion.Z + X * quaternion.Y - Y * quaternion.X + Z * quaternion.W;

            return result;
        }


        public BabylonVector3 Rotate(BabylonVector3 v)
        {
            BabylonMatrix m = new BabylonMatrix();
            toRotationMatrix(m);

            BabylonVector3 result = new BabylonVector3
            {
                X = m.m[0] * v.X + m.m[1] * v.Y + m.m[2] * v.Z,
                Y = m.m[4] * v.X + m.m[5] * v.Y + m.m[6] * v.Z,
                Z = m.m[8] * v.X + m.m[9] * v.Y + m.m[10] * v.Z
            };

            return result;
        }

        public static BabylonQuaternion Slerp(BabylonQuaternion left, BabylonQuaternion right, float amount)
        {
            float num2;
            float num3;
            float num4 = (((left.X * right.X) + (left.Y * right.Y)) + (left.Z * right.Z)) + (left.W * right.W);
            bool flag = false;

            if (num4 < 0)
            {
                flag = true;
                num4 = -num4;
            }

            if (num4 > 0.999999)
            {
                num3 = 1 - amount;
                num2 = flag ? -amount : amount;
            }
            else
            {
                var num5 = Math.Acos((double)num4);
                var num6 = (1.0 / Math.Sin((double)num5));
                num3 = (float)((Math.Sin((1.0 - (double)amount) * (double)num5)) * (double)num6);
                num2 = (float)(flag ? ((-Math.Sin((double)amount * num5)) * num6) : ((Math.Sin((double)amount * num5)) * num6));
            }
            BabylonQuaternion result = new BabylonQuaternion()
            {
                X = (num3 * left.X) + (num2 * right.X),
                Y = (num3 * left.Y) + (num2 * right.Y),
                Z = (num3 * left.Z) + (num2 * right.Z),
                W = (num3 * left.W) + (num2 * right.W)
            };

            return result;
        }
    }
}
