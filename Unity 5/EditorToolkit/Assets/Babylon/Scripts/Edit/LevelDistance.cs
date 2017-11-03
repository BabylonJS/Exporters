using System;
using UnityEngine;
using Unity3D2Babylon;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Level Distance", 14)]
    public sealed class LevelDistance : EditorScriptComponent
    {
        [Range(0.1f, 1.0f)]
        public float cameraDistanceFactor = -1;

        [Header("[Distance Properties]"), Unity3D2Babylon.ReadOnly]
        public LODGroup levelDetailGroup = null;

        [Unity3D2Babylon.ReadOnly]
        public List<string> groupLevelDistances = new List<string>();        

        void OnGUI()
        {
            //foreach (var kvp in _myDictionary)
            //{
            //    GUILayout.Label("Key: " + kvp.Key + " value: " + kvp.Value);
            //}
        }
    }

    [CustomEditor(typeof(LevelDistance)), CanEditMultipleObjects]
    public class LevelDistanceEditor : Editor
    {
        public void OnEnable()
        {
            LevelDistance myScript = (LevelDistance)target;
            myScript.levelDetailGroup = myScript.gameObject.GetComponent<LODGroup>();
            if (myScript.cameraDistanceFactor == -1) {
                myScript.cameraDistanceFactor = ExporterWindow.exportationOptions.CameraDistanceFactor;
            }
            RecalculateLevelDistances();
        }
        public override void OnInspectorGUI()
        {
            LevelDistance myScript = (LevelDistance)target;
            EditorGUILayout.Space();
            serializedObject.Update();
            LevelDistanceEditorList.Show(serializedObject.FindProperty("cameraDistanceFactor"), this);
            LevelDistanceEditorList.Show(serializedObject.FindProperty("levelDetailGroup"), this);
            LevelDistanceEditorList.Show(serializedObject.FindProperty("groupLevelDistances"), this);
            serializedObject.ApplyModifiedProperties();
        }

        public void RecalculateLevelDistances()
        {
            LevelDistance myScript = (LevelDistance)target;
            if (myScript.levelDetailGroup != null) {
                myScript.groupLevelDistances.Clear();
                var lodGroup = myScript.levelDetailGroup;
                float nearClipingPlane = (Camera.main != null) ? Camera.main.nearClipPlane : 0.3f;
                float farClipingPlane = (Camera.main != null) ? Camera.main.farClipPlane : 1000.0f;
                LOD[] lods = lodGroup.GetLODs();
                float startingPercent = -1f;
                float endingPercent = -1f;
                float lastPercent = 0f;
                int startRange = 0;
                int index = 0;
                foreach (var lod in lods) {
                    endingPercent = lod.screenRelativeTransitionHeight;
                    if (startingPercent == -1) startingPercent = 1;
                    else startingPercent = lastPercent;
                    float lodDistance = startingPercent - endingPercent;
                    string lodFormatted = String.Format("LOD{0} - Distance: {1}", index.ToString(), startRange.ToString());
                    myScript.groupLevelDistances.Add(lodFormatted);
                    lastPercent = endingPercent;
                    startRange += Unity3D2Babylon.Tools.CalculateCameraDistance(nearClipingPlane, farClipingPlane, lodDistance, myScript);
                    index++;
                }
                float startCulling = startRange;
                string cullingFormatted = String.Format("LODX - Distance: {0}", startCulling.ToString());
                myScript.groupLevelDistances.Add(cullingFormatted);
            }
        }
    }
   
    public static class LevelDistanceEditorList
    {
        public static void Show (SerializedProperty item, LevelDistanceEditor editor)
        {
            EditorGUILayout.PropertyField(item);
            if (item.isArray) {
                EditorGUI.indentLevel += 1;
                if (item.isExpanded) {
                    if (item.name == "groupLevelDistances") {
                        editor.RecalculateLevelDistances();
                    }
                    for (int i = 0; i < item.arraySize; i++) {
                        EditorGUILayout.PropertyField(item.GetArrayElementAtIndex(i));
                    }
                }
                EditorGUI.indentLevel -= 1;
            }
        }
    }
}