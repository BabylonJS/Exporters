using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [RequireComponent(typeof(UnityEngine.Light))]
    [AddComponentMenu("Babylon/System Components/Light Scale", 0)]
    public sealed class LightScale : EditorScriptComponent
    {
        [Range(0.0f, 10.0f)]
        public float lightIntensity = 1.0f;
        public BabylonLightIntensity intensityMode = BabylonLightIntensity.Automatic;
    }

    [CustomEditor(typeof(UnityEngine.Light)), CanEditMultipleObjects]
    public class LightScaleEditor : LightEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Light script = (Light)target;
            // ..
            // Test Internal Light Editor
            // ..
			//serializedObject.Update();
			//SerializedProperty test = serializedObject.FindProperty("m_Type");
			//if(test != null) EditorGUILayout.PropertyField(test);
			//UnityEngine.Object l = (UnityEngine.Object)test.objectReferenceValue;
            //if(GUILayout.Button(new GUIContent("Scale Lighting", "Scale Babylon Lighting"))) {
            //  TODO: Apply Lighting Scale To Light Properties
            //}
			//serializedObject.ApplyModifiedProperties();
        }
    }    
}