using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity3D2Babylon
{
    public class ExporterOptimizer : EditorWindow
    {
        GameObject parentObject;
        BabylonToolkitType meshToolkitType = BabylonToolkitType.CombineMeshes;
        BabylonTextureMode textureAtlasMode = BabylonTextureMode.CombineMeshes;
        BabylonTextureScale textureImageScaling = BabylonTextureScale.Bilinear;
        BabylonImageFormat textureAtlasFormat = BabylonImageFormat.PNG;
        Shader textureAtlasShader = null;
        int textureAtlasSize = 4096;
        int maxTextureImageSize = 0;
        bool bakeTextureMaps = true;
        bool linearInterpolation = true;
        bool bakeAtlasColliders = true;
        GameObject bakingDestination = null;
        bool bakeAlphaEncoding = false;
        bool keepGeneratorOpen = true;

        // Texture Atlas Buffers        
        Material atlasMaterial = null;
        List<Texture2D> mainTextures = null;
        List<Texture2D> bumpTextures = null;
        List<Texture2D> ambientTextures = null;
        List<Texture2D> emissionTextures = null;
        List<Texture2D> metallicTextures = null;
        List<Texture2D> specularTextures = null;
        string bumpFilename = String.Empty;
        string ambientFilename = String.Empty;
        string emissionFilename = String.Empty;
        string metallicFilename = String.Empty;
        string specularFilename = String.Empty;

        // No Prebaked Lightmap Data
        bool enableLightmapData = false;

        // Bake Blocking Volumes
        BabylonBlockingVolume blockingVolumeMode = BabylonBlockingVolume.BakeColliders;

        bool bakeColliderGeometry = false;

        [MenuItem("Babylon/Geometry Tools", false, 205)]
        public static void InitConverter()
        {
            ExporterOptimizer combiner = ScriptableObject.CreateInstance<ExporterOptimizer>();
            combiner.OnInitialize();
            combiner.ShowUtility();
        }

        public void OnInitialize()
        {
            maxSize = new Vector2(500.0f, 190.0f);
            minSize = this.maxSize;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Geometry Tools");
            textureAtlasShader = Shader.Find("Standard");
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();
            meshToolkitType = (BabylonToolkitType)EditorGUILayout.EnumPopup("Mesh Toolkit Mode:", meshToolkitType, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            string bakeLabel = "Bake Optimized Meshes";
            bool showParentDetination = false;
            if (meshToolkitType == BabylonToolkitType.CombineMeshes) {
                showParentDetination = true;
                bakeLabel = "Bake Combined Meshes";
                if (this.maxSize.y != 139.0f) {
                    this.maxSize = new Vector2(500.0f, 139.0f);
                    this.minSize = this.maxSize;
                }
            } else if (meshToolkitType == BabylonToolkitType.SeperateMeshes) {
                showParentDetination = true;
                bakeLabel = "Bake Seperate Meshes";
                if (this.maxSize.y != 139.0f) {
                    this.maxSize = new Vector2(500.0f, 139.0f);
                    this.minSize = this.maxSize;
                }
            } else if (meshToolkitType == BabylonToolkitType.BlockingVolumes) {
                if (blockingVolumeMode == BabylonBlockingVolume.RemoveColliders) {
                    showParentDetination = false;
                    bakeLabel = "Remove Blocking Volumes";
                    if (this.maxSize.y != 139.0f) {
                        this.maxSize = new Vector2(500.0f, 139.0f);
                        this.minSize = this.maxSize;
                    }
                } else {
                    showParentDetination = true;
                    bakeLabel = "Bake Blocking Volumes";
                    if (this.maxSize.y != 191.0f) {
                        this.maxSize = new Vector2(500.0f, 191.0f);
                        this.minSize = this.maxSize;
                    }
                }
            } else if (meshToolkitType == BabylonToolkitType.BakeTextureAtlas) {
                bakeLabel = "Bake Texture Atlas";
                showParentDetination = true;
                if (this.maxSize.y != 399.0f) {
                    this.maxSize = new Vector2(500.0f, 399.0f);
                    this.minSize = this.maxSize;
                }
            }
            parentObject = EditorGUILayout.ObjectField("Parent Game Object:", parentObject, typeof(GameObject), true) as GameObject;
            EditorGUILayout.Space();

            if (meshToolkitType == BabylonToolkitType.BlockingVolumes)
            {
                blockingVolumeMode = (BabylonBlockingVolume)EditorGUILayout.EnumPopup("Blocking Volume Mode:", blockingVolumeMode, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                if (blockingVolumeMode == BabylonBlockingVolume.BakeColliders) {
                    bakeColliderGeometry = EditorGUILayout.Toggle("Bake Collider Geometry:", bakeColliderGeometry);
                    EditorGUILayout.Space();
                }
            }

            if (meshToolkitType == BabylonToolkitType.BakeTextureAtlas)
            {
                bakeTextureMaps = EditorGUILayout.Toggle("Bake Texture Maps:", bakeTextureMaps);
                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(true);
                bakeAlphaEncoding = EditorGUILayout.Toggle("Bake Image Alpha:", bakeAlphaEncoding);
                EditorGUILayout.Space();
                EditorGUI.EndDisabledGroup();
                textureAtlasMode = (BabylonTextureMode)EditorGUILayout.EnumPopup("Texture Atlas Mode:", textureAtlasMode, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                textureAtlasSize = (int)EditorGUILayout.Slider("Texture Atlas Size:", textureAtlasSize, 128, 8192);
                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(true);
                    textureAtlasShader = EditorGUILayout.ObjectField("Texture Atlas Shader:", textureAtlasShader, typeof(Shader), true) as Shader;
                    EditorGUILayout.Space();
                    textureAtlasFormat = (BabylonImageFormat)EditorGUILayout.EnumPopup("Texture Atlas Format:", textureAtlasFormat, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Space();
                EditorGUI.EndDisabledGroup();
                textureImageScaling = (BabylonTextureScale)EditorGUILayout.EnumPopup("Texture Image Scale:", textureImageScaling, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                maxTextureImageSize = (int)EditorGUILayout.Slider("Texture Image Max:", maxTextureImageSize, 0, 4096);
                EditorGUILayout.Space();
                bakeAtlasColliders = EditorGUILayout.Toggle("Bake Blocking Volumes:", bakeAtlasColliders);
                EditorGUILayout.Space();
                linearInterpolation = EditorGUILayout.Toggle("Use Linear Interpolation:", linearInterpolation);
                EditorGUILayout.Space();
            }

            if (showParentDetination)
            {
                bakingDestination = EditorGUILayout.ObjectField("Alt Bake Destination:", bakingDestination, typeof(GameObject), true) as GameObject;
                EditorGUILayout.Space();
            }
            
            keepGeneratorOpen = EditorGUILayout.Toggle("Keep Generator Open:", keepGeneratorOpen);
            EditorGUILayout.Space();
            if (GUILayout.Button(bakeLabel))
            {
                Optimize();
            }
        }

        public void Optimize()
        {
            // Validate Project Platform
            if (!Unity3D2Babylon.Tools.ValidateProjectPlatform()) return;

            try
            {
                atlasMaterial = null;
                mainTextures = null;
                bumpTextures = null;
                ambientTextures = null;
                emissionTextures = null;
                metallicTextures = null;
                specularTextures = null;
                bumpFilename = String.Empty;
                ambientFilename = String.Empty;
                emissionFilename = String.Empty;
                metallicFilename = String.Empty;
                specularFilename = String.Empty;

                if (meshToolkitType == BabylonToolkitType.BlockingVolumes) {
                    CreateBlockingVolumes();
                } else {
                    OptimizeStaticMeshes();
                    if (meshToolkitType == BabylonToolkitType.BakeTextureAtlas) {
                        PackTextureAtlasNormals();
                        PackTextureAtlasAmbients();
                        PackTextureAtlasEmissions();
                        PackTextureAtlasMetallics();
                        PackTextureAtlasSpeculars();
                    }
                }

                ExporterWindow.ReportProgress(1, "Saving assets to disk...");
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                ExporterWindow.ReportProgress(1, "Refresing assets database...");
                AssetDatabase.Refresh();
            }
            ExporterWindow.ReportProgress(1, "Geometry optimzation complete.");
            EditorUtility.ClearProgressBar();
            if (this.keepGeneratorOpen) {
                ExporterWindow.ShowMessage("Geometry optimzation complete.", "Babylon.js");
            } else {
                this.Close();
            }
        }

        public void CreateBlockingVolumes(string file = null, GameObject baked = null)
        {
            BabylonBlockingVolume bmode = blockingVolumeMode;
            if (!String.IsNullOrEmpty(file)) {
                bmode = BabylonBlockingVolume.BakeColliders;
            }
            string parentName = parentObject.name;
            Collider[] colliders = parentObject.GetComponentsInChildren<Collider>();
            if (colliders != null && colliders.Length > 0)
            {
                if (bmode == BabylonBlockingVolume.RemoveColliders)
                {
                    if (ExporterWindow.ShowMessage("Are you sure you want to remove all colliders?", "Babylon.js", "Remove", "Cancel"))
                    {
                        ExporterWindow.ReportProgress(1, "Removing blocking volume geometry...  This may take a while.");
                        foreach (var collider in colliders)
                        {
                            DestroyImmediate(collider, false);
                        }
                    }                    
                } else { 
                    string filename = (!String.IsNullOrEmpty(file)) ? file : EditorUtility.SaveFilePanelInProject("Static Geometry Tools", "", "asset", "Bake Blocking Volume Colliders");
                    if (!String.IsNullOrEmpty(filename))
                    {
                        ExporterWindow.ReportProgress(1, "Generating blocking volume geometry...  This may take a while.");
                        string filepath = Path.GetDirectoryName(filename);
                        string filelabel = Path.GetFileNameWithoutExtension(filename);
                        Tools.ValidateAssetFolders(filepath.TrimEnd('/'));
                        Material defaultMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                    
                        // Parent game object
                        GameObject po = (baked != null) ? baked : bakingDestination;
                        if (po == null) {
                            po = new GameObject(parentName + " Baked");
                            po.transform.parent = null;
                            po.transform.localPosition = Vector3.zero;
                            po.transform.localRotation = Quaternion.identity;
                            po.transform.localScale = Vector3.one;
                        }
                        int index = 0;
                        Matrix4x4 localMatrix = po.transform.worldToLocalMatrix;
                        foreach (var collider in colliders)
                        {
                            index++;
                            GameObject go = new GameObject(collider.gameObject.name + "_" + index.ToString());
                            go.transform.parent = po.transform;
                            go.transform.localPosition = collider.gameObject.transform.localPosition;
                            go.transform.localRotation = collider.gameObject.transform.localRotation;
                            go.transform.localScale = collider.gameObject.transform.localScale;
                            if (bakeColliderGeometry == true)
                            {
                                // Bake Collision Mesh Geometry
                                Mesh collisionMesh = Tools.CreateCollisionGeometry(collider, localMatrix, enableLightmapData);
                                if (collisionMesh != null) {
                                    string label = Tools.FirstUpper(go.name).MakeSafe();
                                    collisionMesh.name = String.Format("{0}_{1}_Collider", filelabel, label);
                                    string meshFile = String.Format("{0}/Geometry/{1}.asset", filepath.TrimEnd('/'), collisionMesh.name);
                                    AssetDatabase.CreateAsset(collisionMesh, meshFile);
                                    
                                    var filter = go.AddComponent<MeshFilter>();
                                    filter.sharedMesh = collisionMesh;

                                    var arenderer = go.AddComponent<MeshRenderer>();
                                    arenderer.sharedMaterial = defaultMaterial;

                                    var tags = go.AddComponent<TagsComponent>();
                                    tags.babylonTags = "[COLLIDER]";

                                    var details = go.AddComponent<MeshDetails>();
                                    details.generateCollider = false;
                                    details.forceCheckCollisions = true;
                                    details.meshVisibilityProperties = new BabylonOverrideVisibility();
                                    details.meshVisibilityProperties.overrideVisibility = true;
                                    details.meshVisibilityProperties.makeMeshVisible = false;
                                    details.meshVisibilityProperties.newVisibilityLevel = ExporterWindow.exportationOptions.ColliderVisibility;
                                } else {
                                    UnityEngine.Debug.LogWarning("Failed to create collision geometry for collider: " + collider.name);
                                }
                            }
                            else
                            {
                                // Copy Collider Component
                                if (Tools.CopyComponent(collider)) {
                                    if (Tools.PasteComponentAsNew(go)) {
                                        if (collider is MeshCollider) {
                                            // Copy Actual Mesh Collider Geometry                   
                                            MeshCollider meshCollider = collider as MeshCollider;
                                            if (meshCollider.sharedMesh != null) {
                                                Mesh collisionMesh = meshCollider.sharedMesh.Copy(false, localMatrix, enableLightmapData);
                                                if (collisionMesh != null) {
                                                    string label = Tools.FirstUpper(go.name).MakeSafe();
                                                    collisionMesh.name = String.Format("{0}_{1}_Collider", filelabel, label);
                                                    string meshFile = String.Format("{0}/Geometry/{1}.asset", filepath.TrimEnd('/'), collisionMesh.name);
                                                    AssetDatabase.CreateAsset(collisionMesh, meshFile);
                                                    MeshCollider newMeshCollider = go.GetComponent<MeshCollider>();
                                                    if (newMeshCollider != null) {
                                                        newMeshCollider.sharedMesh = collisionMesh;
                                                    } else {
                                                        UnityEngine.Debug.LogWarning("Failed to create new collider for: " + collider.name + " to " + go.name);
                                                    }
                                                }
                                            }
                                        }
                                    } else {
                                        UnityEngine.Debug.LogWarning("Failed to paste collider: " + collider.name + " to " + go.name);
                                    }
                                } else {
                                    UnityEngine.Debug.LogWarning("Failed to copy collider: " + collider.name);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void OptimizeStaticMeshes()
        {
            string parentName = parentObject.name;
            MeshRenderer[] meshRenderers = parentObject.GetComponentsInChildren<MeshRenderer>();
            MeshFilter[] meshFilters = parentObject.GetComponentsInChildren<MeshFilter>();
            if (meshRenderers != null && meshRenderers.Length > 0 && meshFilters != null && meshFilters.Length > 0)
            {
                string filename = EditorUtility.SaveFilePanelInProject("Static Geometry Tools", "", "asset", "Bake Optimized Static Mesh Geometry");
                if (!String.IsNullOrEmpty(filename))
                {
                    ExporterWindow.ReportProgress(1, "Optimizing static mesh geometry...  This may take a while.");
                    string filepath = Path.GetDirectoryName(filename);
                    string filelabel = Path.GetFileNameWithoutExtension(filename);
                    Tools.ValidateAssetFolders(filepath.TrimEnd('/'));

                    // Parent game object
                    GameObject po = bakingDestination;
                    if (po == null) {
                        po = new GameObject(parentName + " Baked");
                        po.transform.parent = null;
                        po.transform.localPosition = Vector3.zero;
                        po.transform.localRotation = Quaternion.identity;
                        po.transform.localScale = Vector3.one;
                    }

                    Matrix4x4 myTransform = po.transform.worldToLocalMatrix;
                    Dictionary<string, Material> namedMaterials = new Dictionary<string, Material>();
                    Dictionary<string, List<BabylonCombineInstance>> bakingList = new Dictionary<string, List<BabylonCombineInstance>>();

                    // Gather All Named Materials
                    foreach (var meshRenderer in meshRenderers) {
                        foreach (var material in meshRenderer.sharedMaterials) {
                            if (material != null && !namedMaterials.ContainsKey(material.name)) {
                                namedMaterials.Add(material.name, material);
                            }
                        }
                    }

                    // Filter All Combined Mesh Instances
                    foreach (var filter in meshFilters) {
                        if (filter.sharedMesh == null || filter.sharedMesh.vertexCount <= 0)
                            continue;
                        var filterRenderer = filter.GetComponent<Renderer>();
                        if (filterRenderer.sharedMaterials == null || filterRenderer.sharedMaterials.Length <= 0)
                            continue;
                        var matrix = myTransform * filter.transform.localToWorldMatrix;
                        int subs = filter.sharedMesh.subMeshCount;
                        if (subs > 1) {
                            for(int i = 0; i < subs; i++) {
                                if (i < filterRenderer.sharedMaterials.Length) {
                                    Mesh mesh = filter.sharedMesh.GetSubmesh(i);
                                    if (mesh != null && mesh.vertexCount > 0) {
                                        var material = filterRenderer.sharedMaterials[i];
                                        string key = material.name;
                                        List<BabylonCombineInstance> list;
                                        if (bakingList.ContainsKey(key)) {
                                            list = bakingList[key];
                                        } else {
                                            list = new List<BabylonCombineInstance>();
                                            bakingList.Add(key, list);
                                        }
                                        list.Add(new BabylonCombineInstance(mesh, matrix, filter));
                                    }
                                }
                            }
                        } else {
                            Mesh mesh = filter.sharedMesh.Copy();
                            if (mesh != null && mesh.vertexCount > 0) {
                                var material = filterRenderer.sharedMaterials[0];
                                string key = material.name;
                                List<BabylonCombineInstance> list;
                                if (bakingList.ContainsKey(key)) {
                                    list = bakingList[key];
                                } else {
                                    list = new List<BabylonCombineInstance>();
                                    bakingList.Add(key, list);
                                }
                                list.Add(new BabylonCombineInstance(mesh, matrix, filter));
                            }
                        }
                    }

                    // Bake All Combined Material Meshes
                    int bakingCount = bakingList.Count;
                    if (bakingCount > 0) {
                        if (meshToolkitType == BabylonToolkitType.BakeTextureAtlas) {
                            GameObject atlasParent = po;
                            GameObject colliderParent = null;
                            ExporterWindow.ReportProgress(1, "Packing texture atlas material... This may take a while.");
                            if (bakeAtlasColliders == true) {
                                atlasParent = new GameObject("Meshes");
                                atlasParent.transform.parent = po.transform;
                                atlasParent.transform.localPosition = Vector3.zero;
                                atlasParent.transform.localRotation = Quaternion.identity;
                                atlasParent.transform.localScale = Vector3.one;
                                colliderParent = new GameObject("Colliders");
                                colliderParent.transform.parent = po.transform;
                                colliderParent.transform.localPosition = Vector3.zero;
                                colliderParent.transform.localRotation = Quaternion.identity;
                                colliderParent.transform.localScale = Vector3.one;
                            }
                            // Bake Texture Atlas
                            string atlasExt = "png";
                            string atlasName = String.Format("{0}_Texture_Atlas", filelabel);
                            string atlasFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), atlasName, atlasExt);
                            string bumpName = String.Format("{0}_Normal_Atlas", filelabel);
                            string bumpFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), bumpName, atlasExt);
                            string ambientName = String.Format("{0}_Ambient_Atlas", filelabel);
                            string ambientFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), ambientName, atlasExt);
                            string emissionName = String.Format("{0}_Emission_Atlas", filelabel);
                            string emissionFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), emissionName, atlasExt);
                            string metallicName = String.Format("{0}_Metallic_Atlas", filelabel);
                            string metallicFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), metallicName, atlasExt);
                            string specularName = String.Format("{0}_Specular_Atlas", filelabel);
                            string specularFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), specularName, atlasExt);
                            string materialName = String.Format("{0}_Material", filelabel);
                            string materialFile = String.Format("{0}/Materials/{1}.asset", filepath.TrimEnd('/'), materialName);
                            // Create atlas textures
                            mainTextures = new List<Texture2D>();
                            bumpTextures = new List<Texture2D>();
                            ambientTextures = new List<Texture2D>();
                            emissionTextures = new List<Texture2D>();
                            metallicTextures = new List<Texture2D>();
                            specularTextures = new List<Texture2D>();
                            foreach (var baker in bakingList)
                            {
                                Material material = namedMaterials[baker.Key];
                                // Combine main textures
                                Texture2D colorTexture = null;
                                Texture2D normalTexture = null;
                                Texture2D ambientTexture = null;
                                Texture2D emissionTexture = null;
                                Texture2D metallicTexture = null;
                                Texture2D specularTexture = null;
                                bool metallicTextureSRGB = true;
                                if (material.mainTexture != null) {
                                    Texture2D primaryTexture = material.mainTexture as Texture2D;
                                    primaryTexture.ForceReadable();
                                    colorTexture = primaryTexture.Copy();
                                    if (colorTexture != null && bakeTextureMaps == true) {
                                        if (material.HasProperty("_BumpMap")) {
                                            Texture2D bumpTexture = material.GetTexture("_BumpMap") as Texture2D;
                                            if (bumpTexture != null && bumpTexture.width == colorTexture.width && bumpTexture.height == colorTexture.height) {
                                                // Format texture import settings
                                                string bumpTexturePath = AssetDatabase.GetAssetPath(bumpTexture);
                                                var importTool = new BabylonTextureImporter(bumpTexturePath);
                                                var importType = importTool.textureImporter.textureType;
                                                try
                                                {
                                                    importTool.textureImporter.isReadable = true;
                                                    importTool.textureImporter.textureType = TextureImporterType.Default;
                                                    importTool.ForceUpdate();
                                                    normalTexture = bumpTexture.Copy();
                                                }
                                                catch(Exception ex)
                                                {
                                                    UnityEngine.Debug.LogException(ex);
                                                }
                                                finally
                                                {
                                                    // Restore texture importer type
                                                    importTool.textureImporter.textureType = importType;
                                                    importTool.ForceUpdate();
                                                }
                                            }
                                        }
                                        if (material.HasProperty("_EmissionMap")) {
                                            Texture2D emitTexture = material.GetTexture("_EmissionMap") as Texture2D;
                                            if (emitTexture != null)
                                            {
                                                // Format texture import settings
                                                string emissionTexturePath = AssetDatabase.GetAssetPath(emitTexture);
                                                var importTool = new BabylonTextureImporter(emissionTexturePath);
                                                var importType = importTool.textureImporter.textureType;
                                                try
                                                {
                                                    importTool.textureImporter.isReadable = true;
                                                    importTool.textureImporter.textureType = TextureImporterType.Default;
                                                    importTool.ForceUpdate();
                                                    emissionTexture = emitTexture.Copy();
                                                    if (emissionTexture.width != colorTexture.width || emissionTexture.height != colorTexture.height) {
                                                        emissionTexture.Scale(colorTexture.width, colorTexture.height);
                                                    }
                                                }
                                                catch(Exception ex)
                                                {
                                                    UnityEngine.Debug.LogException(ex);
                                                }
                                                finally
                                                {
                                                    // Restore texture importer type
                                                    importTool.textureImporter.textureType = importType;
                                                    importTool.ForceUpdate();
                                                }
                                            }
                                        }
                                        if (material.HasProperty("_OcclusionMap")) {
                                            Texture2D occlusionTexture = material.GetTexture("_OcclusionMap") as Texture2D;
                                            if (occlusionTexture != null)
                                            {
                                                // Format texture import settings
                                                string occlusionTexturePath = AssetDatabase.GetAssetPath(occlusionTexture);
                                                var importTool = new BabylonTextureImporter(occlusionTexturePath);
                                                var importType = importTool.textureImporter.textureType;
                                                try
                                                {
                                                    importTool.textureImporter.isReadable = true;
                                                    importTool.textureImporter.textureType = TextureImporterType.Default;
                                                    importTool.ForceUpdate();
                                                    ambientTexture = occlusionTexture.Copy();
                                                    if (ambientTexture.width != colorTexture.width || ambientTexture.height != colorTexture.height) {
                                                        ambientTexture.Scale(colorTexture.width, colorTexture.height);
                                                    }
                                                }
                                                catch(Exception ex)
                                                {
                                                    UnityEngine.Debug.LogException(ex);
                                                }
                                                finally
                                                {
                                                    // Restore texture importer type
                                                    importTool.textureImporter.textureType = importType;
                                                    importTool.ForceUpdate();
                                                }
                                            }
                                        }
                                        if (material.HasProperty("_MetallicGlossMap")) {
                                            Texture2D metalnessTexture = material.GetTexture("_MetallicGlossMap") as Texture2D;
                                            if (metalnessTexture != null)
                                            {
                                                // Format texture import settings
                                                metallicTextureSRGB = metalnessTexture.IsSRGB();
                                                string metalnessTexturePath = AssetDatabase.GetAssetPath(metalnessTexture);
                                                var importTool = new BabylonTextureImporter(metalnessTexturePath);
                                                var importType = importTool.textureImporter.textureType;
                                                try
                                                {
                                                    importTool.textureImporter.isReadable = true;
                                                    importTool.textureImporter.textureType = TextureImporterType.Default;
                                                    importTool.ForceUpdate();
                                                    metallicTexture = metalnessTexture.Copy();
                                                    if (metallicTexture.width != colorTexture.width || metallicTexture.height != colorTexture.height) {
                                                        metallicTexture.Scale(colorTexture.width, colorTexture.height);
                                                    }
                                                }
                                                catch(Exception ex)
                                                {
                                                    UnityEngine.Debug.LogException(ex);
                                                }
                                                finally
                                                {
                                                    // Restore texture importer type
                                                    importTool.textureImporter.textureType = importType;
                                                    importTool.ForceUpdate();
                                                }
                                            }
                                        }
                                        if (material.HasProperty("_SpecGlossMap")) {
                                            Texture2D glossTexture = material.GetTexture("_SpecGlossMap") as Texture2D;
                                            if (glossTexture != null)
                                            {
                                                // Format texture import settings
                                                string specularTexturePath = AssetDatabase.GetAssetPath(glossTexture);
                                                var importTool = new BabylonTextureImporter(specularTexturePath);
                                                var importType = importTool.textureImporter.textureType;
                                                try
                                                {
                                                    importTool.textureImporter.isReadable = true;
                                                    importTool.textureImporter.textureType = TextureImporterType.Default;
                                                    importTool.ForceUpdate();
                                                    specularTexture = glossTexture.Copy();
                                                    if (specularTexture.width != colorTexture.width || specularTexture.height != colorTexture.height) {
                                                        specularTexture.Scale(colorTexture.width, colorTexture.height);
                                                    }
                                                }
                                                catch(Exception ex)
                                                {
                                                    UnityEngine.Debug.LogException(ex);
                                                }
                                                finally
                                                {
                                                    // Restore texture importer type
                                                    importTool.textureImporter.textureType = importType;
                                                    importTool.ForceUpdate();
                                                }
                                            }
                                        }
                                    }
                                }
                                if (colorTexture == null) {
                                    colorTexture = Tools.CreateBlankTextureMap(256, 256, material.color);
                                    normalTexture = null;
                                    ambientTexture = null;
                                    emissionTexture = null;
                                    metallicTexture = null;
                                    specularTexture = null;
                                }
                                var mode = material.HasProperty("_Mode") ? (BlendMode)material.GetFloat("_Mode") : BlendMode.Opaque;
                                if (colorTexture != null && mode == BlendMode.Opaque) {
                                    colorTexture.MakeAlpha(1.0f);
                                }
                                if (normalTexture == null) {
                                    normalTexture = Tools.CreateBlankNormalMap(colorTexture.width, colorTexture.height);
                                }
                                if (emissionTexture == null) {
                                    Color emissionColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : material.HasProperty("_Emission") ? material.GetColor("_Emission") : Color.black;
                                    emissionTexture = Tools.CreateBlankTextureMap(colorTexture.width, colorTexture.height, emissionColor);
                                }
                                if (ambientTexture == null) {
                                    Color ambientColor = material.HasProperty("_AmbientColor") ? material.GetColor("_AmbientColor") : Color.white;
                                    ambientTexture = Tools.CreateBlankTextureMap(colorTexture.width, colorTexture.height, ambientColor);
                                }
                                float metalness = 0.0f, glossiness = 0.5f;
                                if (material.HasProperty("_Metallic")) {
                                    metalness = Tools.GammaToLinearSpace(material.GetFloat("_Metallic"));
                                }
                                if (material.HasProperty("_Roughness")) {
                                    glossiness = (1.0f - material.GetFloat("_Roughness"));
                                } else if (material.HasProperty("_Glossiness")) {
                                    glossiness = material.GetFloat("_Glossiness");
                                } else if (material.HasProperty("_Gloss")) {
                                    glossiness = material.GetFloat("_Gloss");
                                }
                                float glossinessScale = material.HasProperty("_GlossMapScale") ? material.GetFloat("_GlossMapScale") : 0.5f;
                                bool glossyRelfections = (material.HasProperty("_GlossyReflections") && material.GetFloat("_GlossyReflections") != 0.0f);
                                if (glossyRelfections == false) {
                                    glossiness = 0.0f;
                                    glossinessScale = 0.0f;
                                }
                                glossiness = Tools.GetGlossinessScale(glossiness);
                                glossinessScale = Tools.GetGlossinessScale(glossinessScale);
                                if (metallicTexture == null) {
                                    Color metallicColor = Color.white;
                                    metallicTexture = Tools.CreateBlankTextureMap(colorTexture.width, colorTexture.height, metallicColor);
                                    metallicTexture = Tools.EncodeMetallicTextureMap(metallicTexture, metalness, glossinessScale);
                                } else {
                                    metallicTexture = Tools.CreateMetallicTextureMap(metallicTexture, glossinessScale, metallicTextureSRGB);
                                }
                                if (specularTexture == null) {
                                    Color specularColor = material.HasProperty("_SpecColor") ? material.GetColor("_SpecColor") : Color.black;
                                    specularColor.a *= glossiness;
                                    specularTexture = Tools.CreateBlankTextureMap(colorTexture.width, colorTexture.height, specularColor);
                                } else {
                                    specularTexture = Tools.CreateSpecularTextureMap(specularTexture, glossiness);
                                }
                                // Buffer baked material info
                                mainTextures.Add(colorTexture);
                                bumpTextures.Add(normalTexture);
                                ambientTextures.Add(ambientTexture);
                                emissionTextures.Add(emissionTexture);
                                metallicTextures.Add(metallicTexture);
                                specularTextures.Add(specularTexture);
                            }

                            // Bake material texture atlas
                            int textureCount = mainTextures.Count;
                            int normalCount = bumpTextures.Count;
                            if (textureCount == bakingCount && normalCount == bakingCount)
                            {
                                // Encode atlas texture
                                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                                Texture2D packedMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                                Rect[] atlasPackingResult = Tools.PackTextureAtlas(packedMeshAtlas, mainTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                                int atlasCount = atlasPackingResult.Length;
                                if (atlasCount == bakingCount) {
                                    Texture2D packedMeshBuffer = packedMeshAtlas.Copy();
                                    packedMeshBuffer.alphaIsTransparency = true;
                                    packedMeshBuffer.WriteImage(atlasFile, textureAtlasFormat);
                                    AssetDatabase.ImportAsset(atlasFile, ImportAssetOptions.ForceUpdate);

                                    // Create atlas material
                                    if (textureAtlasShader == null) textureAtlasShader = Shader.Find("Standard");
                                    atlasMaterial = new Material(textureAtlasShader);
                                    atlasMaterial.name = materialName;
                                    atlasMaterial.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(atlasFile, typeof(Texture2D));
                                    AssetDatabase.CreateAsset(atlasMaterial, materialFile);
                                    if (bakeTextureMaps) {
                                        bumpFilename = bumpFile;
                                        ambientFilename = ambientFile;
                                        emissionFilename = emissionFile;
                                        metallicFilename = metallicFile;
                                        specularFilename = specularFile;
                                    }
                                    int counter = 0;
                                    Material bakedMaterial = (Material)AssetDatabase.LoadAssetAtPath(materialFile, typeof(Material));
                                    ExporterWindow.ReportProgress(1, "Baking static texture atlas geometry... This may take a while.");
                                    // Offset uv coordinates
                                    List<CombineInstance> buffer = new List<CombineInstance>();
                                    foreach (var baker in bakingList)
                                    {
                                        List<BabylonCombineInstance> combines = baker.Value;
                                        if (combines != null && combines.Count > 0) {
                                            foreach (var source in combines) {
                                                CombineInstanceFilter combined = source.CreateCombineInstance();
                                                if (atlasPackingResult != null && atlasPackingResult.Length > 0) {
                                                    combined.combine.mesh.uv = Tools.GetTextureAtlasCoordinates(combined.combine.mesh.uv, counter, atlasPackingResult, linearInterpolation);
                                                } else {
                                                    UnityEngine.Debug.LogError("Null atlas packing result rects");
                                                }
                                                buffer.Add(combined.combine);
                                            }
                                        } else {
                                            UnityEngine.Debug.LogWarning("Null combined list encounterd for bake buffer: " + baker.Key);
                                        }
                                        counter++;
                                    }
                                    if (buffer.Count > 0)
                                    {
                                        int index = 0;
                                        if (textureAtlasMode == BabylonTextureMode.SeperateMeshes) {
                                            // Bake With Separate Meshes
                                            foreach (var item in buffer)
                                            {
                                                Mesh[] meshes = Tools.CombineStaticMeshes(new CombineInstance[] { item }, true, true, enableLightmapData);
                                                if (meshes != null && meshes.Length > 0)
                                                {
                                                    foreach (var mesh in meshes)
                                                    {
                                                        if (mesh != null && mesh.vertexCount > 0)
                                                        {
                                                            index++;
                                                            string label = Tools.FirstUpper(materialName.Replace("_Material", "")).MakeSafe();
                                                            mesh.name = String.Format("{0}_Mesh_{1}", label, index.ToString());
                                                            string meshFile = String.Format("{0}/Geometry/{1}.asset", filepath.TrimEnd('/'), mesh.name);
                                                            AssetDatabase.CreateAsset(mesh, meshFile);

                                                            var go = new GameObject(mesh.name);
                                                            go.transform.parent = atlasParent.transform;
                                                            go.transform.localPosition = Vector3.zero;
                                                            go.transform.localRotation = Quaternion.identity;
                                                            go.transform.localScale = Vector3.one;

                                                            var filter = go.AddComponent<MeshFilter>();
                                                            filter.sharedMesh = mesh;

                                                            var arenderer = go.AddComponent<MeshRenderer>();
                                                            arenderer.sharedMaterial = bakedMaterial;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Bake With Combined Meshes
                                            Mesh[] meshes = Tools.CombineStaticMeshes(buffer.ToArray(), true, true, enableLightmapData);
                                            if (meshes != null && meshes.Length > 0)
                                            {
                                                foreach (var mesh in meshes)
                                                {
                                                    if (mesh != null && mesh.vertexCount > 0)
                                                    {
                                                        index++;
                                                        string label = Tools.FirstUpper(materialName.Replace("_Material", "")).MakeSafe();
                                                        mesh.name = String.Format("{0}_Mesh_{1}", label, index.ToString());
                                                        string meshFile = String.Format("{0}/Geometry/{1}.asset", filepath.TrimEnd('/'), mesh.name);
                                                        AssetDatabase.CreateAsset(mesh, meshFile);

                                                        var go = new GameObject(mesh.name);
                                                        go.transform.parent = atlasParent.transform;
                                                        go.transform.localPosition = Vector3.zero;
                                                        go.transform.localRotation = Quaternion.identity;
                                                        go.transform.localScale = Vector3.one;

                                                        var filter = go.AddComponent<MeshFilter>();
                                                        filter.sharedMesh = mesh;

                                                        var arenderer = go.AddComponent<MeshRenderer>();
                                                        arenderer.sharedMaterial = bakedMaterial;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.LogError("Failed to generate combined baked mesh list.");
                                    }
                                }
                                else
                                {
                                    UnityEngine.Debug.LogError("Atlas packing results buffer list mismatch.");
                                }
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("Baking material texture altas buffer list mismatch.");
                            }
                            // Bake Blocking Volumes
                            if (bakeAtlasColliders == true)
                            {
                                CreateBlockingVolumes(filename, colliderParent);
                            }
                        } else if (meshToolkitType == BabylonToolkitType.CombineMeshes) {
                            ExporterWindow.ReportProgress(1, "Baking static material mesh geometry... This may take a while.");
                            // Combine By Material
                            foreach (var baker in bakingList)
                            {
                                Material material = namedMaterials[baker.Key];
                                List<BabylonCombineInstance> combines = baker.Value;
                                if (combines != null && combines.Count > 0)
                                {
                                    List<CombineInstance> buffer = new List<CombineInstance>();
                                    foreach (var source in combines) {
                                        CombineInstanceFilter combined = source.CreateCombineInstance();
                                        buffer.Add(combined.combine);
                                    }
                                    if (buffer != null && buffer.Count > 0)
                                    {
                                        string label = Tools.FirstUpper(material.name.Replace("_Material", "")).MakeSafe();
                                        Mesh[] meshes = Tools.CombineStaticMeshes(buffer.ToArray(), true, true, enableLightmapData);
                                        if (meshes != null && meshes.Length > 0)
                                        {
                                            int index = 0;
                                            foreach (var mesh in meshes)
                                            {
                                                if (mesh != null && mesh.vertexCount > 0)
                                                {
                                                    index++;
                                                    mesh.name = String.Format("{0}_{1}_Mesh_{2}", filelabel, label, index.ToString());
                                                    string meshFile = String.Format("{0}/Geometry/{1}.asset", filepath.TrimEnd('/'), mesh.name);
                                                    AssetDatabase.CreateAsset(mesh, meshFile);

                                                    var go = new GameObject(mesh.name.Replace("_Mesh", ""));
                                                    go.transform.parent = po.transform;
                                                    go.transform.localPosition = Vector3.zero;
                                                    go.transform.localRotation = Quaternion.identity;
                                                    go.transform.localScale = Vector3.one;

                                                    var filter = go.AddComponent<MeshFilter>();
                                                    filter.sharedMesh = mesh;

                                                    var arenderer = go.AddComponent<MeshRenderer>();
                                                    arenderer.sharedMaterial = material;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        UnityEngine.Debug.LogError("Failed to generate buffer baked mesh list.");
                                    }
                                }
                                else
                                {
                                    UnityEngine.Debug.LogError("Failed to generate combined baked mesh list.");
                                }
                            }
                        } else if (meshToolkitType == BabylonToolkitType.SeperateMeshes) {
                            ExporterWindow.ReportProgress(1, "Baking static sub mesh geometry... This may take a while.");
                            // Extract Sub Meshes
                            int index = 0;
                            foreach (var baker in bakingList)
                            {
                                Material material = namedMaterials[baker.Key];
                                List<BabylonCombineInstance> combines = baker.Value;
                                if (combines != null && combines.Count > 0) {
                                    foreach (var source in combines) {
                                        string label = Tools.FirstUpper(source.filter.name).MakeSafe();
                                        CombineInstanceFilter combined = source.CreateCombineInstance();
                                        Mesh[] meshes = Tools.CombineStaticMeshes(new CombineInstance[] { combined.combine }, true, true, enableLightmapData);
                                        if (meshes != null && meshes.Length > 0)
                                        {
                                            foreach (var mesh in meshes)
                                            {
                                                if (mesh != null && mesh.vertexCount > 0)
                                                {
                                                    index++;
                                                    mesh.name = String.Format("{0}_{1}_Mesh_{2}", filelabel, label, index.ToString());
                                                    string meshFile = String.Format("{0}/Geometry/{1}.asset", filepath.TrimEnd('/'), mesh.name);
                                                    AssetDatabase.CreateAsset(mesh, meshFile);

                                                    var go = new GameObject(mesh.name.Replace("_Mesh", ""));
                                                    go.transform.parent = po.transform;
                                                    go.transform.localPosition = Vector3.zero;
                                                    go.transform.localRotation = Quaternion.identity;
                                                    go.transform.localScale = Vector3.one;

                                                    var filter = go.AddComponent<MeshFilter>();
                                                    filter.sharedMesh = mesh;

                                                    var arenderer = go.AddComponent<MeshRenderer>();
                                                    arenderer.sharedMaterial = material;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("No baked material meshes generated.");
                    }
                }
            }
            else
            {
                ExporterWindow.ShowMessage("No static child meshes found for parent object");
            }
        }

        private void PackTextureAtlasNormals()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(bumpFilename)) {
                ExporterWindow.ReportProgress(1, "Generating normal map atlas... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D bumpMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(bumpMeshAtlas, bumpTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D bumpMeshBuffer = bumpMeshAtlas.Copy();
                bumpMeshBuffer.WriteImage(bumpFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(bumpFilename, ImportAssetOptions.ForceUpdate);
                // Import As Normal Map
                var importTool = new BabylonTextureImporter(bumpFilename);
                importTool.textureImporter.textureType = TextureImporterType.NormalMap;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetTexture("_BumpMap", (Texture2D)AssetDatabase.LoadAssetAtPath(bumpFilename, typeof(Texture2D)));
            }
        }

        private void PackTextureAtlasAmbients()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(ambientFilename)) {
                ExporterWindow.ReportProgress(1, "Generating ambient occlusion maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D ambientMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(ambientMeshAtlas, ambientTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D ambientMeshBuffer = ambientMeshAtlas.Copy();
                ambientMeshBuffer.WriteImage(ambientFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(ambientFilename, ImportAssetOptions.ForceUpdate);
                // Import As Ambient Map
                var importTool = new BabylonTextureImporter(ambientFilename);
                importTool.textureImporter.textureType = TextureImporterType.Default;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetTexture("_OcclusionMap", (Texture2D)AssetDatabase.LoadAssetAtPath(ambientFilename, typeof(Texture2D)));
            }
        }

        private void PackTextureAtlasEmissions()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(emissionFilename)) {
                ExporterWindow.ReportProgress(1, "Generating emissive color maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D emissionMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(emissionMeshAtlas, emissionTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D emissionMeshBuffer = emissionMeshAtlas.Copy();
                emissionMeshBuffer.WriteImage(emissionFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(emissionFilename, ImportAssetOptions.ForceUpdate);
                // Import As Emission Map
                var importTool = new BabylonTextureImporter(emissionFilename);
                importTool.textureImporter.textureType = TextureImporterType.Default;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetTexture("_EmissionMap", (Texture2D)AssetDatabase.LoadAssetAtPath(emissionFilename, typeof(Texture2D)));
            }
        }

        private void PackTextureAtlasMetallics()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(metallicFilename)) {
                ExporterWindow.ReportProgress(1, "Generating metallic gloss maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D metallicMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(metallicMeshAtlas, metallicTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D metallicMeshBuffer = metallicMeshAtlas.Copy();
                metallicMeshBuffer.WriteImage(metallicFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(metallicFilename, ImportAssetOptions.ForceUpdate);
                // Import As Metallic Map
                var importTool = new BabylonTextureImporter(metallicFilename);
                importTool.textureImporter.textureType = TextureImporterType.Default;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetFloat("_Gloss", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetFloat("_Glossiness", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetFloat("_GlossMapScale", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetTexture("_MetallicGlossMap", (Texture2D)AssetDatabase.LoadAssetAtPath(metallicFilename, typeof(Texture2D)));
            }
        }

        private void PackTextureAtlasSpeculars()
        {
            if (atlasMaterial != null && !String.IsNullOrEmpty(specularFilename)) {
                ExporterWindow.ReportProgress(1, "Generating specular gloss maps... This may take a while.");
                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                Texture2D specularMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Tools.PackTextureAtlas(specularMeshAtlas, specularTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false);
                Texture2D specularMeshBuffer = specularMeshAtlas.Copy();
                specularMeshBuffer.WriteImage(specularFilename, textureAtlasFormat);
                AssetDatabase.ImportAsset(specularFilename, ImportAssetOptions.ForceUpdate);
                // Import As Specular Map
                var importTool = new BabylonTextureImporter(specularFilename);
                importTool.textureImporter.textureType = TextureImporterType.Default;
                importTool.textureImporter.convertToNormalmap = false;
                importTool.ForceUpdate();
                atlasMaterial.SetFloat("_Gloss", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetFloat("_Glossiness", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetFloat("_GlossMapScale", 0.5f); // Default Texture Atlas Scaling
                atlasMaterial.SetTexture("_SpecGlossMap", (Texture2D)AssetDatabase.LoadAssetAtPath(specularFilename, typeof(Texture2D)));
            }
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}
