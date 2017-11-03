using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Text;
using BabylonExport.Entities;

class ExporterTerrain : EditorWindow
{
    static TerrainData terrain;
    static Terrain terrainObject;
    BabylonTerrainFormat saveFormat = BabylonTerrainFormat.Triangles;
    BabylonTerrainResolution saveResolution = BabylonTerrainResolution.FullResolution;

    int tCount;
    int counter;
    int totalCount;
    bool flipNormals = true;
    int progressUpdateInterval = 10000;

    [MenuItem("BabylonJS/Terrain Exporter", false, 205)]
    static void InitTerrain()
    {
        ExporterTerrain terrains = ScriptableObject.CreateInstance<ExporterTerrain>();
        terrains.OnInitialize();
        terrains.ShowUtility();
    }

    public void OnInitialize()
    {
        maxSize = new Vector2(500, 90);
        minSize = this.maxSize;
        terrain = null;

        terrainObject = Selection.activeObject as Terrain;
        if (!terrainObject)
        {
            terrainObject = Terrain.activeTerrain;
        }
        if (terrainObject)
        {
            terrain = terrainObject.terrainData;
        }
    }

    void OnEnable()
    {
        titleContent = new GUIContent("Terrain Exporter");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        if (!terrain)
        {
            GUILayout.Label("No terrain found");
            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }
            return;
        }
        saveResolution = (BabylonTerrainResolution)EditorGUILayout.EnumPopup("Mesh Resolution", saveResolution);
        saveFormat = (BabylonTerrainFormat)EditorGUILayout.EnumPopup("Export Format", saveFormat);
        flipNormals = EditorGUILayout.Toggle("Reverse Normals", flipNormals);
        if (GUILayout.Button("Export Terrain"))
        {
            Export();
        }
    }

    void Export()
    {
        // Validate Project Platform
        if (!Unity3D2Babylon.Tools.ValidateProjectPlatform()) return;
        
        int index = 0;
        string fileName = EditorUtility.SaveFilePanelInProject("Export Terrain Geometry", "Terrain", "obj", "Export Raw Terrain Mesh Geometry - (OBJ)");
        BabylonMesh babylonMesh = new BabylonMesh();
        babylonMesh.numBoneInfluencers = Unity3D2Babylon.Tools.GetMaxBoneInfluencers();
        BabylonTerrainData terrainData = Unity3D2Babylon.Tools.CreateTerrainData(terrain, terrainObject.transform.localPosition, false);
        Unity3D2Babylon.Tools.GenerateBabylonMeshTerrainData(terrainData, babylonMesh, flipNormals);
        
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        StreamWriter sw = new StreamWriter(fileName);
        try
        {
            // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
            // Which is important when you're exporting huge terrains.
            sw.WriteLine("# U3D - BabylonJS - Terrain Geometry File");

            // Write vertices
            counter = tCount = 0;
            totalCount = ((babylonMesh.positions.Length / 3) * 2 + (babylonMesh.indices.Length / 3)) / progressUpdateInterval;
            for (index = 0; index < babylonMesh.positions.Length / 3; index++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("v ", 32);
                sb.Append(babylonMesh.positions[index * 3].ToString()).Append(" ").
                Append(babylonMesh.positions[index * 3 + 1].ToString()).Append(" ").
                Append(babylonMesh.positions[index * 3 + 2].ToString());
                sw.WriteLine(sb);
            }

            // Write normals
            for (index = 0; index < babylonMesh.normals.Length / 3; index++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("vn ", 32);
                sb.Append(babylonMesh.normals[index * 3].ToString()).Append(" ").
                Append(babylonMesh.normals[index * 3 + 1].ToString()).Append(" ").
                Append(babylonMesh.normals[index * 3 + 2].ToString());
                sw.WriteLine(sb);
            }

            // Write uvs
            for (index = 0; index < babylonMesh.uvs.Length / 2; index++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("vt ", 32);
                sb.Append(babylonMesh.uvs[index * 2].ToString()).Append(" ").
                Append(babylonMesh.uvs[index * 2 + 1].ToString());
                sw.WriteLine(sb);
            }

            // Write triangles
            for (int i = 0; i < babylonMesh.indices.Length; i += 3)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("f ", 64);
                sb.Append(babylonMesh.indices[i] + 1).Append("/").Append(babylonMesh.indices[i] + 1).Append(" ").
                Append(babylonMesh.indices[i + 1] + 1).Append("/").Append(babylonMesh.indices[i + 1] + 1).Append(" ").
                Append(babylonMesh.indices[i + 2] + 1).Append("/").Append(babylonMesh.indices[i + 2] + 1);
                sw.WriteLine(sb);
            }
        }
        catch (Exception err)
        {
            Debug.Log("Error saving file: " + err.Message);
        }
        sw.Close();

        terrain = null;
        EditorUtility.DisplayProgressBar("Babylon.js", "Saving terrain geometry data... This may take a while.", 1f);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        this.Close();
    }

    void UpdateProgress()
    {
        if (counter++ == progressUpdateInterval)
        {
            counter = 0;
            EditorUtility.DisplayProgressBar("Generating terrain geometry...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
        }
    }
}