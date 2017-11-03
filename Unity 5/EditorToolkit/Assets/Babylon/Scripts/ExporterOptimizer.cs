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
        BabylonToolkitType meshToolkitType = BabylonToolkitType.CreatePrimitive;
        BabylonPrimitiveType meshPrimitiveType = BabylonPrimitiveType.Ground;
        BabylonTextureMode textureAtlasMode = BabylonTextureMode.CombineMeshes;
        BabylonTextureScale textureImageScaling = BabylonTextureScale.Bilinear;
        BabylonImageFormat textureAtlasFormat = BabylonImageFormat.PNG;
        Shader textureAtlasShader = null;
        int textureAtlasSize = 4096;
        int maxTextureImageSize = 0;
        bool bakeTextureNormals = true;
        bool linearInterpolation = true;
        bool removeAlphaEncoding = true;
        bool bakeAtlasColliders = true;
        GameObject bakingDestination = null;
        bool keepGeneratorOpen = true;

        // Texture Atlas Buffers        
        Material atlasMaterial = null;
        List<Texture2D> mainTextures = null;
        List<Texture2D> bumpTextures = null;
        string bumpFilename = String.Empty;
        bool hasBumpTexture = false;

        // No Prebaked Lightmap Data
        bool enableLightmapData = false;

        // Bake Blocking Volumes
        BabylonBlockingVolume blockingVolumeMode = BabylonBlockingVolume.BakeColliders;

        bool bakeColliderGeometry = false;

        // Plane Primitive Geometry
        float planeLength = 256.0f;
        float planeWidth = 256.0f;
        int planeResX = 24;
        int planeResZ = 24;

        // Cube Primitive Geometry
        float boxLength = 1.0f;
        float boxWidth = 1.0f;
        float boxHeight = 1.0f;

        // Cone Primitive Geometry
        float coneHeight = 1.0f;
        float coneBottomRadius = 0.25f;
        float coneTopRadius = 0.05f;
        int coneNumSides = 18;

        // Tube Primitive Geometry
        float tubeHeight = 1.0f;
        int tubeNumSides = 24;

        // Wheel Primitive Geometry
        float wheelHeight = 1.0f;
        float wheelRadius = 0.5f;
        int wheelNumSegments = 24;

        // Torus Primitive Geometry
        float torusRadius1 = 1.0f;
        float torusRadius2 = 0.03f;
        int torusRadSegments = 24;
        int torusNumSides = 18;

        // Capsule Primitive Geometry
        float capsuleHeight = 2.0f;
        float capsuleRadius = 0.5f;
        int capsuleNumSegments = 24;

        // Sphere Primitive Geometry
        float sphereRadius = 1.0f;
        int sphereNumSegments = 24;
        int sphereNumLatitude = 16;

        [MenuItem("BabylonJS/Geometry Tools", false, 101)]
        public static void InitConverter()
        {
            ExporterOptimizer combiner = ScriptableObject.CreateInstance<ExporterOptimizer>();
            combiner.OnInitialize();
            combiner.ShowUtility();
        }

        public void OnInitialize()
        {
            maxSize = new Vector2(500.0f, 217.0f);
            minSize = this.maxSize;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Geometry Tools");
            textureAtlasShader = Shader.Find("BabylonJS/System/Standard Material");
            textureAtlasFormat = (BabylonImageFormat)ExporterWindow.exportationOptions.ImageEncodingOptions;
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();
            meshToolkitType = (BabylonToolkitType)EditorGUILayout.EnumPopup("Mesh Toolkit Mode:", meshToolkitType, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();
            string bakeLabel = "Bake Optimized Meshes";
            bool showParentDetination = false;
            if (meshToolkitType == BabylonToolkitType.CreatePrimitive) {
                showParentDetination = false;
                bakeLabel = "Create Primitive Mesh";
                if (meshPrimitiveType == BabylonPrimitiveType.Ground) {
                    if (this.maxSize.y != 217.0f) {
                        this.maxSize = new Vector2(500.0f, 217.0f);
                        this.minSize = this.maxSize;
                    }
                } else if (meshPrimitiveType == BabylonPrimitiveType.Cube) {
                    if (this.maxSize.y != 190.0f) {
                        this.maxSize = new Vector2(500.0f, 190.0f);
                        this.minSize = this.maxSize;
                    }
                } else if (meshPrimitiveType == BabylonPrimitiveType.Cone) {
                    if (this.maxSize.y != 217.0f) {
                        this.maxSize = new Vector2(500.0f, 217.0f);
                        this.minSize = this.maxSize;
                    }
                } else if (meshPrimitiveType == BabylonPrimitiveType.Tube) {
                    if (this.maxSize.y != 165.0f) {
                        this.maxSize = new Vector2(500.0f, 165.0f);
                        this.minSize = this.maxSize;
                    }
                } else if (meshPrimitiveType == BabylonPrimitiveType.Wheel) {
                    if (this.maxSize.y != 190.0f) {
                        this.maxSize = new Vector2(500.0f, 190.0f);
                        this.minSize = this.maxSize;
                    }
                } else if (meshPrimitiveType == BabylonPrimitiveType.Torus) {
                    if (this.maxSize.y != 217.0f) {
                        this.maxSize = new Vector2(500.0f, 217.0f);
                        this.minSize = this.maxSize;
                    }
                } else if (meshPrimitiveType == BabylonPrimitiveType.Capsule) {
                    if (this.maxSize.y != 190.0f) {
                        this.maxSize = new Vector2(500.0f, 190.0f);
                        this.minSize = this.maxSize;
                    }
                } else if (meshPrimitiveType == BabylonPrimitiveType.Sphere) {
                    if (this.maxSize.y != 190.0f) {
                        this.maxSize = new Vector2(500.0f, 190.0f);
                        this.minSize = this.maxSize;
                    }
                } else {
                    if (this.maxSize.y != 217.0f) {
                        this.maxSize = new Vector2(500.0f, 217.0f);
                        this.minSize = this.maxSize;
                    }
                }
            } else if (meshToolkitType == BabylonToolkitType.CombineMeshes) {
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
            if (meshToolkitType == BabylonToolkitType.CreatePrimitive)
            {
                meshPrimitiveType = (BabylonPrimitiveType)EditorGUILayout.EnumPopup("Primitive Mesh Type:", meshPrimitiveType, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                if (meshPrimitiveType == BabylonPrimitiveType.Ground) {
                    planeLength = (float)EditorGUILayout.Slider("Ground Length:", planeLength, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    planeWidth = (float)EditorGUILayout.Slider("Ground Width:", planeWidth, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    planeResX = (int)EditorGUILayout.Slider("Ground Resolution X:", planeResX, 2, 256);
                    EditorGUILayout.Space();
                    planeResZ = (int)EditorGUILayout.Slider("Ground Resolution Z:", planeResZ, 2, 256);
                    EditorGUILayout.Space();
                } else if (meshPrimitiveType == BabylonPrimitiveType.Cube) {
                    boxLength = (float)EditorGUILayout.Slider("Cube Length:", boxLength, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    boxWidth = (float)EditorGUILayout.Slider("Cube Width:", boxWidth, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    boxHeight = (float)EditorGUILayout.Slider("Cube Height:", boxHeight, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                } else if (meshPrimitiveType == BabylonPrimitiveType.Cone) {
                    coneHeight = (float)EditorGUILayout.Slider("Cone Height:", coneHeight, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    coneTopRadius = (float)EditorGUILayout.Slider("Cone Top Radius:", coneTopRadius, 0.0f, 100.0f);
                    EditorGUILayout.Space();
                    coneBottomRadius = (float)EditorGUILayout.Slider("Cone Bottom Radius:", coneBottomRadius, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    coneNumSides = (int)EditorGUILayout.Slider("Cone Num Sides:", coneNumSides, 0, 256);
                    EditorGUILayout.Space();
                } else if (meshPrimitiveType == BabylonPrimitiveType.Tube) {
                    tubeHeight = (float)EditorGUILayout.Slider("Tube Height:", tubeHeight, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    tubeNumSides = (int)EditorGUILayout.Slider("Tube Num Sides:", tubeNumSides, 0, 256);
                    EditorGUILayout.Space();
                } else if (meshPrimitiveType == BabylonPrimitiveType.Wheel) {
                    wheelHeight = (float)EditorGUILayout.Slider("Wheel Height:", wheelHeight, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    wheelRadius = (float)EditorGUILayout.Slider("Wheel Radius:", wheelRadius, 0.0f, 100.0f);
                    EditorGUILayout.Space();
                    wheelNumSegments = (int)EditorGUILayout.Slider("Wheel Num Sides:", wheelNumSegments, 0, 256);
                    EditorGUILayout.Space();
                } else if (meshPrimitiveType == BabylonPrimitiveType.Torus) {
                    torusRadius1 = (int)EditorGUILayout.Slider("Torus Radius 1:", torusRadius1, 0.0f, 100.0f);
                    EditorGUILayout.Space();
                    torusRadius2 = (int)EditorGUILayout.Slider("Torus Radius 2:", torusRadius2, 0.0f, 100.0f);
                    EditorGUILayout.Space();
                    torusRadSegments = (int)EditorGUILayout.Slider("Torus Rad Segments:", torusRadSegments, 0, 256);
                    EditorGUILayout.Space();
                    torusNumSides = (int)EditorGUILayout.Slider("Torus Num Sides:", torusNumSides, 0, 256);
                    EditorGUILayout.Space();
                } else if (meshPrimitiveType == BabylonPrimitiveType.Capsule) {
                    capsuleHeight = (float)EditorGUILayout.Slider("Capsule Height:", capsuleHeight, 0.0f, 10000.0f);
                    EditorGUILayout.Space();
                    capsuleRadius = (float)EditorGUILayout.Slider("Capsule Radius:", capsuleRadius, 0.0f, 100.0f);
                    EditorGUILayout.Space();
                    capsuleNumSegments = (int)EditorGUILayout.Slider("Capsule Num Sides:", capsuleNumSegments, 0, 256);
                    EditorGUILayout.Space();
                } else if (meshPrimitiveType == BabylonPrimitiveType.Sphere) {
                    sphereRadius = (float)EditorGUILayout.Slider("Sphere Radius:", sphereRadius, 0.0f, 100.0f);
                    EditorGUILayout.Space();
                    sphereNumSegments = (int)EditorGUILayout.Slider("Sphere Num Segments:", sphereNumSegments, 0, 256);
                    EditorGUILayout.Space();
                    sphereNumLatitude = (int)EditorGUILayout.Slider("Sphere Num Latitude:", sphereNumLatitude, 0, 256);
                    EditorGUILayout.Space();
                }
            }
            else
            {
                parentObject = EditorGUILayout.ObjectField("Parent Game Object:", parentObject, typeof(GameObject), true) as GameObject;
                EditorGUILayout.Space();
            }

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
                bakeTextureNormals = EditorGUILayout.Toggle("Bake Normal Maps:", bakeTextureNormals);
                EditorGUILayout.Space();
                removeAlphaEncoding = EditorGUILayout.Toggle("Remove Image Alpha:", removeAlphaEncoding);
                EditorGUILayout.Space();
                textureAtlasMode = (BabylonTextureMode)EditorGUILayout.EnumPopup("Texture Atlas Mode:", textureAtlasMode, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                textureAtlasSize = (int)EditorGUILayout.Slider("Texture Atlas Size:", textureAtlasSize, 128, 8192);
                EditorGUILayout.Space();
                textureAtlasShader = EditorGUILayout.ObjectField("Texture Atlas Shader:", textureAtlasShader, typeof(Shader), true) as Shader;
                EditorGUILayout.Space();
                textureAtlasFormat = (BabylonImageFormat)EditorGUILayout.EnumPopup("Texture Atlas Format:", textureAtlasFormat, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                textureImageScaling = (BabylonTextureScale)EditorGUILayout.EnumPopup("Texture Image Scaling:", textureImageScaling, GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                maxTextureImageSize = (int)EditorGUILayout.Slider("Texture Image Maximum:", maxTextureImageSize, 0, 4096);
                EditorGUILayout.Space();
                bakeAtlasColliders = EditorGUILayout.Toggle("Bake Blocking Volumes:", bakeAtlasColliders);
                EditorGUILayout.Space();
                linearInterpolation = EditorGUILayout.Toggle("Use Linear Interpolation:", linearInterpolation);
                EditorGUILayout.Space();
            }

            if (showParentDetination)
            {
                bakingDestination = EditorGUILayout.ObjectField("Optional Bake Destination:", bakingDestination, typeof(GameObject), true) as GameObject;
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

            // Validate Parent Object
            if (meshToolkitType != BabylonToolkitType.CreatePrimitive) {
                if (!parentObject)
                {
                    ExporterWindow.ShowMessage("You must select a parent object");
                    return;
                }
            }

            try
            {
                atlasMaterial = null;
                mainTextures = null;
                bumpTextures = null;
                bumpFilename = String.Empty;
                hasBumpTexture = false;

                if (meshToolkitType == BabylonToolkitType.CreatePrimitive) {
                    CreatePrimitiveGeometry();
                } else if (meshToolkitType == BabylonToolkitType.BlockingVolumes) {
                    CreateBlockingVolumes();
                } else {
                    OptimizeStaticMeshes();
                    if (meshToolkitType == BabylonToolkitType.BakeTextureAtlas) {
                        PackTextureAtlasNormals();
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

        public void CreatePrimitiveGeometry()
        {
            string filename = EditorUtility.SaveFilePanelInProject("Static Geometry Tools", "", "asset", "Create Static Primitive Mesh Geometry");
            if (!String.IsNullOrEmpty(filename))
            {
                ExporterWindow.ReportProgress(1, "Optimizing static mesh geometry...  This may take a while.");
                string filepath = Path.GetDirectoryName(filename);
                string filelabel = Path.GetFileNameWithoutExtension(filename);

                // Create primitive geometry mesh
                Mesh myPrimitive = null;
                if (meshPrimitiveType == BabylonPrimitiveType.Ground) {
                    myPrimitive = Tools.CreateGroundMesh(planeLength, planeWidth, planeResX, planeResZ);
                } else if (meshPrimitiveType == BabylonPrimitiveType.Cube) {
                    myPrimitive = Tools.CreateBoxMesh(boxLength, boxWidth, boxHeight);
                } else if (meshPrimitiveType == BabylonPrimitiveType.Cone) {
                    myPrimitive = Tools.CreateConeMesh(coneHeight, coneBottomRadius, coneTopRadius, coneNumSides, 1);
                } else if (meshPrimitiveType == BabylonPrimitiveType.Tube) {
                    myPrimitive = Tools.CreateTubeMesh(tubeHeight, tubeNumSides);
                } else if (meshPrimitiveType == BabylonPrimitiveType.Wheel) {
                    myPrimitive = Tools.CreateWheelMesh(wheelHeight, wheelRadius, wheelNumSegments);
                } else if (meshPrimitiveType == BabylonPrimitiveType.Torus) {
                    myPrimitive = Tools.CreateTorusMesh(torusRadius1, torusRadius2, torusRadSegments, torusNumSides);
                } else if (meshPrimitiveType == BabylonPrimitiveType.Capsule) {
                    myPrimitive = Tools.CreateCapsuleMesh(capsuleHeight, capsuleRadius, capsuleNumSegments);
                } else if (meshPrimitiveType == BabylonPrimitiveType.Sphere) {
                    myPrimitive = Tools.CreateSphereMesh(sphereRadius, sphereNumSegments, sphereNumLatitude);
                } else {
                    myPrimitive = new Mesh();
                }

                // Save primitive geometry mesh
                if (myPrimitive != null) {
                    myPrimitive.ReverseNormals();
                    myPrimitive.RecalculateNormals();
                    myPrimitive.name = filelabel;
                    string meshFile = String.Format("{0}/{1}.asset", filepath.TrimEnd('/'), filelabel);
                    AssetDatabase.CreateAsset(myPrimitive, meshFile);

                    var go = new GameObject(myPrimitive.name);
                    go.transform.parent = null;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                    
                    var filter = go.AddComponent<MeshFilter>();
                    filter.sharedMesh = myPrimitive;

                    var arenderer = go.AddComponent<MeshRenderer>();
                    arenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                } else {
                    UnityEngine.Debug.LogWarning("Failed to create primitive mesh geometry.");
                }
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
                            bool jpeg = (textureAtlasFormat == BabylonImageFormat.JPEG);
                            string atlasExt = (jpeg) ? "jpg" : "png";
                            string atlasName = String.Format("{0}_Atlas", filelabel);
                            string atlasFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), atlasName, atlasExt);
                            string bumpName = String.Format("{0}_Normal", filelabel);
                            string bumpFile = String.Format("{0}/Textures/{1}.{2}", filepath.TrimEnd('/'), bumpName, atlasExt);
                            string materialName = String.Format("{0}_Material", filelabel);
                            string materialFile = String.Format("{0}/Materials/{1}.asset", filepath.TrimEnd('/'), materialName);
                            mainTextures = new List<Texture2D>();
                            bumpTextures = new List<Texture2D>();
                            foreach (var baker in bakingList)
                            {
                                Material material = namedMaterials[baker.Key];
                                // Combine main textures
                                Texture2D colorTexture = null;
                                Texture2D normalTexture = null;
                                if (material.mainTexture != null) {
                                    Texture2D primaryTexture = material.mainTexture as Texture2D;
                                    primaryTexture.ForceReadable();
                                    colorTexture = primaryTexture.Copy();
                                    if (bakeTextureNormals) {
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
                                                    hasBumpTexture = true;
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
                                    colorTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                                    colorTexture.Clear(material.color);
                                    normalTexture = null;
                                }
                                if (normalTexture == null) {
                                    normalTexture = Tools.CreateBlankNormalMap(colorTexture.width, colorTexture.height);
                                }
                                // Buffer baked material info
                                mainTextures.Add(colorTexture);
                                bumpTextures.Add(normalTexture);
                            }

                            // Bake material texture atlas
                            int textureCount = mainTextures.Count;
                            int normalCount = bumpTextures.Count;
                            if (textureCount == bakingCount && normalCount == bakingCount)
                            {
                                // Encode atlas texture
                                bool bilinearScaling = (textureImageScaling == BabylonTextureScale.Bilinear);
                                Texture2D packedMeshAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                                Rect[] atlasPackingResult = Tools.PackTextureAtlas(packedMeshAtlas, mainTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false, removeAlphaEncoding);
                                int atlasCount = atlasPackingResult.Length;
                                if (atlasCount == bakingCount) {
                                    Texture2D packedMeshBuffer = packedMeshAtlas.Copy();
                                    packedMeshBuffer.WriteImage(atlasFile, textureAtlasFormat);
                                    AssetDatabase.ImportAsset(atlasFile, ImportAssetOptions.ForceUpdate);

                                    // Create atlas material
                                    if (textureAtlasShader == null) textureAtlasShader = Shader.Find("BabylonJS/System/Standard Material");
                                    atlasMaterial = new Material(textureAtlasShader);
                                    atlasMaterial.name = materialName;
                                    atlasMaterial.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(atlasFile, typeof(Texture2D));
                                    AssetDatabase.CreateAsset(atlasMaterial, materialFile);
                                    if (bakeTextureNormals && hasBumpTexture) {
                                        bumpFilename = bumpFile;
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
                Tools.PackTextureAtlas(bumpMeshAtlas, bumpTextures.ToArray(), textureAtlasSize, maxTextureImageSize, bilinearScaling, false, removeAlphaEncoding);
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

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}
