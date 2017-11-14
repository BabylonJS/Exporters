using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO.Compression;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Ajax.Utilities;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using FreeImageAPI;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MsgPack.Serialization;
using BabylonExport.Entities;

namespace Unity3D2Babylon
{
    public static class Tools
    {
        public struct Vector
        {
            public double x;
            public double y;
            public double z;
        }

        public class ImageInfo
        {
            public PixelFormat pixelFormat = PixelFormat.RGBQUAD;
            public bool useColorMask = false;
            public uint redColorMask = 0;
            public uint greenColorMask = 0;
            public uint blueColorMask = 0;
            public FREE_IMAGE_TYPE freeImageType = FREE_IMAGE_TYPE.FIT_BITMAP;
        }
        
        public class TextureInfo
        {
            public string filename = null;
            public Texture2D texture = null;
        }

        public enum FlipImage
        {
            None = 0,
            Vertical = 1,
            Horizontal = 2
        }

        public enum PixelFormat
        {
            RGBTRIPLE = 0,
            RGBQUAD = 1,
            RGB16 = 2,
            RGBA16 = 3,
            RGBF = 4,
            RGBAF = 5
        }

        public enum ColorCorrection
        {
            None = 0,
            Gamma = 1,
            Linear = 2
        }
        
        public static BindingFlags FullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
        
        public static float[] ToFloat(this Color32 color)
        {
            var result = new float[4];
            result[0] = color.r;
            result[1] = color.g;
            result[2] = color.b;
            result[3] = color.a;

            return result;
        }
        
        public static float[] ToFloat(this Color color)
        {
            var result = new float[4];
            result[0] = color.r;
            result[1] = color.g;
            result[2] = color.b;
            result[3] = color.a;

            return result;
        }

        public static float[] ToFloat(this Vector2 vector2)
        {
            var result = new float[2];
            result[0] = vector2.x;
            result[1] = vector2.y;

            return result;
        }

        public static float[] ToFloat(this Vector3 vector3)
        {
            var result = new float[3];
            result[0] = vector3.x;
            result[1] = vector3.y;
            result[2] = vector3.z;

            return result;
        }

        public static float[] ToFloat(this Vector4 vector4)
        {
            var result = new float[4];
            result[0] = vector4.x;
            result[1] = vector4.y;
            result[2] = vector4.z;
            result[3] = vector4.w;

            return result;
        }

        public static float[] ToFloat(this SerializableVector3 vector3)
        {
            var result = new float[3];
            result[0] = vector3.X;
            result[1] = vector3.Y;
            result[2] = vector3.Z;

            return result;
        }

        public static float[] ToFloat(this Quaternion quaternion)
        {
            var result = new float[4];
            result[0] = quaternion.x;
            result[1] = quaternion.y;
            result[2] = quaternion.z;
            result[3] = quaternion.w;

            return result;
        }

        public static bool IsPrefab(GameObject go)
        {
            return (go.hideFlags == HideFlags.HideInHierarchy);
        }

        public static string ToJson(object obj, bool pretty = false)
        {
            var jsSettings = new JsonSerializerSettings();
            jsSettings.ContractResolver = new ToolkitContractResolver();
            jsSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
            return JsonConvert.SerializeObject(obj, (pretty == true) ? Formatting.Indented : Formatting.None, jsSettings);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void PackBinary<T>(T obj, Stream stream, SerializationMethod method = SerializationMethod.Map)
        {
            SerializationContext.Default.SerializationMethod = method;
            var serializer = SerializationContext.Default.GetSerializer<T>();
            serializer.Pack(stream, obj);
        }

        public static void JsonExport(BabylonScene scene, string filename, bool pretty = false)
        {
            var json = Tools.ToJson(scene, pretty);
            File.WriteAllText(filename, json);
        }

        public static void BinaryExport(BabylonScene scene, string filename)
        {
            var json = Tools.ToJson(scene, false);
            using (var file = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
                    Tools.PrecompressStream(mem, file, 1024 * 32);
                }
            }
        }

