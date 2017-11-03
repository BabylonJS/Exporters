
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BabylonExport.Entities;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEditor.Animations;
using Object = UnityEngine.Object;

namespace Unity3D2Babylon
{
    public partial class SceneBuilder
    {
        public string OutputPath { get; private set; }
        public string SceneName { get; private set; }
        public string ManifestData { get; private set; }
        public string SceneJavascriptPath { get; private set; }
        public SceneController SceneController { get; private set; }
        public List<MonoScript> MonoRuntimeScripts { get; private set; }
        readonly Dictionary<string, BabylonMaterial> materialsDictionary;
        readonly Dictionary<string, BabylonMultiMaterial> multiMatDictionary;
        readonly BabylonScene babylonScene;
        readonly ExportationOptions exportationOptions;
        BabylonTexture sceneReflectionTexture;
        public static SceneMetaData Metadata = new SceneMetaData();
        private static Dictionary<int, string> UniqueGuids;
        GameObject[] readObjects;
        GameObject[] gameObjects;
        Tools.TextureInfo[] skyboxTextures;
        public BabylonMesh prefabInstances;

        private static Dictionary<Transform, string> RootBoneTransformMap = new Dictionary<Transform, string>();

        private static Dictionary<string, List<BabylonAnimation>> AnimationCurveKeys = new Dictionary<string, List<BabylonAnimation>>();

        public SceneBuilder(string outputPath, string sceneName, ExportationOptions exportationOptions, SceneController controller, string scriptPath)
        {
            OutputPath = outputPath;
            SceneName = string.IsNullOrEmpty(sceneName) ? "scene" : sceneName;
            SceneController = controller;
            SceneJavascriptPath = scriptPath;
            RootBoneTransformMap = new Dictionary<Transform, string>();

            materialsDictionary = new Dictionary<string, BabylonMaterial>();
            multiMatDictionary = new Dictionary<string, BabylonMultiMaterial>();
            SceneBuilder.UniqueGuids = new Dictionary<int, string>();

            babylonScene = new BabylonScene(OutputPath);
            babylonScene.producer = new BabylonProducer
            {
                file = Path.GetFileName(OutputPath),
                version = "Unity3D",
                name = SceneName,
                exporter_version = "0.8.1"
            };
            this.exportationOptions = exportationOptions;
            this.ManifestData = String.Empty;
            if (SceneController != null && SceneController.manifestOptions.exportManifest)
            {
                this.ManifestData = "{" + String.Format("\n\t\"version\" : {0},\n\t\"enableSceneOffline\" : {1},\n\t\"enableTexturesOffline\" : {2}\n", SceneController.manifestOptions.manifestVersion.ToString(), SceneController.manifestOptions.storeSceneOffline.ToString().ToLower(), SceneController.manifestOptions.storeTextureOffline.ToString().ToLower()) + "}";
            }
            else
            {
                this.ManifestData = "{" + String.Format("\n\t\"version\" : {0},\n\t\"enableSceneOffline\" : {1},\n\t\"enableTexturesOffline\" : {2}\n", "1", "false", "false") + "}";
            } 
        }

        public void WriteToBabylonFile(string outputFile, string javascriptFile)
        {
            if (exportationOptions.ScenePackingOptions == (int)BabylonPackingOption.Binary)
            {
                ExporterWindow.ReportProgress(1, "Packing binary scene content... This may take a while.");
                babylonScene.Prepare(false, false);
                Tools.BinaryExport(babylonScene, outputFile);
            }
            else
            {
                ExporterWindow.ReportProgress(1, "Exporting babylon scene content... This may take a while.");
                babylonScene.Prepare(false, false);
                Tools.JsonExport(babylonScene, outputFile, exportationOptions.PrettyPrintExport);
                
                if (exportationOptions.PrecompressContent && File.Exists(outputFile)) {
                    ExporterWindow.ReportProgress(1, "Compressing babylon scene file... This may take a while.");
                    Tools.PrecompressFile(outputFile, outputFile +  ".gz");
                }
            }
            if (!String.IsNullOrEmpty(javascriptFile))
            {
                if (exportationOptions.PrecompressContent && File.Exists(javascriptFile)) {
                    ExporterWindow.ReportProgress(1, "Compressing babylon project script... This may take a while.");
                    Tools.PrecompressFile(javascriptFile, javascriptFile +  ".gz");
                }
            }
            if (!String.IsNullOrEmpty(this.ManifestData))
            {
                var manifestFile = outputFile + ".manifest";
                File.WriteAllText(manifestFile, this.ManifestData);
            }
        }

        public void GenerateStatus(List<string> logs)
        {
            int meshes = (babylonScene.meshes != null) ? babylonScene.meshes.Length : 0;
            int lights = (babylonScene.lights != null) ? babylonScene.lights.Length : 0;
            int cameras = (babylonScene.cameras != null) ? babylonScene.cameras.Length : 0;
            int materials = (babylonScene.materials != null) ? babylonScene.materials.Length : 0;
            int multiMaterials = (babylonScene.multiMaterials != null) ? babylonScene.multiMaterials.Length : 0;

            var initialLog = new List<string> {
                "*Exportation Status:",
                meshes.ToString() + " mesh(es)",
                lights.ToString() + " light(s)",
                cameras.ToString() + " camera(s)",
                materials.ToString() + " material(s)",
                multiMaterials.ToString() + " multi-material(s)",
                "",
                "*Log:"
            };
            logs.InsertRange(0, initialLog);
        }

        string GetParentID(Transform transform)
        {
            if (transform.parent == null)
            {
                return null;
            }
            return GetID(transform.parent.gameObject);
        }

        public static string GetID(GameObject gameObject)
        {
            var key = gameObject.GetInstanceID();
            if (!SceneBuilder.UniqueGuids.ContainsKey(key))
            {
                SceneBuilder.UniqueGuids[key] = Guid.NewGuid().ToString();
            }
            return SceneBuilder.UniqueGuids[key];
        }

