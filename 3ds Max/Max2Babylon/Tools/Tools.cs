﻿using Autodesk.Max;
using BabylonExport.Entities;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Max2Babylon
{
    public static class Tools
    {
        public static Random Random = new Random();

        #region Math

        public static float Lerp(float min, float max, float t)
        {
            return min + (max - min) * t;
        }

        public static int RoundToInt(float f)
        {
            return Convert.ToInt32(Math.Round(f, MidpointRounding.AwayFromZero));
        }

        /**
         * Computes a texture transform matrix with a pre-transformation
         */
        public static BabylonMatrix ComputeTextureTransformMatrix(BabylonVector3 pivotCenter , BabylonVector3 offset, BabylonQuaternion rotation, BabylonVector3 scale)
        {
            var dOffset = new BabylonVector3();
            var dRotation = new BabylonQuaternion();
            var dScale = new BabylonVector3();
            offset.X *= scale.X;
            offset.Y *= scale.Y;
            offset.Z *= 0;

            var transformMatrix = BabylonMatrix.Translation(new BabylonVector3(-pivotCenter.X, -pivotCenter.Y, 0)).multiply(BabylonMatrix.Compose(scale, rotation, offset)).multiply(BabylonMatrix.Translation(pivotCenter));
            return transformMatrix;
        }

        #endregion

        #region Array

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

        #endregion

        #region IIPropertyContainer

        public static string GetStringProperty(this IIGameProperty property)
        {
            string value = "";
            property.GetPropertyValue(ref value, 0);
            return value;
        }

        public static int GetIntValue(this IIGameProperty property)
        {
            int value = 0;
            property.GetPropertyValue(ref value, 0);
            return value;
        }

        public static bool GetBoolValue(this IIGameProperty property)
        {
            return property.GetIntValue() == 1;
        }

        public static float GetFloatValue(this IIGameProperty property)
        {
            float value = 0.0f;
            property.GetPropertyValue(ref value, 0, true);
            return value;
        }

        public static IPoint3 GetPoint3Property(this IIGameProperty property)
        {
            IPoint3 value = Loader.Global.Point3.Create(0, 0, 0);
            property.GetPropertyValue(value, 0);
            return value;
        }

        public static IPoint4 GetPoint4Property(this IIGameProperty property)
        {
            IPoint4 value = Loader.Global.Point4.Create(0, 0, 0, 0);
            property.GetPropertyValue(value, 0);
            return value;
        }

        public static string GetStringProperty(this IIPropertyContainer propertyContainer, int indexProperty)
        {
            return propertyContainer.GetProperty(indexProperty).GetStringProperty();
        }

        public static int GetIntProperty(this IIPropertyContainer propertyContainer, int indexProperty)
        {
            return propertyContainer.GetProperty(indexProperty).GetIntValue();
        }

        public static bool GetBoolProperty(this IIPropertyContainer propertyContainer, int indexProperty)
        {
            return propertyContainer.GetProperty(indexProperty).GetBoolValue();
        }

        public static float GetFloatProperty(this IIPropertyContainer propertyContainer, int indexProperty)
        {
            return propertyContainer.GetProperty(indexProperty).GetFloatValue();
        }

        public static IPoint3 GetPoint3Property(this IIPropertyContainer propertyContainer, int indexProperty)
        {
            return propertyContainer.GetProperty(indexProperty).GetPoint3Property();
        }

        public static IPoint4 GetPoint4Property(this IIPropertyContainer propertyContainer, int indexProperty)
        {
            return propertyContainer.GetProperty(indexProperty).GetPoint4Property();
        }

        #endregion

        // -------------------------

        #region Max Helpers

        public static IntPtr GetNativeHandle(this INativeObject obj)
        {
            return obj.NativePointer;

        }
        static Assembly GetWrappersAssembly()
        {
            return Assembly.Load("Autodesk.Max.Wrappers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        }

        public static IIGameCamera AsGameCamera(this IIGameObject obj)
        {
            var type = GetWrappersAssembly().GetType("Autodesk.Max.Wrappers.IGameCamera");
            var constructor = type.GetConstructors()[0];

            return (IIGameCamera)constructor.Invoke(new object[] { obj.GetNativeHandle(), false });
        }

        public static IIGameMesh AsGameMesh(this IIGameObject obj)
        {
            var type = GetWrappersAssembly().GetType("Autodesk.Max.Wrappers.IGameMesh");
            var constructor = type.GetConstructors()[0];

            return (IIGameMesh)constructor.Invoke(new object[] { obj.GetNativeHandle(), false });
        }

        public static IIGameLight AsGameLight(this IIGameObject obj)
        {
            var type = GetWrappersAssembly().GetType("Autodesk.Max.Wrappers.IGameLight");
            var constructor = type.GetConstructors()[0];

            return (IIGameLight)constructor.Invoke(new object[] { obj.GetNativeHandle(), false });
        }

        public static IIGameMorpher AsGameMorpher(this IIGameModifier obj)
        {
            var type = GetWrappersAssembly().GetType("Autodesk.Max.Wrappers.IGameMorpher");
            var constructor = type.GetConstructors()[0];

            return (IIGameMorpher)constructor.Invoke(new object[] { obj.GetNativeHandle(), false });
        }

        public const float Epsilon = 0.001f;

        public static IPoint3 XAxis { get { return Loader.Global.Point3.Create(1, 0, 0); } }
        public static IPoint3 YAxis { get { return Loader.Global.Point3.Create(0, 1, 0); } }
        public static IPoint3 ZAxis { get { return Loader.Global.Point3.Create(0, 0, 1); } }
        public static IPoint3 Origin { get { return Loader.Global.Point3.Create(0, 0, 0); } }

        public static IInterval Forever
        {
            get { return Loader.Global.Interval.Create(int.MinValue, int.MaxValue); }
        }

        public static IMatrix3 Identity { get { return Loader.Global.Matrix3.Create(XAxis, YAxis, ZAxis, Origin); } }

        public static Vector3 ToEulerAngles(this IQuat q)
        {
            // Store the Euler angles in radians
            var pitchYawRoll = new Vector3();

            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = q.X * q.Y + q.Z * q.W;

            if (test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
            {
                // Singularity at north pole
                pitchYawRoll.Y = 2f * (float)Math.Atan2(q.X, q.W);  // Yaw
                pitchYawRoll.X = (float)Math.PI * 0.5f;             // Pitch
                pitchYawRoll.Z = 0f;                                // Roll
                return pitchYawRoll;
            }
            if (test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
            {
                // Singularity at south pole
                pitchYawRoll.Y = -2f * (float)Math.Atan2(q.X, q.W); // Yaw
                pitchYawRoll.X = -(float)Math.PI * 0.5f;            // Pitch
                pitchYawRoll.Z = 0f;                                // Roll
                return pitchYawRoll;
            }
            pitchYawRoll.Y = (float)Math.Atan2(2f * q.Y * q.W - 2f * q.X * q.Z, sqx - sqy - sqz + sqw);       // Yaw
            pitchYawRoll.X = (float)Math.Asin(2f * test / unit);                                             // Pitch
            pitchYawRoll.Z = (float)Math.Atan2(2f * q.X * q.W - 2f * q.Y * q.Z, -sqx + sqy - sqz + sqw);      // Roll

            return pitchYawRoll;
        }
        public static float[] ToArray(this IGMatrix gmat)
        {
            //float eulX =0,  eulY=0,  eulZ=0;
            //unsafe
            //{
            //    gmat.Rotation.GetEuler( new IntPtr(&eulX), new IntPtr(&eulY), new IntPtr(&eulZ));
            //}
            //return (Matrix.Scaling(gmat.Scaling.X, gmat.Scaling.Y, gmat.Scaling.Z) * Matrix.RotationYawPitchRoll(eulY, eulX, eulZ) * Matrix.Translation(gmat.Translation.X, gmat.Translation.Y, gmat.Translation.Z)).ToArray();
            var r0 = gmat.GetRow(0);
            var r1 = gmat.GetRow(1);
            var r2 = gmat.GetRow(2);
            var r3 = gmat.GetRow(3);
            return new float[] {r0.X, r0.Y, r0.Z, r0.W,
            r1.X, r1.Y,r1.Z, r1.W,
            r2.X, r2.Y,r2.Z, r2.W,
            r3.X, r3.Y,r3.Z, r3.W,};
        }
        public static void PreparePipeline(IINode node, bool deactivate)
        {
            var obj = node.ObjectRef;

            if (obj == null || obj.SuperClassID != SClass_ID.GenDerivob)
            {
                return;
            }

            var derivedObject = obj as IIDerivedObject;

            if (derivedObject == null)
            {
                return;
            }

            for (var index = 0; index < derivedObject.NumModifiers; index++)
            {
                var modifier = derivedObject.GetModifier(index);

                //if (modifier.ClassID.PartA == 9815843 && modifier.ClassID.PartB == 87654) // Skin
                //{
                //    if (deactivate)
                //    {
                //        modifier.DisableMod();
                //    }
                //    else
                //    {
                //        modifier.EnableMod();
                //    }
                //}
            }
        }

        public static VNormal[] ComputeNormals(IMesh mesh, bool optimize)
        {
            var vnorms = new VNormal[mesh.NumVerts];
            var fnorms = new Vector3[mesh.NumFaces];

            for (var index = 0; index < mesh.NumVerts; index++)
            {
                vnorms[index] = new VNormal();
            }

            for (var index = 0; index < mesh.NumFaces; index++)
            {
                var face = mesh.Faces[index];
                Vector3 v0 = mesh.Verts[(int)face.V[0]].ToVector3();
                Vector3 v1 = mesh.Verts[(int)face.V[1]].ToVector3();
                Vector3 v2 = mesh.Verts[(int)face.V[2]].ToVector3();

                fnorms[index] = Vector3.Cross((v1 - v0), (v2 - v1));

                for (var j = 0; j < 3; j++)
                {
                    vnorms[(int)face.V[j]].AddNormal(fnorms[index], optimize ? 1 : face.SmGroup);
                }

                fnorms[index].Normalize();
            }

            for (var index = 0; index < mesh.NumVerts; index++)
            {
                vnorms[index].Normalize();
            }

            return vnorms;
        }

        public static bool IsEqualTo(this float[] value, float[] other)
        {
            if (value.Length != other.Length)
            {
                return false;
            }

            return !value.Where((t, i) => Math.Abs(t - other[i]) > Epsilon).Any();
        }

        public static float[] ToArray(this IMatrix3 value)
        {

            var row0 = value.GetRow(0).ToArraySwitched();
            var row1 = value.GetRow(1).ToArraySwitched();
            var row2 = value.GetRow(2).ToArraySwitched();
            var row3 = value.GetRow(3).ToArraySwitched();

            return new[]
            {
                row0[0], row0[1], row0[2], 0,
                row2[0], row2[1], row2[2], 0,
                row1[0], row1[1], row1[2], 0,
                row3[0], row3[1], row3[2], 1
            };
        }

        public static IPoint3 ToPoint3(this Vector3 value)
        {
            return Loader.Global.Point3.Create(value.X, value.Y, value.Z);
        }

        public static Vector3 ToVector3(this IPoint3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static Quaternion ToQuat(this IQuat value)
        {
            return new Quaternion(value.X, value.Z, value.Y, value.W);
        }
        public static float[] ToArray(this IQuat value)
        {
            return new[] { value.X, value.Z, value.Y, value.W };
        }

        public static float[] Scale(this IColor value, float scale)
        {
            return new[] { value.R * scale, value.G * scale, value.B * scale };
        }

        public static float[] ToArray(this IPoint4 value)
        {
            return new[] { value.X, value.Y, value.Z, value.W };
        }

        public static float[] ToArray(this IPoint3 value)
        {
            return new[] { value.X, value.Y, value.Z };
        }

        public static float[] ToArray(this IPoint2 value)
        {
            return new[] { value.X, value.Y };
        }

        public static float[] ToArraySwitched(this IPoint2 value)
        {
            return new[] { value.X, 1.0f - value.Y };
        }

        public static float[] ToArraySwitched(this IPoint3 value)
        {
            return new[] { value.X, value.Z, value.Y };
        }

        public static float[] ToArray(this IColor value)
        {
            return new[] { value.R, value.G, value.B };
        }

        public static IEnumerable<IINode> Nodes(this IINode node)
        {
            for (int i = 0; i < node.NumberOfChildren; ++i)
                if (node.GetChildNode(i) != null)
                    yield return node.GetChildNode(i);
        }

        public static IEnumerable<IINode> NodeTree(this IINode node)
        {
            foreach (var x in node.Nodes())
            {
                yield return x;
                foreach (var y in x.NodeTree())
                    yield return y;
            }
        }

        public static IEnumerable<IINode> NodesListBySuperClass(this IINode rootNode, SClass_ID sid)
        {
            return from n in rootNode.NodeTree() where n.ObjectRef != null && n.EvalWorldState(0, false).Obj.SuperClassID == sid select n;
        }

        public static IEnumerable<IINode> NodesListBySuperClasses(this IINode rootNode, SClass_ID[] sids)
        {
            return from n in rootNode.NodeTree() where n.ObjectRef != null && sids.Any(sid => n.EvalWorldState(0, false).Obj.SuperClassID == sid) select n;
        }

        public static IINode FindChildNode(this IINode node, uint nodeHandle)
        {
            foreach (IINode childNode in node.NodeTree())
                if (childNode.Handle.Equals(nodeHandle))
                    return childNode;

            return null;
        }

        /// <summary>
        /// Convert horizontal FOV to vertical FOV using default aspect ratio
        /// </summary>
        /// <param name="fov">horizontal FOV</param>
        /// <returns></returns>
        public static float ConvertFov(float fov)
        {
            return (float)(2.0f * Math.Atan(Math.Tan(fov / 2.0f) / Loader.Core.ImageAspRatio));
        }

        public static bool HasParent(this IINode node)
        {
            return node.ParentNode != null && !node.ParentNode.IsRootNode;
        }

        public static bool IsInstance(this IAnimatable node)
        {
            var data = node.GetAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 1);

            if (data != null)
            {
                return data.Data[0] != 0;
            }

            return false;
        }

        public static void MarkAsInstance(this IAnimatable node)
        {
            node.AddAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 1, new byte[] { 1 });
        }

        public static IDictionary<Guid, IAnimatable> guids = new Dictionary<Guid, IAnimatable>();

        public static Guid GetGuid(this IAnimatable node)
        {
            var uidData = node.GetAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 0);
            Guid uid;

            // If current node already has a uid
            if (uidData != null)
            {
                uid = new Guid(uidData.Data);

                if (guids.ContainsKey(uid))
                {
                    // If the uid is already used by another node
                    // This should be very rare, but it is possible:
                    // - when merging max scenes?
                    // - when cloning IAnimatable objects?
                    // - when exporting old assets that were created before GUID uniqueness was checked
                    // - when exporting parts of a scene separately, creating equal guids, and after that exporting them together
                    // All of the above problems disappear after exporting AGAIN, because the GUIDs will be unique then.
                    if (guids[uid].Equals(node as IInterfaceServer) == false)
                    {
                        // Remove old uid
                        node.RemoveAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 0);
                        // Create a new uid for current node
                        uid = Guid.NewGuid();
                        guids.Add(uid, node);
                        node.AddAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 0, uid.ToByteArray());
                    }
                }
                else
                {
                    guids.Add(uid, node);
                }
            }
            else
            {
                uid = Guid.NewGuid();
                // verify that we create a unique guid
                // there is still a possibility for a guid conflict:
                //   e.g. when we export parts of the scene separately an equal Guid can be generated...
                // only after re-exporting again after that will all guids be unique...
                while (guids.ContainsKey(uid))
                    uid = Guid.NewGuid();
                guids.Add(uid, node);
                node.AddAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 0, uid.ToByteArray());
            }

            return uid;
        }

        public static string GetLocalData(this IAnimatable node)
        {
            var uidData = node.GetAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 1);

            if (uidData != null)
            {
                return System.Text.Encoding.UTF8.GetString(uidData.Data);
            }

            return "";
        }

        public static void SetLocalData(this IAnimatable node, string value)
        {
            var uidData = node.GetAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 1);

            if (uidData != null)
            {
                node.RemoveAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 1);
            }

            node.AddAppDataChunk(Loader.Class_ID, SClass_ID.Basenode, 1, System.Text.Encoding.UTF8.GetBytes(value));
        }

        public static IMatrix3 GetWorldMatrix(this IINode node, int t, bool parent)
        {
            var tm = node.GetNodeTM(t, Forever);
            var ptm = node.ParentNode.GetNodeTM(t, Forever);

            if (!parent)
                return tm;

            if (node.ParentNode.SuperClassID == SClass_ID.Camera)
            {
                var r = ptm.GetRow(3);
                ptm.IdentityMatrix();
                ptm.SetRow(3, r);
            }

            ptm.Invert();
            return tm.Multiply(ptm);
        }

        public static IMatrix3 GetWorldMatrixComplete(this IINode node, int t, bool parent)
        {
            var tm = node.GetObjTMAfterWSM(t, Forever);
            var ptm = node.ParentNode.GetObjTMAfterWSM(t, Forever);

            if (!parent)
                return tm;

            if (node.ParentNode.SuperClassID == SClass_ID.Camera)
            {
                var r = ptm.GetRow(3);
                ptm.IdentityMatrix();
                ptm.SetRow(3, r);
            }

            ptm.Invert();
            return tm.Multiply(ptm);
        }

        public static ITriObject GetMesh(this IObject obj)
        {
            var triObjectClassId = Loader.Global.Class_ID.Create(0x0009, 0);

            if (obj.CanConvertToType(triObjectClassId) == 0)
                return null;

            return obj.ConvertToType(0, triObjectClassId) as ITriObject;
        }
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
        public static bool IsAlmostEqualTo(this IPoint4 current, IPoint4 other, float epsilon)
        {
            if (Math.Abs(current.X - other.X) > epsilon)
            {
                return false;
            }

            if (Math.Abs(current.Y - other.Y) > epsilon)
            {
                return false;
            }

            if (Math.Abs(current.Z - other.Z) > epsilon)
            {
                return false;
            }

            if (Math.Abs(current.W - other.W) > epsilon)
            {
                return false;
            }

            return true;
        }

        public static bool IsAlmostEqualTo(this IPoint3 current, IPoint3 other, float epsilon)
        {
            if (Math.Abs(current.X - other.X) > epsilon)
            {
                return false;
            }

            if (Math.Abs(current.Y - other.Y) > epsilon)
            {
                return false;
            }

            if (Math.Abs(current.Z - other.Z) > epsilon)
            {
                return false;
            }

            return true;
        }

        public static bool IsAlmostEqualTo(this IPoint2 current, IPoint2 other, float epsilon)
        {
            if (Math.Abs(current.X - other.X) > epsilon)
            {
                return false;
            }

            if (Math.Abs(current.Y - other.Y) > epsilon)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region UserProperties

        public static void SetStringProperty(this IINode node, string propertyName, string defaultState)
        {
            string state = defaultState;
            node.SetUserPropString(propertyName, state);
        }

        public static bool GetBoolProperty(this IINode node, string propertyName, int defaultState = 0)
        {
            int state = defaultState;
            node.GetUserPropBool(propertyName, ref state);
            return state == 1;
        }

        public static string GetStringProperty(this IINode node, string propertyName, string defaultState)
        {
            string state = defaultState;
            node.GetUserPropString(propertyName, ref state);
            return state;
        }

        public static float GetFloatProperty(this IINode node, string propertyName, float defaultState = 0)
        {
            float state = defaultState;
            node.GetUserPropFloat(propertyName, ref state);
            return state;
        }

        public static float[] GetVector3Property(this IINode node, string propertyName)
        {
            float state0 = 0;
            string name = propertyName + "_x";
            node.GetUserPropFloat(name, ref state0);


            float state1 = 0;
            name = propertyName + "_y";
            node.GetUserPropFloat(name, ref state1);

            float state2 = 0;
            name = propertyName + "_z";
            node.GetUserPropFloat(name, ref state2);

            return new[] { state0, state1, state2 };
        }

        public static string[] GetStringArrayProperty(this IINode node, string propertyName, char itemSeparator = ';')
        {
            string animationListString = string.Empty;
            if (!node.GetUserPropString(propertyName, ref animationListString) || string.IsNullOrEmpty(animationListString))
            {
                return new string[] { };
            }

            return animationListString.Split(itemSeparator);
        }

        public static void SetStringArrayProperty(this IINode node, string propertyName, IEnumerable<string> stringEnumerable, char itemSeparator = ';')
        {
            if (itemSeparator == ' ' || itemSeparator == '=')
                throw new Exception("Illegal separator. Spaces and equal signs are not allowed by the max sdk.");

            string itemSeparatorString = itemSeparator.ToString();
            foreach (string str in stringEnumerable)
            {
                if (str.Contains(" ") || str.Contains("="))
                    throw new Exception("Illegal character(s) in string array. Spaces and equal signs are not allowed by the max sdk.");

                if (str.Contains(itemSeparatorString))
                    throw new Exception("Illegal character(s) in string array. Found a separator ('" + itemSeparatorString + "') character.");
            }

            StringBuilder builder = new StringBuilder();

            bool first = true;
            foreach (string str in stringEnumerable)
            {
                if (first) first = false;
                else builder.Append(itemSeparator);

                builder.Append(str);
            }

            node.SetStringProperty(propertyName, builder.ToString());
        }

        /// <summary>
        /// Removes all properties with the given name found in the UserBuffer. Returns true if a property was found and removed.
        /// </summary>
        /// <param name="propertyName">The name of the property to remove.</param>
        /// <returns>True if a property was found and removed, false otherwise.</returns>
        public static bool DeleteProperty(this IINode node, string propertyName)
        {
            string inBuffer = string.Empty;
            string outBuffer = string.Empty;
            node.GetUserPropBuffer(ref inBuffer);

            bool foundProperty = false;

            using (StringReader reader = new StringReader(inBuffer))
            {
                using (StringWriter writer = new StringWriter())
                {
                    // simply read all lines and write them back into the ouput
                    // skip the lines that have a matching property name
                    for(string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                    {
                        string[] propValuePair = line.Split('=');
                        string currentPropertyName = propValuePair[0].Trim();
                        if (currentPropertyName.Equals(propertyName))
                        {
                            foundProperty = true;
                            continue;
                        }
                        writer.WriteLine(line);
                    }

                    outBuffer = writer.ToString();
                }
            }

            if (foundProperty)
            {
                node.SetUserPropBuffer(outBuffer);
                return true;
            }
            return false;
        }

        #endregion

        #region Windows.Forms.Control Serialization

        public static bool PrepareCheckBox(CheckBox checkBox, IINode node, string propertyName, int defaultState = 0)
        {
            var state = node.GetBoolProperty(propertyName, defaultState);

            if (checkBox.CheckState == CheckState.Indeterminate)
            {
                checkBox.CheckState = state ? CheckState.Checked : CheckState.Unchecked;
            }
            else
            {
                if (checkBox.ThreeState)
                {
                    if (!state && checkBox.CheckState == CheckState.Checked ||
                        state && checkBox.CheckState == CheckState.Unchecked)
                    {
                        checkBox.CheckState = CheckState.Indeterminate;
                        return true;
                    }
                }
                else
                {
                    checkBox.CheckState = state ? CheckState.Checked : CheckState.Unchecked;
                    return true;
                }
            }

            return false;
        }

        public static void PrepareCheckBox(CheckBox checkBox, List<IINode> nodes, string propertyName, int defaultState = 0)
        {
            checkBox.CheckState = CheckState.Indeterminate;
            foreach (var node in nodes)
            {
                if (PrepareCheckBox(checkBox, node, propertyName, defaultState))
                {
                    break;
                }
            }
        }

        public static void PrepareTextBox(TextBox textBox, IINode node, string propertyName, string defaultValue = "")
        {
            var state = node.GetStringProperty(propertyName, defaultValue);
            textBox.Text = state;
        }

        public static void PrepareTextBox(TextBox textBox, List<IINode> nodes, string propertyName, string defaultValue = "")
        {
            foreach(IINode node in nodes)
            {
                PrepareTextBox(textBox, node, propertyName, defaultValue);
            }
        }

        public static void PrepareComboBox(ComboBox comboBox, IINode node, string propertyName, string defaultValue)
        {
            comboBox.SelectedItem = node.GetStringProperty(propertyName, defaultValue);
        }

        public static void UpdateCheckBox(CheckBox checkBox, IINode node, string propertyName)
        {
            if (checkBox.CheckState != CheckState.Indeterminate)
            {
                node.SetUserPropBool(propertyName, checkBox.CheckState == CheckState.Checked);
            }
        }

        public static void UpdateCheckBox(CheckBox checkBox, List<IINode> nodes, string propertyName)
        {
            foreach (var node in nodes)
            {
                UpdateCheckBox(checkBox, node, propertyName);
            }
        }

        public static void UpdateTextBox(TextBox textBox, IINode node, string propertyName)
        {
            node.SetUserPropString(propertyName, textBox.Text);
        }

        public static void UpdateTextBox(TextBox textBox, List<IINode> nodes, string propertyName)
        {
            foreach (var node in nodes)
            {
                var value = textBox.Text;
                node.SetUserPropString(propertyName, value);
            }
        }

        public static void PrepareNumericUpDown(NumericUpDown nup, List<IINode> nodes, string propertyName, float defaultState = 0)
        {
            nup.Value = (decimal)nodes[0].GetFloatProperty(propertyName, defaultState);
        }

        public static void UpdateNumericUpDown(NumericUpDown nup, List<IINode> nodes, string propertyName)
        {
            foreach (var node in nodes)
            {
                node.SetUserPropFloat(propertyName, (float)nup.Value);
            }
        }

        public static void PrepareVector3Control(Vector3Control vector3Control, IINode node, string propertyName, float defaultX = 0, float defaultY = 0, float defaultZ = 0)
        {
            vector3Control.X = node.GetFloatProperty(propertyName + "_x", defaultX);
            vector3Control.Y = node.GetFloatProperty(propertyName + "_y", defaultY);
            vector3Control.Z = node.GetFloatProperty(propertyName + "_z", defaultZ);
        }

        public static void UpdateVector3Control(Vector3Control vector3Control, IINode node, string propertyName)
        {
            string name = propertyName + "_x";
            node.SetUserPropFloat(name, vector3Control.X);

            name = propertyName + "_y";
            node.SetUserPropFloat(name, vector3Control.Y);

            name = propertyName + "_z";
            node.SetUserPropFloat(name, vector3Control.Z);
        }

        public static void UpdateVector3Control(Vector3Control vector3Control, List<IINode> nodes, string propertyName)
        {
            foreach (var node in nodes)
            {
                UpdateVector3Control(vector3Control, node, propertyName);
            }
        }

        public static void UpdateComboBox(ComboBox comboBox, IINode node, string propertyName)
        {
            var value = comboBox.SelectedItem.ToString();
            node.SetUserPropString(propertyName, value);
        }

        public static void UpdateComboBox(ComboBox comboBox, List<IINode> nodes, string propertyName)
        {
            foreach (var node in nodes)
            {
                UpdateComboBox(comboBox, node, propertyName);
            }
        }
        #endregion


        #region Windows.Forms Helpers

        /// <summary>
        /// Enumerates the whole tree, excluding the given node.
        /// </summary>
        public static IEnumerable<TreeNode> NodeTree(this TreeNode node)
        {
            foreach (TreeNode x in node.Nodes)
            {
                yield return x;
                foreach (TreeNode y in x.NodeTree())
                    yield return y;
            }
        }

        #endregion
    }
}