        public static int ExecuteProcess(string fileName, string commandLine, string workingDirectory = null, bool logOutput = true)
        {
            int result = -1;
            string root = (!String.IsNullOrEmpty(workingDirectory)) ? workingDirectory : Tools.GetRootPath();
            //* Execute typescript compiler
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = commandLine;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = root;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            //* Set ONLY ONE handler here.
            process.ErrorDataReceived += new DataReceivedEventHandler(OnBuildProjectTypeScriptError);
            //* Start process
            process.Start();
            //* Read one element asynchronously
            process.BeginErrorReadLine();
            //* Read the other one synchronously
            string output = process.StandardOutput.ReadToEnd();
            //* Log compiler output issues
            if (!String.IsNullOrEmpty(output))
            {
                if (output.IndexOf("): error", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    UnityEngine.Debug.LogError(output);
                }
                else
                {
                    if (logOutput == true) {
                        UnityEngine.Debug.Log(output);
                    }
                }
            }
            process.WaitForExit();
            result = process.ExitCode;
            return result;
        }

        public static string MakeSafe(this string source)
        {
            return source.Replace("@", "").Replace("#", "").Replace("$", "").Replace("%", "").Replace("&", "").Replace("^", "").Replace("*", "").Replace(":", "");
        }

        public static float Normalize(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        public static float Denormalize(float normalized, float min, float max)
        {
            return (normalized * (max - min) + min);
        }

        public static Texture2D BlurImage(Texture2D image, int blurSize)
        {
            Texture2D blurred = new Texture2D(image.width, image.height, image.format, false);
            blurred.Apply();
            return blurred;
        }

        public static Vector3 GetPosition(this Matrix4x4 m)
        {
            return new Vector3(m[0, 3], m[1, 3], m[2, 3]);
        }
    
        public static Vector3 GetScale(this Matrix4x4 m)
        {
            return new Vector3
                (m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
        }
    
        public static Quaternion GetRotation(this Matrix4x4 m)
        {
            Vector3 s = GetScale(m);
    
            // Normalize Scale from Matrix4x4
            float m00 = m[0, 0] / s.x;
            float m01 = m[0, 1] / s.y;
            float m02 = m[0, 2] / s.z;
            float m10 = m[1, 0] / s.x;
            float m11 = m[1, 1] / s.y;
            float m12 = m[1, 2] / s.z;
            float m20 = m[2, 0] / s.x;
            float m21 = m[2, 1] / s.y;
            float m22 = m[2, 2] / s.z;
    
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m00 + m11 + m22)) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m00 - m11 - m22)) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m00 + m11 - m22)) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m00 - m11 + m22)) / 2;
            q.x *= Mathf.Sign(q.x * (m21 - m12));
            q.y *= Mathf.Sign(q.y * (m02 - m20));
            q.z *= Mathf.Sign(q.z * (m10 - m01));
    
            // q.Normalize()
            float qMagnitude = Mathf.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z);
            q.w /= qMagnitude;
            q.x /= qMagnitude;
            q.y /= qMagnitude;
            q.z /= qMagnitude;
    
            return q;
        }

        // will check if the specified layer names are present and add any missing ones
        // it will simply add them from the first open slots so do not depend on any
        // specific order but rather grab layers from the layer names at runtime
        public static void CheckLayers(string[] layerNames)
        {
            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = manager.FindProperty("layers");
 
            foreach (string name in layerNames)
            {
                // check if layer is present
                bool found = false;
                for (int i = 0; i <= 31; i++)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                    if (sp != null && name.Equals(sp.stringValue))
                    {
                        found = true;
                        break;
                    }
                }
 
                // not found, add into 1st open slot
                if (!found)
                {
                    SerializedProperty slot = null;
                    for (int i = 8; i <= 31; i++)
                    {
                        SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                        if (sp != null && string.IsNullOrEmpty(sp.stringValue))
                        {
                            slot = sp;
                            break;
                        }
                    }
 
                    if (slot != null)
                    {
                        slot.stringValue = name;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Could not find an open Layer Slot for: " + name);
                    }
                }
            }
 
            // save
            manager.ApplyModifiedProperties();
        }
 
        public static void CheckTags(string[] tagNames)
        {
            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = manager.FindProperty("tags");
 
            List<string> DefaultTags = new List<string>(){ "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" };
 
            foreach (string name in tagNames)
            {
                if (DefaultTags.Contains(name)) continue;
 
                // check if tag is present
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                    if (t.stringValue.Equals(name)) { found = true; break; }
                }
 
                // if not found, add it
                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(0);
                    SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                    n.stringValue = name;
                }
            }
 
            // save
            manager.ApplyModifiedProperties();
        }
 
        public static void CheckSortLayers(string[] tagNames)
        {
            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty sortLayersProp = manager.FindProperty("m_SortingLayers");
 
            //for (int i = 0; i < sortLayersProp.arraySize; i++)
            //{ // used to figure out how all of this works and what properties values look like
            //    SerializedProperty entry = sortLayersProp.GetArrayElementAtIndex(i);
            //    SerializedProperty name = entry.FindPropertyRelative("name");
            //    SerializedProperty unique = entry.FindPropertyRelative("uniqueID");
            //    SerializedProperty locked = entry.FindPropertyRelative("locked");
            //    Debug.Log(name.stringValue + " => " + unique.intValue + " => " + locked.boolValue);
            //}
 
            foreach (string name in tagNames)
            {
                // check if tag is present
                bool found = false;
                for (int i = 0; i < sortLayersProp.arraySize; i++)
                {
                    SerializedProperty entry = sortLayersProp.GetArrayElementAtIndex(i);
                    SerializedProperty t = entry.FindPropertyRelative("name");
                    if (t.stringValue.Equals(name)) { found = true; break; }
                }
 
                // if not found, add it
                if (!found)
                {
                    manager.ApplyModifiedProperties();
                    Tools.AddSortingLayer();
                    manager.Update();
 
                    int idx = sortLayersProp.arraySize - 1;
                    SerializedProperty entry = sortLayersProp.GetArrayElementAtIndex(idx);
                    SerializedProperty t = entry.FindPropertyRelative("name");
                    t.stringValue = name;
                }
            }
 
            // save
            manager.ApplyModifiedProperties();
        }

        public static string FirstUpper(this string value)
        {
            string result = value;
            if (!String.IsNullOrEmpty(value))
            {
                result = char.ToUpper(result[0]) + result.Substring(1);
            }
            return result;
        }

        public static string FirstLower(this string value)
        {
            string result = value;
            if (!String.IsNullOrEmpty(value))
            {
                result = char.ToLower(result[0]) + result.Substring(1);
            }
            return result;
        }

        public static string GetRootPath()
        {
            return Application.dataPath.Replace("/Assets", "");
        }

        public static string GetNativePath(string assetPath)
        {
            string apath = Tools.GetRootPath();
            return Path.Combine(apath, assetPath);
        }

        public static BabylonIAnimatable FindAnimatableItem(string id, BabylonScene sceneBuilder)
        {
            BabylonIAnimatable result = null;
            if (sceneBuilder.CamerasList != null && sceneBuilder.CamerasList.Count > 0) {
                for (int i = 0; i < sceneBuilder.CamerasList.Count; i++) {
                    var item = sceneBuilder.CamerasList[i];
                    if (item.id == id) {
                        result = item;
                        break;
                    }    
                }
            }
            if (result == null && sceneBuilder.LightsList != null && sceneBuilder.LightsList.Count > 0) {
                for (int i = 0; i < sceneBuilder.LightsList.Count; i++) {
                    var item = sceneBuilder.LightsList[i];
                    if (item.id == id) {
                        result = item;
                        break;
                    }    
                }
            }
            if (result == null && sceneBuilder.MeshesList != null && sceneBuilder.MeshesList.Count > 0) {
                for (int i = 0; i < sceneBuilder.MeshesList.Count; i++) {
                    var item = sceneBuilder.MeshesList[i];
                    if (item.id == id) {
                        result = item;
                        break;
                    }    
                }
            }
            return result;
        }
        
        public static List<AnimationClip> GetAnimationClips(Animation legacy)
        {
            List<AnimationClip> states = new List<AnimationClip>();
            if (legacy != null) {
                AnimationClip defaultClip = (legacy.clip != null) ? legacy.clip : null;
                if (defaultClip != null) {
                    defaultClip.legacy = true;
                    states.Add(defaultClip);
                }
                AnimationClip[] clips = AnimationUtility.GetAnimationClips(legacy.gameObject);
                for (int ii = 0; ii < clips.Length; ii++) {
                    AnimationClip clip = clips[ii];  
                    if (clip != null && !states.Contains(clip)) {
                        clip.legacy = true;
                        states.Add(clip);
                    }
                }
            }
            return states;
        }

        public static List<AnimationClip> GetAnimationClips(Animator animator)
        {
            List<AnimationClip> states = new List<AnimationClip>();
            if (animator != null && animator.runtimeAnimatorController != null && animator.runtimeAnimatorController.animationClips != null && animator.runtimeAnimatorController.animationClips.Length > 0) {
                AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
                if (ac != null && ac.layers.Length > 0 && ac.animationClips != null && ac.animationClips.Length > 0) {
                    foreach (var layer in ac.layers) {
                        AnimatorStateMachine sm = layer.stateMachine;
                        if (sm.defaultState != null && sm.defaultState.motion != null && sm.defaultState.motion is AnimationClip) {
                            var clip = sm.defaultState.motion as AnimationClip;
                            if (!states.Contains(clip)) {
                                clip.legacy = false;
                                states.Add(clip);
                            }
                        }
                    }
                    for (int ii = 0; ii < ac.animationClips.Length; ii++) {
                        AnimationClip clip = ac.animationClips[ii];  
                        if (clip != null && !states.Contains(clip)) {
                            clip.legacy = false;
                            states.Add(clip);
                        }
                    }
                }
            }
            return states;
        }

        public static List<AnimationParameters> GetAnimationParameters(Animator animator)
        {
            List<AnimationParameters> parameters = new List<AnimationParameters>();
            if (animator != null && animator.runtimeAnimatorController != null && animator.runtimeAnimatorController.animationClips != null && animator.runtimeAnimatorController.animationClips.Length > 0) {
                animator.enabled = true;
                animator.Rebind();
                int count = animator.parameterCount;
                if (count > 0) {
                    for (int ii = 0; ii < count; ii++) {
                        var parameter = animator.GetParameter(ii);
                        var px = new AnimationParameters();
                        px.defaultFloat = parameter.defaultFloat;
                        px.defaultBool = parameter.defaultBool;
                        px.defaultInt = parameter.defaultInt;
                        px.name = parameter.name;
                        px.type = (int)parameter.type;
                        px.curve = animator.IsParameterControlledByCurve(parameter.name);
                        parameters.Add(px);
                    }
                }
            }
            return parameters;            
        }
        
        public static AnimationCurve GetAnimationCurve(AnimationClip clip, string name)
        {
            AnimationCurve result = null;
            if (clip != null) {
                var curveBindings = AnimationUtility.GetCurveBindings(clip);
                var curveBindingsCount = curveBindings.Length;
                if (curveBindingsCount > 0) {
                    for (int i = 0; i < curveBindingsCount; i++) {
                        var binding = curveBindings[i];
                        if (binding.propertyName == name) {
                            result = AnimationUtility.GetEditorCurve(clip, binding);
                            break;
                        }
                    }
                }
            }
            return result;
        }
        
        public static Dictionary<string, object> ExportStateMachine(Animator animator, bool active)
        {
            float speed = 1.0f;
            MachineState entry = null;
            List<AnimationParameters> parameters = null;
            List<MachineState> states = new List<MachineState>();
            //List<Dictionary<string, object>> layers = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> entries = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> transitions = new List<Dictionary<string, object>>();
            if (animator != null) {
                parameters = Tools.GetAnimationParameters(animator);
                speed = animator.speed;
                AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
                if (ac != null && ac.layers.Length > 0) {
                    for(int index = 0; index < ac.layers.Length; index++) {
                        var layer = ac.layers[index];
                        /* TODO: Move to Export Skeleton Parse State Machine Layer
                        Dictionary<string, object> maskInfo = new Dictionary<string, object>();
                        if (layer.avatarMask != null) {
                            int tcount = layer.avatarMask.transformCount;
                            for (int ii=0; ii < tcount; ii++) {
                                string wtf = layer.avatarMask.GetTransformPath(ii);
                                UnityEngine.Debug.Log("MASK WTF: " + wtf);
                               
                            }
                        }
                        Dictionary<string, object> layerInfo = new Dictionary<string, object>();
                        layerInfo.Add("name", layer.name);
                        layerInfo.Add("index", index);
                        layerInfo.Add("masks", maskInfo);
                        layers.Add(layerInfo);
                        */
                        AnimatorStateMachine sm = layer.stateMachine;
                        Tools.ParseStateMachine(animator, layer, index, sm, ref states, ref entry, ref entries, ref transitions);
                    }
                }
            } else {
                UnityEngine.Debug.LogWarning("Failed to get 'Animator' component for: " + animator.name);
            }
            Dictionary<string, object> stateInfo = new Dictionary<string, object>();
            string ename = (entry != null) ? entry.name : null;
            stateInfo.Add("active", active);
            stateInfo.Add("speed", speed);
            stateInfo.Add("entry", ename);
            //stateInfo.Add("layers", layers);
            stateInfo.Add("states", states);
            stateInfo.Add("entries", entries);
            stateInfo.Add("parameters", parameters);
            stateInfo.Add("transitions", transitions);
            return stateInfo;
        }  

        public static void ParseStateMachine(Animator animator, AnimatorControllerLayer layer, int index, AnimatorStateMachine machine, ref List<MachineState> states, ref MachineState entry, ref List<Dictionary<string, object>> entries, ref List<Dictionary<string, object>> transitions)
        {
            // Parse State Machine Items
            if (machine.states.Length > 0) {
                for (int ii = 0; ii < machine.states.Length; ii++) {
                    var state = machine.states[ii].state;  
                    if (state != null) {
                        MachineState data = Tools.CreateMachineState(animator, layer, index, machine, state, ref entry);
                        if (data != null) states.Add(data);
                    }
                }
            }
            // Parse Entry State Transitions
            if (machine.entryTransitions != null && machine.entryTransitions.Length > 0) {
                foreach (var entryTransition in machine.entryTransitions)  {
                    List<Dictionary<string, object>> entryConditions = new List<Dictionary<string, object>>();
                    if (entryTransition.conditions != null && entryTransition.conditions.Length > 0) {
                        foreach (var entryCondition in entryTransition.conditions) {
                            Dictionary<string, object> econdInfo = new Dictionary<string, object>();
                            econdInfo.Add("mode", (int)entryCondition.mode);
                            econdInfo.Add("parameter", entryCondition.parameter);
                            econdInfo.Add("threshold", entryCondition.threshold);
                            entryConditions.Add(econdInfo);
                        }
                    }
                    string edestinationName = (entryTransition.destinationState != null) ? entryTransition.destinationState.name : null;
                    Dictionary<string, object> etransInfo = new Dictionary<string, object>();
                    etransInfo.Add("anyState", false);
                    etransInfo.Add("indexLayer", index);
                    etransInfo.Add("machineLayer", layer.name);
                    etransInfo.Add("machineName", machine.name);
                    etransInfo.Add("canTransitionToSelf", false);
                    etransInfo.Add("destination", edestinationName);
                    etransInfo.Add("duration", 0.0f);
                    etransInfo.Add("exitTime", 0.0f);
                    etransInfo.Add("hasExitTime", 0.0f);
                    etransInfo.Add("fixedDuration", 0.0f);
                    etransInfo.Add("intSource", 0);
                    etransInfo.Add("isExit", entryTransition.isExit);
                    etransInfo.Add("mute", entryTransition.mute);
                    etransInfo.Add("name", entryTransition.name);
                    etransInfo.Add("offset", 0.0f);
                    etransInfo.Add("orderedInt", 0);
                    etransInfo.Add("solo", entryTransition.solo);
                    etransInfo.Add("conditions", entryConditions);
                    entries.Add(etransInfo);
                }
            }
            // Parse All Any State Transitions
            if (machine.anyStateTransitions != null && machine.anyStateTransitions.Length > 0) {
                foreach (var anyTransition in machine.anyStateTransitions)  {
                    List<Dictionary<string, object>> anyConditions = new List<Dictionary<string, object>>();
                    if (anyTransition.conditions != null && anyTransition.conditions.Length > 0) {
                        foreach (var anyCondition in anyTransition.conditions) {
                            Dictionary<string, object> acondInfo = new Dictionary<string, object>();
                            acondInfo.Add("mode", (int)anyCondition.mode);
                            acondInfo.Add("parameter", anyCondition.parameter);
                            acondInfo.Add("threshold", anyCondition.threshold);
                            anyConditions.Add(acondInfo);
                        }
                    }
                    string adestinationName = (anyTransition.destinationState != null) ? anyTransition.destinationState.name : null;
                    Dictionary<string, object> atransInfo = new Dictionary<string, object>();
                    atransInfo.Add("anyState", true);
                    atransInfo.Add("indexLayer", index);
                    atransInfo.Add("machineLayer", layer.name);
                    atransInfo.Add("machineName", machine.name);
                    atransInfo.Add("canTransitionToSelf", anyTransition.canTransitionToSelf);
                    atransInfo.Add("destination", adestinationName);
                    atransInfo.Add("duration", anyTransition.duration);
                    atransInfo.Add("exitTime", anyTransition.exitTime);
                    atransInfo.Add("hasExitTime", anyTransition.hasExitTime);
                    atransInfo.Add("fixedDuration", anyTransition.hasFixedDuration);
                    atransInfo.Add("intSource", (int)anyTransition.interruptionSource);
                    atransInfo.Add("isExit", anyTransition.isExit);
                    atransInfo.Add("mute", anyTransition.mute);
                    atransInfo.Add("name", anyTransition.name);
                    atransInfo.Add("offset", anyTransition.offset);
                    atransInfo.Add("orderedInt", anyTransition.orderedInterruption);
                    atransInfo.Add("solo", anyTransition.solo);
                    atransInfo.Add("conditions", anyConditions);
                    transitions.Add(atransInfo);
                }
            }
            // Recurse All Sub State Machines
            if (machine.stateMachines != null && machine.stateMachines.Length > 0) {
                foreach (var sub in machine.stateMachines) {
                    AnimatorStateMachine sm = sub.stateMachine;
                    Tools.ParseStateMachine(animator, layer, index, sm, ref states, ref entry, ref entries, ref transitions);
                }
            }
        }

        public static MachineState CreateMachineState(Animator animator, AnimatorControllerLayer layer, int index, AnimatorStateMachine machine, AnimatorState state, ref MachineState entry)
        {
            MachineState result = new MachineState();
            result.tag = state.tag;
            result.time = 0.0f;
            result.name = state.name;
            result.type = (state.motion != null && state.motion is BlendTree) ? (int)MotionType.Tree : (int)MotionType.Clip;
            result.motion = (state.motion != null) ? state.motion.name : String.Empty;
            result.branch = null;
            result.rate = (state.motion != null && state.motion is AnimationClip) ? ((AnimationClip)state.motion).frameRate : 0.0f;
            result.length = (state.motion != null && state.motion is AnimationClip) ? ((AnimationClip)state.motion).length : 0.0f;
            result.layer = layer.name;
            result.index = index;
            result.machine = machine.name;
            result.interupted = false;
            result.cycleOffset = state.cycleOffset;
            result.cycleOffsetParameter = state.cycleOffsetParameter;
            result.cycleOffsetParameterActive = state.cycleOffsetParameterActive;
            result.iKOnFeet = state.iKOnFeet;
            result.mirror = state.mirror;
            result.mirrorParameter = state.mirrorParameter;
            result.mirrorParameterActive = state.mirrorParameterActive;
            result.speed = state.speed;
            result.speedParameter = state.speedParameter;
            result.speedParameterActive = state.speedParameterActive;
            result.animations = null;
            result.transitions = new List<Dictionary<string, object>>();
            result.customCurves = new List<Dictionary<string, BabylonAnimation>>();
            result.apparentSpeed = 0.0f;
            result.averageAngularSpeed = 0.0f;
            result.averageDuration = 0.0f;
            result.averageSpeed = new float[] { 0.0f, 0.0f, 0.0f };
            if (state.motion != null) {
                result.apparentSpeed = state.motion.apparentSpeed;
                result.averageAngularSpeed = state.motion.averageAngularSpeed;
                result.averageDuration = state.motion.averageDuration;
                result.averageSpeed = state.motion.averageSpeed.ToFloat();
            }
            if (state.transitions != null && state.transitions.Length > 0) {
                foreach (var transition in state.transitions)  {
                    List<Dictionary<string, object>> conditions = new List<Dictionary<string, object>>();
                    if (transition.conditions != null && transition.conditions.Length > 0) {
                        foreach (var condition in transition.conditions) {
                            Dictionary<string, object> condInfo = new Dictionary<string, object>();
                            condInfo.Add("mode", (int)condition.mode);
                            condInfo.Add("parameter", condition.parameter);
                            condInfo.Add("threshold", condition.threshold);
                            conditions.Add(condInfo);
                        }
                    }
                    string destinationName = (transition.destinationState != null) ? transition.destinationState.name : null;
                    Dictionary<string, object> transInfo = new Dictionary<string, object>();
                    transInfo.Add("anyState", false);
                    transInfo.Add("indexLayer", result.index);
                    transInfo.Add("machineLayer", result.layer);
                    transInfo.Add("machineName", result.machine);
                    transInfo.Add("canTransitionToSelf", transition.canTransitionToSelf);
                    transInfo.Add("destination", destinationName);
                    transInfo.Add("duration", transition.duration);
                    transInfo.Add("exitTime", transition.exitTime);
                    transInfo.Add("hasExitTime", transition.hasExitTime);
                    transInfo.Add("fixedDuration", transition.hasFixedDuration);
                    transInfo.Add("intSource", (int)transition.interruptionSource);
                    transInfo.Add("isExit", transition.isExit);
                    transInfo.Add("mute", transition.mute);
                    transInfo.Add("name", transition.name);
                    transInfo.Add("offset", transition.offset);
                    transInfo.Add("orderedInt", transition.orderedInterruption);
                    transInfo.Add("solo", transition.solo);
                    transInfo.Add("conditions", conditions);
                    result.transitions.Add(transInfo);
                }
            }
            result.blendtree = Tools.ParseBlendingTrees(animator, index, state.motion, state.name);
            Tools.DumpCustomCurves(animator, result);
            if (entry == null && state == machine.defaultState) {
                entry = result;
            }
            return result;
        }

        public static Dictionary<string, object> ParseBlendingTrees(Animator animator, int index, Motion motion, string statename)
        {
            Dictionary<string, object> result = null;
            if (motion != null && motion is BlendTree) {
                BlendTree tree = motion as BlendTree;
                List<Dictionary<string, object>> children = new List<Dictionary<string, object>>();
                if (tree.children != null && tree.children.Length > 0) {
                    foreach (var child in tree.children) {
                        float childApparentSpeed = 0.0f;
                        float childAverageAngularSpeed = 0.0f;
                        float childAverageDuration = 0.0f;
                        float[] childAverageSpeed = new float[] { 0.0f, 0.0f, 0.0f };
                        if (child.motion != null) {
                            childApparentSpeed = child.motion.apparentSpeed;
                            childAverageAngularSpeed = child.motion.averageAngularSpeed;
                            childAverageDuration = child.motion.averageDuration;
                            childAverageSpeed = child.motion.averageSpeed.ToFloat();
                        }
                        // Temp: Using Rounded Threadhold And Position Values
                        Dictionary<string, object> childInfo = new Dictionary<string, object>();
                        childInfo.Add("cycleOffset", child.cycleOffset); // Note: Is Normalized Value
                        childInfo.Add("directBlendParameter", child.directBlendParameter);
                        childInfo.Add("apparentSpeed", childApparentSpeed);
                        childInfo.Add("averageAngularSpeed", childAverageAngularSpeed);
                        childInfo.Add("averageDuration", childAverageDuration);
                        childInfo.Add("averageSpeed", childAverageSpeed);
                        childInfo.Add("mirror", child.mirror);
                        childInfo.Add("type", (child.motion != null && child.motion is BlendTree) ? (int)MotionType.Tree : (int)MotionType.Clip);
                        childInfo.Add("motion", (child.motion != null) ? child.motion.name : String.Empty);
                        childInfo.Add("position", new float[] { (float)Math.Round(child.position.x, 2), (float)Math.Round(child.position.y, 2) });
                        childInfo.Add("threshold", (float)Math.Round(child.threshold, 2));
                        childInfo.Add("timescale", child.timeScale);
                        childInfo.Add("subtree", Tools.ParseBlendingTrees(animator, index, child.motion, statename));
                        children.Add(childInfo);
                    }
                }
                Dictionary<string, object> treeInfo = new Dictionary<string, object>();
                treeInfo.Add("name", tree.name);
                treeInfo.Add("state", statename);
                treeInfo.Add("children", children);
                treeInfo.Add("indexLayer", index);
                treeInfo.Add("apparentSpeed", tree.apparentSpeed);
                treeInfo.Add("averageAngularSpeed", tree.averageAngularSpeed);
                treeInfo.Add("averageDuration", tree.averageDuration);
                treeInfo.Add("averageSpeed", tree.averageSpeed.ToFloat());
                treeInfo.Add("blendParameterX", tree.blendParameter);
                treeInfo.Add("blendParameterY", tree.blendParameterY);
                treeInfo.Add("blendType", (int)tree.blendType);
                treeInfo.Add("isAnimatorMotion", !tree.legacy);
                treeInfo.Add("isHumanMotion", tree.isHumanMotion);
                treeInfo.Add("isLooping", tree.isLooping);
                treeInfo.Add("minThreshold", tree.minThreshold);
                treeInfo.Add("maxThreshold", tree.maxThreshold);
                treeInfo.Add("useAutomaticThresholds", tree.useAutomaticThresholds);
                result = treeInfo;
            }
            return result;
        }

        public static void DumpCustomCurves(Animator animator, MachineState state)
        {
            // TODO: Attach Machine State Curves
            // TODO: Remove Custom Curve Keys From Client DOM
            //state.customCurves.Add();
            /*
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
            
                var aparams = Tools.GetAnimationParameters(animator);
                var customkeys = new Dictionary<string, List<BabylonAnimationKey>>();
                if (cycled == false) {
                    // Animation Clip Custom Curves
                    if (aparams != null && aparams.Count > 0) {
                        foreach (var aparam in aparams) {
                            if (aparam.curve == true) {
                                var curve = Tools.GetAnimationCurve(clip, aparam.name);
                                if (curve != null) {
                                    List<BabylonAnimationKey> curveKeys = null;
                                    if (customkeys.ContainsKey(aparam.name)) {
                                        curveKeys = customkeys[aparam.name];
                                    } else {
                                        curveKeys = new List<BabylonAnimationKey>();
                                        customkeys.Add(aparam.name, curveKeys);
                                    }
                                    float lastValue = Tools.GetAnimationCurveLast(curveKeys);
                                    float curveValue = Tools.GetAnimationCurveValue(frameIndex, curve.keys, lastValue); 
                                    curveKeys.Add(new BabylonAnimationKey {
                                        frame = frameIndex,
                                        values = new[] { curveValue }
                                    });
                                } else {
                                    string empties = "EmptyKeys";
                                    List<BabylonAnimationKey> curveKeys = null;
                                    if (customkeys.ContainsKey(empties)) {
                                        curveKeys = customkeys[empties];
                                    } else {
                                        curveKeys = new List<BabylonAnimationKey>();
                                        customkeys.Add(empties, curveKeys);
                                    }
                                    curveKeys.Add(new BabylonAnimationKey {
                                        frame = frameIndex,
                                        values = new[] { 0.0f }
                                    });
                                }
                            }
                        }
                    }
                }
                //
                // Format Custom Curve Keys
                //
                if (customkeys != null && customkeys.Keys.Count > 0) {
                    foreach (var key in customkeys.Keys) {
                        var cxkeys = customkeys[key];
                        property = "none";
                        if (cxkeys.Count > 0) {
                            property = "metadata.state.floats." + key;
                            anims.Add(new BabylonAnimation
                            {
                                dataType = (int)BabylonAnimation.DataType.Float,
                                name = property + " animation",
                                keys = cxkeys.ToArray(),
                                framePerSecond = frameRate,
                                enableBlending = false,
                                blendingSpeed = 0.0f,
                                loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                                property = property
                            });
                        }
                    }
                }
             */
        }

        public static float ComputeBlendingSpeed(float rate, float duration)
        {
            return 1 / (rate * duration);
        }

        public static string LoadTextAsset(string filename)
        {
            return File.ReadAllText(filename);
        }

        public static List<Texture> GetTextures(this Material source)
        {
            Shader shader = source.shader;
            List<Texture> materials = new List<Texture>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    materials.Add(source.GetTexture(ShaderUtil.GetPropertyName(shader, i)));
                }
            }
            return materials;
        }
        
        public static TextureInfo[] ExportCubemap(Cubemap cubemap, string filename, BabylonImageFormat imageFormat, BabylonSplitterOptions splitterOptions = null)
        {
            string extension = ".jpg";
            if (imageFormat == BabylonImageFormat.PNG) extension = ".png";
            string inputFile = AssetDatabase.GetAssetPath(cubemap);
            BabylonTextureImporter importTool = new BabylonTextureImporter(inputFile);
            importTool.SetCubemap();
            List<TextureInfo> faces = new List<TextureInfo>();
            foreach (CubemapFace face in Enum.GetValues(typeof(CubemapFace))) {
                var faceTexturePath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
                switch (face) {
                    case CubemapFace.PositiveX:
                        faceTexturePath += ("_px" + extension);
                        break;
                    case CubemapFace.NegativeX:
                        faceTexturePath += ("_nx" + extension);
                        break;
                    case CubemapFace.PositiveY:
                        faceTexturePath += ("_py" + extension);
                        break;
                    case CubemapFace.NegativeY:
                        faceTexturePath += ("_ny" + extension);
                        break;
                    case CubemapFace.PositiveZ:
                        faceTexturePath += ("_pz" + extension);
                        break;
                    case CubemapFace.NegativeZ:
                        faceTexturePath += ("_nz" + extension);
                        break;
                    default:
                        continue;
                }
                if (splitterOptions != null && splitterOptions.progress == true) {
                    ExporterWindow.ReportProgress(1, "Baking cubemap texture face: " + Path.GetFileName(faceTexturePath));
                }
                var faceTexture = new Texture2D(cubemap.width, cubemap.height, TextureFormat.RGBA32, false);
                Tools.CopyCubePixels(cubemap, face, ref faceTexture);
                if (splitterOptions != null && splitterOptions.resolution > 0) {
                    faceTexture.Scale(splitterOptions.resolution, splitterOptions.resolution, splitterOptions.bilinear);
                }
                faceTexture.WriteImage(faceTexturePath, imageFormat);
                faces.Add(new TextureInfo { filename = faceTexturePath, texture = faceTexture });
            }
            return (faces.Count > 0) ? faces.ToArray() : null;
        }

        public static void CopyCubePixels(Cubemap cubemap, CubemapFace face, ref Texture2D destination)
        {
            Color[] pixels = cubemap.GetPixels(face);
            destination.SetPixels(pixels);
            destination.Apply();
            destination = Tools.FlipTexture(destination);
        }

        private static bool FreeImageMessageHandled = false;
        public static bool IsFreeImageAvailable()
        {
            bool result = FreeImage.IsAvailable();
            if (result && Tools.FreeImageMessageHandled == false) {
    			FreeImageEngine.Message += new OutputMessageFunction(Tools.FreeImageMessages);
                FreeImageMessageHandled = true;
            }
            return result;
        }

		public static void FreeImageMessages(FREE_IMAGE_FORMAT fif, string message)
		{
			string error = String.Format("[FreeImage] {0}: {1}", fif.ToString(), message);
            UnityEngine.Debug.LogError(error);
		}

        public static void ConvertFreeImage(string sourceFile, FREE_IMAGE_FORMAT sourceFormat, string destFile, FREE_IMAGE_FORMAT destFormat,
        FREE_IMAGE_TYPE imageType = FREE_IMAGE_TYPE.FIT_UNKNOWN, bool scaleLinear = true,
        FREE_IMAGE_COLOR_DEPTH colorDepth = FREE_IMAGE_COLOR_DEPTH.FICD_AUTO, 
        FREE_IMAGE_LOAD_FLAGS loadFlags = FREE_IMAGE_LOAD_FLAGS.DEFAULT, 
        FREE_IMAGE_SAVE_FLAGS saveFlags = FREE_IMAGE_SAVE_FLAGS.DEFAULT,
        double rotateAngle = 0.0, bool flipHorizontal = false, bool flipVertical = false,
        int rescaleWidth = 0, int rescaleHeight = 0, FREE_IMAGE_FILTER rescaleFilter = FREE_IMAGE_FILTER.FILTER_LANCZOS3,
        bool toneMapping = false, FREE_IMAGE_TMO toneMappingMode = FREE_IMAGE_TMO.FITMO_DRAGO03, double toneMappingParam1 = 0.0, double toneMappingParam2 = 0.0)
        {
            FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            FileStream destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write);
            try {
                Tools.ConvertFreeImage(sourceStream, sourceFormat, destStream, destFormat, imageType, scaleLinear, colorDepth, loadFlags, saveFlags, rotateAngle, flipHorizontal, flipVertical, rescaleWidth, rescaleHeight, rescaleFilter, toneMapping, toneMappingMode, toneMappingParam1, toneMappingParam2);
            } catch(Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                if (sourceStream != null) sourceStream.Close();
                if (destStream != null) destStream.Close();
            }
        }
        
        public static void ConvertFreeImage(Stream sourceStream, FREE_IMAGE_FORMAT sourceFormat, Stream destStream, FREE_IMAGE_FORMAT destFormat, 
        FREE_IMAGE_TYPE imageType = FREE_IMAGE_TYPE.FIT_UNKNOWN, bool scaleLinear = true,
        FREE_IMAGE_COLOR_DEPTH colorDepth = FREE_IMAGE_COLOR_DEPTH.FICD_AUTO, 
        FREE_IMAGE_LOAD_FLAGS loadFlags = FREE_IMAGE_LOAD_FLAGS.DEFAULT, 
        FREE_IMAGE_SAVE_FLAGS saveFlags = FREE_IMAGE_SAVE_FLAGS.DEFAULT,
        double rotateAngle = 0.0, bool flipHorizontal = false, bool flipVertical = false,
        int rescaleWidth = 0, int rescaleHeight = 0, FREE_IMAGE_FILTER rescaleFilter = FREE_IMAGE_FILTER.FILTER_LANCZOS3,
        bool toneMapping = false, FREE_IMAGE_TMO toneMappingMode = FREE_IMAGE_TMO.FITMO_DRAGO03, double toneMappingParam1 = 0.0, double toneMappingParam2 = 0.0)
        {
            if (Tools.IsFreeImageAvailable()) {
                try {
                    FIBITMAP dib = FreeImage.LoadFromStream(sourceStream, loadFlags, ref sourceFormat);
                    if (!dib.IsNull) {
                        if (rescaleWidth > 0 && rescaleHeight > 0) {
                            FIBITMAP scale = FreeImage.Rescale(dib, rescaleWidth, rescaleHeight, rescaleFilter);
                            FreeImage.UnloadEx(ref dib);
                            dib = scale;
                        }
                        if (rotateAngle != 0.0) {
                            FIBITMAP rotate = FreeImage.Rotate(dib, rotateAngle);
                            FreeImage.UnloadEx(ref dib);
                            dib = rotate;
                        }
                        if (flipHorizontal == true) {
                            FreeImage.FlipHorizontal(dib);
                        }
                        if (flipVertical == true) {
                            FreeImage.FlipVertical(dib);
                        }
                        if (toneMapping) {
                            FIBITMAP map = FreeImage.ToneMapping(dib, toneMappingMode, toneMappingParam1, toneMappingParam2);
                            FreeImage.UnloadEx(ref dib);
                            if (!FreeImage.SaveToStream(ref map, destStream, destFormat, saveFlags, colorDepth, true)) {
                                FreeImage.UnloadEx(ref map);
                            }
                        } else if (imageType != FREE_IMAGE_TYPE.FIT_UNKNOWN) {
                            FIBITMAP img = FreeImage.ConvertToType(dib, imageType, scaleLinear);
                            FreeImage.UnloadEx(ref dib);
                            if (!FreeImage.SaveToStream(ref img, destStream, destFormat, saveFlags, colorDepth, true)) {
                                FreeImage.UnloadEx(ref img);
                            }
                        } else {
                            if (!FreeImage.SaveToStream(ref dib, destStream, destFormat, saveFlags, colorDepth, true)) {
                                FreeImage.UnloadEx(ref dib);
                            }
                        }
                    } else {
                        throw new Exception("[FreeImage]: Failed to load image from stream");
                    }                    
                } catch(Exception ex) {
                    UnityEngine.Debug.LogException(ex);
                }
            } else {
                UnityEngine.Debug.LogError("[FreeImage] Library not available.");
            }
        }

        public static void WriteFreeImage(ImageInfo info, int width, int height, Color[] pixels, string destFile, FREE_IMAGE_FORMAT destFormat,
        FREE_IMAGE_COLOR_DEPTH colorDepth = FREE_IMAGE_COLOR_DEPTH.FICD_AUTO, FREE_IMAGE_SAVE_FLAGS saveFlags = FREE_IMAGE_SAVE_FLAGS.DEFAULT,
        double rotateAngle = 0.0, bool flipHorizontal = false, bool flipVertical = false,
        int rescaleWidth = 0, int rescaleHeight = 0, FREE_IMAGE_FILTER rescaleFilter = FREE_IMAGE_FILTER.FILTER_LANCZOS3, ColorCorrection colorCorrection = ColorCorrection.None)
        {
            FileStream destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write);
            try {
                Tools.WriteFreeImage(info, width, height, pixels, destStream, destFormat, colorDepth, saveFlags, rotateAngle, flipHorizontal, flipVertical, rescaleWidth, rescaleHeight, rescaleFilter, colorCorrection);
            } catch(Exception ex) {
                UnityEngine.Debug.LogException(ex);
            } finally {
                if (destStream != null) destStream.Close();
            }
        }
        
        public static void WriteFreeImage(ImageInfo info, int width, int height, Color[] pixels, Stream destStream, FREE_IMAGE_FORMAT destFormat,
        FREE_IMAGE_COLOR_DEPTH colorDepth = FREE_IMAGE_COLOR_DEPTH.FICD_AUTO, FREE_IMAGE_SAVE_FLAGS saveFlags = FREE_IMAGE_SAVE_FLAGS.DEFAULT,
        double rotateAngle = 0.0, bool flipHorizontal = false, bool flipVertical = false,
        int rescaleWidth = 0, int rescaleHeight = 0, FREE_IMAGE_FILTER rescaleFilter = FREE_IMAGE_FILTER.FILTER_LANCZOS3, ColorCorrection colorCorrection = ColorCorrection.None)
        {
            if (Tools.IsFreeImageAvailable()) {
                try {
                    FIBITMAP dib;
                    int bpp = 32;
                    switch (info.pixelFormat) {
                        case PixelFormat.RGBTRIPLE:
                            bpp = 24;
                            break;
                        case PixelFormat.RGBQUAD:
                            bpp = 32;
                            break;
                        case PixelFormat.RGB16:
                            bpp = 48;
                            break;
                        case PixelFormat.RGBA16:
                            bpp = 64;
                            break;
                        case PixelFormat.RGBF:
                            bpp = 96;
                            break;
                        case PixelFormat.RGBAF:
                            bpp = 128;
                            break;
                        default:
                            bpp = 32;
                            break;
                    }
                    if (info.useColorMask == true) {
                        dib = FreeImage.AllocateT(info.freeImageType, width, height, bpp, info.redColorMask, info.greenColorMask, info.blueColorMask);
                    } else {
                        dib = FreeImage.AllocateT(info.freeImageType, width, height, bpp);
                    }
                    int errors = 0;
                    switch (info.pixelFormat) {
                        case PixelFormat.RGBTRIPLE:
                            errors = Tools.WriteFreeImage_RGBTRIPLE(ref dib, width, height, ref pixels);
                            break;
                        case PixelFormat.RGBQUAD:
                            errors = Tools.WriteFreeImage_RGBQUAD(ref dib, width, height, ref pixels);
                            break;
                        case PixelFormat.RGB16:
                            errors = Tools.WriteFreeImage_FIRGB16(ref dib, width, height, ref pixels);
                            break;
                        case PixelFormat.RGBA16:
                            errors = Tools.WriteFreeImage_FIRGBA16(ref dib, width, height, ref pixels);
                            break;
                        case PixelFormat.RGBF:
                            errors = Tools.WriteFreeImage_FIRGBF(ref dib, width, height, ref pixels, colorCorrection);
                            break;
                        case PixelFormat.RGBAF:
                            errors = Tools.WriteFreeImage_FIRGBAF(ref dib, width, height, ref pixels, colorCorrection);
                            break;
                        default:
                            errors = Tools.WriteFreeImage_RGBQUAD(ref dib, width, height, ref pixels);
                            break;
                    }
                    if (errors > 0) {
                        UnityEngine.Debug.LogError("Bad Scan Lines Encountered: " + errors.ToString());
                    }
                    if (rescaleWidth > 0 && rescaleHeight > 0) {
                        FIBITMAP scale = FreeImage.Rescale(dib, rescaleWidth, rescaleHeight, rescaleFilter);
                        FreeImage.UnloadEx(ref dib);
                        dib = scale;
                    }
                    if (rotateAngle != 0.0) {
                        FIBITMAP rotate = FreeImage.Rotate(dib, rotateAngle);
                        FreeImage.UnloadEx(ref dib);
                        dib = rotate;
                    }
                    if (flipHorizontal == true) {
                        FreeImage.FlipHorizontal(dib);
                    }
                    if (flipVertical == true) {
                        FreeImage.FlipVertical(dib);
                    }
                    if (!FreeImage.SaveToStream(ref dib, destStream, destFormat, saveFlags, colorDepth, true)) {
                        FreeImage.UnloadEx(ref dib);
                    }
                } catch (Exception ex) {
                    UnityEngine.Debug.LogException(ex);
                }
            } else {
                UnityEngine.Debug.LogError("[FreeImage] Library not available.");
            }
        }

        public static Color[] ReadFreeImage_RGBTRIPLE(ref FIBITMAP dib, int width, int height)
        {
            int index = 0;
            float factor = 255.0f;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)  {
                Scanline<RGBTRIPLE> line = new Scanline<RGBTRIPLE>(dib, y);
                RGBTRIPLE[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color color = new Color();
                        color.r = (buffer[x].rgbtRed / factor);
                        color.g = (buffer[x].rgbtGreen / factor);
                        color.b = (buffer[x].rgbtBlue / factor);
                        pixels[index] = color;
                        index++;
                    }
                }
            }
            return pixels;
        }

        public static int WriteFreeImage_RGBTRIPLE(ref FIBITMAP dib, int width, int height, ref Color[] pixels)
        {
            int index = 0;
            int errors = 0;
            float factor = 255.0f;
            for (int y = 0; y < height; y++)  {
                Scanline<RGBTRIPLE> line = new Scanline<RGBTRIPLE>(dib, y);
                RGBTRIPLE[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color pixel = pixels[index];
                        buffer[x].rgbtBlue = (byte)(Tools.ClampColorSpace(pixel.r) * factor);
                        buffer[x].rgbtGreen = (byte)(Tools.ClampColorSpace(pixel.g) * factor);
                        buffer[x].rgbtBlue = (byte)(Tools.ClampColorSpace(pixel.b) * factor);
                        index++;
                    }
                } else {
                    errors++;
                }
                line.Data = buffer;
            }
            return errors;
        }

        public static Color[] ReadFreeImage_RGBQUAD(ref FIBITMAP dib, int width, int height)
        {
            int index = 0;
            float factor = 255.0f;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)  {
                Scanline<RGBQUAD> line = new Scanline<RGBQUAD>(dib, y);
                RGBQUAD[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color color = new Color();
                        color.r = (buffer[x].rgbRed / factor);
                        color.g = (buffer[x].rgbGreen / factor);
                        color.b = (buffer[x].rgbBlue / factor);
                        color.a = (buffer[x].rgbReserved / factor);
                        pixels[index] = color;
                        index++;
                    }
                }
            }
            return pixels;
        }

        public static int WriteFreeImage_RGBQUAD(ref FIBITMAP dib, int width, int height, ref Color[] pixels)
        {
            int index = 0;
            int errors = 0;
            float factor = 255.0f;
            for (int y = 0; y < height; y++)  {
                Scanline<RGBQUAD> line = new Scanline<RGBQUAD>(dib, y);
                RGBQUAD[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color pixel = pixels[index];
                        buffer[x].rgbRed = (byte)(Tools.ClampColorSpace(pixel.r) * factor);
                        buffer[x].rgbGreen = (byte)(Tools.ClampColorSpace(pixel.g) * factor);
                        buffer[x].rgbBlue = (byte)(Tools.ClampColorSpace(pixel.b) * factor);
                        buffer[x].rgbReserved = (byte)(Tools.ClampColorSpace(pixel.a) * factor);
                        index++;
                    }
                } else {
                    errors++;
                }
                line.Data = buffer;
            }
            return errors;
        }

        public static Color[] ReadFreeImage_FIRGB16(ref FIBITMAP dib, int width, int height)
        {
            int index = 0;
            float factor = 65535.0f;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)  {
                Scanline<FIRGB16> line = new Scanline<FIRGB16>(dib, y);
                FIRGB16[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color color = new Color();
                        color.r = (buffer[x].red / factor);
                        color.g = (buffer[x].green / factor);
                        color.b = (buffer[x].blue / factor);
                        pixels[index] = color;
                        index++;
                    }
                }
            }
            return pixels;
        }

        public static int WriteFreeImage_FIRGB16(ref FIBITMAP dib, int width, int height, ref Color[] pixels)
        {
            int index = 0;
            int errors = 0;
            float factor = 65535.0f;
            for (int y = 0; y < height; y++)  {
                Scanline<FIRGB16> line = new Scanline<FIRGB16>(dib, y);
                FIRGB16[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color pixel = pixels[index];
                        buffer[x].red = (ushort)(Tools.ClampColorSpace(pixel.r) * factor);
                        buffer[x].green = (ushort)(Tools.ClampColorSpace(pixel.g) * factor);
                        buffer[x].blue = (ushort)(Tools.ClampColorSpace(pixel.b) * factor);
                        index++;
                    }
                } else {
                    errors++;
                }
                line.Data = buffer;
            }
            return errors;
        }

        public static Color[] ReadFreeImage_FIRGBA16(ref FIBITMAP dib, int width, int height)
        {
            int index = 0;
            float factor = 65535.0f;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)  {
                Scanline<FIRGBA16> line = new Scanline<FIRGBA16>(dib, y);
                FIRGBA16[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color color = new Color();
                        color.r = (buffer[x].red / factor);
                        color.g = (buffer[x].green / factor);
                        color.b = (buffer[x].blue / factor);
                        color.a = (buffer[x].alpha / factor);
                        pixels[index] = color;
                        index++;
                    }
                }
            }
            return pixels;
        }

        public static int WriteFreeImage_FIRGBA16(ref FIBITMAP dib, int width, int height, ref Color[] pixels)
        {
            int index = 0;
            int errors = 0;
            float factor = 65535.0f;
            for (int y = 0; y < height; y++)  {
                Scanline<FIRGBA16> line = new Scanline<FIRGBA16>(dib, y);
                FIRGBA16[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color pixel = pixels[index];
                        buffer[x].red = (ushort)(Tools.ClampColorSpace(pixel.r) * factor);
                        buffer[x].green = (ushort)(Tools.ClampColorSpace(pixel.g) * factor);
                        buffer[x].blue = (ushort)(Tools.ClampColorSpace(pixel.b) * factor);
                        buffer[x].alpha = (ushort)(Tools.ClampColorSpace(pixel.a) * factor);
                        index++;
                    }
                } else {
                    errors++;
                }
                line.Data = buffer;
            }
            return errors;
        }

        public static Color[] ReadFreeImage_FIRGBF(ref FIBITMAP dib, int width, int height, ColorCorrection correction = ColorCorrection.None)
        {
            int index = 0;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)  {
                Scanline<FIRGBF> line = new Scanline<FIRGBF>(dib, y);
                FIRGBF[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color color = new Color();
                        if (correction == ColorCorrection.Gamma) {
                            color.r = Tools.GammaToLinearSpace(buffer[x].red);
                            color.g = Tools.GammaToLinearSpace(buffer[x].green);
                            color.b = Tools.GammaToLinearSpace(buffer[x].blue);
                        } else if (correction == ColorCorrection.Linear) {
                            color.r = Tools.LinearToGammaSpace(buffer[x].red);
                            color.g = Tools.LinearToGammaSpace(buffer[x].green);
                            color.b = Tools.LinearToGammaSpace(buffer[x].blue);
                        } else {
                            color.r = buffer[x].red;
                            color.g = buffer[x].green;
                            color.b = buffer[x].blue;
                        }
                        pixels[index] = color;
                        index++;
                    }
                }
            }
            return pixels;
        }

        public static int WriteFreeImage_FIRGBF(ref FIBITMAP dib, int width, int height, ref Color[] pixels, ColorCorrection correction = ColorCorrection.None)
        {
            int index = 0;
            int errors = 0;
            for (int y = 0; y < height; y++)  {
                Scanline<FIRGBF> line = new Scanline<FIRGBF>(dib, y);
                FIRGBF[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color pixel = pixels[index];
                        if (correction == ColorCorrection.Gamma) {
                            buffer[x].red = Tools.GammaToLinearSpace(pixel.r);
                            buffer[x].green = Tools.GammaToLinearSpace(pixel.g);
                            buffer[x].blue = Tools.GammaToLinearSpace(pixel.b);
                        } else if (correction == ColorCorrection.Linear) {
                            buffer[x].red = Tools.LinearToGammaSpace(pixel.r);
                            buffer[x].green = Tools.LinearToGammaSpace(pixel.g);
                            buffer[x].blue = Tools.LinearToGammaSpace(pixel.b);
                        } else {
                            buffer[x].red = pixel.r;
                            buffer[x].green = pixel.g;
                            buffer[x].blue = pixel.b;
                        }
                        index++;
                    }
                } else {
                    errors++;
                }
                line.Data = buffer;
            }
            return errors;
        }

        public static Color[] ReadFreeImage_FIRGBAF(ref FIBITMAP dib, int width, int height, ColorCorrection correction = ColorCorrection.None)
        {
            int index = 0;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)  {
                Scanline<FIRGBAF> line = new Scanline<FIRGBAF>(dib, y);
                FIRGBAF[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color color = new Color();
                        if (correction == ColorCorrection.Gamma) {
                            color.r = Tools.GammaToLinearSpace(buffer[x].red);
                            color.g = Tools.GammaToLinearSpace(buffer[x].green);
                            color.b = Tools.GammaToLinearSpace(buffer[x].blue);
                            color.a = Tools.GammaToLinearSpace(buffer[x].alpha);
                        } else if (correction == ColorCorrection.Linear) {
                            color.r = Tools.LinearToGammaSpace(buffer[x].red);
                            color.g = Tools.LinearToGammaSpace(buffer[x].green);
                            color.b = Tools.LinearToGammaSpace(buffer[x].blue);
                            color.a = Tools.LinearToGammaSpace(buffer[x].alpha);
                        } else {
                            color.r = buffer[x].red;
                            color.g = buffer[x].green;
                            color.b = buffer[x].blue;
                            color.a = buffer[x].alpha;
                        }
                        pixels[index] = color;
                        index++;
                    }
                }
            }
            return pixels;
        }

        public static int WriteFreeImage_FIRGBAF(ref FIBITMAP dib, int width, int height, ref Color[] pixels, ColorCorrection correction = ColorCorrection.None)
        {
            int index = 0;
            int errors = 0;
            for (int y = 0; y < height; y++)  {
                Scanline<FIRGBAF> line = new Scanline<FIRGBAF>(dib, y);
                FIRGBAF[] buffer = line.Data;
                if (buffer.Length == width) {
                    for (int x = 0; x < width; x++)  {
                        Color pixel = pixels[index];
                        if (correction == ColorCorrection.Gamma) {
                            buffer[x].red = Tools.GammaToLinearSpace(pixel.r);
                            buffer[x].green = Tools.GammaToLinearSpace(pixel.g);
                            buffer[x].blue = Tools.GammaToLinearSpace(pixel.b);
                            buffer[x].alpha = Tools.GammaToLinearSpace(pixel.a);
                        } else if (correction == ColorCorrection.Linear) {
                            buffer[x].red = Tools.LinearToGammaSpace(pixel.r);
                            buffer[x].green = Tools.LinearToGammaSpace(pixel.g);
                            buffer[x].blue = Tools.LinearToGammaSpace(pixel.b);
                            buffer[x].alpha = Tools.LinearToGammaSpace(pixel.a);
                        } else {
                            buffer[x].red = pixel.r;
                            buffer[x].green = pixel.g;
                            buffer[x].blue = pixel.b;
                            buffer[x].alpha = pixel.a;
                        }
                        index++;
                    }
                } else {
                    errors++;
                }
                line.Data = buffer;
            }
            return errors;
        }

        public static float ClampColorSpace(float f)
        {
            return Mathf.Clamp(f, 0.0f, 1.0f);
        }

        public static float GammaToLinearSpace(float f)
        {
            //return Mathf.Pow(f, 2.2f);
            return Mathf.GammaToLinearSpace(f);
        }

        public static float LinearToGammaSpace(float f)
        {
            //return Mathf.Pow(f, 1f / 2.2f);
            return Mathf.LinearToGammaSpace(f);
        }

        public static float GetTextureScale(float scale)
        {
            return scale * 1.0f; // No Change
        }

        public static float GetTerrainScale(float scale)
        {
            return scale * ExporterWindow.exportationOptions.TerrainScaleFactor;
        }

        public static Color GetAmbientColor()
        {
            return RenderSettings.ambientSkyColor;
        }

        public static Color GetGroundColor()
        {
            return RenderSettings.ambientGroundColor;
        }

        public static float GetAmbientIntensity(SceneController controller)
        {
            float ambientScale = (controller != null) ? controller.lightingOptions.ambientScale : ExporterWindow.DefaultAmbientScale;
            return (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox && RenderSettings.skybox != null) ? (RenderSettings.ambientIntensity * ambientScale) : (1.0f * ambientScale);
        }

        public static float GetReflectIntensity(SceneController controller)
        {
            float reflectScale = (controller != null) ? controller.lightingOptions.reflectionScale : ExporterWindow.DefaultReflectScale;
            return RenderSettings.reflectionIntensity * reflectScale;
        }

        public static void SetTextureWrapMode(BabylonTexture babylonTexture, Texture unityTexture)
        {
            if (babylonTexture != null) {
                int wrapMode = (unityTexture != null && unityTexture.wrapMode == TextureWrapMode.Clamp) ? 0 : 1;
                babylonTexture.wrapU = (wrapMode == 0) ? BabylonTexture.AddressMode.CLAMP_ADDRESSMODE : BabylonTexture.AddressMode.WRAP_ADDRESSMODE;
                babylonTexture.wrapV = (wrapMode == 0) ? BabylonTexture.AddressMode.CLAMP_ADDRESSMODE : BabylonTexture.AddressMode.WRAP_ADDRESSMODE;
            }
        }

        public static Texture2D FlipTexture(Texture2D original)
        {
            Texture2D flipped = new Texture2D(original.width, original.height, original.format, false);
            for (int i = 0; i < original.width; i++) {
                for (int j = 0; j < original.height; j++) {
                    flipped.SetPixel(i, original.height - j - 1, original.GetPixel(i, j));
                }
            }
            flipped.Apply();
            return flipped;
        }

        public static Texture2D RotateTexture(Texture2D tex, float angle)
        {
            Texture2D rotImage = new Texture2D(tex.width, tex.height);
            int  x,y;
            float x1, y1, x2,y2;
    
            int w = tex.width;
            int h = tex.height;
            float x0 = Tools.Rotate_X(angle, -w/2.0f, -h/2.0f) + w/2.0f;
            float y0 = Tools.Rotate_Y(angle, -w/2.0f, -h/2.0f) + h/2.0f;
    
            float dx_x = Tools.Rotate_X(angle, 1.0f, 0.0f);
            float dx_y = Tools.Rotate_Y(angle, 1.0f, 0.0f);
            float dy_x = Tools.Rotate_X(angle, 0.0f, 1.0f);
            float dy_y = Tools.Rotate_Y(angle, 0.0f, 1.0f);
        
            x1 = x0;
            y1 = y0;
    
            for (x = 0; x < tex.width; x++) {
                x2 = x1;
                y2 = y1;
                for ( y = 0; y < tex.height; y++) {
                    //rotImage.SetPixel (x1, y1, Color.clear);          
                    x2 += dx_x;//Tools.Rotate_X(angle, x1, y1);
                    y2 += dx_y;//Tools.Rotate_Y(angle, x1, y1);
                    rotImage.SetPixel((int)Mathf.Floor(x), (int)Mathf.Floor(y), Tools.GetPixel(tex,x2, y2));
                }
    
                x1 += dy_x;
                y1 += dy_y;
            }
    
            rotImage.Apply();
            return rotImage;
        }

        private static Color GetPixel(Texture2D tex, float x, float y)
        {
            Color pix;
            int x1 = (int)Mathf.Floor(x);
            int y1 = (int)Mathf.Floor(y);
    
            if(x1 > tex.width || x1 < 0 ||
            y1 > tex.height || y1 < 0) {
                pix = Color.clear;
            } else {
                pix = tex.GetPixel(x1,y1);
            }
        
            return pix;
        }
    
        private static float Rotate_X(float angle, float x, float y) {
            float cos = Mathf.Cos(angle/180.0f*Mathf.PI);
            float sin = Mathf.Sin(angle/180.0f*Mathf.PI);
            return (x * cos + y * (-sin));
        }
        private static float Rotate_Y(float angle, float x, float y) {
            float cos = Mathf.Cos(angle/180.0f*Mathf.PI);
            float sin = Mathf.Sin(angle/180.0f*Mathf.PI);
            return (x * sin + y * cos);
        }

        public static void RemoveReferencePaths(string filename)
        {
            if (File.Exists(filename)) {
                List<string> outputBuffer = new List<string>();
                string[] lineBuffer = File.ReadAllLines(filename);
                foreach (var line in lineBuffer)
                {
                    if (!line.Trim().StartsWith("/// <reference path", StringComparison.OrdinalIgnoreCase))
                    {
                        outputBuffer.Add(line);
                    }
                }
                File.WriteAllLines(filename, outputBuffer.ToArray());
            }
        }

        public static List<string> GetTextureNames(this Material source)
        {
            Shader shader = source.shader;
            List<string> names = new List<string>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    names.Add(ShaderUtil.GetPropertyName(shader, i));
                }
            }
            return names;
        }

        public static List<string> GetFloatNames(this Material source)
        {
            Shader shader = source.shader;
            List<string> names = new List<string>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.Float)
                {
                    names.Add(ShaderUtil.GetPropertyName(shader, i));
                }
            }
            return names;
        }

        public static List<string> GetRangeNames(this Material source)
        {
            Shader shader = source.shader;
            List<string> names = new List<string>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.Range)
                {
                    names.Add(ShaderUtil.GetPropertyName(shader, i));
                }
            }
            return names;
        }

        public static List<string> GetColorNames(this Material source)
        {
            Shader shader = source.shader;
            List<string> names = new List<string>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.Color)
                {
                    names.Add(ShaderUtil.GetPropertyName(shader, i));
                }
            }
            return names;
        }

        public static List<string> GetVectorNames(this Material source)
        {
            Shader shader = source.shader;
            List<string> names = new List<string>();
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.Vector)
                {
                    names.Add(ShaderUtil.GetPropertyName(shader, i));
                }
            }
            return names;
        }

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public static int GetMaxBoneInfluencers()
        {
            int result = 4;
            if (QualitySettings.blendWeights == BlendWeights.OneBone) result = 1;
            else if (QualitySettings.blendWeights == BlendWeights.TwoBones) result = 2;
            return result;
        }

        public static bool ValidateProjectPlatform()
        {
            bool result = false;
            #if UNITY_STANDALONE
                result = true;
            #else
                UnityEngine.Debug.LogWarning("Unity Standalone Platform Not Detected.");
            #endif            
            if (result == false)
            {
                if (ExporterWindow.ShowMessage("WARNING: Unity Standalone platform not selected.", "Babylon.js", "Continue", "Cancel"))
                {
                    result = true;
                }
            }
            return result;
        }

        public static void ValidateProjectLayers()
        {
            bool updated = false;
            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = manager.FindProperty("layers");
            SerializedProperty statics = layers.GetArrayElementAtIndex(ExporterWindow.StaticIndex);
            SerializedProperty prefabs = layers.GetArrayElementAtIndex(ExporterWindow.PrefabIndex);
            if (statics != null)
            {
                if (String.IsNullOrEmpty(statics.stringValue) || statics.stringValue.Equals(ExporterWindow.StaticLabel, StringComparison.Ordinal) == false)
                {
                    statics.stringValue = ExporterWindow.StaticLabel;
                    updated = true;
                }
            }
            if (prefabs != null)
            {
                if (String.IsNullOrEmpty(prefabs.stringValue) || prefabs.stringValue.Equals(ExporterWindow.PrefabLabel, StringComparison.Ordinal) == false)
                {
                    prefabs.stringValue = ExporterWindow.PrefabLabel;
                    updated = true;
                }
            }
            if (updated)
            {
                manager.ApplyModifiedPropertiesWithoutUndo();
                UnityEngine.Debug.LogWarning("Updated default project layers");
            }
        }

        public static void ValidateAssetFolders(string directory)
        {
            if (!String.IsNullOrEmpty(directory))
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                string geometry = Path.Combine(directory, "Geometry");
                if (!Directory.Exists(geometry)) Directory.CreateDirectory(geometry);
                string materials = Path.Combine(directory, "Materials");
                if (!Directory.Exists(materials)) Directory.CreateDirectory(materials);
                string textures = Path.Combine(directory, "Textures");
                if (!Directory.Exists(textures)) Directory.CreateDirectory(textures);
            }
        }

        public static void EnableRemoteCertificates()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(delegate { return true; });
        }

        public static bool DownloadFile(string url, string dest)
        {
            bool result = false;
            string tempfile = Path.GetTempFileName();
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    webClient.DownloadFile(url, tempfile);
                }
            }
            catch (System.Exception ex)
            {
                string msg = System.String.Format("Failed to download: {0} - {1}", url, ex.Message);
                UnityEngine.Debug.LogWarning(msg);
            }
            finally
            {
                FileInfo info = new FileInfo(tempfile);
                result = (info.Exists && info.Length > 0);
                if (result == true)
                {
                    File.Copy(tempfile, dest, true);
                }
                try { info.Delete(); } catch { }
            }
            return result;
        }

        public static void PrecompressFile(string source, string dest, int packets = 0)
        {
            Stream sourceFile = new FileStream(source, FileMode.Open, FileAccess.Read);
            Stream destFile = new FileStream(dest, FileMode.Create, FileAccess.Write);
            Tools.PrecompressStream(sourceFile, destFile, packets);
        }

        public static void PrecompressStream(Stream source, Stream dest, int packets = 0)
        {
            int packetSize = (packets > 0) ? packets : 1024 * 32;
            try {
                source.CopyTo(dest, CopyToOptions.FlushFinal, CompressionMode.Compress, DecompressionMethods.GZip, packetSize);
            } finally {
                try{ source.Close(); }catch{}
                try{ dest.Close(); }catch{}
            }
        }

        public static void CreatePrefabAsset(GameObject prefabObject, string prefabPath)
        {
            if (prefabObject != null && !String.IsNullOrEmpty(prefabPath))
            {
                UnityEngine.Object prefabAsset = PrefabUtility.CreateEmptyPrefab(prefabPath);
                PrefabUtility.ReplacePrefab(prefabObject, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
                AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
            }
        }

        public static SceneController GetSceneController()
        {
            SceneController result = null;
            var gameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            if (gameObjects != null && gameObjects.Length > 0)
            {
                foreach (var gameObject in gameObjects)
                {
                    var controller = gameObject.GetComponent<SceneController>();
                    if (controller != null)
                    {
                        result = controller;
                        break;
                    }
                }
            }
            return result;
        }

        public static string FormatBase64(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        public static string FormatSafePath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string MakeRelativePath(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            return referenceUri.MakeRelativeUri(fileUri).ToString();
        }

        public static string MinifyJavascriptCode(string script, string name)
        {
            if (ExporterWindow.exportationOptions.PrettyPrintExport == true || ExporterWindow.exportationOptions.MinifyScriptFiles == false) return script;
            ExporterWindow.ReportProgress(1, "Minifying project script: " + name + "... This may take a while.");
            var minifier = new Minifier();
            var settings = new CodeSettings();
            settings.EvalTreatment = EvalTreatment.MakeAllSafe;
            string min_script = minifier.MinifyJavaScript(script, settings);
            if (minifier.Errors.Count > 0)
            {
                foreach (string error in minifier.Errors)
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        UnityEngine.Debug.LogError("Javascript Minifier: " + error);
                    }
                }
            }
            return min_script;
        }
        
        public static float CalculateCameraDistance(Camera camera, float percent, LevelDistance distance = null)
        {
            float nearClipingPlane = (camera != null) ? camera.nearClipPlane : 0.3f;
            float farClipingPlane = (camera != null) ? camera.farClipPlane : 1000.0f;
            return Tools.CalculateCameraDistance(nearClipingPlane, farClipingPlane, percent, distance);
        }

        public static int CalculateCameraDistance(float near, float far, float percent, LevelDistance distance = null)
        {
            float cameraDistanceFactor = (distance != null) ? distance.cameraDistanceFactor : ExporterWindow.exportationOptions.CameraDistanceFactor;
            return (int)(Tools.Denormalize(percent, near, far) * cameraDistanceFactor);
        }

        public static List<I> FindObjectsOfInterface<I>() where I : class
        {
            MonoBehaviour[] monoBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            List<I> list = new List<I>();

            foreach (MonoBehaviour behaviour in monoBehaviours)
            {

                I component = behaviour.GetComponent(typeof(I)) as I;

                if (component != null)
                {
                    list.Add(component);
                }
            }

            return list;
        }

        public static T[] GetAssetsOfType<T>(string fileExtension) where T : UnityEngine.Object
        {
            List<T> tempObjects = new List<T>();
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
            FileInfo[] goFileInfo = directory.GetFiles("*" + fileExtension, SearchOption.AllDirectories);

            int i = 0; int goFileInfoLength = goFileInfo.Length;
            FileInfo tempGoFileInfo; string tempFilePath;
            T tempGO;
            for (; i < goFileInfoLength; i++)
            {
                tempGoFileInfo = goFileInfo[i];
                if (tempGoFileInfo == null)
                    continue;

                tempFilePath = tempGoFileInfo.FullName;
                tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");
                tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(T)) as T;
                if (tempGO == null)
                {
                    continue;
                }
                else if (!(tempGO is T))
                {
                    continue;
                }

                tempObjects.Add(tempGO);
            }

            return tempObjects.ToArray();
        }

        public static T GetReflectionField<T>(object source, string name)
        {
            object result = null;
            var field = source.GetType().GetField(name, Tools.FullBinding);
            if (field != null)
            {
                result = field.GetValue(source);
            }
            return (T)result;
        }

        public static T GetReflectionProperty<T>(object source, string name)
        {
            object result = null;
            var property = source.GetType().GetProperty(name, Tools.FullBinding);
            if (property != null)
            {
                result = property.GetGetMethod(true).Invoke(source, null);
            }
            return (T)result;
        }

        public static void SetReflectionField(object source, string name, object value)
        {
            var field = source.GetType().GetField(name, Tools.FullBinding);
            if (field != null)
            {
                field.SetValue(source, value);
            }
        }

        public static void SetReflectionProperty(object source, string name, object value)
        {
            var property = source.GetType().GetProperty(name, Tools.FullBinding);
            if (property != null)
            {
                property.GetSetMethod(true).Invoke(source, new object[] { value });
            }
        }

        public static void CallReflectionMethod(object source, string method, params object[] args)
        {
            Tools.CallReflectionMethod<object>(source, method, args);
        }

        public static T CallReflectionMethod<T>(object source, string method, params object[] args)
        {
            object result = null;
            var caller = source.GetType().GetMethod(method, Tools.FullBinding);
            if (caller != null)
            {
                result = caller.Invoke(source, args);
            }
            return (T)result;
        }

        public static T GetStaticReflectionField<T>(Type type, string name)
        {
            object result = null;
            var field = type.GetField(name, Tools.FullBinding);
            if (field != null)
            {
                result = field.GetValue(null);
            }
            return (T)result;
        }
        public static T GetStaticReflectionProperty<T>(Type type, string name)
        {
            object result = null;
            var property = type.GetProperty(name, Tools.FullBinding);
            if (property != null)
            {
                result = property.GetGetMethod(true).Invoke(null, null);
            }
            return (T)result;
        }

        public static void SetStaticReflectionField(Type type, string name, object value)
        {
            var field = type.GetField(name, Tools.FullBinding);
            if (field != null)
            {
                field.SetValue(null, value);
            }
        }

        public static void SetStaticReflectionProperty(Type type, string name, object value)
        {
            var property = type.GetProperty(name, Tools.FullBinding);
            if (property != null)
            {
                property.GetSetMethod(true).Invoke(null, new object[] { value });
            }
        }

        public static void CallStaticReflectionMethod(Assembly assembly, string type, string method, params object[] args)
        {
            Type result = assembly.GetType(type);
            Tools.CallStaticReflectionMethod<object>(result, method, args);
        }

        public static void CallStaticReflectionMethod(Type type, string method, params object[] args)
        {
            Tools.CallStaticReflectionMethod<object>(type, method, args);
        }

        public static T CallStaticReflectionMethod<T>(Assembly assembly, string type, string method, params object[] args)
        {
            Type result = assembly.GetType(type);
            return Tools.CallStaticReflectionMethod<T>(result, method, args);
        }

        public static T CallStaticReflectionMethod<T>(Type type, string method, params object[] args)
        {
            object result = null;
            var caller = type.GetMethod(method, Tools.FullBinding);
            if (caller != null)
            {
                result = caller.Invoke(null, args);
            }
            return (T)result;
        }


        public static Type GetTypeFromAllAssemblies(string typeName)
        {
            Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
                        return type;
                }
            }
            return null;
        }

        public static bool IsLightapStatic(this GameObject source)
        {
            return GameObjectUtility.AreStaticEditorFlagsSet(source, StaticEditorFlags.LightmapStatic);
        }

        public static bool IsBatchingStatic(this GameObject source)
        {
            return GameObjectUtility.AreStaticEditorFlagsSet(source, StaticEditorFlags.BatchingStatic);
        }

        public static bool IsNavigationStatic(this GameObject source)
        {
            return GameObjectUtility.AreStaticEditorFlagsSet(source, StaticEditorFlags.NavigationStatic);
        }

        public static bool CopyComponent(Component component)
        {
            return UnityEditorInternal.ComponentUtility.CopyComponent(component);     
        }
        
        public static bool PasteComponentValues(Component component)
        {
            return UnityEditorInternal.ComponentUtility.PasteComponentValues(component);     
        }

        public static bool PasteComponentAsNew(GameObject gameObject)
        {
            return UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameObject);     
        }
        
        public static BabylonMesh GenerateCollisionMesh(Collider collider)
        {
            BabylonMesh collisionMesh = null;
            if (ExporterWindow.exportationOptions.GenerateColliders && collider != null && collider.enabled)
            {
                // Check collider override
                GameObject gameObject = collider.gameObject;
                var meshDetails = gameObject.GetComponent<MeshDetails>();
                if (meshDetails != null && meshDetails.generateCollider == false) return null;
                // Generate collision mesh
                string parent = SceneBuilder.GetID(gameObject);
                int segments = 12;
                BabylonColliderDetail detail = (BabylonColliderDetail)ExporterWindow.exportationOptions.DefaultColliderDetail;
                var collisionData = new UnityMetaData();
                collisionData.objectId = Guid.NewGuid().ToString();
                collisionData.objectName = gameObject.name + "_Metadata";
                if (collider is MeshCollider)
                {
                    var meshCollider = collider as MeshCollider;
                    collisionMesh = new BabylonMesh();
                    collisionMesh.tags = "[MESHCOLLIDER]";
                    // Generate Mesh Collider Geometry
                    if(!meshCollider.sharedMesh)
                    {
                        UnityEngine.Debug.LogWarning(meshCollider.gameObject + " has a Mesh Collider component without a mesh");
                    }
                    else
                    {
                        Tools.GenerateBabylonMeshData(meshCollider.sharedMesh, collisionMesh);
                    }
                    collisionMesh.position = Vector3.zero.ToFloat();
                    collisionMesh.rotation = Vector3.zero.ToFloat();
                    float factorX = 1f, factorY = 1f, factorZ = 1f;
                    if (meshCollider.inflateMesh && meshCollider.skinWidth > 0f)
                    {
                        Vector3 localScale = gameObject.transform.localScale;
                        factorX += (meshCollider.skinWidth / localScale.x);
                        factorY += (meshCollider.skinWidth / localScale.y);
                        factorZ += (meshCollider.skinWidth / localScale.z);
                    }
                    collisionMesh.scaling = new Vector3(factorX, factorY, factorZ).ToFloat();
                    // Export Mesh Collider Metadata
                    collisionData.tagName = "MeshCollider";
                    collisionData.properties.Add("type", "Mesh");
                    collisionData.properties.Add("convex", meshCollider.convex);
                    collisionData.properties.Add("inflateMesh", meshCollider.inflateMesh);
                    collisionData.properties.Add("skinWidth", meshCollider.skinWidth);
                }
                else if (collider is CapsuleCollider)
                {
                    var capsuleCollider = collider as CapsuleCollider;
                    collisionMesh = new BabylonMesh();
                    collisionMesh.tags = "[CAPSULECOLLIDER]";
                    switch (detail)
                    {
                        case BabylonColliderDetail.HighResolution:
                            segments = 24;
                            break;
                        case BabylonColliderDetail.MediumResolution:
                            segments = 12;
                            break;
                        case BabylonColliderDetail.LowResolution:
                            segments = 8;
                            break;
                        case BabylonColliderDetail.VeryLowResolution:
                            segments = 6;
                            break;
                        case BabylonColliderDetail.MinimumResolution:
                            segments = 4;
                            break;
                        default:
                            segments = 8;
                            break;
                    }
                    // Format capsule rotation
                    var capsuleRotation = Vector3.zero;
                    if (capsuleCollider.direction == 0) {
                        // X-Axis - capsuleRotation.z = 90f * (float)Math.PI / 180f;
                        capsuleRotation.z = 90f;
                    } else if (capsuleCollider.direction == 1) {
                        // Y-Axis
                    } else if (capsuleCollider.direction == 2) {
                        // Z-Axis - capsuleRotation.x = 90f * (float)Math.PI / 180f;
                        capsuleRotation.x = 90f;
                    }
                    // Generate Capsule Collider Geometry
                    Mesh sourceMesh = Tools.CreateCapsuleMesh(capsuleCollider.height, capsuleCollider.radius, segments);
                    Mesh capsuleMesh = sourceMesh.Rotate(capsuleRotation);
                    Tools.GenerateBabylonMeshData(capsuleMesh, collisionMesh);
                    collisionMesh.position = new float[3];
                    collisionMesh.position[0] = capsuleCollider.center.x;
                    collisionMesh.position[1] = capsuleCollider.center.y;
                    collisionMesh.position[2] = capsuleCollider.center.z;
                    collisionMesh.rotation = Vector3.zero.ToFloat();
                    collisionMesh.scaling = new Vector3(1, 1, 1).ToFloat();
                    // Export Capsule Collider Metadata
                    collisionData.tagName = "CapsuleCollider";
                    collisionData.properties.Add("type", "Capsule");
                    collisionData.properties.Add("center", capsuleCollider.center.ToFloat());
                    collisionData.properties.Add("radius", capsuleCollider.radius);
                    collisionData.properties.Add("height", capsuleCollider.height);
                    collisionData.properties.Add("direction", capsuleCollider.direction);
                }
                else if (collider is SphereCollider)
                {
                    var sphereCollider = collider as SphereCollider;
                    collisionMesh = new BabylonMesh();
                    collisionMesh.tags = "[SPHERECOLLIDER]";
                    switch (detail)
                    {
                        case BabylonColliderDetail.HighResolution:
                            segments = 24;
                            break;
                        case BabylonColliderDetail.MediumResolution:
                            segments = 12;
                            break;
                        case BabylonColliderDetail.LowResolution:
                            segments = 8;
                            break;
                        case BabylonColliderDetail.VeryLowResolution:
                            segments = 6;
                            break;
                        case BabylonColliderDetail.MinimumResolution:
                            segments = 4;
                            break;
                        default:
                            segments = 8;
                            break;
                    }
                    // Generate Sphere Collider Geometry
                    Mesh sphereMesh = Tools.CreateSphereMesh(sphereCollider.radius, segments, segments);
                    Tools.GenerateBabylonMeshData(sphereMesh, collisionMesh);
                    collisionMesh.position = new float[3];
                    collisionMesh.position[0] = sphereCollider.center.x;
                    collisionMesh.position[1] = sphereCollider.center.y;
                    collisionMesh.position[2] = sphereCollider.center.z;
                    collisionMesh.rotation = Vector3.zero.ToFloat();
                    collisionMesh.scaling = new Vector3(1f, 1f, 1f).ToFloat();
                    // Export Sphere Collider Metadata
                    collisionData.tagName = "SphereCollider";
                    collisionData.properties.Add("type", "Sphere");
                    collisionData.properties.Add("center", sphereCollider.center.ToFloat());
                    collisionData.properties.Add("radius", sphereCollider.radius);
                }
                else if (collider is WheelCollider)
                {
                    var wheelCollider = collider as WheelCollider;
                    collisionMesh = new BabylonMesh();
                    collisionMesh.tags = "[WHEELCOLLIDER]";
                    switch (detail)
                    {
                        case BabylonColliderDetail.HighResolution:
                            segments = 64;
                            break;
                        case BabylonColliderDetail.MediumResolution:
                            segments = 48;
                            break;
                        case BabylonColliderDetail.LowResolution:
                            segments = 32;
                            break;
                        case BabylonColliderDetail.VeryLowResolution:
                            segments = 24;
                            break;
                        case BabylonColliderDetail.MinimumResolution:
                            segments = 12;
                            break;
                        default:
                            segments = 24;
                            break;
                    }
                    // Format capsule rotation
                    var wheelRotation = Vector3.zero;
                    // Z-Axis - wheelRotation.z = 90f * (float)Math.PI / 180f;
                    wheelRotation.z = 90f;
                    // Generate Wheel Collider Geometry
                    Mesh sourceMesh = Tools.CreateWheelMesh(wheelCollider.suspensionDistance, wheelCollider.radius, segments);
                    Mesh wheelMesh = sourceMesh.Rotate(wheelRotation);
                    Tools.GenerateBabylonMeshData(wheelMesh, collisionMesh);
                    collisionMesh.position = new float[3];
                    collisionMesh.position[0] = wheelCollider.center.x;
                    collisionMesh.position[1] = wheelCollider.center.y;
                    collisionMesh.position[2] = wheelCollider.center.z;
                    collisionMesh.rotation = Vector3.zero.ToFloat();
                    collisionMesh.scaling = new Vector3(1f, 1f, 1f).ToFloat();
                    // Export Wheel Collider Metadata
                    collisionData.tagName = "WheelCollider";
                    collisionData.properties.Add("type", "Wheel");
                    collisionData.properties.Add("center", wheelCollider.center.ToFloat());
                    collisionData.properties.Add("radius", wheelCollider.radius);
                }
                else if (collider is BoxCollider)
                {
                    var boxCollider = collider as BoxCollider;
                    collisionMesh = new BabylonMesh();
                    collisionMesh.tags = "[BOXCOLLIDER]";
                    // Generate Box Collider Geometry
                    Mesh boxMesh = Tools.CreateBoxMesh(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z);
                    Tools.GenerateBabylonMeshData(boxMesh, collisionMesh);
                    collisionMesh.position = new float[3];
                    collisionMesh.position[0] = boxCollider.center.x;
                    collisionMesh.position[1] = boxCollider.center.y;
                    collisionMesh.position[2] = boxCollider.center.z;
                    collisionMesh.rotation = Vector3.zero.ToFloat();
                    collisionMesh.scaling = new Vector3(1f, 1f, 1f).ToFloat();
                    // Export Box Collider Metadata
                    collisionData.tagName = "BoxCollider";
                    collisionData.properties.Add("type", "Box");
                    collisionData.properties.Add("center", boxCollider.center.ToFloat());
                    collisionData.properties.Add("size", boxCollider.size.ToFloat());
                }
                if (collisionMesh != null)
                {
                    collisionMesh.id = Guid.NewGuid().ToString();
                    collisionMesh.name = gameObject.name + "_Collider";
                    collisionMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
                    // Default Check Collisions False
                    collisionMesh.checkCollisions = false;
                    collisionData.properties.Add("isTrigger", collider.isTrigger);
                    collisionData.properties.Add("parrentId", parent);
                    collisionData.properties.Add("transform", Tools.GetTransformPropertyValue(gameObject.transform));
                    collisionMesh.metadata = (ExporterWindow.exportationOptions.ExportMetadata) ? collisionData : null;
                }
            }
            return collisionMesh;
        }

        public static Mesh CreateCollisionGeometry(Collider collider, Matrix4x4? matrix = null, bool lightmaps = true)
        {
            Mesh result = null;
            if (collider != null)
            {
                int segments = 12;
                GameObject gameObject = collider.gameObject;
                BabylonColliderDetail detail = (BabylonColliderDetail)ExporterWindow.exportationOptions.DefaultColliderDetail;
                if (collider is MeshCollider)
                {
                    var meshCollider = collider as MeshCollider;
                    if(!meshCollider.sharedMesh) {
                        UnityEngine.Debug.LogWarning(meshCollider.gameObject + " has a Mesh Collider component without a mesh");
                    } else {
                        result = meshCollider.sharedMesh.Copy(false, matrix, lightmaps);
                    }
                }
                else if (collider is CapsuleCollider)
                {
                    var capsuleCollider = collider as CapsuleCollider;
                    switch (detail)
                    {
                        case BabylonColliderDetail.HighResolution:
                            segments = 24;
                            break;
                        case BabylonColliderDetail.MediumResolution:
                            segments = 12;
                            break;
                        case BabylonColliderDetail.LowResolution:
                            segments = 8;
                            break;
                        case BabylonColliderDetail.VeryLowResolution:
                            segments = 6;
                            break;
                        case BabylonColliderDetail.MinimumResolution:
                            segments = 4;
                            break;
                        default:
                            segments = 8;
                            break;
                    }
                    // Format capsule rotation
                    var capsuleRotation = Vector3.zero;
                    if (capsuleCollider.direction == 0) {
                        // X-Axis - capsuleRotation.z = 90f * (float)Math.PI / 180f;
                        capsuleRotation.z = 90f;
                    } else if (capsuleCollider.direction == 1) {
                        // Y-Axis
                    } else if (capsuleCollider.direction == 2) {
                        // Z-Axis - capsuleRotation.x = 90f * (float)Math.PI / 180f;
                        capsuleRotation.x = 90f;
                    }
                    Mesh sourceMesh = Tools.CreateCapsuleMesh(capsuleCollider.height, capsuleCollider.radius, segments);
                    result = sourceMesh.Rotate(capsuleRotation);
                }
                else if (collider is SphereCollider)
                {
                    var sphereCollider = collider as SphereCollider;
                    switch (detail)
                    {
                        case BabylonColliderDetail.HighResolution:
                            segments = 24;
                            break;
                        case BabylonColliderDetail.MediumResolution:
                            segments = 12;
                            break;
                        case BabylonColliderDetail.LowResolution:
                            segments = 8;
                            break;
                        case BabylonColliderDetail.VeryLowResolution:
                            segments = 6;
                            break;
                        case BabylonColliderDetail.MinimumResolution:
                            segments = 4;
                            break;
                        default:
                            segments = 8;
                            break;
                    }
                    result = Tools.CreateSphereMesh(sphereCollider.radius, segments, segments);
                }
                else if (collider is WheelCollider)
                {
                    var wheelCollider = collider as WheelCollider;
                    switch (detail)
                    {
                        case BabylonColliderDetail.HighResolution:
                            segments = 64;
                            break;
                        case BabylonColliderDetail.MediumResolution:
                            segments = 48;
                            break;
                        case BabylonColliderDetail.LowResolution:
                            segments = 32;
                            break;
                        case BabylonColliderDetail.VeryLowResolution:
                            segments = 24;
                            break;
                        case BabylonColliderDetail.MinimumResolution:
                            segments = 12;
                            break;
                        default:
                            segments = 24;
                            break;
                    }
                    // Format capsule rotation
                    var wheelRotation = Vector3.zero;
                    // Z-Axis - wheelRotation.z = 90f * (float)Math.PI / 180f;
                    wheelRotation.z = 90f;
                    Mesh sourceMesh = Tools.CreateWheelMesh(wheelCollider.suspensionDistance, wheelCollider.radius, segments);
                    result = sourceMesh.Rotate(wheelRotation);
                }
                else if (collider is BoxCollider)
                {
                    var boxCollider = collider as BoxCollider;
                    result = Tools.CreateBoxMesh(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z);
                }
            }
            return result;
        }

        public static object GetTransformPropertyValue(Transform transform)
        {
            if (transform == null) return null;
            Dictionary<string, object> position = new Dictionary<string, object>();
            position.Add("x", transform.localPosition.x);
            position.Add("y", transform.localPosition.y);
            position.Add("z", transform.localPosition.z);
            Dictionary<string, object> rotation = new Dictionary<string, object>();
            rotation.Add("x", transform.localRotation.x);
            rotation.Add("y", transform.localRotation.y);
            rotation.Add("z", transform.localRotation.z);
            Dictionary<string, object> scale = new Dictionary<string, object>();
            scale.Add("x", transform.localScale.x);
            scale.Add("y", transform.localScale.y);
            scale.Add("z", transform.localScale.z);
            Dictionary<string, object> transformInfo = new Dictionary<string, object>();
            transformInfo.Add("type", transform.GetType().FullName);
            transformInfo.Add("id", SceneBuilder.GetID(transform.gameObject));
            transformInfo.Add("position", position);
            transformInfo.Add("rotation", rotation);
            transformInfo.Add("scale", scale);
            return transformInfo;
        }

        public static Vector2[] GetTextureAtlasCoordinates(Vector2[] source, int index, Rect[] rects, bool lerp = true)
        {
            Vector2[] uva = source;
            Vector2[] uvb = new Vector2[uva.Length];
            for (int k = 0; k < uva.Length; k++) {
                if (lerp == true) {
                    uvb[k].x = Mathf.Lerp(rects[index].xMin, rects[index].xMax, uva[k].x);
                    uvb[k].y = Mathf.Lerp(rects[index].yMin, rects[index].yMax, uva[k].y);
                } else {
                    uvb[k].x = (uva[k].x * rects[index].width) + rects[index].x;
                    uvb[k].y = (uva[k].y * rects[index].height) + rects[index].y;
                }
            }
            return uvb;
        }

        public static Rect[] PackTextureAtlas(Texture2D source, Texture2D[] textures, int textureAtlasSize = 4096, int maxTextureImageSize = 0, bool bilinearScaling = true, bool fixBorderEdges = false, bool removeAlpha = false)
        {
            Rect[] result = null;
            if (textures != null && textures.Length > 0) {
                List<Texture2D> packingBuffer = new List<Texture2D>();
                foreach (var texture in textures) {
                    if (texture != null) {
                        Texture2D item = texture.Copy();
                        if (item != null) {
                            if (maxTextureImageSize > 0 && (item.width > maxTextureImageSize || item.height > maxTextureImageSize)) {
                                item.Scale(maxTextureImageSize, maxTextureImageSize, bilinearScaling);
                                if (item == null) {
                                    UnityEngine.Debug.LogWarning("Failed to scale packing texture atlas image");
                                }
                            }
                            //
                            if (fixBorderEdges) {
                                Texture2D extrude = item.Extrude(32, bilinearScaling, TextureExtrude.Green);
                                if (extrude == null) {
                                    UnityEngine.Debug.LogWarning("Failed to extrude a texture atlas image");
                                } else {
                                    item = extrude;
                                }
                            }
                            //
                            packingBuffer.Add(item);
                        } else {
                            UnityEngine.Debug.LogWarning("Failed to copy packing texture atlas image");
                        }
                    } else {
                        UnityEngine.Debug.LogWarning("Null texture atlas packing image encounterd");
                    }
                }
                // Encode texture atlas package
                if (packingBuffer.Count > 0) {
                    result = source.PackTextures(packingBuffer.ToArray(), 0, textureAtlasSize, false);
                    Color32[] pixels = source.Copy().GetPixels32();
                    if (pixels != null) {
                        if (removeAlpha) {
                            for (int index = 0; index < pixels.Length; index++) {
                                pixels[index].a = (byte)255.0f;
                            }
                        }
                        source.SetPixels32(pixels);
                        source.Apply();
                    } else {
                        UnityEngine.Debug.LogWarning("===> Failed to format texture packing result copy");
                    }
                } else {
                    UnityEngine.Debug.LogWarning("===> No texture atlas packing buffer items");
                }
            }
            return result;
        }

        public static Texture2D RawNormalMapToUnityFormat(Texture2D aTexture) {
            Texture2D normalTexture = new Texture2D(aTexture.width, aTexture.height, TextureFormat.RGBA32, aTexture.mipmapCount > 1);
            Color[] pixels = aTexture.GetPixels(0);
            Color[] nPixels = new Color[pixels.Length];
            for (int y=0; y<aTexture.height; y++) {
                for (int x=0; x<aTexture.width; x++) {
                    Color p = pixels[(y * aTexture.width) + x];
                    Color np = new Color(0,0,0,0);
                    np.r = p.g;
                    np.g = p.g; // waste of memory space if you ask me
                    np.b = p.g;
                    np.a = p.r;  
                    nPixels[(y * aTexture.width) + x] = np;
                }
            }
            normalTexture.SetPixels(nPixels);
            normalTexture.Apply();
            return normalTexture;
        }

        public static Texture2D CreateBlankNormalMap(int width, int height) 
        {
            int index = 0;
            float xLeft = 0.0f, xRight = 0.0f, yUp = 0.0f, yDown = 0.0f, yDelta, xDelta = 0.0f;
            Texture2D normalTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[normalTexture.width * normalTexture.height];
            for (int y=0; y < normalTexture.height; y++) {
                for (int x=0; x < normalTexture.width; x++) {
                    xDelta = ((xLeft - xRight) + 1.0f) * 0.5f;
                    yDelta = ((yUp - yDown) + 1.0f) * 0.5f;
                    pixels[index] = new Color(xDelta, yDelta, 1.0f, yDelta);
                    index++;
               }
            }
            normalTexture.SetPixels(pixels);
            normalTexture.Apply();
			return normalTexture;
        }        

        public static Texture2D CreateTextureNormalMap(Texture2D source, float strength) 
        {
            Texture2D normalTexture = null;
            if (source != null) {
                int index = 0;
                float xLeft = 0.0f, xRight = 0.0f, yUp = 0.0f, yDown = 0.0f, yDelta, xDelta = 0.0f;
                strength = Mathf.Clamp(strength, 0.0F, 1.0F);
                normalTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[normalTexture.width * normalTexture.height];
                for (int y=0; y < normalTexture.height; y++) {
                    for (int x=0; x < normalTexture.width; x++) {
                        xLeft = source.GetPixel(x - 1, y).grayscale * strength;
                        xRight = source.GetPixel(x + 1, y).grayscale * strength;
                        yUp = source.GetPixel(x, y - 1).grayscale * strength;
                        yDown = source.GetPixel(x, y + 1).grayscale * strength;
                        
                        xDelta = ((xLeft - xRight) + 1) * 0.5f;
                        yDelta = ((yUp - yDown) + 1) * 0.5f;

                        pixels[index] = new Color(xDelta, yDelta, 1.0f, yDelta);
                        index++;
                    }
                }
                normalTexture.SetPixels(pixels);
                normalTexture.Apply();
            }
			return normalTexture;
        }

        public static Mesh[] CombineStaticMeshes(CombineInstance[] combine, bool mergeSubMeshes, bool useMatrices, bool hasLightmapData = true, bool clearAdditionalUvs = true)
        {
            Mesh[] result = null;
            List<Mesh> meshes = new List<Mesh>();
            BabylonCombinedMeshHelper helper = new BabylonCombinedMeshHelper();
            foreach (var item in combine)
            {
                helper.AddItem(item);               
            }
            List<CombineInstance[]> groups = helper.GetCombinedGroups();
            if (groups != null && groups.Count > 0)
            {
                foreach (var group in groups)
                {
                    if (group != null && group.Length > 0)
                    {
                        Mesh mesh = new Mesh();
                        mesh.CombineMeshes(group, mergeSubMeshes, useMatrices, hasLightmapData);
                        if (mesh != null && mesh.vertexCount > 0)
                        {
                            if (clearAdditionalUvs)
                            {
                                mesh.uv2 = null;
                                mesh.uv3 = null;
                                mesh.uv4 = null;
                            }
                            mesh.RecalculateBounds();
                            meshes.Add(mesh);
                        }
                    }
                }
            }
            if (meshes.Count > 0)
            {
                result = meshes.ToArray();
            }
            return result;
        }

        public static Mesh CreateGroundMesh(float length = 1f, float width = 1f, int resX = 2, int resZ = 2)
        {
            Mesh result = new Mesh();
            
            #region Vertices		
            Vector3[] vertices = new Vector3[ resX * resZ ];
            for(int z = 0; z < resZ; z++)
            {
                // [ -length / 2, length / 2 ]
                float zPos = ((float)z / (resZ - 1) - .5f) * length;
                for(int x = 0; x < resX; x++)
                {
                    // [ -width / 2, width / 2 ]
                    float xPos = ((float)x / (resX - 1) - .5f) * width;
                    vertices[ x + z * resX ] = new Vector3( xPos, 0f, zPos );
                }
            }
            #endregion
            
            #region Normales
            Vector3[] normales = new Vector3[ vertices.Length ];
            for( int n = 0; n < normales.Length; n++ )
                normales[n] = Vector3.up;
            #endregion
            
            #region UVs		
            Vector2[] uvs = new Vector2[ vertices.Length ];
            for(int v = 0; v < resZ; v++)
            {
                for(int u = 0; u < resX; u++)
                {
                    uvs[ u + v * resX ] = new Vector2( (float)u / (resX - 1), (float)v / (resZ - 1) );
                }
            }
            #endregion
            
            #region Triangles
            int nbFaces = (resX - 1) * (resZ - 1);
            int[] triangles = new int[ nbFaces * 6 ];
            int t = 0;
            for(int face = 0; face < nbFaces; face++ )
            {
                // Retrieve lower left corner from face ind
                int i = face % (resX - 1) + (face / (resZ - 1) * resX);
            
                triangles[t++] = i + resX;
                triangles[t++] = i + 1;
                triangles[t++] = i;
            
                triangles[t++] = i + resX;	
                triangles[t++] = i + resX + 1;
                triangles[t++] = i + 1; 
            }
            #endregion
            
            // Mesh Result
            result.vertices = vertices;
            result.normals = normales;
            result.uv = uvs;
            result.triangles = triangles;
            result.RecalculateBounds();
            return result;          
        }

        public static Mesh CreateBoxMesh(float length = 1f, float width = 1f, float height = 1f)
        {
            Mesh result = new Mesh();

            // Vertices
            Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
            Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
            Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
            Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);
            Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
            Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
            Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
            Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);
            Vector3[] vertices = new Vector3[] {
                // Bottom
                p0, p1, p2, p3,
                // Left
                p7, p4, p0, p3,
                // Front
                p4, p5, p1, p0,
                // Back
                p6, p7, p3, p2,
                // Right
                p5, p6, p2, p1,
                // Top
                p7, p6, p5, p4
            };

            // Normales
            Vector3 up = Vector3.up;
            Vector3 down = Vector3.down;
            Vector3 front = Vector3.forward;
            Vector3 back = Vector3.back;
            Vector3 left = Vector3.left;
            Vector3 right = Vector3.right;
            Vector3[] normales = new Vector3[] {
                // Bottom
                down, down, down, down,
                // Left
                left, left, left, left,
                // Front
                front, front, front, front,
                // Back
                back, back, back, back,
                // Right
                right, right, right, right,
                // Top
                up, up, up, up
            };

            // UVs
            Vector2 _00 = new Vector2(0f, 0f);
            Vector2 _10 = new Vector2(1f, 0f);
            Vector2 _01 = new Vector2(0f, 1f);
            Vector2 _11 = new Vector2(1f, 1f);
            Vector2[] uvs = new Vector2[] {
                // Bottom
                _11, _01, _00, _10,
                // Left
                _11, _01, _00, _10,
                // Front
                _11, _01, _00, _10,
                // Back
                _11, _01, _00, _10,
                // Right
                _11, _01, _00, _10,
                // Top
                _11, _01, _00, _10,
            };

            // Triangles
            int[] triangles = new int[] {
                // Bottom
                3, 1, 0,
                3, 2, 1,			
                // Left
                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
                // Front
                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
                // Back
                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
                // Right
                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
                // Top
                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
            };
            // Mesh Result
            result.vertices = vertices;
            result.normals = normales;
            result.uv = uvs;
            result.triangles = triangles;
            result.RecalculateBounds();
            return result;
        }

        public static Mesh CreateConeMesh(float height = 1f, float bottomRadius = 0.25f, float topRadius = 0.05f, int nbSides = 18, int nbHeightSeg = 1)
        {
            Mesh result = new Mesh();
            int nbVerticesCap = nbSides + 1;
            #region Vertices
            
            // bottom + top + sides
            Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + nbSides * nbHeightSeg * 2 + 2];
            int vert = 0;
            float _2pi = Mathf.PI * 2f;
            
            // Bottom cap
            vertices[vert++] = new Vector3(0f, 0f, 0f);
            while( vert <= nbSides )
            {
                float rad = (float)vert / nbSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
                vert++;
            }
            
            // Top cap
            vertices[vert++] = new Vector3(0f, height, 0f);
            while (vert <= nbSides * 2 + 1)
            {
                float rad = (float)(vert - nbSides - 1)  / nbSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
                vert++;
            }
            
            // Sides
            int v = 0;
            while (vert <= vertices.Length - 4 )
            {
                float rad = (float)v / nbSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
                vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
                vert+=2;
                v++;
            }
            vertices[vert] = vertices[ nbSides * 2 + 2 ];
            vertices[vert + 1] = vertices[nbSides * 2 + 3 ];
            #endregion
            
            #region Normales
            
            // bottom + top + sides
            Vector3[] normales = new Vector3[vertices.Length];
            vert = 0;
            
            // Bottom cap
            while( vert  <= nbSides )
            {
                normales[vert++] = Vector3.down;
            }
            
            // Top cap
            while( vert <= nbSides * 2 + 1 )
            {
                normales[vert++] = Vector3.up;
            }
            
            // Sides
            v = 0;
            while (vert <= vertices.Length - 4 )
            {			
                float rad = (float)v / nbSides * _2pi;
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);
            
                normales[vert] = new Vector3(cos, 0f, sin);
                normales[vert+1] = normales[vert];
            
                vert+=2;
                v++;
            }
            normales[vert] = normales[ nbSides * 2 + 2 ];
            normales[vert + 1] = normales[nbSides * 2 + 3 ];
            #endregion
            
            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            
            // Bottom cap
            int u = 0;
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= nbSides)
            {
                float rad = (float)u / nbSides * _2pi;
                uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
                u++;
            }
            
            // Top cap
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= nbSides * 2 + 1)
            {
                float rad = (float)u / nbSides * _2pi;
                uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
                u++;
            }
            
            // Sides
            int u_sides = 0;
            while (u <= uvs.Length - 4 )
            {
                float t = (float)u_sides / nbSides;
                uvs[u] = new Vector3(t, 1f);
                uvs[u + 1] = new Vector3(t, 0f);
                u += 2;
                u_sides++;
            }
            uvs[u] = new Vector2(1f, 1f);
            uvs[u + 1] = new Vector2(1f, 0f);
            #endregion 
            
            #region Triangles
            int nbTriangles = nbSides + nbSides + nbSides*2;
            int[] triangles = new int[nbTriangles * 3 + 3];
            
            // Bottom cap
            int tri = 0;
            int i = 0;
            while (tri < nbSides - 1)
            {
                triangles[ i ] = 0;
                triangles[ i+1 ] = tri + 1;
                triangles[ i+2 ] = tri + 2;
                tri++;
                i += 3;
            }
            triangles[i] = 0;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = 1;
            tri++;
            i += 3;
            
            // Top cap
            //tri++;
            while (tri < nbSides*2)
            {
                triangles[ i ] = tri + 2;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = nbVerticesCap;
                tri++;
                i += 3;
            }
            
            triangles[i] = nbVerticesCap + 1;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = nbVerticesCap;		
            tri++;
            i += 3;
            tri++;
            
            // Sides
            while( tri <= nbTriangles )
            {
                triangles[ i ] = tri + 2;
                triangles[ i+1 ] = tri + 1;
                triangles[ i+2 ] = tri + 0;
                tri++;
                i += 3;
            
                triangles[ i ] = tri + 1;
                triangles[ i+1 ] = tri + 2;
                triangles[ i+2 ] = tri + 0;
                tri++;
                i += 3;
            }
            #endregion
            
            // Mesh Result
            result.vertices = vertices;
            result.normals = normales;
            result.uv = uvs;
            result.triangles = triangles;
            result.RecalculateBounds();
            return result;
        }

        public static Mesh CreateTubeMesh(float height = 1f, int nbSides = 24)
        {
            Mesh result = new Mesh();
            
            // Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
            float bottomRadius1 = .5f;
            float bottomRadius2 = .15f; 
            float topRadius1 = .5f;
            float topRadius2 = .15f;
            
            int nbVerticesCap = nbSides * 2 + 2;
            int nbVerticesSides = nbSides * 2 + 2;
            #region Vertices
            
            // bottom + top + sides
            Vector3[] vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
            int vert = 0;
            float _2pi = Mathf.PI * 2f;
            
            // Bottom cap
            int sideCounter = 0;
            while( vert < nbVerticesCap )
            {
                sideCounter = sideCounter == nbSides ? 0 : sideCounter;
            
                float r1 = (float)(sideCounter++) / nbSides * _2pi;
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);
                vertices[vert] = new Vector3( cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
                vertices[vert+1] = new Vector3( cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
                vert += 2;
            }
            
            // Top cap
            sideCounter = 0;
            while( vert < nbVerticesCap * 2 )
            {
                sideCounter = sideCounter == nbSides ? 0 : sideCounter;
            
                float r1 = (float)(sideCounter++) / nbSides * _2pi;
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);
                vertices[vert] = new Vector3( cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
                vertices[vert+1] = new Vector3( cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
                vert += 2;
            }
            
            // Sides (out)
            sideCounter = 0;
            while (vert < nbVerticesCap * 2 + nbVerticesSides )
            {
                sideCounter = sideCounter == nbSides ? 0 : sideCounter;
            
                float r1 = (float)(sideCounter++) / nbSides * _2pi;
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);
            
                vertices[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
                vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
                vert+=2;
            }
            
            // Sides (in)
            sideCounter = 0;
            while (vert < vertices.Length )
            {
                sideCounter = sideCounter == nbSides ? 0 : sideCounter;
            
                float r1 = (float)(sideCounter++) / nbSides * _2pi;
                float cos = Mathf.Cos(r1);
                float sin = Mathf.Sin(r1);
            
                vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
                vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
                vert += 2;
            }
            #endregion
            
            #region Normales
            
            // bottom + top + sides
            Vector3[] normales = new Vector3[vertices.Length];
            vert = 0;
            
            // Bottom cap
            while( vert < nbVerticesCap )
            {
                normales[vert++] = Vector3.down;
            }
            
            // Top cap
            while( vert < nbVerticesCap * 2 )
            {
                normales[vert++] = Vector3.up;
            }
            
            // Sides (out)
            sideCounter = 0;
            while (vert < nbVerticesCap * 2 + nbVerticesSides )
            {
                sideCounter = sideCounter == nbSides ? 0 : sideCounter;
            
                float r1 = (float)(sideCounter++) / nbSides * _2pi;
            
                normales[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
                normales[vert+1] = normales[vert];
                vert+=2;
            }
            
            // Sides (in)
            sideCounter = 0;
            while (vert < vertices.Length )
            {
                sideCounter = sideCounter == nbSides ? 0 : sideCounter;
            
                float r1 = (float)(sideCounter++) / nbSides * _2pi;
            
                normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
                normales[vert+1] = normales[vert];
                vert+=2;
            }
            #endregion
            
            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            
            vert = 0;
            // Bottom cap
            sideCounter = 0;
            while( vert < nbVerticesCap )
            {
                float t = (float)(sideCounter++) / nbSides;
                uvs[ vert++ ] = new Vector2( 0f, t );
                uvs[ vert++ ] = new Vector2( 1f, t );
            }
            
            // Top cap
            sideCounter = 0;
            while( vert < nbVerticesCap * 2 )
            {
                float t = (float)(sideCounter++) / nbSides;
                uvs[ vert++ ] = new Vector2( 0f, t );
                uvs[ vert++ ] = new Vector2( 1f, t );
            }
            
            // Sides (out)
            sideCounter = 0;
            while (vert < nbVerticesCap * 2 + nbVerticesSides )
            {
                float t = (float)(sideCounter++) / nbSides;
                uvs[ vert++ ] = new Vector2( t, 0f );
                uvs[ vert++ ] = new Vector2( t, 1f );
            }
            
            // Sides (in)
            sideCounter = 0;
            while (vert < vertices.Length )
            {
                float t = (float)(sideCounter++) / nbSides;
                uvs[ vert++ ] = new Vector2( t, 0f );
                uvs[ vert++ ] = new Vector2( t, 1f );
            }
            #endregion
            
            #region Triangles
            int nbFace = nbSides * 4;
            int nbTriangles = nbFace * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];
            
            // Bottom cap
            int i = 0;
            sideCounter = 0;
            while (sideCounter < nbSides)
            {
                int current = sideCounter * 2;
                int next = sideCounter * 2 + 2;
            
                triangles[ i++ ] = next + 1;
                triangles[ i++ ] = next;
                triangles[ i++ ] = current;
            
                triangles[ i++ ] = current + 1;
                triangles[ i++ ] = next + 1;
                triangles[ i++ ] = current;
            
                sideCounter++;
            }
            
            // Top cap
            while (sideCounter < nbSides * 2)
            {
                int current = sideCounter * 2 + 2;
                int next = sideCounter * 2 + 4;
            
                triangles[ i++ ] = current;
                triangles[ i++ ] = next;
                triangles[ i++ ] = next + 1;
            
                triangles[ i++ ] = current;
                triangles[ i++ ] = next + 1;
                triangles[ i++ ] = current + 1;
            
                sideCounter++;
            }
            
            // Sides (out)
            while( sideCounter < nbSides * 3 )
            {
                int current = sideCounter * 2 + 4;
                int next = sideCounter * 2 + 6;
            
                triangles[ i++ ] = current;
                triangles[ i++ ] = next;
                triangles[ i++ ] = next + 1;
            
                triangles[ i++ ] = current;
                triangles[ i++ ] = next + 1;
                triangles[ i++ ] = current + 1;
            
                sideCounter++;
            }
            
            
            // Sides (in)
            while( sideCounter < nbSides * 4 )
            {
                int current = sideCounter * 2 + 6;
                int next = sideCounter * 2 + 8;
            
                triangles[ i++ ] = next + 1;
                triangles[ i++ ] = next;
                triangles[ i++ ] = current;
            
                triangles[ i++ ] = current + 1;
                triangles[ i++ ] = next + 1;
                triangles[ i++ ] = current;
            
                sideCounter++;
            }
            #endregion
            
            // Mesh Result
            result.vertices = vertices;
            result.normals = normales;
            result.uv = uvs;
            result.triangles = triangles;
            result.RecalculateBounds();
            return result;
        }

        public static Mesh CreateWheelMesh(float height = 1f, float radius = 0.5f, int segments = 24)
        {
            Mesh result = new Mesh();

            float topRadius = radius;
            float bottomRadius = radius;
            int nbSides = segments;
            int nbHeightSeg = 1; // Not implemented yet

            int nbVerticesCap = nbSides + 1;
            // Vertices

            // bottom + top + sides
            Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + nbSides * nbHeightSeg * 2 + 2];
            int vert = 0;
            float _2pi = Mathf.PI * 2f;

            // Bottom cap
            vertices[vert++] = new Vector3(0f, 0f, 0f);
            while (vert <= nbSides)
            {
                float rad = (float)vert / nbSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
                vert++;
            }

            // Top cap
            vertices[vert++] = new Vector3(0f, height, 0f);
            while (vert <= nbSides * 2 + 1)
            {
                float rad = (float)(vert - nbSides - 1) / nbSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
                vert++;
            }

            // Sides
            int v = 0;
            while (vert <= vertices.Length - 4)
            {
                float rad = (float)v / nbSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
                vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
                vert += 2;
                v++;
            }
            vertices[vert] = vertices[nbSides * 2 + 2];
            vertices[vert + 1] = vertices[nbSides * 2 + 3];

            // Normales

            // bottom + top + sides
            Vector3[] normales = new Vector3[vertices.Length];
            vert = 0;

            // Bottom cap
            while (vert <= nbSides)
            {
                normales[vert++] = Vector3.down;
            }

            // Top cap
            while (vert <= nbSides * 2 + 1)
            {
                normales[vert++] = Vector3.up;
            }

            // Sides
            v = 0;
            while (vert <= vertices.Length - 4)
            {
                float rad = (float)v / nbSides * _2pi;
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                normales[vert] = new Vector3(cos, 0f, sin);
                normales[vert + 1] = normales[vert];

                vert += 2;
                v++;
            }
            normales[vert] = normales[nbSides * 2 + 2];
            normales[vert + 1] = normales[nbSides * 2 + 3];

            // UVs
            Vector2[] uvs = new Vector2[vertices.Length];

            // Bottom cap
            int u = 0;
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= nbSides)
            {
                float rad = (float)u / nbSides * _2pi;
                uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
                u++;
            }

            // Top cap
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= nbSides * 2 + 1)
            {
                float rad = (float)u / nbSides * _2pi;
                uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
                u++;
            }

            // Sides
            int u_sides = 0;
            while (u <= uvs.Length - 4)
            {
                float t = (float)u_sides / nbSides;
                uvs[u] = new Vector3(t, 1f);
                uvs[u + 1] = new Vector3(t, 0f);
                u += 2;
                u_sides++;
            }
            uvs[u] = new Vector2(1f, 1f);
            uvs[u + 1] = new Vector2(1f, 0f);

            // Triangles
            int nbTriangles = nbSides + nbSides + nbSides * 2;
            int[] triangles = new int[nbTriangles * 3 + 3];

            // Bottom cap
            int tri = 0;
            int i = 0;
            while (tri < nbSides - 1)
            {
                triangles[i] = 0;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = tri + 2;
                tri++;
                i += 3;
            }
            triangles[i] = 0;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = 1;
            tri++;
            i += 3;

            // Top cap
            //tri++;
            while (tri < nbSides * 2)
            {
                triangles[i] = tri + 2;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = nbVerticesCap;
                tri++;
                i += 3;
            }

            triangles[i] = nbVerticesCap + 1;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = nbVerticesCap;
            tri++;
            i += 3;
            tri++;

            // Sides
            while (tri <= nbTriangles)
            {
                triangles[i] = tri + 2;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = tri + 0;
                tri++;
                i += 3;

                triangles[i] = tri + 1;
                triangles[i + 1] = tri + 2;
                triangles[i + 2] = tri + 0;
                tri++;
                i += 3;
            }

            // Mesh Result
            result.vertices = vertices;
            result.normals = normales;
            result.uv = uvs;
            result.triangles = triangles;
            result.RecalculateBounds();
            return result;
        }

        public static Mesh CreateTorusMesh(float radius1 = 1f, float radius2 = 0.3f, int nbRadSeg = 24, int nbSides = 18)
        {
            Mesh result = new Mesh();
            
            #region Vertices		
            Vector3[] vertices = new Vector3[(nbRadSeg+1) * (nbSides+1)];
            float _2pi = Mathf.PI * 2f;
            for( int seg = 0; seg <= nbRadSeg; seg++ )
            {
                int currSeg = seg  == nbRadSeg ? 0 : seg;
                
            
                float t1 = (float)currSeg / nbRadSeg * _2pi;
                Vector3 r1 = new Vector3( Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1 );
            
                for( int side = 0; side <= nbSides; side++ )
                {
                    int currSide = side == nbSides ? 0 : side;
            
                    // Vector3 normale = Vector3.Cross( r1, Vector3.up );
                    float t2 = (float)currSide / nbSides * _2pi;
                    Vector3 r2 = Quaternion.AngleAxis( -t1 * Mathf.Rad2Deg, Vector3.up ) *new Vector3( Mathf.Sin(t2) * radius2, Mathf.Cos(t2) * radius2 );
            
                    vertices[side + seg * (nbSides+1)] = r1 + r2;
                }
            }
            #endregion
            
            #region Normales		
            Vector3[] normales = new Vector3[vertices.Length];
            for( int seg = 0; seg <= nbRadSeg; seg++ )
            {
                int currSeg = seg  == nbRadSeg ? 0 : seg;
            
                float t1 = (float)currSeg / nbRadSeg * _2pi;
                Vector3 r1 = new Vector3( Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1 );
            
                for( int side = 0; side <= nbSides; side++ )
                {
                    normales[side + seg * (nbSides+1)] = (vertices[side + seg * (nbSides+1)] - r1).normalized;
                }
            }
            #endregion
            
            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            for( int seg = 0; seg <= nbRadSeg; seg++ )
                for( int side = 0; side <= nbSides; side++ )
                    uvs[side + seg * (nbSides+1)] = new Vector2( (float)seg / nbRadSeg, (float)side / nbSides );
            #endregion
            
            #region Triangles
            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[ nbIndexes ];
            
            int i = 0;
            for( int seg = 0; seg <= nbRadSeg; seg++ )
            {			
                for( int side = 0; side <= nbSides - 1; side++ )
                {
                    int current = side + seg * (nbSides+1);
                    int next = side + (seg < (nbRadSeg) ?(seg+1) * (nbSides+1) : 0);
            
                    if( i < triangles.Length - 6 )
                    {
                        triangles[i++] = current;
                        triangles[i++] = next;
                        triangles[i++] = next+1;
            
                        triangles[i++] = current;
                        triangles[i++] = next+1;
                        triangles[i++] = current+1;
                    }
                }
            }
            #endregion
            
            // Mesh Result
            result.vertices = vertices;
            result.normals = normales;
            result.uv = uvs;
            result.triangles = triangles;
            result.RecalculateBounds();
            return result;
        }

        public static Mesh CreateCapsuleMesh(float height = 2f, float radius = 0.5f, int segments = 24)
        {
            Mesh result = new Mesh();

            // Make segments an even number
            if (segments % 2 != 0)
            {
                segments++;
            }
            // Extra vertex on the seam
            int points = segments + 1;
            // Calculate points around a circle
            float[] pX = new float[points];
            float[] pZ = new float[points];
            float[] pY = new float[points];
            float[] pR = new float[points];
            float calcH = 0f;
            float calcV = 0f;
            for (int i = 0; i < points; i++)
            {
                pX[i] = Mathf.Sin(calcH * Mathf.Deg2Rad);
                pZ[i] = Mathf.Cos(calcH * Mathf.Deg2Rad);
                pY[i] = Mathf.Cos(calcV * Mathf.Deg2Rad);
                pR[i] = Mathf.Sin(calcV * Mathf.Deg2Rad);

                calcH += 360f / (float)segments;
                calcV += 180f / (float)segments;
            }
            // Vertices and UVs
            Vector3[] vertices = new Vector3[points * (points + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            int ind = 0;
            // Y-Offset is half the height minus the diameter
            float yOff = (height - (radius * 2f)) * 0.5f;
            if (yOff < 0) yOff = 0;
            // UV Calculations
            float stepX = 1f / ((float)(points - 1));
            float uvX, uvY;
            // Top Hemisphere
            int top = Mathf.CeilToInt((float)points * 0.5f);
            for (int y = 0; y < top; y++)
            {
                for (int x = 0; x < points; x++)
                {
                    vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                    vertices[ind].y = yOff + vertices[ind].y;

                    uvX = 1f - (stepX * (float)x);
                    uvY = (vertices[ind].y + (height * 0.5f)) / height;
                    uvs[ind] = new Vector2(uvX, uvY);

                    ind++;
                }
            }
            // Bottom Hemisphere
            int btm = Mathf.FloorToInt((float)points * 0.5f);
            for (int y = btm; y < points; y++)
            {
                for (int x = 0; x < points; x++)
                {
                    vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                    vertices[ind].y = -yOff + vertices[ind].y;

                    uvX = 1f - (stepX * (float)x);
                    uvY = (vertices[ind].y + (height * 0.5f)) / height;
                    uvs[ind] = new Vector2(uvX, uvY);

                    ind++;
                }
            }
            // Triangles
            int[] triangles = new int[(segments * (segments + 1) * 2 * 3)];
            for (int y = 0, t = 0; y < segments + 1; y++)
            {
                for (int x = 0; x < segments; x++, t += 6)
                {
                    triangles[t + 0] = ((y + 0) * (segments + 1)) + x + 0;
                    triangles[t + 1] = ((y + 1) * (segments + 1)) + x + 0;
                    triangles[t + 2] = ((y + 1) * (segments + 1)) + x + 1;

                    triangles[t + 3] = ((y + 0) * (segments + 1)) + x + 1;
                    triangles[t + 4] = ((y + 0) * (segments + 1)) + x + 0;
                    triangles[t + 5] = ((y + 1) * (segments + 1)) + x + 1;
                }
            }

            // Mesh Result
            result.vertices = vertices;
            result.uv = uvs;
            result.triangles = triangles;
            result.RecalculateBounds();
            //result.RecalculateNormals();
            return result;
        }

        public static Mesh CreateSphereMesh(float radius = 1.0f, int segments = 24, int latitude = 16)
        {
            Mesh result = new Mesh();

            // Longitude |||
            int nbLong = segments; //24;
            // Latitude ---
            int nbLat = latitude; //16;

            // Vertices
            Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
            float _pi = Mathf.PI;
            float _2pi = _pi * 2f;
            vertices[0] = Vector3.up * radius;
            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
                }
            }
            vertices[vertices.Length - 1] = Vector3.up * -radius;

            // Normales		
            Vector3[] normales = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
            {
                normales[n] = vertices[n].normalized;
            }

            // UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            uvs[0] = Vector2.up;
            uvs[uvs.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
            {
                for (int lon = 0; lon <= nbLong; lon++)
                {
                    uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
                }
            }

            // Triangles
            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            // Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            // Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
            {
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    triangles[i++] = current;
                    triangles[i++] = current + 1;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = next;
                }
            }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }

            // Mesh Result
            result.vertices = vertices;
            result.normals = normales;
            result.uv = uvs;
            result.triangles = triangles;
            result.RecalculateBounds();
            return result;
        }

        public static void ReverseNormals(this Mesh mesh)
        {
            Vector3[] normals = mesh.normals;
			for (int i=0;i<normals.Length;i++) {
				normals[i] = -normals[i];
            }
			mesh.normals = normals;
			for (int m=0;m<mesh.subMeshCount;m++) {
				int[] triangles = mesh.GetTriangles(m);
				for (int i=0;i<triangles.Length;i+=3) {
					int temp = triangles[i + 0];
					triangles[i + 0] = triangles[i + 1];
					triangles[i + 1] = temp;
				}
				mesh.SetTriangles(triangles, m);
			}            
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public static Mesh[] SplitTerrainData(Terrain parentTerrain, int splitFactor)
        {
            TerrainInfo terrainInfo = new TerrainInfo();
            terrainInfo.chunkCountHorizontal = terrainInfo.chunkCountVertical = splitFactor;
            terrainInfo.vertexCountHorizontal = terrainInfo.vertexCountVertical = ((parentTerrain.terrainData.heightmapResolution - 1) / splitFactor) + 1;
            return TerrainGenerator.ConvertTerrain(parentTerrain, terrainInfo);
        }

        public static BabylonTerrainData CreateTerrainData(TerrainData terrain, Vector3 position, bool invert = false)
        {
            int resolution = 0; // Always Full Resolution
            BabylonTerrainData result = new BabylonTerrainData();
            int w = terrain.heightmapWidth;
            int h = terrain.heightmapHeight;
            Vector3 meshScale = terrain.size;
            int tRes = (int)Mathf.Pow(2, resolution);
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            float[,] tData = terrain.GetHeights(0, 0, w, h);
            w = (w - 1) / tRes + 1;
            h = (h - 1) / tRes + 1;
            result.width = w;
            result.height = h;
            result.vertices = new Vector3[w * h];
            result.normals = new Vector3[w * h];
            result.uvs = new Vector2[w * h];
            result.triangles = new int[(w - 1) * (h - 1) * 6];
            // Build vertices and UVs
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (invert)
                    {
                        result.vertices[x * w + y] = Vector3.Scale(meshScale, new Vector3(y, tData[x * tRes, y * tRes], x)) + position;
                        result.normals[x * w + y] = new Vector3(0, 0, 0);
                        result.uvs[x * w + y] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
                    }
                    else
                    {
                        result.vertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x)) + position;
                        result.normals[y * w + x] = new Vector3(0, 0, 0);
                        result.uvs[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
                    }
                }
            }
            int index = 0;
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    result.triangles[index++] = (y * w) + x;
                    result.triangles[index++] = ((y + 1) * w) + x;
                    result.triangles[index++] = (y * w) + x + 1;

                    result.triangles[index++] = ((y + 1) * w) + x;
                    result.triangles[index++] = ((y + 1) * w) + x + 1;
                    result.triangles[index++] = (y * w) + x + 1;
                }
            }
            return result;
        }

        public static void GenerateBabylonMeshData(Mesh mesh, BabylonMesh babylonMesh, BabylonScene babylonScene = null, Transform transform = null)
        {
            int index = 0;
            babylonMesh.positions = new float[mesh.vertexCount * 3];
            foreach (Vector3 vv in mesh.vertices)
            {
                babylonMesh.positions[index * 3] = vv.x;
                babylonMesh.positions[(index * 3) + 1] = vv.y;
                babylonMesh.positions[(index * 3) + 2] = vv.z;
                if (babylonScene != null && transform != null)
                {
                    // Computing world extends
                    Vector3 worldPosition = transform.TransformPoint(vv);
                    if (worldPosition.x > babylonScene.MaxVector.X)
                    {
                        babylonScene.MaxVector.X = worldPosition.x;
                    }
                    if (worldPosition.y > babylonScene.MaxVector.Y)
                    {
                        babylonScene.MaxVector.Y = worldPosition.y;
                    }
                    if (worldPosition.z > babylonScene.MaxVector.Z)
                    {
                        babylonScene.MaxVector.Z = worldPosition.z;
                    }

                    if (worldPosition.x < babylonScene.MinVector.X)
                    {
                        babylonScene.MinVector.X = worldPosition.x;
                    }
                    if (worldPosition.y < babylonScene.MinVector.Y)
                    {
                        babylonScene.MinVector.Y = worldPosition.y;
                    }
                    if (worldPosition.z < babylonScene.MinVector.Z)
                    {
                        babylonScene.MinVector.Z = worldPosition.z;
                    }
                }
                index++;
            }
            index = 0;
            babylonMesh.normals = new float[mesh.vertexCount * 3];
            foreach (Vector3 nn in mesh.normals)
            {
                babylonMesh.normals[index * 3] = nn.x;
                babylonMesh.normals[(index * 3) + 1] = nn.y;
                babylonMesh.normals[(index * 3) + 2] = nn.z;
                index++;
            }
            index = 0;
            babylonMesh.uvs = new float[mesh.vertexCount * 2];
            foreach (Vector3 v in mesh.uv)
            {
                babylonMesh.uvs[index * 2] = v.x;
                babylonMesh.uvs[(index * 2) + 1] = v.y;
                index++;
            }
            index = 0;
            babylonMesh.uvs2 = new float[mesh.vertexCount * 2];
            if (mesh.uv2 != null && mesh.uv2.Length > 0)
            {
                index = 0;
                foreach (Vector3 v in mesh.uv2)
                {
                    babylonMesh.uvs2[index * 2] = v.x;
                    babylonMesh.uvs2[(index * 2) + 1] = v.y;
                    index++;
                }
            }
            else
            {
                index = 0;
                foreach (Vector3 v in mesh.uv)
                {
                    babylonMesh.uvs2[index * 2] = v.x;
                    babylonMesh.uvs2[(index * 2) + 1] = v.y;
                    index++;
                }
            }
            index = 0;
            int loop = 0;
            babylonMesh.indices = new int[mesh.triangles.Length];
            List<Vector3> vectors = new List<Vector3>();
            Vector3 vector = new Vector3(0, 0, 0);
            foreach (int poly in mesh.triangles)
            {
                if (loop == 0)
                {
                    vector = new Vector3(0, 0, 0);
                    vector.z = poly;
                }
                else if (loop == 1)
                {
                    vector.y = poly;
                }
                else if (loop == 2)
                {
                    vector.x = poly;
                    vectors.Add(vector);
                }
                loop++;
                if (loop >= 3) loop = 0;
            }
            index = 0;
            foreach (Vector3 vvv in vectors)
            {
                babylonMesh.indices[index * 3] = (int)vvv.x;
                babylonMesh.indices[(index * 3) + 1] = (int)vvv.y;
                babylonMesh.indices[(index * 3) + 2] = (int)vvv.z;
                index++;
            }
        }

        public static void GenerateBabylonMeshTerrainData(BabylonTerrainData terrainData, BabylonMesh babylonMesh, bool reverseNormals = false, BabylonScene babylonScene = null, Transform transform = null)
        {
            int index = 0;
            babylonMesh.positions = new float[terrainData.vertices.Length * 3];
            foreach (Vector3 vv in terrainData.vertices)
            {
                babylonMesh.positions[index * 3] = vv.x;
                babylonMesh.positions[(index * 3) + 1] = vv.y;
                babylonMesh.positions[(index * 3) + 2] = vv.z;
                if (babylonScene != null && transform != null)
                {
                    // Computing world extends
                    Vector3 worldPosition = transform.TransformPoint(vv);
                    if (worldPosition.x > babylonScene.MaxVector.X)
                    {
                        babylonScene.MaxVector.X = worldPosition.x;
                    }
                    if (worldPosition.y > babylonScene.MaxVector.Y)
                    {
                        babylonScene.MaxVector.Y = worldPosition.y;
                    }
                    if (worldPosition.z > babylonScene.MaxVector.Z)
                    {
                        babylonScene.MaxVector.Z = worldPosition.z;
                    }

                    if (worldPosition.x < babylonScene.MinVector.X)
                    {
                        babylonScene.MinVector.X = worldPosition.x;
                    }
                    if (worldPosition.y < babylonScene.MinVector.Y)
                    {
                        babylonScene.MinVector.Y = worldPosition.y;
                    }
                    if (worldPosition.z < babylonScene.MinVector.Z)
                    {
                        babylonScene.MinVector.Z = worldPosition.z;
                    }
                }
                index++;
            }
            index = 0;
            babylonMesh.normals = new float[terrainData.vertices.Length * 3];
            foreach (Vector3 nn in terrainData.normals)
            {
                babylonMesh.normals[index * 3] = nn.x;
                babylonMesh.normals[(index * 3) + 1] = nn.y;
                babylonMesh.normals[(index * 3) + 2] = nn.z;
                index++;
            }
            index = 0;
            babylonMesh.uvs = new float[terrainData.vertices.Length * 2];
            foreach (Vector3 v in terrainData.uvs)
            {
                babylonMesh.uvs[index * 2] = v.x;
                babylonMesh.uvs[(index * 2) + 1] = v.y;
                index++;
            }
            index = 0;
            babylonMesh.uvs2 = new float[terrainData.vertices.Length * 2];
            foreach (Vector3 v in terrainData.uvs)
            {
                babylonMesh.uvs2[index * 2] = v.x;
                babylonMesh.uvs2[(index * 2) + 1] = v.y;
                index++;
            }
            index = 0;
            int loop = 0;
            babylonMesh.indices = new int[terrainData.triangles.Length];
            List<Vector3> vectors = new List<Vector3>();
            Vector3 vector = new Vector3(0, 0, 0);
            foreach (int poly in terrainData.triangles)
            {
                if (loop == 0)
                {
                    vector = new Vector3(0, 0, 0);
                    vector.z = poly;
                }
                else if (loop == 1)
                {
                    vector.y = poly;
                }
                else if (loop == 2)
                {
                    vector.x = poly;
                    vectors.Add(vector);
                }
                loop++;
                if (loop >= 3) loop = 0;
            }
            index = 0;
            foreach (Vector3 vvv in vectors)
            {
                babylonMesh.indices[index * 3] = (int)vvv.x;
                babylonMesh.indices[(index * 3) + 1] = (int)vvv.y;
                babylonMesh.indices[(index * 3) + 2] = (int)vvv.z;
                index++;
            }
            Tools.RecalculateMeshNormals(babylonMesh, reverseNormals);
        }

        public static void RecalculateMeshNormals(BabylonMesh mesh, bool flip = false)
        {
            int index = 0;

            float p1p2x = 0.0f;
            float p1p2y = 0.0f;
            float p1p2z = 0.0f;
            float p3p2x = 0.0f;
            float p3p2y = 0.0f;
            float p3p2z = 0.0f;
            float faceNormalx = 0.0f;
            float faceNormaly = 0.0f;
            float faceNormalz = 0.0f;

            float length = 0.0f;

            int i1 = 0;
            int i2 = 0;
            int i3 = 0;

            // indice triplet = 1 face
            var nbFaces = mesh.indices.Length / 3;
            for (index = 0; index < nbFaces; index++)
            {
                i1 = mesh.indices[index * 3];            // get the indexes of each vertex of the face
                i2 = mesh.indices[index * 3 + 1];
                i3 = mesh.indices[index * 3 + 2];

                p1p2x = mesh.positions[i1 * 3] - mesh.positions[i2 * 3];          // compute two vectors per face
                p1p2y = mesh.positions[i1 * 3 + 1] - mesh.positions[i2 * 3 + 1];
                p1p2z = mesh.positions[i1 * 3 + 2] - mesh.positions[i2 * 3 + 2];

                p3p2x = mesh.positions[i3 * 3] - mesh.positions[i2 * 3];
                p3p2y = mesh.positions[i3 * 3 + 1] - mesh.positions[i2 * 3 + 1];
                p3p2z = mesh.positions[i3 * 3 + 2] - mesh.positions[i2 * 3 + 2];

                faceNormalx = p1p2y * p3p2z - p1p2z * p3p2y;            // compute the face normal with cross product
                faceNormaly = p1p2z * p3p2x - p1p2x * p3p2z;
                faceNormalz = p1p2x * p3p2y - p1p2y * p3p2x;

                length = (float)Math.Sqrt(faceNormalx * faceNormalx + faceNormaly * faceNormaly + faceNormalz * faceNormalz);
                length = (length == 0) ? 1.0f : length;
                faceNormalx /= length;                                  // normalize this normal
                faceNormaly /= length;
                faceNormalz /= length;

                mesh.normals[i1 * 3] += faceNormalx;                         // accumulate all the normals per face
                mesh.normals[i1 * 3 + 1] += faceNormaly;
                mesh.normals[i1 * 3 + 2] += faceNormalz;
                mesh.normals[i2 * 3] += faceNormalx;
                mesh.normals[i2 * 3 + 1] += faceNormaly;
                mesh.normals[i2 * 3 + 2] += faceNormalz;
                mesh.normals[i3 * 3] += faceNormalx;
                mesh.normals[i3 * 3 + 1] += faceNormaly;
                mesh.normals[i3 * 3 + 2] += faceNormalz;
            }

            // last normalization of each normal
            float flipValue = (flip) ? -1.0f : 1.0f;
            for (index = 0; index < mesh.normals.Length / 3; index++)
            {
                faceNormalx = mesh.normals[index * 3];
                faceNormaly = mesh.normals[index * 3 + 1];
                faceNormalz = mesh.normals[index * 3 + 2];

                length = (float)Math.Sqrt(faceNormalx * faceNormalx + faceNormaly * faceNormaly + faceNormalz * faceNormalz);
                length = (length == 0) ? 1.0f : length;
                faceNormalx /= length;
                faceNormaly /= length;
                faceNormalz /= length;

                mesh.normals[index * 3] = (faceNormalx * flipValue);
                mesh.normals[index * 3 + 1] = (faceNormaly * flipValue);
                mesh.normals[index * 3 + 2] = (faceNormalz * flipValue);
            }

            // flip triangles for all normals
            if (flip)
            {
                int[] triangles = mesh.indices;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.indices = triangles;
            }
        }

        public static void CombineSkinnedMeshes(GameObject go)
        {        
            SkinnedMeshRenderer[] smRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            List<Transform> bones = new List<Transform>();        
            List<BoneWeight> boneWeights = new List<BoneWeight>();        
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            List<Texture2D> textures = new List<Texture2D>();
            int numSubs = 0;
    
            foreach(SkinnedMeshRenderer smr in smRenderers) {
                numSubs += smr.sharedMesh.subMeshCount;
            }
    
            int[] meshIndex = new int[numSubs];
            int boneOffset = 0;
            for( int s = 0; s < smRenderers.Length; s++ ) {
                SkinnedMeshRenderer smr = smRenderers[s];          
    
                BoneWeight[] meshBoneweight = smr.sharedMesh.boneWeights;
    
                // May want to modify this if the renderer shares bones as unnecessary bones will get added.
                foreach( BoneWeight bw in meshBoneweight ) {
                    BoneWeight bWeight = bw;
    
                    bWeight.boneIndex0 += boneOffset;
                    bWeight.boneIndex1 += boneOffset;
                    bWeight.boneIndex2 += boneOffset;
                    bWeight.boneIndex3 += boneOffset;                
    
                    boneWeights.Add( bWeight );
                }
                boneOffset += smr.bones.Length;
    
                Transform[] meshBones = smr.bones;
                foreach( Transform bone in meshBones ) {
                    bones.Add( bone );
                }
    
                var renderer = smr.GetComponent<Renderer>();
                if( renderer != null & renderer.sharedMaterial.mainTexture != null ) {
                    Texture2D smt = renderer.sharedMaterial.mainTexture as Texture2D;
                    textures.Add(smt);
                }
    
                CombineInstance ci = new CombineInstance();
                ci.mesh = smr.sharedMesh;
                meshIndex[s] = ci.mesh.vertexCount;
                ci.transform = smr.transform.localToWorldMatrix;
                combineInstances.Add( ci );
    
                GameObject.DestroyImmediate(smr.gameObject);
            }
    
            List<Matrix4x4> bindposes = new List<Matrix4x4>();
    
            for( int b = 0; b < bones.Count; b++ ) {
                bindposes.Add( bones[b].worldToLocalMatrix * go.transform.worldToLocalMatrix );
            }
    
            SkinnedMeshRenderer r = go.AddComponent<SkinnedMeshRenderer>();
            r.sharedMesh = new Mesh();
            r.sharedMesh.CombineMeshes( combineInstances.ToArray(), true, true );
    
            Texture2D skinnedMeshAtlas = new Texture2D( 128, 128 );
            Rect[] packingResult = skinnedMeshAtlas.PackTextures( textures.ToArray(), 0 );
            Vector2[] originalUVs = r.sharedMesh.uv;
            Vector2[] atlasUVs = new Vector2[originalUVs.Length];
    
            int rectIndex = 0;
            int vertTracker = 0;
            for( int i = 0; i < atlasUVs.Length; i++ ) {

                try {
                    atlasUVs[i].x = Mathf.Lerp( packingResult[rectIndex].xMin, packingResult[rectIndex].xMax, originalUVs[i].x );
                } catch (Exception ex) {
                    UnityEngine.Debug.LogWarning(ex.Message);
                }
                
                try {
                    atlasUVs[i].y = Mathf.Lerp( packingResult[rectIndex].yMin, packingResult[rectIndex].yMax, originalUVs[i].y );            
                } catch (Exception ex) {
                    UnityEngine.Debug.LogWarning(ex.Message);
                }
    
                if( i >= meshIndex[rectIndex] + vertTracker ) {                
                    vertTracker += meshIndex[rectIndex];
                    rectIndex++;                
                }
            }
    
            Material combinedMat = new Material( Shader.Find( "Diffuse" ) );
            combinedMat.mainTexture = skinnedMeshAtlas;
            r.sharedMesh.uv = atlasUVs;
            r.sharedMaterial = combinedMat;
    
            r.bones = bones.ToArray();
            r.sharedMesh.boneWeights = boneWeights.ToArray();
            r.sharedMesh.bindposes = bindposes.ToArray();
            r.sharedMesh.RecalculateBounds();
        }

        public static void ClearConsoleLog(bool show = false) {
            //Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            //Type logEntries = assembly.GetType ("UnityEditorInternal.LogEntries");
            //MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
            //clearConsoleMethod.Invoke (new object (), null);
            //if (show) {
                // TODO: Show/Active Console Window 
            //}
        }

        public static string GetAssetsRootPath()
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - 6);
        }

        public static EditorWindow GetAssetStoreWindow()
        {
            EditorWindow result = null;
            Type AssetWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AssetStoreWindow");
            if (AssetWindowType != null)
            {
                result = (EditorWindow)AssetWindowType.GetMethod("Init", Tools.FullBinding).Invoke(null, null);
            }
            return result;
        }

        public static EditorWindow AttachToAssetStoreWindow(string title, string url)
        {
            EditorWindow result = null;
            EditorWindow assetStore = Tools.GetAssetStoreWindow();
            if (assetStore != null)
            {
                object webView = Unity3D2Babylon.Tools.GetInstanceField(assetStore.GetType(), assetStore, "webView");
                if (webView != null)
                {
                    if (!String.IsNullOrEmpty(url))
                    {
                        webView.GetType().GetMethod("LoadURL", Tools.FullBinding).Invoke(webView, new object[] { url });
                        result = assetStore;
                        if (!String.IsNullOrEmpty(title))
                        {
                            assetStore.titleContent.text = title;
                        }
                    }
                }
                assetStore.Show();
            }
            return result;
        }

        public static void ValidateLightmapSettings()
        {
            if (Lightmapping.realtimeGI != false) {
                UnityEngine.Debug.LogWarning("Lightmapping: Realtime global illumination should be disabled.");
            }
            if (Lightmapping.bakedGI != true) {
                UnityEngine.Debug.LogWarning("Lightmapping: Baked global illumination should be enabled.");
            }
            try
            {
                UnityEngine.Object lightmapSettings = Tools.GetLightmapSettings();
                if (lightmapSettings != null) {
                    SerializedObject serializedSettings = new SerializedObject(lightmapSettings);
                    SerializedProperty mixedLightingMode = serializedSettings.FindProperty("m_LightmapEditorSettings.m_MixedBakeMode");
                    if (mixedLightingMode.intValue != 1) {
                        UnityEngine.Debug.LogWarning("Lightmapping: Mixed lighting mode should be subtractive.");
                    }
                } else {
                    UnityEngine.Debug.LogWarning("Lightmapping: Failed to get lightmap editor settings");
                }
            } catch (Exception ex) {
                UnityEngine.Debug.LogException(ex);
            }
        }

        public static void RebuildProjectSourceCode()
        {
            string tag = "REBUILD";
            string tagx = tag + ";";
            string rebuild = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            if (rebuild.IndexOf(tagx) >= 0) {
                rebuild = rebuild.Replace(tagx, "");
            } else if (rebuild.IndexOf(tag) >= 0) {
                rebuild = rebuild.Replace(tag, "");
            } else {
                if (!String.IsNullOrEmpty(rebuild)) {
                    rebuild = tagx + rebuild;
                } else {
                    rebuild = tag + rebuild;
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, rebuild);
        }

        public static string GetDefaultProjectFolder()
        {
            string project = Tools.FormatSafePath(Application.dataPath.Replace("/Assets", "/Export"));
            if (!Directory.Exists(project))
            {
                Directory.CreateDirectory(project);
            }   
            return project;
        }

        public static string GetDefaultTypeScriptPath()
        {
            string result = String.Empty;
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                string tscOSX = "/usr/local/bin/tsc";
                if (File.Exists(tscOSX)) {
                    result = tscOSX;
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal).Replace("\\My Documents", "\\").Replace("\\Documents", "\\");
                string tscWIN = Path.Combine(home, "AppData\\Roaming\\npm\\node_modules\\typescript\\bin\\tsc");
                if (File.Exists(tscWIN)) {
                    result = tscWIN;
                }
            }
            return result;
        }

        public static string GetDefaultNodeRuntimePath()
        {
            string result = String.Empty;
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                string nodeOSX = "/usr/local/bin/node";
                if (File.Exists(nodeOSX)) {
                    result = nodeOSX;
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                string nodeWIN = "C:\\Program Files\\nodejs\\node.exe";
                if (File.Exists(nodeWIN)) {
                    result = nodeWIN;
                }
            }
            return result;
        }

        public static string GetShaderProgramSection(string name, string program)
        {
            string result = String.Empty;
            string[] lines = program.Split('\n');
            int babylonIndexStart = -1, babylonIndexEnd = -1;
            for (int ii = 0; ii < lines.Length; ii++)
            {
                string line = lines[ii];
                if (babylonIndexStart < 0 && line.IndexOf("#ifdef BABYLON_INFO", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    babylonIndexStart = ii;
                }
                if (babylonIndexEnd < 0 && line.IndexOf("#endif //BABYLON_INFO_END", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    babylonIndexEnd = ii;
                }
            }
            // Note: All Babylon Shader Blocks Are Required
            if (babylonIndexStart >= 0 && babylonIndexEnd >= 0)
            {
                int lineStart = babylonIndexStart, lineEnd = babylonIndexEnd;
                if (lineStart >= 0 && lineEnd >= 0)
                {
                    lineStart++;
                    for (int xx = lineStart; xx < lineEnd; xx++)
                    {
                        string buffer = lines[xx];
                        buffer = buffer.TrimEnd('\n').TrimEnd('\r');
                        result += (buffer + "\r\n");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Invalid Babylon Shader Block Lines: " + name);
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Invalid Babylon Shader Block Format: " + name);
            }
            return result;
        }

        public static string FormatProjectJavaScript(string buildFolder, string outputFile)
        {
            string javascriptFile = outputFile + ".js";
            if (File.Exists(javascriptFile))
            {
                try
                {
                    File.Delete(javascriptFile);
                }
                catch (Exception ex1)
                {
                    UnityEngine.Debug.LogException(ex1);
                }
            }
            if (File.Exists(javascriptFile))
            {
                UnityEngine.Debug.LogError("Failed to clear build file: " + javascriptFile);
            }
            return javascriptFile;
        }

        public static string GetSceneFileExtension()
        {
            return (ExporterWindow.exportationOptions.ScenePackingOptions == (int)BabylonPackingOption.Binary) ? ".babylon.bin" : ".babylon";
        }

        public static void GenerateProjectIndexPage(string project, bool debug, string scenePath, string sceneFilename, string scriptPath, string projectScript, string binaries, bool antialias, bool adaptive)
        {
            string responseText = String.Empty;
            string binariesPath = Path.Combine(project, binaries);
            string previewLibrary = Path.Combine(Application.dataPath, "Babylon/Library/");
            string previewTemplate = Path.Combine(Application.dataPath, "Babylon/Template/Config/index.html");
            if (!String.IsNullOrEmpty(previewTemplate) && File.Exists(previewTemplate))
            {
                responseText = File.ReadAllText(previewTemplate);
            }
            string faviconIco = Path.Combine(Application.dataPath, "Babylon/Template/Config/favicon.ico");
            string faviconIcoFile = Path.Combine(project, Path.GetFileName(faviconIco));
            try
            {
                File.Copy(faviconIco, faviconIcoFile, true);
            }
            catch (Exception ex0)
            {
                UnityEngine.Debug.LogException(ex0);
            }
            // Build project bin files
            if (ExporterWindow.exportationOptions.ExportHttpModule)
            {
                string httpModuleDll = Path.Combine(Application.dataPath, "Babylon/Template/Module/HttpBabylon.dll");
                string httpModuleDllFile = Path.Combine(binariesPath, Path.GetFileName(httpModuleDll));
                try
                {
                    File.Copy(httpModuleDll, httpModuleDllFile, true);
                }
                catch (Exception ex0)
                {
                    UnityEngine.Debug.LogException(ex0);
                }
                string httpModuleTxt = Path.Combine(Application.dataPath, "Babylon/Template/Module/HttpBabylon.txt");
                string httpModuleTxtFile = Path.Combine(binariesPath, Path.GetFileName(httpModuleTxt));
                try
                {
                    File.Copy(httpModuleTxt, httpModuleTxtFile, true);
                }
                catch (Exception ex0)
                {
                    UnityEngine.Debug.LogException(ex0);
                }
                string httpConfigTxt = Path.Combine(Application.dataPath, "Babylon/Template/Config/web.config");
                string httpConfigTxtFile = Path.Combine(project, Path.GetFileName(httpConfigTxt));
                try
                {
                    if (!File.Exists(httpConfigTxtFile)) {
                        File.Copy(httpConfigTxt, httpConfigTxtFile, true);
                    }
                }
                catch (Exception ex0)
                {
                    UnityEngine.Debug.LogException(ex0);
                }
            }
            // Build project script files
            string scripts = Path.Combine(project, scriptPath);
            string[] libs = Directory.GetFiles(previewLibrary, "*.bjs");
            if (libs != null && libs.Length > 0)
            {
                foreach (string lib in libs)
                {
                    string script = Path.Combine(scripts, (Path.GetFileNameWithoutExtension(lib) + ".js"));
                    try
                    {
                        string jscript = File.ReadAllText(lib);
                        string jsminify = Tools.MinifyJavascriptCode(jscript, Path.GetFileNameWithoutExtension(lib));
                        File.WriteAllText(script, jsminify);
                        // Compress javascript files
                        if (ExporterWindow.exportationOptions.PrecompressContent && File.Exists(script))
                        {
                            Tools.PrecompressFile(script, script +  ".gz");
                        }
                    }
                    catch (Exception ex0)
                    {
                        UnityEngine.Debug.LogException(ex0);
                    }
                }
            }

            // Build terrain shader script
            string shaderScript = String.Empty;
            DefaultAsset[] shaderVxs = Tools.GetAssetsOfType<DefaultAsset>(".vertex.fx");
            if (shaderVxs != null && shaderVxs.Length > 0)
            {
                foreach (var shaderVx in shaderVxs)
                {
                    // Terrain Vertex Program Only
                    if (shaderVx.name.Equals("splatmap.vertex", StringComparison.OrdinalIgnoreCase)) {
                        string basenameVx = shaderVx.name.Replace(".vertex", "").Replace("/", "_").Replace(" ", "_");
                        string filenameVx = AssetDatabase.GetAssetPath(shaderVx);
                        string programVx = Tools.LoadTextAsset(filenameVx);
                        if (!String.IsNullOrEmpty(programVx)) {
                            string programNameVx = basenameVx + "VertexShader";
                            shaderScript += String.Format("BABYLON.Effect.ShadersStore['{0}'] = window.atob(\"{1}\");\n\n", programNameVx, Tools.FormatBase64(programVx));
                        }
                    }
                }
            }
            DefaultAsset[] shaderFxs = Tools.GetAssetsOfType<DefaultAsset>(".fragment.fx");
            if (shaderFxs != null && shaderFxs.Length > 0)
            {
                foreach (var shaderFx in shaderFxs)
                {
                    // Terrain Fragment Program Only
                    if (shaderFx.name.Equals("splatmap.fragment", StringComparison.OrdinalIgnoreCase)) {
                        string basenameFx = shaderFx.name.Replace(".fragment", "").Replace("/", "_").Replace(" ", "_");
                        string filenameFx = AssetDatabase.GetAssetPath(shaderFx);
                        string programFx = Tools.LoadTextAsset(filenameFx);
                        if (!String.IsNullOrEmpty(programFx)) {
                            string programNameFx = basenameFx + "PixelShader";
                            shaderScript += String.Format("BABYLON.Effect.ShadersStore['{0}'] = window.atob(\"{1}\");\n\n", programNameFx, Tools.FormatBase64(programFx));
                        }
                    }
                }
            }
            if (String.IsNullOrEmpty(shaderScript))
            {
                shaderScript = "// BabylonJS Terrain Splatmap Shader";
            }
            string shaderScriptFile = Path.Combine(scripts, "terrains.js");
            File.WriteAllText(shaderScriptFile, shaderScript); 
            // Compress javascript files
            if (ExporterWindow.exportationOptions.PrecompressContent && File.Exists(shaderScriptFile))
            {
                Tools.PrecompressFile(shaderScriptFile, shaderScriptFile +  ".gz");
            }
            // Format index page tempate
            string formattedScript = scriptPath.TrimStart('/').TrimEnd('/').ToLower();
            responseText = responseText.Replace("###PROJECT###", projectScript);
            responseText = responseText.Replace("###TITLE###", Application.productName);
            responseText = responseText.Replace("###SCENE###", sceneFilename);
            responseText = responseText.Replace("###DEBUG###", debug.ToString().ToLower());
            responseText = responseText.Replace("###SCRIPT###", formattedScript);
            responseText = responseText.Replace("###PATH###", scenePath.TrimStart('/').TrimEnd('/').ToLower());
            responseText = responseText.Replace("###ANTIALIAS###", antialias.ToString().ToLower());
            responseText = responseText.Replace("###ADAPTIVE###", adaptive.ToString().ToLower());
            string page = Path.Combine(project, ExporterWindow.exportationOptions.DefaultIndexPage);
            File.WriteAllText(page, responseText);
        }

        public static void BuildProjectJavaScript(string buildFolder, string javascriptFile)
        {
            StreamWriter streamWriter = new StreamWriter(new FileStream(javascriptFile, FileMode.Create, FileAccess.Write));
            try
            {
                DefaultAsset[] assets = Tools.GetAssetsOfType<DefaultAsset>(".bjs");
                if (assets != null && assets.Length > 0)
                {
                    foreach (var asset in assets)
                    {
                        string path = AssetDatabase.GetAssetPath(asset);
                        if (!String.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            // Note: Babylon library javascript go to the script folder
                            if (path.IndexOf("/Babylon/Library/", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                string javascript = File.ReadAllText(path);
                                if (!String.IsNullOrEmpty(javascript))
                                {
                                    string name = Path.GetFileName(path).Replace(".bjs", ".js");
                                    string work = Path.Combine(buildFolder, name);
                                    File.WriteAllText(work, javascript);
                                    string jsminify = Tools.MinifyJavascriptCode(javascript, Path.GetFileNameWithoutExtension(work));
                                    if (!String.IsNullOrEmpty(jsminify))
                                    {
                                        streamWriter.Write(String.Format("// {0}\r\n", name));
                                        streamWriter.Write(jsminify);
                                        streamWriter.Write("\r\n\r\n");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                UnityEngine.Debug.LogException(ex2);
            }
            finally
            {
                streamWriter.Close();
            }
        }

        public static int BuildProjectTypeScript(string nodePath, string tscPath, string buildFolder, string javascriptFile, string tsconfigJson = null)
        {
            int result = -1;
            bool tscExists = (!String.IsNullOrEmpty(tscPath) && File.Exists(tscPath));
            bool nodeExists = (!String.IsNullOrEmpty(nodePath) && File.Exists(nodePath));
            if (nodeExists && tscExists)
            {
                string maps = (ExporterWindow.exportationOptions.PrettyPrintExport == true) ? "true" : "false";
                string work = Path.Combine(buildFolder, Path.GetFileName(javascriptFile.Replace(".babylon", "")));
                string root = Application.dataPath.Replace("/Assets", "");
                string json = Path.Combine(root, "tsconfig.json");
                string config = @" {
                    ""experimentalDecorators"": true,                    
                    ""compilerOptions"": {
                    ""target"": ""ES5"",
                    ""module"": ""system"",
                    ""allowJs"": false,
                    ""declaration"": true,
                    ""inlineSourceMap"": false,
                    ""sourceMap"": ###SOURCEMAP###,
                    ""outFile"": ""###OUTFILE###""
                    }
                }";
                if (String.IsNullOrEmpty(tsconfigJson))
                {
                    tsconfigJson = config;
                }
                tsconfigJson = tsconfigJson.Replace("###SOURCEMAP###", maps);
                tsconfigJson = tsconfigJson.Replace("###OUTFILE###", Tools.FormatSafePath(work));
                tsconfigJson = tsconfigJson.Replace("###OUTDIR###", Tools.FormatSafePath(buildFolder));
                File.WriteAllText(json, tsconfigJson);
                //* Execute typescript compiler
                Process process = new Process();
                process.StartInfo.FileName = nodePath;
                process.StartInfo.Arguments = String.Format("{0} -p .", tscPath);
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = root;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                //* Set ONLY ONE handler here.
                process.ErrorDataReceived += new DataReceivedEventHandler(OnBuildProjectTypeScriptError);
                //* Start process
                process.Start();
                //* Read one element asynchronously
                process.BeginErrorReadLine();
                //* Read the other one synchronously
                string output = process.StandardOutput.ReadToEnd();
                //* Log compiler output issues
                if (!String.IsNullOrEmpty(output))
                {
                    if (output.IndexOf("): error", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        UnityEngine.Debug.LogError(output);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning(output);
                    }
                }
                process.WaitForExit();
                result = process.ExitCode;
                if (result == 0)
                {
                    //* Parse compiled output files
                    if (File.Exists(work))
                    {
                        string sourcemap = work + ".map";
                        string javascript = File.ReadAllText(work);
                        if (!String.IsNullOrEmpty(javascript))
                        {
                            javascript = javascript.Replace(".js.map", ".babylon.js.map");
                            StreamWriter streamWriter = new StreamWriter(new FileStream(javascriptFile, FileMode.Append, FileAccess.Write));
                            try
                            {
                                string jsminify = Tools.MinifyJavascriptCode(javascript, Path.GetFileNameWithoutExtension(work));
                                if (!String.IsNullOrEmpty(jsminify))
                                {
                                    streamWriter.Write(String.Format("// {0}.ts\r\n", Path.GetFileNameWithoutExtension(javascriptFile).Replace(".babylon", "")));
                                    streamWriter.Write(jsminify);
                                    streamWriter.Write("\r\n\r\n");
                                }
                            }
                            catch (Exception ex)
                            {
                                UnityEngine.Debug.LogException(ex);
                            }
                            finally
                            {
                                streamWriter.Close();
                            }
                            // Copy Source Map Files
                            if (ExporterWindow.exportationOptions.PrettyPrintExport == true) {
                                if (File.Exists(sourcemap)) {
                                    string sourcemapFile = javascriptFile + ".map";
                                    File.Copy(sourcemap, sourcemapFile, true);
                                }
                            }
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Failed to parse output work script");
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to compile project script files");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Typescript files not compiled. Failed to locate typescript compilers.");
            }
            return result;
        }

        private static void OnBuildProjectTypeScriptError(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // * Log process output error
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                UnityEngine.Debug.LogError(outLine.Data);
            }
        }

        private static Assembly editorAsm;
        private static MethodInfo AddSortingLayer_Method;
         /// <summary> add a new sorting layer with default name </summary>
        public static void AddSortingLayer()
        {
            if (AddSortingLayer_Method == null)
            {
                if (editorAsm == null) editorAsm = Assembly.GetAssembly(typeof(Editor));
                System.Type t = editorAsm.GetType("UnityEditorInternal.InternalEditorUtility");
                AddSortingLayer_Method = t.GetMethod("AddSortingLayer", (BindingFlags.Static | BindingFlags.NonPublic));
            }
            AddSortingLayer_Method.Invoke(null, null);
        }

        /// <summary> Gets Lightmap Editor Settings </summary>
        public static UnityEngine.Object GetLightmapSettings()
        {
            return Tools.CallStaticReflectionMethod<UnityEngine.Object>(typeof(UnityEditor.LightmapEditorSettings), "GetLightmapSettings");
        }
    }

    public class BabylonCombinedMeshHelper
    {
        private List<BabylonCombinedMeshBuffer> bufferList;
        
        public BabylonCombinedMeshHelper()
        {
            bufferList = new List<BabylonCombinedMeshBuffer>();
        }

        public void AddItem(CombineInstance item)
        {
            if (item.mesh != null && item.mesh.vertexCount > 0)
            {
                BabylonCombinedMeshBuffer buffer = this.GetNextFreeBufferList(item.mesh.vertexCount);
                if (buffer == null)
                {
                    buffer = new BabylonCombinedMeshBuffer();
                    bufferList.Add(buffer);
                }
                if (item.mesh != null)
                {
                    CombineInstance combine = new CombineInstance();
                    combine.mesh = item.mesh;
                    combine.subMeshIndex = item.subMeshIndex;
                    combine.lightmapScaleOffset = item.lightmapScaleOffset;
                    combine.realtimeLightmapScaleOffset = item.realtimeLightmapScaleOffset;
                    combine.transform = item.transform;
                    buffer.items.Add(combine);
                }
            }
        }

        public List<CombineInstance[]> GetCombinedGroups()
        {
            List<CombineInstance[]> result = null;
            if (bufferList.Count > 0)
            {
                result = new List<CombineInstance[]>();
                foreach (var buffer in bufferList)
                {
                    result.Add(buffer.items.ToArray());
                }
            }
            return result;
        }

        private BabylonCombinedMeshBuffer GetNextFreeBufferList(int vertexCount)
        {
            BabylonCombinedMeshBuffer result = null;
            if (bufferList.Count > 0)
            {
                foreach (var buffer in bufferList)
                {
                    if (result == null) {
                        if (buffer.GetFreeBufferVertexSpace() >= vertexCount) {
                            result = buffer;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }

    public class BabylonCombinedMeshBuffer 
    {
        public List<CombineInstance> items;

        public BabylonCombinedMeshBuffer()
        {
            items = new List<CombineInstance>();
        }

        public int GetBufferVertexCount()
        {
            int count = 0;
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    count += item.mesh.vertexCount;
                }
            }
            return count;
        }

        public int GetFreeBufferVertexSpace()
        {
            return (ExporterWindow.MaxVerticies - this.GetBufferVertexCount());
        }
    }

    public class ToolkitContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            List<string> names = new List<string>();
            if (typeof(Vector2).IsAssignableFrom(type)) {
                names.AddRange(new string[] { "x", "y" });
            } else if (typeof(Vector3).IsAssignableFrom(type)) {
                names.AddRange(new string[] { "x", "y", "z" });
            } else if (typeof(Vector4).IsAssignableFrom(type)) {
                names.AddRange(new string[] { "x", "y", "z", "w" });
            } else if (typeof(Color).IsAssignableFrom(type)) {
                names.AddRange(new string[] { "r", "g", "b", "a" });
            } else if (typeof(Color32).IsAssignableFrom(type)) {
                names.AddRange(new string[] { "r", "g", "b", "a" });
            }
            if (names.Count > 0) {
                // only serializer properties that are in the name list
                properties = properties.Where(p => names.Contains(p.PropertyName)).ToList();
            }
            return properties;
        }
    }
}