        public void ConvertFromUnity()
        {
            var index = 0;
            this.skyboxTextures = null;
            ExporterWindow.ReportProgress(0, "Starting exportation process...");
            readObjects = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            if (readObjects == null || readObjects.Length == 0)
            {
                ExporterWindow.ShowMessage("No gameobject! - Please add at least a gameobject to export");
                return;
            }
            // Parse all export buffers
            List<GameObject> gameBuffer = new List<GameObject>();
            List<GameObject> boneBuffer = new List<GameObject>();
            List<GameObject> staticBuffer = new List<GameObject>();
            List<MeshFilter> staticFilters = new List<MeshFilter>();
            List<Collider> staticColliders = new List<Collider>();
            // Filter All Bones
            foreach (var ro in readObjects)
            {
                var skin = ro.GetComponent<SkinnedMeshRenderer>();
                if (skin != null && skin.rootBone != null && skin.rootBone.gameObject != null) {
                    Transform[] bxs = skin.rootBone.gameObject.GetComponentsInChildren<Transform>();
                    foreach (var bx in bxs)
                    {
                        GameObject bo = bx.gameObject;
                        if (bo != null) {
                            if (!boneBuffer.Contains(bo)) {
                                boneBuffer.Add(bo);
                            }
                        }
                    }
                }
            }
            // Filter All Statics
            foreach (var ro in readObjects)
            {
                if (!boneBuffer.Contains(ro)) {
                    if (ro.layer == ExporterWindow.StaticIndex) {
                        staticBuffer.Add(ro);
                    } else {
                        gameBuffer.Add(ro);
                    }
                }
            }
            // Filter Static Layer
            GameObject staticParent = null;
            if (staticBuffer.Count > 0)
            {
                staticParent = new GameObject("(Babylon Static)");
                staticParent.transform.parent = null;
                staticParent.transform.localPosition = Vector3.zero;
                staticParent.transform.localRotation = Quaternion.identity;
                staticParent.transform.localScale = Vector3.one;
                gameBuffer.Add(staticParent);
                foreach(var so in staticBuffer)
                {
                    var volume = so.GetComponent<Collider>();
                    if (volume != null) {
                        staticColliders.Add(volume);
                    }
                    var filter = so.GetComponent<MeshFilter>();
                    if (filter != null && filter.sharedMesh != null && filter.sharedMesh.vertexCount > 0) {
                        staticFilters.Add(filter);
                    }
                }
            }
            // Static batching by mesh Filter
            if (staticParent != null && staticFilters.Count > 0)
            {
                ExporterWindow.ReportProgress(1, "Preparing static batching meshes... This may take a while.");
                GameObject babylonStatic = new GameObject("Meshes");
                babylonStatic.transform.parent = staticParent.transform;
                babylonStatic.transform.localPosition = Vector3.zero;
                babylonStatic.transform.localRotation = Quaternion.identity;
                babylonStatic.transform.localScale = Vector3.one;
                gameBuffer.Add(babylonStatic);
                // Prepare static batching buffer
                Matrix4x4 myTransform = babylonStatic.transform.worldToLocalMatrix;
                Dictionary<string, List<BabylonCombineInstance>> bakingList = new Dictionary<string, List<BabylonCombineInstance>>();
                Dictionary<string, Material> namedMaterials = new Dictionary<string, Material>();
                foreach (var filter in staticFilters) {
                    if (filter.sharedMesh == null || filter.sharedMesh.vertexCount <= 0)
                        continue;
                    var filterRenderer = filter.GetComponent<Renderer>();
                    if (filterRenderer.sharedMaterials == null || filterRenderer.sharedMaterials.Length <= 0)
                        continue;
                    foreach (var material in filterRenderer.sharedMaterials) {
                        if (material != null && !namedMaterials.ContainsKey(material.name)) {
                            namedMaterials.Add(material.name, material);
                        }
                    }
                    // Prepare static baking list
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
                if (bakingList.Count > 0) {
                    bool enableLightmapData = true;
                    foreach (var baker in bakingList) {
                        Material material = namedMaterials[baker.Key];
                        if (material != null) {
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
                                    string label = Tools.FirstUpper(material.name);
                                    Mesh[] meshes = Tools.CombineStaticMeshes(buffer.ToArray(), true, true, enableLightmapData);
                                    if (meshes != null && meshes.Length > 0)
                                    {
                                        index = 0;
                                        foreach (var mesh in meshes)
                                        {
                                            if (mesh != null && mesh.vertexCount > 0)
                                            {
                                                index++;
                                                mesh.name = String.Format("{0}_StaticMesh_{1}", label, index.ToString());
                                                GameObject go = new GameObject(mesh.name);
                                                go.layer = ExporterWindow.StaticIndex;
                                                go.transform.parent = babylonStatic.transform;
                                                go.transform.localPosition = Vector3.zero;
                                                go.transform.localRotation = Quaternion.identity;
                                                go.transform.localScale = Vector3.one;

                                                var filter = go.AddComponent<MeshFilter>();
                                                filter.sharedMesh = mesh;

                                                var arenderer = go.AddComponent<MeshRenderer>();
                                                arenderer.sharedMaterial = material;

                                                gameBuffer.Add(go);
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
                    }
                }
            }
            // Static batching by collider
            if (staticParent != null && staticColliders.Count > 0)
            {
                ExporterWindow.ReportProgress(1, "Preparing static collider meshes... This may take a while.");
                GameObject colliderStatic = new GameObject("Colliders");
                colliderStatic.transform.parent = staticParent.transform;
                colliderStatic.transform.localPosition = Vector3.zero;
                colliderStatic.transform.localRotation = Quaternion.identity;
                colliderStatic.transform.localScale = Vector3.one;
                gameBuffer.Add(colliderStatic);
                // Prepare static collision buffer
                foreach (var volume in staticColliders)
                {
                    BabylonMesh collisionMesh = Tools.GenerateCollisionMesh(volume);
                    if (collisionMesh != null)
                    {
                        collisionMesh.parentId = GetID(colliderStatic);
                        collisionMesh.isVisible = exportationOptions.ShowDebugColliders;
                        collisionMesh.visibility = exportationOptions.ColliderVisibility;
                        collisionMesh.checkCollisions = (exportationOptions.ExportCollisions && volume.isTrigger == false);
                        collisionMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
                        collisionMesh.position = volume.transform.localPosition.ToFloat();
                        collisionMesh.rotation = new float[3];
                        collisionMesh.rotation[0] = volume.transform.localRotation.eulerAngles.x * (float)Math.PI / 180;
                        collisionMesh.rotation[1] = volume.transform.localRotation.eulerAngles.y * (float)Math.PI / 180;
                        collisionMesh.rotation[2] = volume.transform.localRotation.eulerAngles.z * (float)Math.PI / 180;
                        collisionMesh.scaling = volume.transform.localScale.ToFloat();
                        // Attach collision data
                        UnityMetaData collisionData = new UnityMetaData();
                        collisionData.api = true;
                        collisionData.prefab = false;
                        collisionData.tagName = "[COLLIDER]";
                        collisionData.objectId = collisionMesh.id;
                        collisionData.objectName = collisionMesh.name;
                        collisionData.layerIndex = ExporterWindow.StaticIndex;        
                        // Babylon physics state
                        if (exportationOptions.ExportPhysics)
                        {
                            var physics = volume.gameObject.GetComponent<PhysicsState>();
                            if (physics != null && physics.isActiveAndEnabled == true)
                            {
                                collisionMesh.tags += " [PHYSICS]";
                                collisionData.properties.Add("physicsTag", volume.gameObject.tag);
                                // Note: Is Non Movable Static Collision Mesh - physics.mass = 0
                                collisionData.properties.Add("physicsMass", 0);
                                collisionData.properties.Add("physicsFriction", physics.friction);
                                collisionData.properties.Add("physicsRestitution", physics.restitution);
                                collisionData.properties.Add("physicsImpostor", (int)physics.imposter);
                                collisionData.properties.Add("physicsRotation", (int)physics.rotation);
                                collisionData.properties.Add("physicsCollisions", (physics.type == BabylonCollisionType.Collider));
                                collisionData.properties.Add("physicsEnginePlugin", exportationOptions.DefaultPhysicsEngine);
                                SceneBuilder.Metadata.properties["hasPhysicsMeshes"] = true;                
                            }
                        }
                        collisionMesh.metadata = collisionData;
                        babylonScene.MeshesList.Add(collisionMesh);
                    }
                }
            }

            // Validate game object buffer
            if (gameBuffer.Count <= 0)
            {
                ExporterWindow.ShowMessage("No gameobject! - Please add at least a non batching static gameobject to export");
                return;
            }

            // Prepare scene builder buffers
            gameObjects = gameBuffer.ToArray();
            SceneBuilder.Metadata = new SceneMetaData();

            // Create Prefab Instance Parent
            prefabInstances = new BabylonMesh { name = "Prefab.Instances" };
            prefabInstances.id = System.Guid.NewGuid().ToString();
            prefabInstances.tags += " [PREFAB]";
            prefabInstances.parentId = null;
            prefabInstances.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
            prefabInstances.position = Vector3.zero.ToFloat();
            prefabInstances.rotation = Vector3.zero.ToFloat();
            prefabInstances.scaling = new Vector3(1.0f, 1.0f, 1.0f).ToFloat();
            prefabInstances.isEnabled = false;
            prefabInstances.isVisible = true;
            prefabInstances.visibility = 1;
            prefabInstances.checkCollisions = false;

            // Attach Prefab Instance Information
            UnityMetaData prefabInstanceData = new UnityMetaData();
            prefabInstanceData.api = true;
            prefabInstanceData.prefab = true;
            prefabInstanceData.tagName = "[INSTANCES]";
            prefabInstanceData.objectId = prefabInstances.id;
            prefabInstanceData.objectName = prefabInstances.name;
            prefabInstanceData.layerIndex = ExporterWindow.PrefabIndex;                
            prefabInstances.metadata = prefabInstanceData;

            // Parse all scene game objects
            index = 0;
            var itemsCount = gameObjects.Length;
            var lensFlareSystems = new List<UnityFlareSystem>();
            var particleSystems = new List<BabylonExport.Entities.BabylonParticleSystem>();
            ExporterWindow.ReportProgress(1, "Exporting game objects from scene...");
            babylonScene.physicsEngine = (exportationOptions.DefaultPhysicsEngine == 1) ? "oimo" : "cannon";
            try
            {
                bool foundController = false;
                foreach (var gameObject in gameObjects)
                {
                    var progress = ((float)index / itemsCount);
                    index++;

                    // Unity metadata
                    var metaData = new UnityMetaData();
                    metaData.objectId = GetID(gameObject);
                    metaData.objectName = gameObject.name;
                    metaData.tagName = gameObject.tag;
                    metaData.layerIndex = gameObject.layer;
                    metaData.layerName = LayerMask.LayerToName(gameObject.layer);

                    // Export hooking
                    var exportObject = gameObject;

                    // Components tags
                    string componentTags = String.Empty;
                    if (!String.IsNullOrEmpty(gameObject.tag) && !gameObject.tag.Equals("Untagged", StringComparison.OrdinalIgnoreCase))
                    {
                        componentTags = gameObject.tag;
                    }

                    // Navigation area
                    metaData.areaIndex = -1;
                    bool navigationStatic = gameObject.IsNavigationStatic();
                    if (navigationStatic)
                    {
                        metaData.areaIndex = GameObjectUtility.GetNavMeshArea(gameObject);
                    }

                    // Navigation agent
                    metaData.navAgent = null;
                    var navigationAgent = gameObject.GetComponent<NavMeshAgent>();
                    if (navigationAgent != null && navigationAgent.isActiveAndEnabled)
                    {
                        componentTags += " [NAVAGENT]";
                        Dictionary<string, object> agentInfo = new Dictionary<string, object>();
                        agentInfo.Add("name", navigationAgent.name);
                        agentInfo.Add("radius", navigationAgent.radius);
                        agentInfo.Add("height", navigationAgent.height);
                        agentInfo.Add("speed", navigationAgent.speed);
                        agentInfo.Add("acceleration", navigationAgent.acceleration);
                        agentInfo.Add("angularSpeed", navigationAgent.angularSpeed);
                        agentInfo.Add("areaMask", navigationAgent.areaMask);
                        agentInfo.Add("autoBraking", navigationAgent.autoBraking);
                        agentInfo.Add("autoTraverseOffMeshLink", navigationAgent.autoTraverseOffMeshLink);
                        agentInfo.Add("avoidancePriority", navigationAgent.avoidancePriority);
                        agentInfo.Add("baseOffset", navigationAgent.baseOffset);
                        agentInfo.Add("obstacleAvoidanceType", navigationAgent.obstacleAvoidanceType.ToString());
                        agentInfo.Add("stoppingDistance", navigationAgent.stoppingDistance);
                        metaData.navAgent = agentInfo;
                    }

                    // Navigation link
                    metaData.meshLink = null;
                    var navigationLink = gameObject.GetComponent<OffMeshLink>();
                    if (navigationLink != null && navigationLink.isActiveAndEnabled)
                    {
                        componentTags += " [MESHLINK]";
                        Dictionary<string, object> linkInfo = new Dictionary<string, object>();
                        linkInfo.Add("name", navigationLink.name);
                        linkInfo.Add("activated", navigationLink.activated);
                        linkInfo.Add("area", navigationLink.area);
                        linkInfo.Add("autoUpdatePositions", navigationLink.autoUpdatePositions);
                        linkInfo.Add("biDirectional", navigationLink.biDirectional);
                        linkInfo.Add("costOverride", navigationLink.costOverride);
                        linkInfo.Add("occupied", navigationLink.occupied);
                        linkInfo.Add("start", GetTransformPropertyValue(navigationLink.startTransform));
                        linkInfo.Add("end", GetTransformPropertyValue(navigationLink.endTransform));
                        metaData.meshLink = linkInfo;
                    }

                    // Navigation obstacle
                    metaData.meshObstacle = null;
                    var navigationObstacle = gameObject.GetComponent<NavMeshObstacle>();
                    if (navigationObstacle != null && navigationObstacle.isActiveAndEnabled)
                    {
                        componentTags += " [MESHOBSTACLE]";
                        Dictionary<string, object> obstacleInfo = new Dictionary<string, object>();
                        obstacleInfo.Add("name", navigationObstacle.name);
                        obstacleInfo.Add("carving", navigationObstacle.carving);
                        obstacleInfo.Add("carveOnlyStationary", navigationObstacle.carveOnlyStationary);
                        obstacleInfo.Add("carvingMoveThreshold", navigationObstacle.carvingMoveThreshold);
                        obstacleInfo.Add("carvingTimeToStationary", navigationObstacle.carvingTimeToStationary);
                        obstacleInfo.Add("shape", navigationObstacle.shape.ToString());
                        obstacleInfo.Add("radius", navigationObstacle.radius);
                        obstacleInfo.Add("center", navigationObstacle.center.ToFloat());
                        obstacleInfo.Add("size", navigationObstacle.size.ToFloat());
                        metaData.meshObstacle = obstacleInfo;
                    }
                    var legacyControl = gameObject.GetComponent<Animation>();
                    var animatorControl = gameObject.GetComponent<Animator>();
                    var animationState = gameObject.GetComponent<UnityEditor.AnimationState>();
                    if (animationState != null && animationState.isActiveAndEnabled) {
                        // Animation State
                        if (animationState.enabled && animatorControl != null) {
                            metaData.properties["stateMachineInfo"] = Tools.ExportStateMachine(animatorControl, animationState.enableStateMachine);
                        }
                        // Animation Events
                        if (animationState.enableEvents && (animatorControl != null || legacyControl != null)) {
                            int frameRate = 0;
                            int frameOffest = 0;
                            int totalFrameCount = 0;
                            List<AnimationClip> animationClips = null;
                            if (legacyControl != null) {
                                animationClips = Tools.GetAnimationClips(legacyControl);
                            } else {
                                animationClips = Tools.GetAnimationClips(animatorControl);
                            }
                            if (animationClips != null && animationClips.Count > 0)
                            {
                                foreach (var state in animationClips)
                                {
                                    AnimationClip clip = state as AnimationClip;
                                    if (frameRate <= 0) frameRate = (int)clip.frameRate;
                                    int clipFrameCount = (int)(clip.length * frameRate);
                                    if (clip.events != null && clip.events.Length > 0)
                                    {
                                        foreach(var animEvent in clip.events)
                                        {
                                            string objectIdParameter = null;
                                            if (animEvent.objectReferenceParameter != null && animEvent.objectReferenceParameter is GameObject)
                                            {
                                                GameObject gox = animEvent.objectReferenceParameter as GameObject;
                                                objectIdParameter = GetID(gox);
                                            }
                                            Dictionary<string, object> animEventInfo = new Dictionary<string, object>();
                                            int eventFrame = (int)((animEvent.time * frameRate) + frameOffest);
                                            animEventInfo.Add("clip", clip.name);
                                            animEventInfo.Add("frame", eventFrame);
                                            animEventInfo.Add("functionName", animEvent.functionName);
                                            animEventInfo.Add("intParameter", animEvent.intParameter);
                                            animEventInfo.Add("floatParameter", animEvent.floatParameter);
                                            animEventInfo.Add("stringParameter", animEvent.stringParameter);
                                            animEventInfo.Add("objectIdParameter", objectIdParameter);
                                            metaData.animationEvents.Add(animEventInfo);
                                        }
                                    }
                                    frameOffest += clipFrameCount;
                                    totalFrameCount += clipFrameCount;
                                }
                            }
                            if (metaData.animationEvents.Count > 0)
                            {
                                componentTags += " [ANIMEVENTS]";
                                SceneBuilder.Metadata.properties["hasAnimationEvents"] = true;
                            }
                        }
                    }

                    // Tags component
                    var tagsComponent = gameObject.GetComponent<TagsComponent>();
                    if (tagsComponent != null && tagsComponent.isActiveAndEnabled)
                    {
                        if (!String.IsNullOrEmpty(tagsComponent.babylonTags))
                        {
                            componentTags += (" " + tagsComponent.babylonTags);
                        }
                    }

                    // Script components
                    var gameComponents = gameObject.GetComponents<EditorScriptComponent>();
                    if (gameComponents != null)
                    {
                        var components = new List<object>();
                        foreach (var gameComponent in gameComponents)
                        {
                            if (gameComponent.isActiveAndEnabled == false) continue;
                            bool exportAsComponent = true;
                            if (gameComponent is UnityEditor.ParticleSystems) {
                                exportAsComponent = false; // Note: Default False Until Export Particle Check
                                var particle = gameComponent as UnityEditor.ParticleSystems;
                                if (particle.isActiveAndEnabled) {
                                    if (exportationOptions.ExportMetadata == false || particle.exportOption == BabylonParticleExporter.NativeSceneFile) {
                                        exportAsComponent = true; // Note: Export particle System To Native Scene File
                                        var particleSystem = new BabylonExport.Entities.BabylonParticleSystem();
                                        string pname = (!String.IsNullOrEmpty(particle.particleName)) ? particle.particleName : String.Format("PX_Unknown_" + Guid.NewGuid().ToString());
                                        particleSystem.name = pname;
                                        particleSystem.emitterId = metaData.objectId;
                                        particleSystem.linkToEmitter = true;
                                        particleSystem.preventAutoStart = !particle.autoStart;
                                        particleSystem.textureMask = particle.textureMask.ToFloat();
                                        particleSystem.updateSpeed = particle.startSpeed;
                                        particleSystem.emitRate = particle.emitRate;
                                        particleSystem.gravity = particle.gravityVector.ToFloat();
                                        particleSystem.blendMode = (int)particle.blendMode;
                                        particleSystem.capacity = particle.capacity;
                                        particleSystem.color1 = particle.color1.ToFloat();
                                        particleSystem.color2 = particle.color2.ToFloat();
                                        particleSystem.colorDead = particle.colorDead.ToFloat();
                                        particleSystem.direction1 = particle.direction1.ToFloat();
                                        particleSystem.direction2 = particle.direction2.ToFloat();
                                        particleSystem.minEmitBox = particle.minEmitBox.ToFloat();
                                        particleSystem.maxEmitBox = particle.maxEmitBox.ToFloat();
                                        particleSystem.minEmitPower = particle.emitPower.x;
                                        particleSystem.maxEmitPower = particle.emitPower.y;
                                        particleSystem.minLifeTime = particle.lifeTime.x;
                                        particleSystem.maxLifeTime = particle.lifeTime.y;
                                        particleSystem.minSize = particle.particleSize.x;
                                        particleSystem.maxSize = particle.particleSize.y;
                                        particleSystem.minAngularSpeed = particle.angularSpeed.x;
                                        particleSystem.maxAngularSpeed = particle.angularSpeed.y;
                                        particleSystem.targetStopDuration = particle.duration;
                                        //particleSystem.disposeOnStop = particle.disposeOnStop;
                                        particleSystem.animations = null; // TODO: Support Particle System Animations
                                        particleSystem.customShader = this.GetParticleSystemShader(particle);
                                        if (particle.textureImage != null)
                                        {
                                            var babylonTexture = new BabylonTexture();
                                            var texturePath = AssetDatabase.GetAssetPath(particle.textureImage);
                                            CopyTexture(texturePath, particle.textureImage, babylonTexture);
                                            particleSystem.textureName = Path.GetFileName(texturePath);
                                        }
                                        particleSystems.Add(particleSystem);
                                    } else {
                                        exportAsComponent = true; // Note: Export particle System As Script Component
                                    }
                                }
                            }
                            if (exportAsComponent) {
                                Type componentType = gameComponent.GetType();
                                string componentName = componentType.FullName;
                                var component = new UnityScriptComponent();
                                MonoScript componentScript = MonoScript.FromMonoBehaviour(gameComponent);
                                component.order = MonoImporter.GetExecutionOrder(componentScript);
                                component.name = componentName;
                                component.klass = gameComponent.babylonClass;
                                component.update = true;
                                SceneController controller = (gameComponent is SceneController) ? gameComponent as SceneController : null;
                                if (controller != null)
                                {
                                    component.order = -1;
                                    if (foundController == false)
                                    {
                                        foundController = true;
                                        componentTags += " [CONTROLLER]";
                                        string interfaceMode = "None";
                                        object userInterface = null;
                                        EmbeddedAsset guiAsset = controller.sceneOptions.graphicUserInterface;
                                        if (guiAsset != null)
                                        {
                                            userInterface = GetEmbeddedAssetPropertyValue(guiAsset);
                                            if (userInterface != null) {
                                                interfaceMode = "Html";
                                            }
                                        }
                                        SceneBuilder.Metadata.properties.Add("autoDraw", controller.sceneOptions.autoDrawInterface);
                                        SceneBuilder.Metadata.properties.Add("interfaceMode", interfaceMode);
                                        SceneBuilder.Metadata.properties.Add("userInterface", userInterface);
                                        SceneBuilder.Metadata.properties.Add("controllerPresent", true);
                                        SceneBuilder.Metadata.properties.Add("controllerObjectId", metaData.objectId);
                                    }
                                    else
                                    {
                                        Debug.LogError("Duplicate scene controller detected: " + component.name);
                                    }
                                }
                                FieldInfo[] componentFields = componentType.GetFields();
                                if (componentFields != null)
                                {
                                    foreach (var componentField in componentFields)
                                    {
                                        var componentAttribute = (BabylonPropertyAttribute)Attribute.GetCustomAttribute(componentField, typeof(BabylonPropertyAttribute));
                                        if (componentAttribute != null && componentField.Name != "babylonClass")
                                        {
                                            component.properties.Add(componentField.Name, GetComponentPropertyValue(componentField, gameComponent));
                                        }
                                    }
                                }
                                if (gameComponent.OnExportProperties != null) {
                                    gameComponent.OnExportProperties(exportObject, component.properties);
                                }
                                components.Add(component);
                            }
                        }
                        if (components.Count > 0)
                        {
                            metaData.components = components;
                        }
                    }

                    // Format tags
                    if (!String.IsNullOrEmpty(componentTags))
                    {
                        componentTags = componentTags.Trim();
                    }

                    // Setup Meshes
                    var meshFilter = gameObject.GetComponent<MeshFilter>();
                    var skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();

                    // Audio sources
                    if (SceneController != null && SceneController.sceneOptions.audioSources)
                    {
                        var audioComponents = gameObject.GetComponents<AudioTrack>();
                        if (audioComponents != null)
                        {
                            foreach (var item in audioComponents)
                            {
                                if (item != null && item.isActiveAndEnabled && item.audioClip != null)
                                {
                                    string soundPath = AssetDatabase.GetAssetPath(item.audioClip);
                                    if (!String.IsNullOrEmpty(soundPath))
                                    {
                                        string soundName = Path.GetFileName(soundPath).Replace(" ", "");
                                        string outputFile = Path.Combine(OutputPath, soundName);
                                        if (File.Exists(soundPath))
                                        {
                                            File.Copy(soundPath, outputFile, true);
                                            var sound = new BabylonSound();
                                            sound.name = soundName;
                                            sound.volume = item.soundTrack.volume;
                                            sound.playbackRate = item.soundTrack.playbackRate;
                                            sound.autoplay = item.soundTrack.autoplay;
                                            sound.loop = item.soundTrack.loop;
                                            sound.soundTrackId = item.soundTrack.soundTrackId;
                                            sound.spatialSound = item.soundTrack.spatialSound;
                                            sound.position = item.soundTrack.position.ToFloat();
                                            sound.refDistance = item.soundTrack.refDistance;
                                            sound.rolloffFactor = item.soundTrack.rolloffFactor;
                                            sound.maxDistance = item.soundTrack.maxDistance;
                                            sound.distanceModel = item.soundTrack.distanceModel;
                                            sound.panningModel = item.soundTrack.panningModel;
                                            sound.isDirectional = item.soundTrack.isDirectional;
                                            sound.coneInnerAngle = item.soundTrack.coneInnerAngle;
                                            sound.coneOuterAngle = item.soundTrack.coneOuterAngle;
                                            sound.coneOuterGain = item.soundTrack.coneOuterGain;
                                            sound.localDirectionToMesh = item.soundTrack.directionToMesh.ToFloat();
                                            sound.connectedMeshId = null;
                                            if (sound.spatialSound) {
                                                if (meshFilter != null || skinnedMesh != null) {
                                                    sound.connectedMeshId = GetID(gameObject);
                                                }
                                            }
                                            babylonScene.SoundsList.Add(sound);
                                        }
                                        else
                                        {
                                            Debug.LogError("Fail to locate audio file: " + soundPath);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError("Null audio clip path for: " + item.audioClip.name);
                                    }
                                }
                            }
                        }
                    }

                    // Lod Groups
                    var lodGroup = gameObject.GetComponent<LODGroup>();
                    if (lodGroup != null) {
                        var levelDistance = gameObject.GetComponent<LevelDistance>();
                        float nearClipingPlane = (Camera.main != null) ? Camera.main.nearClipPlane : 0.3f;
                        float farClipingPlane = (Camera.main != null) ? Camera.main.farClipPlane : 1000.0f;
                        List<object> lodDetails = new List<object>();
                        LOD[] lods = lodGroup.GetLODs();
                        float startingPercent = -1f;
                        float endingPercent = -1f;
                        float lastPercent = 0f;
                        int startRange = 0;
                        foreach (var lod in lods) {
                            List<object> lodRenderers = new List<object>();
                            foreach (var renderer in lod.renderers) {
                                Dictionary<string, object> rendererDetail = new Dictionary<string, object>();
                                rendererDetail.Add("name", renderer.gameObject.name);
                                rendererDetail.Add("source", GetID(renderer.gameObject));
                                lodRenderers.Add(rendererDetail);
                            };
                            endingPercent = lod.screenRelativeTransitionHeight;
                            if (startingPercent == -1) startingPercent = 1;
                            else startingPercent = lastPercent;
                            float lodDistance = startingPercent - endingPercent;
                            Dictionary<string, object> lodDetail = new Dictionary<string, object>();
                            lodDetail.Add("screenHeight", lod.screenRelativeTransitionHeight);
                            lodDetail.Add("fadeTransition", lod.fadeTransitionWidth);
                            lodDetail.Add("startingPercent", startingPercent);
                            lodDetail.Add("endingPercent", endingPercent);
                            lodDetail.Add("rendererCount", lodRenderers.Count);
                            lodDetail.Add("lodRenderers", (lodRenderers.Count > 0) ? lodRenderers.ToArray() : null);
                            lodDetail.Add("lodDistance", lodDistance);
                            lodDetail.Add("startRange", startRange);
                            lodDetails.Add(lodDetail);
                            lastPercent = endingPercent;
                            startRange += Tools.CalculateCameraDistance(nearClipingPlane, farClipingPlane, lodDistance, levelDistance);
                        }
                        float startCulling = startRange;
                        Dictionary<string, object> lodGroupInfo = new Dictionary<string, object>();
                        lodGroupInfo.Add("lodCount", lodGroup.lodCount);
                        lodGroupInfo.Add("lodDetails", (lodDetails.Count > 0) ? lodDetails.ToArray() : null);
                        lodGroupInfo.Add("fadeMode", lodGroup.fadeMode.ToString());
                        lodGroupInfo.Add("crossFading", lodGroup.animateCrossFading);
                        lodGroupInfo.Add("startCulling", startCulling);
                        lodGroupInfo.Add("nearClipingPlane", nearClipingPlane);
                        lodGroupInfo.Add("farClipingPlane", farClipingPlane);
                        lodGroupInfo.Add("cameraDistanceFactor", ExporterWindow.exportationOptions.CameraDistanceFactor);
                        metaData.properties["lodGroupInfo"] = lodGroupInfo;
                    }

                    // Scene lights
                    var light = gameObject.GetComponent<Light>();
                    if (light != null && light.isActiveAndEnabled)
                    {
                        ConvertUnityLightToBabylon(light, gameObject, progress, ref metaData, ref lensFlareSystems, ref componentTags);
                        continue;
                    }

                    // Scene cameras
                    var camera = gameObject.GetComponent<Camera>();
                    if (camera != null && camera.isActiveAndEnabled)
                    {
                        ConvertUnityCameraToBabylon(camera, gameObject, progress, ref metaData, ref lensFlareSystems, ref componentTags);
                        ConvertUnitySkyboxToBabylon(camera, progress);
                        continue;
                    }

                    // Terrain meshes
                    var terrainMesh = gameObject.GetComponent<Terrain>();
                    if (terrainMesh != null && terrainMesh.isActiveAndEnabled)
                    {
                        ConvertUnityTerrainToBabylon(terrainMesh, gameObject, progress, ref metaData, ref lensFlareSystems, ref componentTags);
                        continue;
                    }

                    // Collision meshes
                    var collider = gameObject.GetComponent<Collider>();
                    BabylonMesh collisionMesh = Tools.GenerateCollisionMesh(collider);

                    // Skinned meshes
                    if (skinnedMesh != null && skinnedMesh.enabled)
                    {
                        var babylonMesh = ConvertUnityMeshToBabylon(skinnedMesh.sharedMesh, skinnedMesh.transform, gameObject, progress, ref metaData, ref lensFlareSystems, ref componentTags, collisionMesh, collider);
                        if (skinnedMesh.rootBone != null) {
                            if (SceneBuilder.RootBoneTransformMap.ContainsKey(skinnedMesh.rootBone)) {
                                string sharedSkeletonId = SceneBuilder.RootBoneTransformMap[skinnedMesh.rootBone];
                                metaData.properties["sharedSkeletonId"] = sharedSkeletonId; 
                            } else {
                                var skeleton = ConvertUnitySkeletonToBabylon(skinnedMesh, gameObject, progress, ref metaData);
                                babylonMesh.skeletonId = skeleton.id;
                                string sharedSkeletonId = babylonMesh.id;
                                ExportSkeletonAnimation(skinnedMesh, babylonMesh, skeleton, ref metaData);
                                SceneBuilder.RootBoneTransformMap.Add(skinnedMesh.rootBone, sharedSkeletonId);
                            }
                        }
                        continue;
                    }

                    // Static meshes
                    if (meshFilter != null)
                    {
                        var renderer = gameObject.GetComponent<Renderer>();
                        if (renderer != null && renderer.enabled) {
                            ConvertUnityMeshToBabylon(meshFilter.sharedMesh, meshFilter.transform, gameObject, progress, ref metaData, ref lensFlareSystems, ref componentTags, collisionMesh, collider);
                            continue;
                        }
                    }

                    // Empty objects
                    ConvertUnityEmptyObjectToBabylon(gameObject, ref metaData, ref lensFlareSystems, ref componentTags, collisionMesh, collider);
                }

                // Materials
                foreach (var mat in materialsDictionary)
                {
                    babylonScene.MaterialsList.Add(mat.Value);
                }
                foreach (var multiMat in multiMatDictionary)
                {
                    babylonScene.MultiMaterialsList.Add(multiMat.Value);
                }

                // Collisions
                if (exportationOptions.ExportCollisions)
                {
                    babylonScene.workerCollisions = exportationOptions.WorkerCollisions;
                    if (SceneController != null) {
                        babylonScene.gravity = SceneController.sceneOptions.defaultGravity.ToFloat();
                    }
                }

                // Babylon Physics
                if (exportationOptions.ExportPhysics)
                {
                    babylonScene.physicsEnabled = true;
                    if (SceneController != null) {
                        babylonScene.physicsGravity = SceneController.sceneOptions.defaultGravity.ToFloat();
                    }
                }

                // Scene Controller
                babylonScene.ambientColor = Color.clear.ToFloat();
                if (SceneController != null)
                {
                    babylonScene.autoClear = SceneController.sceneOptions.clearCanvas;
                    int fogmode = 0;
                    if (RenderSettings.fog)
                    {
                        switch (RenderSettings.fogMode)
                        {
                            case FogMode.Exponential:
                                fogmode = 1;
                                break;
                            case FogMode.ExponentialSquared:
                                fogmode = 2;
                                break;
                            case FogMode.Linear:
                                fogmode = 3;
                                break;
                        }
                    }
                    babylonScene.fogMode = fogmode;
                    babylonScene.fogDensity = RenderSettings.fogDensity;
                    babylonScene.fogColor = RenderSettings.fogColor.ToFloat();
                    babylonScene.fogStart = RenderSettings.fogStartDistance;
                    babylonScene.fogEnd = RenderSettings.fogEndDistance;
                    SceneController.configOptions.inputProperties.joystickInputValue = (int)SceneController.configOptions.inputProperties.joystickInputMode;
                    SceneController.configOptions.inputProperties.joystickRightColorText = ("#" + ColorUtility.ToHtmlStringRGBA(SceneController.configOptions.inputProperties.joystickRightColor));
                    SceneBuilder.Metadata.properties.Add("resizeCameras", SceneController.sceneOptions.resizeCameras);
                    SceneBuilder.Metadata.properties.Add("timeManagement", SceneController.sceneOptions.enableTime);
                    SceneBuilder.Metadata.properties.Add("enableUserInput", SceneController.configOptions.enableInput);
                    SceneBuilder.Metadata.properties.Add("preventDefault", SceneController.configOptions.preventDefault);
                    SceneBuilder.Metadata.properties.Add("useCapture", SceneController.configOptions.captureInput);
                    SceneBuilder.Metadata.properties.Add("userInput", SceneController.configOptions.inputProperties);
                    if (SceneController.lightingOptions.ambientLight == BabylonAmbientLighting.UnityAmbientLighting)
                    {
                        var ambientLight = new BabylonLight
                        {
                            name = "Ambient Light",
                            id = Guid.NewGuid().ToString(),
                            parentId = null,
                            metadata = null,
                            position = null,
                            type = 3
                        };
                        Color ambientGroundColor = Tools.GetGroundColor();
                        Color ambientDiffuseColor = Tools.GetAmbientColor();
                        Color ambientSpecularColor = SceneController.lightingOptions.ambientSpecular;
                        Vector3 ambientLightDirection = new Vector3(0.0f, 1.0f, 0.0f);
                        ambientLight.intensity = Tools.GetAmbientIntensity(SceneController);
                        ambientLight.direction = ambientLightDirection.ToFloat();
                        ambientLight.diffuse = ambientDiffuseColor.ToFloat();
                        ambientLight.specular = ambientSpecularColor.ToFloat();
                        ambientLight.groundColor = ambientGroundColor.ToFloat();
                        babylonScene.ambientColor = ambientDiffuseColor.ToFloat();
                        babylonScene.LightsList.Add(ambientLight);
                        ExporterWindow.ReportProgress(0, "Exporting ambient light at intensity level: " + ambientLight.intensity.ToString());
                    }
                    if (SceneController.sceneOptions.navigationMesh == BabylonNavigationMesh.EnableNavigation)
                    {
                        ExporterWindow.ReportProgress(0, "Parsing scene navigation mesh...");
                        NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();
                        if (triangulatedNavMesh.vertices != null && triangulatedNavMesh.vertices.Length > 0 && triangulatedNavMesh.indices != null && triangulatedNavMesh.indices.Length > 0)
                        {
                            int vertexCount = triangulatedNavMesh.vertices.Length;
                            if (vertexCount <= ExporterWindow.MaxVerticies)
                            {
                                ExporterWindow.ReportProgress(0, "Generating navigation mesh vertices: " + vertexCount.ToString());
                                var navData = new UnityMetaData();
                                navData.type = "NavMesh";
                                navData.objectId = Guid.NewGuid().ToString();
                                navData.objectName = "Navigation_Mesh";
                                var areaTable = new List<object>();
                                string[] areaNavigation = GameObjectUtility.GetNavMeshAreaNames();
                                foreach (string areaName in areaNavigation)
                                {
                                    var bag = new Dictionary<string, object>();
                                    int areaIndex = NavMesh.GetAreaFromName(areaName);
                                    float areaCost = NavMesh.GetAreaCost(areaIndex);
                                    bag.Add("index", areaIndex);
                                    bag.Add("area", areaName);
                                    bag.Add("cost", areaCost);
                                    areaTable.Add(bag);
                                }
                                navData.properties.Add("table", areaTable);
                                navData.properties.Add("areas", triangulatedNavMesh.areas);

                                Mesh mesh = new Mesh();
                                mesh.name = "sceneNavigationMesh";
                                mesh.vertices = triangulatedNavMesh.vertices;
                                mesh.triangles = triangulatedNavMesh.indices;
                                mesh.RecalculateNormals();

                                BabylonMesh babylonMesh = new BabylonMesh();
                                babylonMesh.tags = "[NAVMESH]";
                                babylonMesh.metadata = (exportationOptions.ExportMetadata) ? navData : null;
                                babylonMesh.name = mesh.name;
                                babylonMesh.id = Guid.NewGuid().ToString();
                                babylonMesh.parentId = null;
                                babylonMesh.position = Vector3.zero.ToFloat();
                                babylonMesh.rotation = Vector3.zero.ToFloat();
                                babylonMesh.scaling = new Vector3(1, 1, 1).ToFloat();
                                babylonMesh.isVisible = false;
                                babylonMesh.visibility = 0.75f;
                                babylonMesh.checkCollisions = false;
                                babylonMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
                                Tools.GenerateBabylonMeshData(mesh, babylonMesh);
                                babylonScene.MeshesList.Add(babylonMesh);
                                SceneBuilder.Metadata.properties["hasNavigationMesh"] = true;
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("Navigation mesh exceeds max (65000) vertex limit: " + vertexCount.ToString());
                            }
                        }
                    }
                    if (SceneController.sceneOptions.lensFlareSystems)
                    {
                        if (lensFlareSystems != null && lensFlareSystems.Count > 0)
                        {
                            var lfs_buffer = new List<BabylonLensFlareSystem>();
                            foreach (var ulfs in lensFlareSystems)
                            {
                                var lfs = new BabylonLensFlareSystem();
                                lfs.borderLimit = ulfs.borderLimit;
                                lfs.emitterId = ulfs.emitterId;
                                var lfx = new List<BabylonLensFlare>();
                                foreach (var ulf in ulfs.lensFlares)
                                {
                                    var lf = new BabylonLensFlare();
                                    lf.textureName = ulf.textureName;
                                    lf.position = ulf.position;
                                    lf.color = ulf.color;
                                    lf.size = ulf.size;
                                    lfx.Add(lf);
                                }
                                lfs.flares = lfx.ToArray();
                                lfs_buffer.Add(lfs);
                            }
                            // Scene Lens Flare System
                            if (lfs_buffer.Count > 0) {
                                babylonScene.lensFlareSystems = lfs_buffer.ToArray();
                            }
                        }
                    }

                    // Scene Particle Systems
                    if (particleSystems.Count > 0) {
                        babylonScene.particleSystems = particleSystems.ToArray();
                    }

                    // Custom Animation Nodes
                    if (SceneBuilder.AnimationCurveKeys != null && SceneBuilder.AnimationCurveKeys.Keys.Count > 0) {
                        foreach (var key in SceneBuilder.AnimationCurveKeys.Keys) {
                            var item = Tools.FindAnimatableItem(key, babylonScene);
                            if (item != null) {
                                var anims = SceneBuilder.AnimationCurveKeys[key];
                                if (anims != null) {
                                    if (item.animations != null && item.animations.Length > 0) {
                                        foreach (var old in item.animations) {
                                            anims.Add(old);
                                        }
                                    }
                                    if (anims.Count > 0) {
                                        item.animations = anims.ToArray();
                                    }
                                }
                            }
                        }
                    }

                    // Scene With Debug Sockets
                    SceneBuilder.Metadata.properties["colliderVisibility"] = exportationOptions.ColliderVisibility;
                    SceneBuilder.Metadata.properties["socketColliderSize"] = exportationOptions.SocketColliderSize;
                    SceneBuilder.Metadata.properties["showDebugSockets"] = exportationOptions.ShowDebugSockets;
                    SceneBuilder.Metadata.properties["staticVertexLimit"] = exportationOptions.StaticVertexLimit;

                    //  Scene Environment Texture
                    if (sceneReflectionTexture == null) sceneReflectionTexture = DumpLightingReflectionTexture();
                    SceneBuilder.Metadata.properties["environmentTexture"] = sceneReflectionTexture;

                    // Scene Prefab Instance Meshes                    
                    if (prefabInstances != null && SceneBuilder.Metadata.properties.ContainsKey("hasInstanceMeshes"))  {
                        babylonScene.MeshesList.Add(prefabInstances);
                    }

                    //  Scene Import Mesh Dependencies
                    if (SceneController.sceneOptions.importSceneMeshes != null && SceneController.sceneOptions.importSceneMeshes.Length > 0)
                    {
                        foreach (var importer in SceneController.sceneOptions.importSceneMeshes)
                        {
                            if (importer != null && !String.IsNullOrEmpty(importer.name)) {
                                string sceneName = importer.name + Tools.GetSceneFileExtension();
                                if (!SceneBuilder.Metadata.imports.Contains(sceneName)) {
                                    SceneBuilder.Metadata.imports.Add(sceneName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (exportationOptions.ExportMetadata)
                {
                    babylonScene.metadata = SceneBuilder.Metadata;
                }
                if (staticParent != null)
                {
                    GameObject.DestroyImmediate(staticParent);
                    staticParent = null;
                }
            }
        }

        private void ParseLensFlares(GameObject gameObject, string emitterId, ref List<UnityFlareSystem> lens)
        {
            var flare = gameObject.GetComponent<FlareSystem>();
            if (flare != null && flare.isActiveAndEnabled && flare.lensFlares != null && flare.lensFlares.Length > 0)
            {
                string fname = (!String.IsNullOrEmpty(flare.flareName)) ? flare.flareName : String.Format("lensFlare#" + Guid.NewGuid().ToString());
                var flareSystem = new UnityFlareSystem();
                flareSystem.name = fname;
                flareSystem.emitterId = emitterId;
                flareSystem.borderLimit = flare.borderLimit;
                var flareBuffer = new List<UnityFlareItem>();
                foreach (var flareItem in flare.lensFlares)
                {
                    var item = new UnityFlareItem();
                    item.size = flareItem.size;
                    item.position = flareItem.position;
                    item.color = flareItem.color.ToFloat();
                    if (flareItem.texture != null)
                    {
                        var babylonTexture = new BabylonTexture();
                        var texturePath = AssetDatabase.GetAssetPath(flareItem.texture);
                        CopyTexture(texturePath, flareItem.texture, babylonTexture);
                        item.textureName = Path.GetFileName(texturePath);
                    }
                    flareBuffer.Add(item);
                }
                flareSystem.lensFlares = flareBuffer.ToArray();
                lens.Add(flareSystem);
            }
        }

        private object GetParticleSystemShader(UnityEditor.ParticleSystems particle)
        {
            if (particle == null || String.IsNullOrEmpty(particle.customShaderEffect)) return null;
            var result = new Dictionary<string, object>();            
            string basename = particle.customShaderEffect;
            var shaderPath = new Dictionary<string, object>();
            shaderPath.Add("fragmentElement", basename);
            result.Add("shaderPath", shaderPath);
            var shaderOptions = new Dictionary<string, object>();
            shaderOptions.Add("uniforms", particle.customShaderUniforms);
            shaderOptions.Add("samplers", particle.customShaderSamplers);
            shaderOptions.Add("defines", particle.customShaderDefines);
            result.Add("shaderOptions", shaderOptions);
            return result;
        }        

        private object GetComponentPropertyValue(FieldInfo field, EditorScriptComponent component)
        {
            object result = null;
            object fvalue = field.GetValue(component);
            if (fvalue != null)
            {
                Type ftype = fvalue.GetType();
                if (typeof(System.Enum).IsAssignableFrom(ftype))
                {
                    result = Convert.ToInt32((System.Enum)fvalue);
                }
                else if (typeof(Boolean).IsAssignableFrom(ftype) || typeof(Byte).IsAssignableFrom(ftype) || typeof(SByte).IsAssignableFrom(ftype) || typeof(Int16).IsAssignableFrom(ftype) || typeof(UInt16).IsAssignableFrom(ftype) || typeof(Int32).IsAssignableFrom(ftype) || typeof(UInt32).IsAssignableFrom(ftype) || typeof(Int64).IsAssignableFrom(ftype) || typeof(UInt64).IsAssignableFrom(ftype) || typeof(IntPtr).IsAssignableFrom(ftype) || typeof(UIntPtr).IsAssignableFrom(ftype) || typeof(Char).IsAssignableFrom(ftype) || typeof(Double).IsAssignableFrom(ftype) || typeof(Single).IsAssignableFrom(ftype))
                {
                    result = fvalue;
                }
                else if (typeof(System.String).IsAssignableFrom(ftype))
                {
                    result = fvalue;
                }
                else if (typeof(Color).IsAssignableFrom(ftype) || typeof(Color32).IsAssignableFrom(ftype))
                {
                    result = fvalue;
                }
                else if (typeof(Vector2).IsAssignableFrom(ftype))
                {
                    result = fvalue;
                }
                else if (typeof(Vector3).IsAssignableFrom(ftype))
                {
                    result = fvalue;
                }
                else if (typeof(Vector4).IsAssignableFrom(ftype))
                {
                    result = fvalue;
                }
                else if (typeof(Transform).IsAssignableFrom(ftype))
                {
                    var transform = (Transform)fvalue;
                    result = GetTransformPropertyValue(transform);
                }
                else if (typeof(Texture2D).IsAssignableFrom(ftype))
                {
                    var texture = (Texture2D)fvalue;
                    result = GetTexturePropertyValue(texture);
                }
                else if (typeof(Cubemap).IsAssignableFrom(ftype))
                {
                    var cubemap = (Cubemap)fvalue;
                    result = GetCubemapPropertyValue(cubemap);
                }
                else if (typeof(Material).IsAssignableFrom(ftype))
                {
                    var material = (Material)fvalue;
                    result = GetMaterialPropertyValue(material);
                }
                else if (typeof(GameObject).IsAssignableFrom(ftype))
                {
                    var gobject = (GameObject)fvalue;
                    result = GetGamePropertyValue(gobject);
                }
                else if (typeof(Camera).IsAssignableFrom(ftype))
                {
                    var acamera = (Camera)fvalue;
                    result = GetCameraPropertyValue(acamera);
                }
                else if (typeof(AudioClip).IsAssignableFrom(ftype))
                {
                    var aclip = (AudioClip)fvalue;
                    result = GetAudioClipPropertyValue(aclip);
                }
                else if (typeof(AnimationCurve).IsAssignableFrom(ftype))
                {
                    var acurve = (AnimationCurve)fvalue;
                    result = GetCurvePropertyValue(acurve);
                }
                else if (typeof(EmbeddedAsset).IsAssignableFrom(ftype))
                {
                    var easset = (EmbeddedAsset)fvalue;
                    result = GetEmbeddedAssetPropertyValue(easset);
                }
                else if (typeof(TextAsset).IsAssignableFrom(ftype))
                {
                    var tasset = (TextAsset)fvalue;
                    result = GetTextAssetPropertyValue(tasset);
                }
                else if (typeof(DefaultAsset).IsAssignableFrom(ftype))
                {
                    var dasset = (DefaultAsset)fvalue;
                    result = GetDefaultAssetPropertyValue(dasset);
                }
                else if (ftype.IsArray)
                {
                    if (typeof(Boolean[]).IsAssignableFrom(ftype) || typeof(Byte[]).IsAssignableFrom(ftype) || typeof(SByte[]).IsAssignableFrom(ftype) || typeof(Int16[]).IsAssignableFrom(ftype) || typeof(UInt16[]).IsAssignableFrom(ftype) || typeof(Int32[]).IsAssignableFrom(ftype) || typeof(UInt32[]).IsAssignableFrom(ftype) || typeof(Int64[]).IsAssignableFrom(ftype) || typeof(UInt64[]).IsAssignableFrom(ftype) || typeof(IntPtr[]).IsAssignableFrom(ftype) || typeof(UIntPtr[]).IsAssignableFrom(ftype) || typeof(Char[]).IsAssignableFrom(ftype) || typeof(Double[]).IsAssignableFrom(ftype) || typeof(Single[]).IsAssignableFrom(ftype))
                    {
                        result = fvalue;
                    }
                    else if (typeof(System.String[]).IsAssignableFrom(ftype))
                    {
                        result = fvalue;
                    }
                    else if (typeof(Color[]).IsAssignableFrom(ftype) || typeof(Color32[]).IsAssignableFrom(ftype))
                    {
                        result = fvalue;
                    }
                    else if (typeof(Vector2[]).IsAssignableFrom(ftype))
                    {
                        result = fvalue;
                    }
                    else if (typeof(Vector3[]).IsAssignableFrom(ftype))
                    {
                        result = fvalue;
                    }
                    else if (typeof(Vector4[]).IsAssignableFrom(ftype))
                    {
                        result = fvalue;
                    }
                    else if (typeof(Transform[]).IsAssignableFrom(ftype))
                    {
                        var transforms = (Transform[])fvalue;
                        var transform_list = new List<object>();
                        foreach (var transform in transforms)
                        {
                            transform_list.Add(GetTransformPropertyValue(transform));
                        }
                        result = transform_list.ToArray();
                    }
                    else if (typeof(Texture2D[]).IsAssignableFrom(ftype))
                    {
                        var textures = (Texture2D[])fvalue;
                        var texture_list = new List<object>();
                        foreach (var texture in textures)
                        {
                            texture_list.Add(GetTexturePropertyValue(texture));
                        }
                        result = texture_list.ToArray();
                    }
                    else if (typeof(Cubemap[]).IsAssignableFrom(ftype))
                    {
                        var cubemaps = (Cubemap[])fvalue;
                        var cubemap_list = new List<object>();
                        foreach (var cubemap in cubemaps)
                        {
                            cubemap_list.Add(GetCubemapPropertyValue(cubemap));
                        }
                        result = cubemap_list.ToArray();
                    }
                    else if (typeof(Material[]).IsAssignableFrom(ftype))
                    {
                        var materials = (Material[])fvalue;
                        var material_list = new List<object>();
                        foreach (var material in materials)
                        {
                            material_list.Add(GetMaterialPropertyValue(material));
                        }
                        result = material_list.ToArray();
                    }
                    else if (typeof(GameObject[]).IsAssignableFrom(ftype))
                    {
                        var gobjects = (GameObject[])fvalue;
                        var gobject_list = new List<object>();
                        foreach (var gobject in gobjects)
                        {
                            gobject_list.Add(GetGamePropertyValue(gobject));
                        }
                        result = gobject_list.ToArray();
                    }
                    else if (typeof(Camera[]).IsAssignableFrom(ftype))
                    {
                        var acameras = (Camera[])fvalue;
                        var acamera_list = new List<object>();
                        foreach (var acamera in acameras)
                        {
                            acamera_list.Add(GetCameraPropertyValue(acamera));
                        }
                        result = acamera_list.ToArray();
                    }
                    else if (typeof(AudioClip[]).IsAssignableFrom(ftype))
                    {
                        var aclips = (AudioClip[])fvalue;
                        var aclip_list = new List<object>();
                        foreach (var aclip in aclips)
                        {
                            aclip_list.Add(GetAudioClipPropertyValue(aclip));
                        }
                        result = aclip_list.ToArray();
                    }
                    else if (typeof(AnimationCurve[]).IsAssignableFrom(ftype))
                    {
                        var acurves = (AnimationCurve[])fvalue;
                        var acurve_list = new List<object>();
                        foreach (var acurve in acurves)
                        {
                            acurve_list.Add(GetCurvePropertyValue(acurve));
                        }
                        result = acurve_list.ToArray();
                    }
                    else if (typeof(EmbeddedAsset[]).IsAssignableFrom(ftype))
                    {
                        var eassets = (EmbeddedAsset[])fvalue;
                        var easset_list = new List<object>();
                        foreach (var easset in eassets)
                        {
                            easset_list.Add(GetEmbeddedAssetPropertyValue(easset));
                        }
                        result = easset_list.ToArray();
                    }
                    else if (typeof(TextAsset[]).IsAssignableFrom(ftype))
                    {
                        var tassets = (TextAsset[])fvalue;
                        var tasset_list = new List<object>();
                        foreach (var tasset in tassets)
                        {
                            tasset_list.Add(GetTextAssetPropertyValue(tasset));
                        }
                        result = tasset_list.ToArray();
                    }
                    else if (typeof(DefaultAsset[]).IsAssignableFrom(ftype))
                    {
                        var dassets = (DefaultAsset[])fvalue;
                        var dasset_list = new List<object>();
                        foreach (var dasset in dassets)
                        {
                            dasset_list.Add(GetDefaultAssetPropertyValue(dasset));
                        }
                        result = dasset_list.ToArray();
                    }
                    else if (typeof(Object[]).IsAssignableFrom(ftype))
                    {
                        var oassets = (Object[])fvalue;
                        var oasset_list = new List<object>();
                        foreach (var oasset in oassets)
                        {
                            if (oasset.GetType().IsSerializable) {
                                oasset_list.Add(oasset);
                            }
                        }
                        result = oasset_list.ToArray();
                    }
                    else if (ftype.IsSerializable)
                    {
                        result = fvalue;
                    }
                }
                else if (ftype.IsSerializable)
                {
                    result = fvalue;
                }
            }
            return result;
        }

        private object GetGamePropertyValue(GameObject game)
        {
            if (game == null) return null;
            Dictionary<string, object> objectInfo = new Dictionary<string, object>();
            objectInfo.Add("type", game.GetType().FullName);
            objectInfo.Add("id", GetID(game));
            objectInfo.Add("tag", game.tag);
            objectInfo.Add("name", game.name);
            objectInfo.Add("layer", game.layer);
            objectInfo.Add("isStatic", game.isStatic);
            objectInfo.Add("hideFlags", game.hideFlags.ToString());
            return objectInfo;
        }

        private object GetCameraPropertyValue(Camera camera)
        {
            if (camera == null) return null;
            Dictionary<string, object> objectInfo = new Dictionary<string, object>();
            objectInfo.Add("type", camera.GetType().FullName);
            objectInfo.Add("id", GetID(camera.gameObject));
            objectInfo.Add("tag", camera.gameObject.tag);
            objectInfo.Add("name", camera.gameObject.name);
            objectInfo.Add("layer", camera.gameObject.layer);
            objectInfo.Add("isStatic", camera.gameObject.isStatic);
            objectInfo.Add("hideFlags", camera.gameObject.hideFlags.ToString());
            return objectInfo;
        }

        private object GetTransformPropertyValue(Transform transform)
        {
            return Tools.GetTransformPropertyValue(transform);
        }

        private object GetMaterialPropertyValue(Material material)
        {
            if (material == null) return null;
            BabylonMaterial babylonMaterial = DumpMaterial(material);
            Dictionary<string, object> materialInfo = new Dictionary<string, object>();
            materialInfo.Add("type", material.GetType().FullName);
            materialInfo.Add("id", babylonMaterial.id);
            materialInfo.Add("name", babylonMaterial.name);
            materialInfo.Add("alpha", babylonMaterial.alpha);
            materialInfo.Add("wireframe", babylonMaterial.wireframe);
            materialInfo.Add("backFaceCulling", babylonMaterial.backFaceCulling);
            return materialInfo;
        }

        private object GetTexturePropertyValue(Texture2D texture)
        {
            if (texture == null) return null;
            var texturePath = AssetDatabase.GetAssetPath(texture);
            if (String.IsNullOrEmpty(texturePath)) return null;

            var babylonTexture = new BabylonTexture();
            CopyTexture(texturePath, texture, babylonTexture);
            Dictionary<string, object> textureInfo = new Dictionary<string, object>();
            textureInfo.Add("type", texture.GetType().FullName);
            textureInfo.Add("name", babylonTexture.name);
            textureInfo.Add("level", babylonTexture.level);
            textureInfo.Add("wrapU", babylonTexture.wrapU);
            textureInfo.Add("wrapV", babylonTexture.wrapV);
            textureInfo.Add("isCube", babylonTexture.isCube);
            textureInfo.Add("hasAlpha", babylonTexture.hasAlpha);
            textureInfo.Add("coordinatesMode", babylonTexture.coordinatesMode);
            textureInfo.Add("coordinatesIndex", babylonTexture.coordinatesIndex);
            return textureInfo;
        }

        private object GetCubemapPropertyValue(Cubemap cubemap)
        {
            if (cubemap == null) return null;
            var texturePath = AssetDatabase.GetAssetPath(cubemap);
            if (String.IsNullOrEmpty(texturePath)) return null;

            var textureName = Path.GetFileName(texturePath);
            var outputPath = Path.Combine(babylonScene.OutputPath, textureName);
            File.Copy(texturePath, outputPath, true);
            Dictionary<string, object> textureInfo = new Dictionary<string, object>();
            textureInfo.Add("type", cubemap.GetType().FullName);
            textureInfo.Add("name", textureName);
            textureInfo.Add("width", cubemap.width);
            textureInfo.Add("height", cubemap.height);
            textureInfo.Add("anisoLevel", cubemap.anisoLevel);
            textureInfo.Add("texelSizeX", cubemap.texelSize.x);
            textureInfo.Add("texelSizeY", cubemap.texelSize.y);
            textureInfo.Add("dimension", cubemap.dimension.ToString());
            textureInfo.Add("filterMode", cubemap.filterMode.ToString());
            textureInfo.Add("format", cubemap.format.ToString());
            textureInfo.Add("hideFlags", cubemap.hideFlags.ToString());
            textureInfo.Add("mipMapBias", cubemap.mipMapBias.ToString());
            textureInfo.Add("mipmapCount", cubemap.mipmapCount.ToString());
            textureInfo.Add("wrapMode", cubemap.wrapMode.ToString());
            return textureInfo;
        }

        private object GetEmbeddedAssetPropertyValue(EmbeddedAsset embedded)
        {
            if (embedded == null) return null;
            var assetPath = AssetDatabase.GetAssetPath(embedded.textAsset);
            if (String.IsNullOrEmpty(assetPath)) return null;

            var assetName = Path.GetFileName(assetPath);
            Dictionary<string, object> assetInfo = new Dictionary<string, object>();
            assetInfo.Add("type", embedded.GetType().FullName);
            assetInfo.Add("filename", assetName);
            assetInfo.Add("embedded", true);
            if (embedded.encoding == BabylonTextEncoding.RawBytes)
            {
                assetInfo.Add("base64", Convert.ToBase64String(embedded.textAsset.bytes));
            }
            else
            {
                assetInfo.Add("base64", Tools.FormatBase64(embedded.textAsset.text));
            }
            return assetInfo;
        }

        private object GetTextAssetPropertyValue(TextAsset asset)
        {
            if (asset == null) return null;
            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (String.IsNullOrEmpty(assetPath)) return null;

            var assetName = Path.GetFileName(assetPath);
            var outputPath = Path.Combine(babylonScene.OutputPath, assetName);
            File.Copy(assetPath, outputPath, true);
            Dictionary<string, object> assetInfo = new Dictionary<string, object>();
            assetInfo.Add("type", asset.GetType().FullName);
            assetInfo.Add("filename", assetName);
            assetInfo.Add("embedded", false);
            assetInfo.Add("base64", null);
            return assetInfo;
        }

        private object GetAudioClipPropertyValue(AudioClip clip)
        {
            if (clip == null) return null;
            var assetPath = AssetDatabase.GetAssetPath(clip);
            if (String.IsNullOrEmpty(assetPath)) return null;

            var assetName = Path.GetFileName(assetPath);
            var outputPath = Path.Combine(babylonScene.OutputPath, assetName);
            File.Copy(assetPath, outputPath, true);
            Dictionary<string, object> assetInfo = new Dictionary<string, object>();
            assetInfo.Add("type", clip.GetType().FullName);
            assetInfo.Add("filename", assetName);
            assetInfo.Add("length", clip.length);
            assetInfo.Add("channels", clip.channels);
            assetInfo.Add("frequency", clip.frequency);
            assetInfo.Add("samples", clip.samples);
            return assetInfo;
        }

        private object GetCurvePropertyValue(AnimationCurve curve)
        {
            if (curve == null || curve.length <= 0) return null;
            Dictionary<string, object> objectInfo = new Dictionary<string, object>();
            List<BabylonCurveKeyframe> keyframes = new List<BabylonCurveKeyframe>();
            foreach (var key in curve.keys) {
                keyframes.Add(new BabylonCurveKeyframe {
                    time = key.time,
                    value = key.value,
                    tangentMode = key.tangentMode,
                    inTangent = key.inTangent,
                    outTangent = key.outTangent
                });                    
            }
            objectInfo.Add("length", curve.length);
            objectInfo.Add("preWrapMode", curve.preWrapMode.ToString());
            objectInfo.Add("postWrapMode", curve.postWrapMode.ToString());
            objectInfo.Add("keyframes", keyframes.ToArray());
            return objectInfo;
        }

        private object GetDefaultAssetPropertyValue(DefaultAsset asset)
        {
            if (asset == null) return null;
            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (String.IsNullOrEmpty(assetPath)) return null;

            var assetName = Path.GetFileName(assetPath);
            var outputPath = Path.Combine(babylonScene.OutputPath, assetName);
            File.Copy(assetPath, outputPath, true);
            Dictionary<string, object> assetInfo = new Dictionary<string, object>();
            assetInfo.Add("type", asset.GetType().FullName);
            assetInfo.Add("filename", assetName);
            return assetInfo;
        }

        private static void ExportSkeletonAnimation(SkinnedMeshRenderer skinnedMesh, BabylonMesh babylonMesh, BabylonSkeleton skeleton, ref UnityMetaData metaData)
        {
            ExporterWindow.ReportProgress(1, "Exporting Animations: " + skinnedMesh.name);
            Animator animator = skinnedMesh.rootBone.gameObject.GetComponent<Animator>();
            Animation legacy = skinnedMesh.rootBone.gameObject.GetComponent<Animation>();
            if (legacy != null) UnityEngine.Debug.LogWarning("Legacy animation component not supported for skinned mesh: " + skinnedMesh.rootBone.gameObject.name);
            if (animator != null && animator.runtimeAnimatorController != null && animator.runtimeAnimatorController.animationClips != null && animator.runtimeAnimatorController.animationClips.Length > 0) {
                UnityEditor.AnimationState astate = skinnedMesh.rootBone.gameObject.GetComponent<UnityEditor.AnimationState>();
                if (astate == null) UnityEngine.Debug.LogWarning("AnimationState component not found for skinned mesh: " + skinnedMesh.rootBone.gameObject.name);
                if (astate != null && astate.isActiveAndEnabled == true && astate.controlType == BabylonAnimationMode.Skeleton) {
                    if (animator != null) animator.enabled = true;
                    ExportSkeletonAnimationClips(animator, skeleton, skinnedMesh, babylonMesh, astate, ref metaData);
                }
            }
            else
            {
                var parent = skinnedMesh.rootBone.parent;
                while (parent != null)
                {
                    animator = parent.gameObject.GetComponent<Animator>();
                    if (animator != null && animator.runtimeAnimatorController != null && animator.runtimeAnimatorController.animationClips != null && animator.runtimeAnimatorController.animationClips.Length > 0) {
                        UnityEditor.AnimationState astate = parent.gameObject.GetComponent<UnityEditor.AnimationState>();
                        if (astate == null) UnityEngine.Debug.LogWarning("AnimationState component not found for skinned mesh: " + parent.gameObject.name);
                        if (astate != null && astate.isActiveAndEnabled == true && astate.controlType == BabylonAnimationMode.Skeleton) {
                            if (animator != null) animator.enabled = true;
                            ExportSkeletonAnimationClips(animator, skeleton, skinnedMesh, babylonMesh, astate, ref metaData);
                            break;
                        }
                    }
                    parent = parent.parent;
                }
            }
        }
    }
}
