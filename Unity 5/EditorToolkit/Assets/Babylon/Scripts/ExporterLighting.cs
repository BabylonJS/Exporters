#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


public static class LightingSettingsHelper
{
    public static void SetFloat(string name, float val)
    {
        ChangeLighmapSettingsProperty(name, property => property.floatValue= val);
    }

    public static void SetInt(string name, int val)
    {
        ChangeLighmapSettingsProperty(name, property => property.intValue = val);
    }

    public static void SetBool(string name, bool val)
    {
        ChangeLighmapSettingsProperty(name, property => property.boolValue = val);
    }

    public static void ChangeLighmapSettingsProperty(string name, Action<SerializedProperty> changer)
    {
        var lightmapSettings = getLighmapSettings();
        var prop = lightmapSettings.FindProperty(name);
        if (prop != null)
        {
            changer(prop);
            lightmapSettings.ApplyModifiedProperties();
        }
        else Debug.LogError("lighmap property not found: " + name);
    }

    static SerializedObject getLighmapSettings()
    {
        var getLightmapSettingsMethod = typeof(LightmapEditorSettings).GetMethod("GetLightmapSettings", BindingFlags.Static | BindingFlags.NonPublic);
        var lightmapSettings = getLightmapSettingsMethod.Invoke(null, null) as Object;
        return new SerializedObject(lightmapSettings);
    }
    
    public static void SetScaleInLightmap(this MeshRenderer mr, float val)
    {
        SerializedObject so = new SerializedObject(mr);
        so.FindProperty("m_ScaleInLightmap").floatValue = val;
        so.ApplyModifiedProperties();
    }


    public static void SetIndirectResolution(float val)
    {
        SetFloat("m_LightmapEditorSettings.m_Resolution", val);
    }

    public static void SetAmbientOcclusion(float val)
    {
        SetFloat("m_LightmapEditorSettings.m_CompAOExponent", val);
    }

    public static void SetAmbientOcclusionEnabled(bool enabled)
    {
        SetBool("m_LightmapEditorSettings.m_AO", enabled);
    }

    public static void SetBakedGiEnabled(bool enabled)
    {
        SetBool("m_GISettings.m_EnableBakedLightmaps", enabled);
    }

    public static void SetFinalGatherEnabled(bool enabled)
    {
        SetBool("m_LightmapEditorSettings.m_FinalGather", enabled);
    }

    public static void SetFinalGatherRayCount(int val)
    {
        SetInt("m_LightmapEditorSettings.m_FinalGatherRayCount", val);
    }

    public static void SetPrecomputedRealtimeGIEnabled(bool enabled)
    {
        SetBool("m_GISettings.m_EnableRealtimeLightmaps", enabled);
    }

    public static void SetDirectionalMode(LightmapsMode mode)
    {
        SetInt("m_LightmapEditorSettings.m_LightmapsBakeMode", (int) mode);
    }

    public static void SetSettings(LightmapSettingsWrapper s)
    {
        LightmapEditorSettings.bakeResolution = s.bakedResolution;
        SetIndirectResolution(s.indirectResolution);
        SetAmbientOcclusion(s.ambientOcclusion);
        LightmapEditorSettings.aoMaxDistance = s.aoMaxDistance;
        SetFinalGatherEnabled(s.finalGather);
        SetFinalGatherRayCount(s.finalGatherRayCount);
        LightmapEditorSettings.maxAtlasWidth = LightmapEditorSettings.maxAtlasHeight = s.atlasSize;
        SetPrecomputedRealtimeGIEnabled(s.realtimeGi);
        SetBakedGiEnabled(s.bakedGi);
        SetDirectionalMode(s.lightmapsMode);
    }
}


[Serializable]
public class LightmapSettingsWrapper
{
    public float bakedResolution = 40;
    public float indirectResolution = 1;
    public float ambientOcclusion = .5f;
    public float aoMaxDistance = 1.5f;
    public int atlasSize = 128;

    [Header("Final gather")]
    public bool finalGather = false;
    public int finalGatherRayCount = 1024;

    [Header("Other")]
    public bool realtimeGi = false;
    public bool bakedGi = true;
    public LightmapsMode lightmapsMode;
}
#endif