using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using BabylonExport.Entities;
using UnityEditor;
using UnityEngine;

namespace Unity3D2Babylon
{
    partial class SceneBuilder
    {
        private BabylonMesh ConvertUnityEmptyObjectToBabylon(GameObject gameObject, ref UnityMetaData metaData, ref List<UnityFlareSystem> lensFlares, ref string componentTags, BabylonMesh collisionMesh = null, Collider collider = null)
        {
            BabylonMesh babylonMesh = new BabylonMesh { name = gameObject.name, id = GetID(gameObject) };
            babylonMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
            metaData.type = "Game";
            if (!String.IsNullOrEmpty(componentTags))
            {
                babylonMesh.tags = componentTags;
            }

            var transform = gameObject.transform;

            babylonMesh.parentId = GetParentID(transform);

            babylonMesh.position = transform.localPosition.ToFloat();

            babylonMesh.rotation = new float[3];
            babylonMesh.rotation[0] = transform.localRotation.eulerAngles.x * (float)Math.PI / 180;
            babylonMesh.rotation[1] = transform.localRotation.eulerAngles.y * (float)Math.PI / 180;
            babylonMesh.rotation[2] = transform.localRotation.eulerAngles.z * (float)Math.PI / 180;

            babylonMesh.scaling = transform.localScale.ToFloat();

            babylonMesh.isVisible = false;
            babylonMesh.visibility = 0;
            babylonMesh.checkCollisions = false;
            if (metaData.layerIndex == ExporterWindow.PrefabIndex) {
                metaData.prefab = true;
                babylonMesh.tags += " [PREFAB]";
                babylonMesh.isEnabled = false;
                babylonMesh.name = "Prefab." + babylonMesh.name;
                SceneBuilder.Metadata.properties["hasPrefabMeshes"] = true;                
            }

            // Collision mesh
            if (collider != null && collisionMesh != null)
            {
                babylonMesh.isVisible = exportationOptions.ShowDebugColliders;
                babylonMesh.visibility = exportationOptions.ColliderVisibility;
                babylonMesh.checkCollisions = (exportationOptions.ExportCollisions && collider.isTrigger == false);

                // Append collision mesh offsets
                if (collisionMesh.position[0] != 0.0f) {
                    babylonMesh.position[0] += collisionMesh.position[0];
                }
                if (collisionMesh.position[1] != 0.0f) {
                    babylonMesh.position[1] += collisionMesh.position[1];
                }
                if (collisionMesh.position[2] != 0.0f) {
                    babylonMesh.position[2] += collisionMesh.position[2];
                }

                // Update collision mesh geometry
                babylonMesh.positions = collisionMesh.positions;
                babylonMesh.indices = collisionMesh.indices;
                babylonMesh.normals = collisionMesh.normals;
                babylonMesh.uvs = collisionMesh.uvs;
                babylonMesh.uvs2 = collisionMesh.uvs2;
            }

            // Babylon physics state
            if (exportationOptions.ExportPhysics)
            {
                var physics = gameObject.GetComponent<PhysicsState>();
                if (physics != null && physics.isActiveAndEnabled == true)
                {
                    babylonMesh.tags += " [PHYSICS]";
                    metaData.properties.Add("physicsTag", gameObject.tag);
                    metaData.properties.Add("physicsMass", physics.mass);
                    metaData.properties.Add("physicsFriction", physics.friction);
                    metaData.properties.Add("physicsRestitution", physics.restitution);
                    metaData.properties.Add("physicsImpostor", (int)physics.imposter);
                    metaData.properties.Add("physicsRotation", (int)physics.rotation);
                    metaData.properties.Add("physicsCollisions", (physics.type == BabylonCollisionType.Collider));
                    metaData.properties.Add("physicsEnginePlugin", exportationOptions.DefaultPhysicsEngine);
                    SceneBuilder.Metadata.properties["hasPhysicsMeshes"] = true;                
                }
            }

            // Animations
            ExportTransformAnimationClips(transform, babylonMesh, ref metaData);
            if (IsRotationQuaternionAnimated(babylonMesh))
            {
                babylonMesh.rotationQuaternion = transform.localRotation.ToFloat();
            }

            // Tagging
            if (!String.IsNullOrEmpty(babylonMesh.tags))
            {
                babylonMesh.tags = babylonMesh.tags.Trim();
            }

            // Lens Flares
            ParseLensFlares(gameObject, babylonMesh.id, ref lensFlares);

            // Override Details
            var details = gameObject.GetComponent<MeshDetails>();
            if (details != null)
            {
                metaData.properties["defaultEllipsoid"] = details.meshEllipsoidProperties.defaultEllipsoid.ToFloat();
                metaData.properties["ellipsoidOffset"] = details.meshEllipsoidProperties.ellipsoidOffset.ToFloat();
                metaData.properties["freezeWorldMatrix"] = details.meshRuntimeProperties.freezeWorldMatrix;
                metaData.properties["convertToUnIndexed"] = details.meshRuntimeProperties.convertToUnIndexed;
                metaData.properties["convertToFlatShaded"] = details.meshRuntimeProperties.convertToFlatShaded;
                if (details.meshRuntimeProperties.detachFromParent == true) {
                    babylonMesh.parentId = null;
                }
                // Validate Not On Prefab Layer
                if (metaData.layerIndex != ExporterWindow.PrefabIndex) {
                    babylonMesh.isEnabled = details.enableMesh;
                    if (details.meshVisibilityProperties.overrideVisibility == true) {
                        babylonMesh.isVisible = details.meshVisibilityProperties.makeMeshVisible;
                        babylonMesh.visibility = details.meshVisibilityProperties.newVisibilityLevel;
                    }
                }
                // Convert To Prefab Mesh Instance
                if (metaData.layerIndex == ExporterWindow.PrefabIndex && details.meshPrefabProperties.makePrefabInstance == true) {
                    string parentId = babylonMesh.parentId;
                    string sourceId = babylonMesh.id;
                    string sourceName = babylonMesh.name;
                    // TODO: Reparent To Instance Group
                    babylonMesh.parentId = (prefabInstances != null) ? prefabInstances.id : null;
                    // Create Instance Mesh Holder
                    BabylonMesh instanceMesh = new BabylonMesh { name = sourceName + "_Instance" };
                    instanceMesh.id = System.Guid.NewGuid().ToString();
                    instanceMesh.tags += " [PREFAB]";
                    instanceMesh.parentId = parentId;
                    instanceMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
                    instanceMesh.position = Vector3.zero.ToFloat();
                    instanceMesh.rotation = Vector3.zero.ToFloat();
                    instanceMesh.scaling = new Vector3(1.0f, 1.0f, 1.0f).ToFloat();
                    instanceMesh.isEnabled = false;
                    instanceMesh.isVisible = true;
                    instanceMesh.visibility = 1;
                    instanceMesh.checkCollisions = false;
                    // Attach Prefab Instance Information
                    UnityMetaData instanceData = new UnityMetaData();
                    instanceData.api = true;
                    instanceData.prefab = true;
                    instanceData.tagName = "[INSTANCE]";
                    instanceData.objectId = instanceMesh.id;
                    instanceData.objectName = instanceMesh.name;
                    instanceData.layerIndex = ExporterWindow.PrefabIndex;                
                    instanceData.properties["prefabSource"] = sourceId;
                    instanceData.properties["prefabOffset"] = details.meshPrefabProperties.offsetPrefabPosition;
                    instanceData.properties["prefabPosition"] = details.meshPrefabProperties.prefabOffsetPosition.ToFloat();
                    SceneBuilder.Metadata.properties["hasPrefabMeshes"] = true;                
                    SceneBuilder.Metadata.properties["hasInstanceMeshes"] = true;                
                    // Add Instance Mesh To Scene Entities
                    instanceMesh.metadata = instanceData;
                    babylonScene.MeshesList.Add(instanceMesh);
                }
                // Last Chance Force Check Collision Override
                if (details.forceCheckCollisions == true) {
                    babylonMesh.checkCollisions = true;
                    var tags = gameObject.GetComponent<TagsComponent>();
                    if (tags != null) {
                        if (tags.babylonTags.IndexOf("[COLLIDER]") >= 0) {
                            babylonMesh.isVisible = exportationOptions.ShowDebugColliders;
                            babylonMesh.visibility = exportationOptions.ColliderVisibility;
                        }
                    }
                }
            }

            babylonMesh.metadata = metaData;
            babylonScene.MeshesList.Add(babylonMesh);

            if (!exportationOptions.ExportMetadata) babylonMesh.metadata = null;
            return babylonMesh;
        }
        
        private BabylonMesh ConvertUnityMeshToBabylon(Mesh mesh, Transform transform, GameObject gameObject, float progress, ref UnityMetaData metaData, ref List<UnityFlareSystem> lensFlares, ref string componentTags, BabylonMesh collisionMesh = null, Collider collider = null)
        {
            BabylonMesh babylonMesh = new BabylonMesh();
            babylonMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
            metaData.type = "Mesh";
            if (!String.IsNullOrEmpty(componentTags))
            {
                babylonMesh.tags = componentTags;
            }

            ExporterWindow.ReportProgress(progress, "Exporting mesh: " + gameObject.name);

            babylonMesh.name = gameObject.name;
            babylonMesh.id = GetID(transform.gameObject);

            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                babylonMesh.receiveShadows = renderer.receiveShadows;
            }

            babylonMesh.parentId = GetParentID(transform);

            babylonMesh.position = transform.localPosition.ToFloat();

            babylonMesh.rotation = new float[3];
            babylonMesh.rotation[0] = transform.localRotation.eulerAngles.x * (float)Math.PI / 180;
            babylonMesh.rotation[1] = transform.localRotation.eulerAngles.y * (float)Math.PI / 180;
            babylonMesh.rotation[2] = transform.localRotation.eulerAngles.z * (float)Math.PI / 180;

            babylonMesh.scaling = transform.localScale.ToFloat();
            
            babylonMesh.isVisible = true;
            babylonMesh.visibility = 1;
            babylonMesh.checkCollisions = false;
            if (metaData.layerIndex == ExporterWindow.PrefabIndex) {
                metaData.prefab = true;
                babylonMesh.tags += " [PREFAB]";
                babylonMesh.isEnabled = false;
                babylonMesh.name = "Prefab." + babylonMesh.name;
                SceneBuilder.Metadata.properties["hasPrefabMeshes"] = true;                
            }

            // Collision detail meshes
            if (collider != null)
            {
                if (collisionMesh != null)
                {
                    collisionMesh.parentId = babylonMesh.id;
                    collisionMesh.isVisible = exportationOptions.ShowDebugColliders;
                    collisionMesh.visibility = exportationOptions.ColliderVisibility;
                    collisionMesh.checkCollisions = (exportationOptions.ExportCollisions && collider.isTrigger == false);
                    babylonScene.MeshesList.Add(collisionMesh);
                }
                else
                {
                    babylonMesh.checkCollisions = exportationOptions.ExportCollisions;
                }
            }

            // Babylon physics state (With trigger colliders)
            if (exportationOptions.ExportPhysics)
            {
                var physics = gameObject.GetComponent<PhysicsState>();
                if (physics != null && physics.isActiveAndEnabled == true)
                {
                    babylonMesh.tags += " [PHYSICS]";
                    metaData.properties.Add("physicsTag", gameObject.tag);
                    metaData.properties.Add("physicsMass", physics.mass);
                    metaData.properties.Add("physicsFriction", physics.friction);
                    metaData.properties.Add("physicsRestitution", physics.restitution);
                    metaData.properties.Add("physicsImpostor", (int)physics.imposter);
                    metaData.properties.Add("physicsRotation", (int)physics.rotation);
                    metaData.properties.Add("physicsCollisions", (physics.type == BabylonCollisionType.Collider));
                    metaData.properties.Add("physicsEnginePlugin", exportationOptions.DefaultPhysicsEngine);
                    SceneBuilder.Metadata.properties["hasPhysicsMeshes"] = true;                
                }
            }

            if (mesh != null)
            {
                Tools.GenerateBabylonMeshData(mesh, babylonMesh, babylonScene, transform);
                int index = 0;
                if (mesh.boneWeights.Length == mesh.vertexCount)
                {
                    babylonMesh.matricesIndices = new int[mesh.vertexCount];
                    babylonMesh.matricesWeights = new float[mesh.vertexCount * 4];
                    index = 0;
                    foreach (BoneWeight bw in mesh.boneWeights)
                    {
                        babylonMesh.matricesIndices[index] = (bw.boneIndex3 << 24) | (bw.boneIndex2 << 16) | (bw.boneIndex1 << 8) | bw.boneIndex0;
                        babylonMesh.matricesWeights[index * 4 + 0] = bw.weight0;
                        babylonMesh.matricesWeights[index * 4 + 1] = bw.weight1;
                        babylonMesh.matricesWeights[index * 4 + 2] = bw.weight2;
                        babylonMesh.matricesWeights[index * 4 + 3] = bw.weight3;
                        var totalWeight = bw.weight0 + bw.weight1 + bw.weight2 + bw.weight3;
                        if (Mathf.Abs(totalWeight - 1.0f) > 0.01f)
                        {
                            throw new Exception("Total bone weights is not normalized for: " + mesh);
                        }
                        index++;
                    }
                }
                index = 0;
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    // Validate Multi Materials
                    if (mesh.subMeshCount > 1)
                    {
                        BabylonMultiMaterial bMultiMat;

                        string multiMatName = "";
                        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                        {
                            multiMatName += renderer.sharedMaterials[i].name;
                        }

                        if (!multiMatDictionary.ContainsKey(multiMatName))
                        {
                            bMultiMat = new BabylonMultiMaterial
                            {
                                materials = new string[mesh.subMeshCount],
                                id = Guid.NewGuid().ToString(),
                                name = multiMatName
                            };

                            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                            {
                                var sharedMaterial = renderer.sharedMaterials[i];
                                BabylonMaterial babylonMaterial;

                                //UnityEngine.Debug.LogWarning("===> DUMP SHARED RENDERER: " + renderer.name);
                                babylonMaterial = DumpMaterial(sharedMaterial, renderer.lightmapIndex, renderer.lightmapScaleOffset);

                                bMultiMat.materials[i] = babylonMaterial.id;
                            }
                            if (mesh.subMeshCount > 1)
                            {
                                multiMatDictionary.Add(bMultiMat.name, bMultiMat);
                            }
                        }
                        else
                        {
                            bMultiMat = multiMatDictionary[multiMatName];
                        }

                        babylonMesh.materialId = bMultiMat.id;
                        babylonMesh.subMeshes = new BabylonSubMesh[mesh.subMeshCount];

                        var offset = 0;
                        for (int materialIndex = 0; materialIndex < mesh.subMeshCount; materialIndex++)
                        {
                            var unityTriangles = mesh.GetTriangles(materialIndex);
                            babylonMesh.subMeshes[materialIndex] = new BabylonSubMesh
                            {
                                verticesStart = 0,
                                verticesCount = mesh.vertexCount,
                                materialIndex = materialIndex,
                                indexStart = offset,
                                indexCount = unityTriangles.Length
                            };
                            offset += unityTriangles.Length;
                        }
                    }
                    else
                    {
                        //UnityEngine.Debug.LogWarning("===> DUMP RAW RENDERER: " + renderer.name);
                        babylonMesh.materialId = DumpMaterial(renderer.sharedMaterial, renderer.lightmapIndex, renderer.lightmapScaleOffset).id;
                    }
                }

                // Animations
                ExportTransformAnimationClips(transform, babylonMesh, ref metaData);
                if (IsRotationQuaternionAnimated(babylonMesh))
                {
                    babylonMesh.rotationQuaternion = transform.localRotation.ToFloat();
                }

                // Tagging
                if (!String.IsNullOrEmpty(babylonMesh.tags))
                {
                    babylonMesh.tags = babylonMesh.tags.Trim();
                }

                // Lens Flares
                ParseLensFlares(gameObject, babylonMesh.id, ref lensFlares);

                // Mesh Instances
                var tree = gameObject.GetComponent<Tree>();
                if (tree != null && metaData.layerIndex == ExporterWindow.PrefabIndex)
                {
                    string parentId = babylonMesh.parentId;
                    string sourceId = babylonMesh.id;
                    string sourceName = babylonMesh.name;
                    // TODO: Reparent To Instance Group
                    babylonMesh.parentId = (prefabInstances != null) ? prefabInstances.id : null;
                    // Create Instance Mesh Holder
                    BabylonMesh treeMesh = new BabylonMesh { name = sourceName + "_Instance" };
                    treeMesh.id = System.Guid.NewGuid().ToString();
                    treeMesh.tags += " [PREFAB]";
                    treeMesh.parentId = parentId;
                    treeMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
                    treeMesh.position = Vector3.zero.ToFloat();
                    treeMesh.rotation = Vector3.zero.ToFloat();
                    treeMesh.scaling = new Vector3(1.0f, 1.0f, 1.0f).ToFloat();
                    treeMesh.isEnabled = false;
                    treeMesh.isVisible = true;
                    treeMesh.visibility = 1;
                    treeMesh.checkCollisions = false;
                    // Attach Prefab Instance Information
                    UnityMetaData treeData = new UnityMetaData();
                    treeData.api = true;
                    treeData.prefab = true;
                    treeData.tagName = "[INSTANCE]";
                    treeData.objectId = treeMesh.id;
                    treeData.objectName = treeMesh.name;
                    treeData.layerIndex = ExporterWindow.PrefabIndex;                
                    treeData.properties["prefabSource"] = sourceId;
                    treeData.properties["prefabOffset"] = false;
                    treeData.properties["prefabPosition"] = new float[] { 0.0f, 0.0f, 0.0f };
                    SceneBuilder.Metadata.properties["hasPrefabMeshes"] = true;                
                    SceneBuilder.Metadata.properties["hasInstanceMeshes"] = true;                
                    // Add Instance Mesh To Scene Entities
                    treeMesh.metadata = treeData;
                    babylonScene.MeshesList.Add(treeMesh);
                }
                else
                {
                    // Override Details
                    var details = gameObject.GetComponent<MeshDetails>();
                    if (details != null)
                    {
                        metaData.properties["defaultEllipsoid"] = details.meshEllipsoidProperties.defaultEllipsoid.ToFloat();
                        metaData.properties["ellipsoidOffset"] = details.meshEllipsoidProperties.ellipsoidOffset.ToFloat();
                        metaData.properties["freezeWorldMatrix"] = details.meshRuntimeProperties.freezeWorldMatrix;
                        metaData.properties["convertToUnIndexed"] = details.meshRuntimeProperties.convertToUnIndexed;
                        metaData.properties["convertToFlatShaded"] = details.meshRuntimeProperties.convertToFlatShaded;
                        if (details.meshRuntimeProperties.detachFromParent == true) {
                            babylonMesh.parentId = null;
                        }
                        // Validate Not On Prefab Layer
                        if (metaData.layerIndex != ExporterWindow.PrefabIndex) {
                            babylonMesh.isEnabled = details.enableMesh;
                            if (details.meshVisibilityProperties.overrideVisibility == true) {
                                babylonMesh.isVisible = details.meshVisibilityProperties.makeMeshVisible;
                                babylonMesh.visibility = details.meshVisibilityProperties.newVisibilityLevel;
                            }
                        }
                        // Convert To Prefab Mesh Instance
                        if (metaData.layerIndex == ExporterWindow.PrefabIndex && details.meshPrefabProperties.makePrefabInstance == true) {
                            string parentId = babylonMesh.parentId;
                            string sourceId = babylonMesh.id;
                            string sourceName = babylonMesh.name;
                            // TODO: Reparent To Instance Group
                            babylonMesh.parentId = (prefabInstances != null) ? prefabInstances.id : null;
                            // Create Instance Mesh Holder
                            BabylonMesh instanceMesh = new BabylonMesh { name = sourceName + "_Instance" };
                            instanceMesh.id = System.Guid.NewGuid().ToString();
                            instanceMesh.tags += " [PREFAB]";
                            instanceMesh.parentId = parentId;
                            instanceMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
                            instanceMesh.position = Vector3.zero.ToFloat();
                            instanceMesh.rotation = Vector3.zero.ToFloat();
                            instanceMesh.scaling = new Vector3(1.0f, 1.0f, 1.0f).ToFloat();
                            instanceMesh.isEnabled = false;
                            instanceMesh.isVisible = true;
                            instanceMesh.visibility = 1;
                            instanceMesh.checkCollisions = false;
                            // Attach Prefab Instance Information
                            UnityMetaData instanceData = new UnityMetaData();
                            instanceData.api = true;
                            instanceData.prefab = true;
                            instanceData.tagName = "[INSTANCE]";
                            instanceData.objectId = instanceMesh.id;
                            instanceData.objectName = instanceMesh.name;
                            instanceData.layerIndex = ExporterWindow.PrefabIndex;                
                            instanceData.properties["prefabSource"] = sourceId;
                            instanceData.properties["prefabOffset"] = details.meshPrefabProperties.offsetPrefabPosition;
                            instanceData.properties["prefabPosition"] = details.meshPrefabProperties.prefabOffsetPosition.ToFloat();
                            SceneBuilder.Metadata.properties["hasPrefabMeshes"] = true;                
                            SceneBuilder.Metadata.properties["hasInstanceMeshes"] = true;                
                            // Add Instance Mesh To Scene Entities
                            instanceMesh.metadata = instanceData;
                            babylonScene.MeshesList.Add(instanceMesh);
                        }
                        // Last Chance Force Check Collision Override
                        if (details.forceCheckCollisions == true) {
                            babylonMesh.checkCollisions = true;
                            var tags = gameObject.GetComponent<TagsComponent>();
                            if (tags != null) {
                                if (tags.babylonTags.IndexOf("[COLLIDER]") >= 0) {
                                    babylonMesh.isVisible = exportationOptions.ShowDebugColliders;
                                    babylonMesh.visibility = exportationOptions.ColliderVisibility;
                                }
                            }
                        }
                    }
                }

                babylonMesh.metadata = metaData;
                babylonScene.MeshesList.Add(babylonMesh);
            }

            if (!exportationOptions.ExportMetadata) babylonMesh.metadata = null;
            return babylonMesh;
        }

        private BabylonSkeleton ConvertUnitySkeletonToBabylon(SkinnedMeshRenderer skin, GameObject gameObject, float progress, ref UnityMetaData metaData)
        {
            ExporterWindow.ReportProgress(progress, "Exporting Skeleton: " + gameObject.name);

            Transform root = skin.rootBone;
            Transform[] bones = skin.bones;
            Matrix4x4[] bindPoses = skin.sharedMesh.bindposes;
            Transform transform = skin.transform;

            BabylonSkeleton babylonSkeleton = new BabylonSkeleton();
            babylonSkeleton.name = gameObject.name;
            babylonSkeleton.id = Math.Abs(GetID(transform.gameObject).GetHashCode());
            babylonSkeleton.needInitialSkinMatrix = false;
            
            // Prefilled to keep order and track parents.
            var transformToBoneMap = new Dictionary<Transform, BabylonBone>();
            for (var i = 0; i < bones.Length; i++)
            {
                Transform unityBone = bones[i];

                string exmsg = "Exporting bone: " + unityBone.name + " at index " + i.ToString();
                ExporterWindow.ReportProgress(progress, exmsg);

                var babylonBone = new BabylonBone();
                babylonBone.name = unityBone.name;
                babylonBone.index = i;
                transformToBoneMap[unityBone] = babylonBone;
            }
            
            // Attaches Matrix and parent.
            for (var i = 0; i < bones.Length; i++)
            {
                var unityBone = bones[i];
                var babylonBone = transformToBoneMap[unityBone];
                
                Matrix4x4 localTransform;
                
                // Unity BindPose is already inverse so take the inverse again :-)
                if (transformToBoneMap.ContainsKey(unityBone.parent))
                {
                    var babylonParentBone = transformToBoneMap[unityBone.parent];
                    babylonBone.parentBoneIndex = babylonParentBone.index;
                    localTransform = bindPoses[babylonBone.parentBoneIndex] * bindPoses[i].inverse;
                }
                else
                {
                    if (unityBone != root) {
                        UnityEngine.Debug.LogWarning("Parent bone transform '" + unityBone.parent.name + "' missing weights for child bone '" + unityBone.name);
                    }
                    babylonBone.parentBoneIndex = -1;
                    localTransform = bindPoses[i].inverse;
                }

                // Support socket meshes
                var gox = unityBone.gameObject;
                var socket = gox.GetComponent<SocketMesh>();
                if (socket != null && socket.createSocketMesh == true) {
                    float posX = socket.socketMeshPosition.x, posY = socket.socketMeshPosition.y, posZ = socket.socketMeshPosition.z;
                    float rotX = (socket.socketMeshRotation.x * (float)Math.PI / 180), rotY = (socket.socketMeshRotation.y * (float)Math.PI / 180), rotZ = (socket.socketMeshRotation.z * (float)Math.PI / 180);
                    string prefab_name = (socket.socketPrefabProperties.defaultSocketPrefab == true && !String.IsNullOrEmpty(socket.socketPrefabProperties.socketPrefabName)) ? socket.socketPrefabProperties.socketPrefabName : null;
                    float prefab_posX = socket.socketPrefabProperties.socketPrefabPosition.x, prefab_posY = socket.socketPrefabProperties.socketPrefabPosition.y, prefab_posZ = socket.socketPrefabProperties.socketPrefabPosition.z;
                    float prefab_rotX = (socket.socketPrefabProperties.socketPrefabRotation.x * (float)Math.PI / 180), prefab_rotY = (socket.socketPrefabProperties.socketPrefabRotation.y * (float)Math.PI / 180), prefab_rotZ = (socket.socketPrefabProperties.socketPrefabRotation.z * (float)Math.PI / 180);
                    metaData.socketList.Add(new SocketData { 
                        boneIndex = i,
                        boneName = unityBone.name, 
                        socketMesh = null, 
                        positionX = posX, 
                        positionY = posY, 
                        positionZ = posZ,
                        rotationX = rotX, 
                        rotationY = rotY, 
                        rotationZ = rotZ,
                        prefabName = prefab_name,
                        prefabPositionX = prefab_posX, 
                        prefabPositionY = prefab_posY, 
                        prefabPositionZ = prefab_posZ,
                        prefabRotationX = prefab_rotX, 
                        prefabRotationY = prefab_rotY, 
                        prefabRotationZ = prefab_rotZ
                    });
                }
                
                // Transform matrix bone maps
                transformToBoneMap[unityBone].matrix = new[] {
                    localTransform[0, 0], localTransform[1, 0], localTransform[2, 0], localTransform[3, 0],
                    localTransform[0, 1], localTransform[1, 1], localTransform[2, 1], localTransform[3, 1],
                    localTransform[0, 2], localTransform[1, 2], localTransform[2, 2], localTransform[3, 2],
                    localTransform[0, 3], localTransform[1, 3], localTransform[2, 3], localTransform[3, 3]
                };
            }

            // Reorder and attach the skeleton.
            babylonSkeleton.bones = transformToBoneMap.Values.OrderBy(b => b.index).ToArray();
            babylonScene.SkeletonsList.Add(babylonSkeleton);

            return babylonSkeleton;
        }

        private BabylonMesh ConvertUnityTerrainToBabylon(Terrain terrain, GameObject gameObject, float progress, ref UnityMetaData metaData, ref List<UnityFlareSystem> lensFlares, ref string componentTags)
        {
            ExporterWindow.ReportProgress(progress, "Exporting terrain: " + gameObject.name);
            var transform = gameObject.transform;
            float[] position = transform.localPosition.ToFloat();
            float[] rotation = new float[3];
            rotation[0] = transform.localRotation.eulerAngles.x * (float)Math.PI / 180;
            rotation[1] = transform.localRotation.eulerAngles.y * (float)Math.PI / 180;
            rotation[2] = transform.localRotation.eulerAngles.z * (float)Math.PI / 180;
            float[] scaling = transform.localScale.ToFloat();

            BabylonMesh babylonMesh = new BabylonMesh { name = gameObject.name, id = GetID(gameObject) };
            babylonMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
            metaData.type = "Terrain";
            if (!String.IsNullOrEmpty(componentTags))
            {
                babylonMesh.tags = componentTags;
            }
            babylonMesh.tags += " [TERRAIN]";
            if (!String.IsNullOrEmpty(babylonMesh.tags))
            {
                babylonMesh.tags = babylonMesh.tags.Trim();
            }
            babylonMesh.parentId = GetParentID(transform);
            babylonMesh.position = Vector3.zero.ToFloat();
            babylonMesh.rotation = rotation;
            babylonMesh.scaling = scaling;
            babylonMesh.isVisible = true;
            babylonMesh.visibility = 1;
            babylonMesh.checkCollisions = false;

            if (terrain != null)
            {
                // Terrain detail prototypes
                List<object> detailPrototypes = null;
                if (terrain.terrainData.detailPrototypes != null && terrain.terrainData.detailPrototypes.Length > 0) {
                    detailPrototypes = new List<object>();
                    foreach (var detailPrototype in terrain.terrainData.detailPrototypes) {
                        Dictionary<string, object> detailInfo = new Dictionary<string, object>();
                        detailInfo.Add("prefab", detailPrototype.prototype.name);
                        detailInfo.Add("bendFactor", detailPrototype.bendFactor);
                        detailInfo.Add("dryColor", detailPrototype.dryColor.ToFloat());
                        detailInfo.Add("healthyColor", detailPrototype.healthyColor.ToFloat());
                        detailInfo.Add("maxHeight", detailPrototype.maxHeight);
                        detailInfo.Add("maxWidth", detailPrototype.maxWidth);
                        detailInfo.Add("minHeight", detailPrototype.minHeight);
                        detailInfo.Add("minWidth", detailPrototype.minWidth);
                        detailInfo.Add("noiseSpread", detailPrototype.noiseSpread);
                        detailPrototypes.Add(detailInfo);
                    }
                }
                
                // Terrain tree instances
                List<object> treeInstances = null;
                int treeCount = terrain.terrainData.treeInstanceCount;
                if (treeCount > 0) {
                    treeInstances = new List<object>();
                    foreach (var treeInstance in terrain.terrainData.treeInstances) {
                        Vector3 prefabPosition = Vector3.Scale(treeInstance.position, terrain.terrainData.size) + terrain.gameObject.transform.position;
                        string prefabName = terrain.terrainData.treePrototypes[treeInstance.prototypeIndex].prefab.name;
                        Dictionary<string, object> treeInfo = new Dictionary<string, object>();
                        treeInfo.Add("prefab", prefabName);
                        treeInfo.Add("color", treeInstance.color.ToFloat());
                        treeInfo.Add("heightScale", treeInstance.heightScale);
                        treeInfo.Add("lightmapColor", treeInstance.lightmapColor.ToFloat());
                        treeInfo.Add("position", prefabPosition.ToFloat());
                        treeInfo.Add("rotation", treeInstance.rotation);
                        treeInfo.Add("widthScale", treeInstance.widthScale);
                        treeInstances.Add(treeInfo);
                    }
                }

                // Terrain metadata infomation
                Vector3 terrainSize = terrain.terrainData.size;
                metaData.properties.Add("width", terrainSize.x);
                metaData.properties.Add("length", terrainSize.z);
                metaData.properties.Add("height", terrainSize.y);
                metaData.properties.Add("position", position);
                metaData.properties.Add("rotation", rotation);
                metaData.properties.Add("scaling", scaling);
                metaData.properties.Add("thickness", terrain.terrainData.thickness);
                metaData.properties.Add("detailWidth", terrain.terrainData.detailWidth);
                metaData.properties.Add("detailHeight", terrain.terrainData.detailHeight);
                metaData.properties.Add("heightmapWidth", terrain.terrainData.heightmapWidth);
                metaData.properties.Add("heightmapHeight", terrain.terrainData.heightmapHeight);
                metaData.properties.Add("wavingGrassAmount", terrain.terrainData.wavingGrassAmount);
                metaData.properties.Add("wavingGrassSpeed", terrain.terrainData.wavingGrassSpeed);
                metaData.properties.Add("wavingGrassStrength", terrain.terrainData.wavingGrassStrength);
                metaData.properties.Add("wavingGrassTint", terrain.terrainData.wavingGrassTint.ToFloat());
                metaData.properties.Add("detailPrototypes", detailPrototypes);
                metaData.properties.Add("treeInstanceCount", terrain.terrainData.treeInstanceCount);
                metaData.properties.Add("treeInstances", treeInstances);

                // Detailed Terrain Geometry
                if (terrain.isActiveAndEnabled)
                {
                    TerrainData data = terrain.terrainData;
                    string tname = Tools.FirstUpper(terrain.name.Replace(" ", "_"));
                    ExporterWindow.ReportProgress(progress, "Exporting terrain geometry: " + gameObject.name);
                    terrain.terrainData.RefreshPrototypes();
                    BabylonTerrainData terrainMeshData = Unity3D2Babylon.Tools.CreateTerrainData(terrain.terrainData, transform.localPosition, true);
                    Tools.GenerateBabylonMeshTerrainData(terrainMeshData, babylonMesh, false, babylonScene, transform);
                    // Create Terrain Materials
                    BabylonUniversalMaterial terrainMaterial = CreateSplatmapMaterial(terrain, terrain.materialTemplate, terrain.lightmapIndex, terrain.lightmapScaleOffset, exportationOptions.TerrainCoordinatesIndex);
                    babylonMesh.materialId = terrainMaterial.id;
                    // Generate Terrain Splatmaps
                    ExporterWindow.ReportProgress(progress, "Generating terrain splatmap data for: " + gameObject.name);
                    float[,,] maps = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
                    int numSplats = maps.GetLength(2);
                    if (numSplats > 12) UnityEngine.Debug.LogWarning("Max texture splats encountered. Toolkit supports up to 12 texture splats.");
                    if (numSplats > 0) {
                        List<Texture2D> splatmaps = new List<Texture2D>();
                        Color32[] splatmap1 = new Color32[data.alphamapWidth * data.alphamapHeight];
                        Color32[] splatmap2 = new Color32[data.alphamapWidth * data.alphamapHeight];
                        Color32[] splatmap3 = new Color32[data.alphamapWidth * data.alphamapHeight];
                        Color32[] splatmap4 = new Color32[data.alphamapWidth * data.alphamapHeight];
                        for (int y = 0; y < data.alphamapHeight; ++y) {
                            for (int x = 0; x < data.alphamapWidth; ++x) {
                                int imageIndex = y * data.alphamapWidth + x;
                                // Splatmap Texture One
                                splatmap1[imageIndex].r = (byte)(maps[y, x, 0] * 255.0f);
                                splatmap1[imageIndex].g = (numSplats > 1) ? (byte)(maps[y, x, 1] * 255.0f) : (byte)0;
                                splatmap1[imageIndex].b = (numSplats > 2) ? (byte)(maps[y, x, 2] * 255.0f) : (byte)0;
                                splatmap1[imageIndex].a = (byte)255.0f;
                                // Splatmap Texture Two
                                splatmap2[imageIndex].r = (numSplats > 3) ? (byte)(maps[y, x, 3] * 255.0f) : (byte)0;
                                splatmap2[imageIndex].g = (numSplats > 4) ? (byte)(maps[y, x, 4] * 255.0f) : (byte)0;
                                splatmap2[imageIndex].b = (numSplats > 5) ? (byte)(maps[y, x, 5] * 255.0f) : (byte)0;
                                splatmap2[imageIndex].a = (byte)255.0f;
                                // Splatmap Texture Three
                                splatmap3[imageIndex].r = (numSplats > 6) ? (byte)(maps[y, x, 6] * 255.0f) : (byte)0;
                                splatmap3[imageIndex].g = (numSplats > 7) ? (byte)(maps[y, x, 7] * 255.0f) : (byte)0;
                                splatmap3[imageIndex].b = (numSplats > 8) ? (byte)(maps[y, x, 8] * 255.0f) : (byte)0;
                                splatmap3[imageIndex].a = (byte)255.0f;
                                // Splatmap Texture Four
                                splatmap4[imageIndex].r = (numSplats > 9) ? (byte)(maps[y, x, 9] * 255.0f) : (byte)0;
                                splatmap4[imageIndex].g = (numSplats > 10) ? (byte)(maps[y, x, 10] * 255.0f) : (byte)0;
                                splatmap4[imageIndex].b = (numSplats > 11) ? (byte)(maps[y, x, 11] * 255.0f) : (byte)0;
                                splatmap4[imageIndex].a = (byte)255.0f;
                            }
                        }
                        // Encode Packed Splatmap Data With Zero Rotation
                        float rotateAngle = 0.0f;
                        Texture2D splatTexture1 = new Texture2D(data.alphamapWidth, data.alphamapHeight, TextureFormat.RGBA32, false);
                        splatTexture1.SetPixels32(splatmap1);
                        splatTexture1.Apply();
                        splatmaps.Add(Tools.RotateTexture(splatTexture1, rotateAngle));
                        if (numSplats > 3) {
                            Texture2D splatTexture2 = new Texture2D(data.alphamapWidth, data.alphamapHeight, TextureFormat.RGBA32, false);
                            splatTexture2.SetPixels32(splatmap2);
                            splatTexture2.Apply();
                            splatmaps.Add(Tools.RotateTexture(splatTexture2, rotateAngle));
                            if (numSplats > 6) {
                                Texture2D splatTexture3 = new Texture2D(data.alphamapWidth, data.alphamapHeight, TextureFormat.RGBA32, false);
                                splatTexture3.SetPixels32(splatmap3);
                                splatTexture3.Apply();
                                splatmaps.Add(Tools.RotateTexture(splatTexture3, rotateAngle));
                                if (numSplats > 9) {
                                    Texture2D splatTexture4 = new Texture2D(data.alphamapWidth, data.alphamapHeight, TextureFormat.RGBA32, false);
                                    splatTexture4.SetPixels32(splatmap4);
                                    splatTexture4.Apply();
                                    splatmaps.Add(Tools.RotateTexture(splatTexture4, rotateAngle));
                                }
                            }
                        }
                        ExporterWindow.ReportProgress(progress, "Packing texture atlas splatmaps... This may take a while.");
                        int atlasCount = 0;
                        // Texture atlas rects
                        Vector4 splatmapRect1 = new Vector4(0, 0, 0, 0);
                        Vector4 splatmapRect2 = new Vector4(0, 0, 0, 0);
                        Vector4 splatmapRect3 = new Vector4(0, 0, 0, 0);
                        Vector4 splatmapRect4 = new Vector4(0, 0, 0, 0);
                        // Encode texture atlas
                        bool bilinearScaling = (exportationOptions.TerrainImageScaling == (int)BabylonTextureScale.Bilinear);
                        Texture2D splatmapAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                        Rect[] atlasPackingResult = Tools.PackTextureAtlas(splatmapAtlas, splatmaps.ToArray(), exportationOptions.TerrainAtlasSize, 0, bilinearScaling, false, true);
                        Texture2D splatmapBuffer = splatmapAtlas.Copy();
                        // Export texture atlas
                        string atlasExt = "png";
                        string atlasName = String.Format("{0}_{1}_Splatmap.{2}", SceneName, tname, atlasExt);
                        string atlasFile = Path.Combine(OutputPath, atlasName);
                        string atlasLabel = Path.GetFileName(atlasFile);
                        splatmapBuffer.WriteImage(atlasFile, BabylonImageFormat.PNG);
                        
                        terrainMaterial.textures.Add("splatmap", new BabylonTexture { name = atlasLabel, samplingMode = BabylonTexture.SamplingMode.NEAREST_NEAREST_MIPLINEAR });
                        if (atlasPackingResult != null && atlasPackingResult.Length > 0) {
                            atlasCount = atlasPackingResult.Length;
                            // Texture atlas rects
                            if (atlasCount >= 1) {
                                splatmapRect1.x = atlasPackingResult[0].x;
                                splatmapRect1.y = atlasPackingResult[0].y;
                                splatmapRect1.z = atlasPackingResult[0].height;
                                splatmapRect1.w = atlasPackingResult[0].width;
                            }
                            if (atlasCount >= 2) {
                                splatmapRect2.x = atlasPackingResult[1].x;
                                splatmapRect2.y = atlasPackingResult[1].y;
                                splatmapRect2.z = atlasPackingResult[1].height;
                                splatmapRect2.w = atlasPackingResult[1].width;
                            }
                            if (atlasCount >= 3) {
                                splatmapRect3.x = atlasPackingResult[2].x;
                                splatmapRect3.y = atlasPackingResult[2].y;
                                splatmapRect3.z = atlasPackingResult[2].height;
                                splatmapRect3.w = atlasPackingResult[2].width;
                            }
                            if (atlasCount >= 4) {
                                splatmapRect4.x = atlasPackingResult[3].x;
                                splatmapRect4.y = atlasPackingResult[3].y;
                                splatmapRect4.z = atlasPackingResult[3].height;
                                splatmapRect4.w = atlasPackingResult[3].width;
                            }
                        } else {
                            UnityEngine.Debug.LogError("Null atlas packing result rects");
                        }
                        terrainMaterial.vectors4.Add("splatmapRect1", splatmapRect1.ToFloat());
                        terrainMaterial.vectors4.Add("splatmapRect2", splatmapRect2.ToFloat());
                        terrainMaterial.vectors4.Add("splatmapRect3", splatmapRect3.ToFloat());
                        terrainMaterial.vectors4.Add("splatmapRect4", splatmapRect4.ToFloat());
                        terrainMaterial.floats.Add("splatmapRects", (float)atlasCount);
                    }

                    // Terrain receive shadows support
                    babylonMesh.receiveShadows = exportationOptions.TerrainReceiveShadows;

                    // Terrain collision mesh support
                    var terrainCollider = gameObject.GetComponent<TerrainCollider>();
                    if (terrainCollider != null && terrainCollider.enabled)
                    {
                        BabylonTerrainColliders segments = (BabylonTerrainColliders)exportationOptions.TerrainMeshSegemnts;
                        if (exportationOptions.GenerateColliders == false || segments == BabylonTerrainColliders.TerrainMesh) {
                            babylonMesh.checkCollisions = true;
                        } else {
                            int splitFactor = (int)segments;
                            Mesh[] terrainColliderMeshes = Tools.SplitTerrainData(terrain, splitFactor);
                            if (terrainColliderMeshes != null && terrainColliderMeshes.Length > 0) {
                                babylonMesh.checkCollisions = false;
                                int index = 0;
                                foreach (var terrainColliderMesh in terrainColliderMeshes)
                                {
                                    index++;
                                    
                                    BabylonMesh collisionMesh = new BabylonMesh();
                                    collisionMesh.tags = "[MESHCOLLIDER]";
                                    collisionMesh.id = Guid.NewGuid().ToString();
                                    collisionMesh.name = terrain.name + index.ToString() + "_Collider";

                                    collisionMesh.parentId = babylonMesh.id;
                                    collisionMesh.isVisible = exportationOptions.ShowDebugColliders;
                                    collisionMesh.visibility = exportationOptions.ColliderVisibility;
                                    collisionMesh.checkCollisions = exportationOptions.ExportCollisions;
                                    collisionMesh.numBoneInfluencers = Tools.GetMaxBoneInfluencers();

                                    collisionMesh.position = transform.localPosition.ToFloat();
                                    collisionMesh.rotation = Vector3.zero.ToFloat();
                                    collisionMesh.scaling = new Vector3(1f, 1f, 1f).ToFloat();

                                    // TODO: Add Default Metedata and Pysics State to each terrain collision mesh

                                    Tools.GenerateBabylonMeshData(terrainColliderMesh, collisionMesh, babylonScene, transform);
                                    babylonScene.MeshesList.Add(collisionMesh);
                                }
                            } else {
                                babylonMesh.checkCollisions = true;
                                UnityEngine.Debug.LogWarning("Failed to segment terrain colliders for: " + terrain.name);
                            }
                        }
                    }

                    // Babylon physics state (With trigger colliders)
                    if (exportationOptions.ExportPhysics)
                    {
                        var physics = gameObject.GetComponent<PhysicsState>();
                        if (physics != null && physics.isActiveAndEnabled == true)
                        {
                            babylonMesh.tags += " [PHYSICS]";
                            metaData.properties.Add("physicsTag", gameObject.tag);
                            metaData.properties.Add("physicsMass", physics.mass);
                            metaData.properties.Add("physicsFriction", physics.friction);
                            metaData.properties.Add("physicsRestitution", physics.restitution);
                            metaData.properties.Add("physicsImpostor", (int)physics.imposter);
                            metaData.properties.Add("physicsRotation", (int)physics.rotation);
                            metaData.properties.Add("physicsCollisions", (physics.type == BabylonCollisionType.Collider));
                            metaData.properties.Add("physicsEnginePlugin", exportationOptions.DefaultPhysicsEngine);
                            SceneBuilder.Metadata.properties["hasPhysicsMeshes"] = true;                
                        }
                    }

                    // Animations
                    ExportTransformAnimationClips(transform, babylonMesh, ref metaData);
                    if (IsRotationQuaternionAnimated(babylonMesh))
                    {
                        babylonMesh.rotationQuaternion = transform.localRotation.ToFloat();
                    }

                    // Tagging
                    if (!String.IsNullOrEmpty(babylonMesh.tags))
                    {
                        babylonMesh.tags = babylonMesh.tags.Trim();
                    }

                    // Lens Flares
                    ParseLensFlares(gameObject, babylonMesh.id, ref lensFlares);

                    // Override Details
                    var details = gameObject.GetComponent<MeshDetails>();
                    if (details != null)
                    {
                        metaData.properties["defaultEllipsoid"] = details.meshEllipsoidProperties.defaultEllipsoid.ToFloat();
                        metaData.properties["ellipsoidOffset"] = details.meshEllipsoidProperties.ellipsoidOffset.ToFloat();
                        metaData.properties["freezeWorldMatrix"] = details.meshRuntimeProperties.freezeWorldMatrix;
                        metaData.properties["convertToUnIndexed"] = details.meshRuntimeProperties.convertToUnIndexed;
                        metaData.properties["convertToFlatShaded"] = details.meshRuntimeProperties.convertToFlatShaded;
                        if (details.meshRuntimeProperties.detachFromParent == true) {
                            babylonMesh.parentId = null;
                        }
                        // Validate Not On Prefab Layer
                        if (metaData.layerIndex != ExporterWindow.PrefabIndex) {
                            babylonMesh.isEnabled = details.enableMesh;
                            if (details.meshVisibilityProperties.overrideVisibility == true) {
                                babylonMesh.isVisible = details.meshVisibilityProperties.makeMeshVisible;
                                babylonMesh.visibility = details.meshVisibilityProperties.newVisibilityLevel;
                            }
                        }
                        // Last Chance Force Check Collision Override
                        if (details.forceCheckCollisions == true) {
                            babylonMesh.checkCollisions = true;
                            var tags = gameObject.GetComponent<TagsComponent>();
                            if (tags != null) {
                                if (tags.babylonTags.IndexOf("[COLLIDER]") >= 0) {
                                    babylonMesh.isVisible = exportationOptions.ShowDebugColliders;
                                    babylonMesh.visibility = exportationOptions.ColliderVisibility;
                                }
                            }
                        }
                    }

                    babylonMesh.metadata = metaData;
                    babylonScene.MeshesList.Add(babylonMesh);
                    SceneBuilder.Metadata.properties["hasTerrainMeshes"] = true;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("No valid terrain component found for: " + gameObject.name);
            }

            if (!exportationOptions.ExportMetadata) babylonMesh.metadata = null;
            return babylonMesh;
        }

        private BabylonUniversalMaterial CreateSplatmapMaterial(Terrain terrain, Material material = null, int lightmapIndex = -1, Vector4 lightmapScaleOffset = default(Vector4), int lightmapCoordIndex = -1)
        {
            bool hasLightmap = (exportationOptions.ExportLightmaps && lightmapIndex >= 0 && lightmapIndex != 65535 && LightmapSettings.lightmaps.Length > lightmapIndex);
            // Use Babylon Specular Glossiness Workflow
            var terrainId = Guid.NewGuid().ToString();
            var babylonTerrainMaterial = new BabylonUniversalMaterial
            {
                name = terrain.name + ".Splatmap." + terrainId,
                id = terrainId,
                ambient = Color.black.ToFloat(),
                diffuse = Color.white.ToFloat(),
                specular = Color.black.ToFloat(),
                emissive = Color.black.ToFloat(),
                specularPower = 64,
                disableLighting = false,
                useSpecularOverAlpha = false,
                useEmissiveAsIllumination = false
            };
            babylonTerrainMaterial.SetCustomType("BABYLON.UniversalTerrainMaterial");
            string tname = Tools.FirstUpper(terrain.name.Replace(" ", "_"));

            float diffuseTextureLevel = 1.0f;
            float defaultTextureScale = (SceneController != null) ? SceneController.lightingOptions.textureLevel : 1.0f;
            float defaultLightmapScale = (SceneController != null) ? SceneController.lightingOptions.lightmapScale : 1.0f;
            ExporterWindow.ReportProgress(1, "Baking terrain atlas materials... This may take a while.");
            ///////////////////////////
            //  TEXTURE ATLAS INFOS  //
            ///////////////////////////
            Vector4[] atlasInfos = new Vector4[] { 
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0)
            };
            Vector4[] atlasRects = new Vector4[] { 
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0)
            };
            /////////////////////////////
            //  TEXTURE ATLAS VERSION  //
            /////////////////////////////
            int infos = 0;
            int rects = 0;
            int albedos = 0;
            int normals = 0;
            bool hasNormalMap = false;
            List<Texture2D> albedoList = new List<Texture2D>();
            BabylonTerrainSplat[] albedoInfo = GetTerrainTextureAtlasInfo(terrain, false);
            List<Texture2D> normalList = new List<Texture2D>();
            BabylonTerrainSplat[] normalInfo = GetTerrainTextureAtlasInfo(terrain, true);
            if (albedoInfo != null && albedoInfo.Length > 0 && normalInfo != null && normalInfo.Length > 0 && normalInfo.Length == albedoInfo.Length)
            {
                infos = albedoInfo.Length;
                for (int ii = 0; ii < infos; ii++)
                {
                    if (ii < atlasInfos.Length) {
                        var info = albedoInfo[ii];
                        Vector4 atlasInfo = new Vector4(Tools.GetTerrainScale(info.TileSize.x), Tools.GetTerrainScale(info.TileSize.y), info.TileOffset.x, info.TileOffset.y);
                        babylonTerrainMaterial.vectors4.Add("atlasInfo" + (ii + 1).ToString(), atlasInfo.ToFloat());
                        atlasInfos[ii] = atlasInfo;
                        // Albedo Texture
                        Texture2D albedoTexture = new Texture2D(info.Width, info.Height, TextureFormat.RGBA32, false);
                        if (info.Splat != null) {
                            albedoTexture.SetPixels32(info.Splat);
                            albedoTexture.Apply();
                        } else {
                            albedoTexture.Clear(Color.white);
                        }
                        albedoList.Add(albedoTexture);
                        // Normal Texture
                        var norm = normalInfo[ii];
                        Texture2D normalTexture = null;
                        if (norm.Splat != null) {
                            hasNormalMap = true;
                            normalTexture = new Texture2D(norm.Width, norm.Height, TextureFormat.RGBA32, false);
                            normalTexture.SetPixels32(norm.Splat);
                            normalTexture.Apply();
                        } else {
                            normalTexture = Tools.CreateBlankNormalMap(info.Width, info.Height);
                        }
                        normalList.Add(normalTexture);
                    }
                }
            }
            
            // Encode texture atlas info
            BabylonImageFormat format = (BabylonImageFormat)ExporterWindow.exportationOptions.ImageEncodingOptions;
            bool jpeg = ( format == BabylonImageFormat.JPEG);
            string atlasExt = (jpeg) ? "jpg" : "png";
            albedos = albedoList.Count;
            normals = normalList.Count;
            if (infos > 0 && albedos == infos && normals == infos) {
                // Pack albedo atlas rects
                string atlasName = String.Format("{0}_{1}_Atlas.{2}", SceneName, tname, atlasExt);
                string atlasFile = Path.Combine(OutputPath, atlasName);
                string atlasLabel = Path.GetFileName(atlasFile);
                bool bilinearScaling = (exportationOptions.TerrainImageScaling == (int)BabylonTextureScale.Bilinear);
                Texture2D albedoTextureAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Rect[] albedoPackingResult = Tools.PackTextureAtlas(albedoTextureAtlas, albedoList.ToArray(), exportationOptions.TerrainAtlasSize, exportationOptions.TerrainMaxImageSize, bilinearScaling, true, true);
                Texture2D albedoTextureBuffer = albedoTextureAtlas.Copy();
                albedoTextureBuffer.WriteImage(atlasFile, format);
                babylonTerrainMaterial.diffuseTexture = new BabylonTexture { name = atlasLabel, samplingMode = BabylonTexture.SamplingMode.NEAREST_NEAREST_MIPLINEAR };
                babylonTerrainMaterial.diffuseTexture.uScale = 1;
                babylonTerrainMaterial.diffuseTexture.vScale = 1;
                babylonTerrainMaterial.diffuseTexture.uOffset = 0;
                babylonTerrainMaterial.diffuseTexture.vOffset = 0;
                if (albedoPackingResult != null && albedoPackingResult.Length > 0) {
                    rects = albedoPackingResult.Length;
                    for (int ii = 0; ii < rects; ii++)
                    {
                        if (ii < atlasRects.Length) {
                            var albedoRect = albedoPackingResult[ii];
                            Vector4 atlasRect = new Vector4(albedoRect.x, albedoRect.y, albedoRect.height, albedoRect.width);
                            babylonTerrainMaterial.vectors4.Add("atlasRect" + (ii + 1).ToString(), atlasRect.ToFloat());
                            atlasRects[ii] = atlasRect;
                        }
                    }
                    // Pack normal atlas rects
                    if (hasNormalMap && albedoList.Count == normalList.Count) {
                        string normalName = String.Format("{0}_{1}_Normal.{2}", SceneName, tname, atlasExt);
                        string normalFile = Path.Combine(OutputPath, normalName);
                        string normalLabel = Path.GetFileName(normalFile);
                        Texture2D normalTextureAtlas = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                        Tools.PackTextureAtlas(normalTextureAtlas, normalList.ToArray(), exportationOptions.TerrainAtlasSize, exportationOptions.TerrainMaxImageSize, bilinearScaling, true, true);
                        Texture2D normalTextureBuffer = normalTextureAtlas.Copy();
                        normalTextureBuffer.WriteImage(normalFile, format);
                        babylonTerrainMaterial.bumpTexture = new BabylonTexture { name = normalLabel, samplingMode = BabylonTexture.SamplingMode.NEAREST_NEAREST_MIPLINEAR };
                        babylonTerrainMaterial.bumpTexture.uScale = 1;
                        babylonTerrainMaterial.bumpTexture.vScale = 1;
                        babylonTerrainMaterial.bumpTexture.uOffset = 0;
                        babylonTerrainMaterial.bumpTexture.vOffset = 0;
                    }
                } else {
                    throw new Exception("Null terrain texture atlas packing rects.");
                }
                babylonTerrainMaterial.floats.Add("atlasInfos", infos);
                babylonTerrainMaterial.floats.Add("atlasRects", rects);
            } else {
                throw new Exception("Terrain texture atlas packing list mismatch.");
            }

            // Default Material Properties
            float lightmapScale = 1.0f;
            float reflectionScale = 1.0f;
            if (material != null) {
                if (material.HasProperty("_TextureLevel")) {
                    diffuseTextureLevel = material.GetFloat("_TextureLevel");
                }
                
                if (material.HasProperty("_Shininess")) {
                    var specShininess = material.GetFloat("_Shininess");
                    babylonTerrainMaterial.specularPower = specShininess * 128;
                }
                if (material.HasProperty("_Color")) {
                    babylonTerrainMaterial.diffuse = material.color.ToFloat();
                }
                if (material.HasProperty("_AmbientColor")) {
                    var ambientColor = material.GetColor("_AmbientColor");
                    babylonTerrainMaterial.ambient = ambientColor.ToFloat();
                }
                if (material.HasProperty("_SpecColor")) {
                    var specColor = material.GetColor("_SpecColor");
                    babylonTerrainMaterial.specular = specColor.ToFloat();
                }
                if (material.HasProperty("_Emission"))
                {
                    if (material.GetColorNames().IndexOf("_Emission") >= 0)
                    {
                        var emissiveColor = material.GetColor("_Emission");
                        babylonTerrainMaterial.emissive = emissiveColor.ToFloat();
                    }
                    else if (material.GetFloatNames().IndexOf("_Emission") >= 0)
                    {
                        // TODO: Convert Lightmapper Emission Color
                        UnityEngine.Debug.LogWarning("Material Emission Is Float Not Color: " + material.name);
                    }
                }

                if (material.HasProperty("_AlphaMode")) {
                    babylonTerrainMaterial.alphaMode = material.GetInt("_AlphaMode");
                }
                if (material.HasProperty("_Wireframe")) {
                    babylonTerrainMaterial.wireframe = (material.GetInt("_Wireframe") != 0);
                }
                if (material.HasProperty("_DisableLighting")) {
                    babylonTerrainMaterial.disableLighting = (material.GetInt("_DisableLighting") != 0);
                }
                if (material.HasProperty("_BackFaceCulling")) {
                    babylonTerrainMaterial.backFaceCulling = (material.GetInt("_BackFaceCulling") != 0);
                }
                if (material.HasProperty("_TwoSidedLighting")) {
                    babylonTerrainMaterial.twoSidedLighting = (material.GetInt("_TwoSidedLighting") != 0);
                }
                if (material.HasProperty("_MaxSimultaneousLights")) {
                    babylonTerrainMaterial.maxSimultaneousLights = material.GetInt("_MaxSimultaneousLights");
                }
                if (material.HasProperty("_UseEmissiveAsIllumination")) {
                    babylonTerrainMaterial.useEmissiveAsIllumination = (material.GetInt("_UseEmissiveAsIllumination") != 0);
                }

                babylonTerrainMaterial.ambientTexture = DumpTextureFromMaterial(material, "_LightMap");
                babylonTerrainMaterial.emissiveTexture = DumpTextureFromMaterial(material, "_EmissionMap");
                if (babylonTerrainMaterial.emissiveTexture == null) babylonTerrainMaterial.emissiveTexture = DumpTextureFromMaterial(material, "_Illum");
                babylonTerrainMaterial.reflectionTexture = DumpTextureFromMaterial(material, "_Cube");
                if (material.HasProperty("_ReflectionScale")) {
                    reflectionScale = material.GetFloat("_ReflectionScale");
                }
            }

            // Diffuse Texture Level
            if (babylonTerrainMaterial.diffuseTexture != null) {
                babylonTerrainMaterial.diffuseTexture.level = diffuseTextureLevel * defaultTextureScale;
            }

            // Reflection Scale
            if (babylonTerrainMaterial.reflectionTexture != null) {
                babylonTerrainMaterial.reflectionTexture.level *= reflectionScale;
            }

            // Lightmapping Scale
            float terrrainLightmapScale = defaultLightmapScale * lightmapScale;

            // Lightmapping Texture
            if (hasLightmap)
            {
                if (babylonTerrainMaterial.ambientTexture == null) {
                    var lightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapColor;
                    var texturePath = AssetDatabase.GetAssetPath(lightmap);
                    if (!String.IsNullOrEmpty(texturePath))
                    {
                        ExporterWindow.ReportProgress(1, "Dumping splatmap material lightmap: " + lightmap.name);
                        babylonTerrainMaterial.lightmapTexture = DumpTerrainLightmap(lightmap);
                        babylonTerrainMaterial.lightmapTexture.coordinatesIndex = (lightmapCoordIndex >= 0) ? lightmapCoordIndex : exportationOptions.DefaultCoordinatesIndex;
                        babylonTerrainMaterial.useLightmapAsShadowmap = true;

                        babylonTerrainMaterial.lightmapTexture.uScale = Tools.GetTextureScale(lightmapScaleOffset.x);
                        babylonTerrainMaterial.lightmapTexture.vScale = Tools.GetTextureScale(lightmapScaleOffset.y);

                        babylonTerrainMaterial.lightmapTexture.uOffset = lightmapScaleOffset.z;
                        babylonTerrainMaterial.lightmapTexture.vOffset = lightmapScaleOffset.w;

                        babylonTerrainMaterial.lightmapTexture.level *= terrrainLightmapScale;
                    }
                } else {
                    UnityEngine.Debug.LogWarning("Terrain Lightmap material already has occlusion texture for: " + babylonTerrainMaterial.name);
                }
            }
            materialsDictionary.Add(babylonTerrainMaterial.name, babylonTerrainMaterial);
            return babylonTerrainMaterial;
        }

        private BabylonTerrainSplat[] GetTerrainTextureAtlasInfo(Terrain terrain, bool bump)
        {
            List<BabylonTerrainSplat> result = new List<BabylonTerrainSplat>();
            if (terrain.terrainData.splatPrototypes != null && terrain.terrainData.splatPrototypes.Length > 0) {
                foreach (var splat in terrain.terrainData.splatPrototypes) {
                    Vector2 tileSize = new Vector2(0, 0);
                    Vector2 tileOffset = new Vector2(0, 0);
                    Texture2D splatTexture = null;
                    if (bump) {
                        if (splat.normalMap != null) {
                            string bumpTexturePath = AssetDatabase.GetAssetPath(splat.normalMap);
                            var importTool = new BabylonTextureImporter(bumpTexturePath);
                            var importType = importTool.textureImporter.textureType;
                            try
                            {
                                importTool.textureImporter.isReadable = true;
                                importTool.textureImporter.textureType = TextureImporterType.Default;
                                importTool.ForceUpdate();
                                tileSize = splat.tileSize;
                                tileOffset = splat.tileOffset;
                                splatTexture = splat.normalMap.Copy();
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
                    } else {
                        if (splat.texture != null) {
                            tileSize = splat.tileSize;
                            tileOffset = splat.tileOffset;
                            Texture2D tempTexture = splat.texture;
                            tempTexture.ForceReadable();
                            splatTexture = tempTexture.Copy();
                        } else {
                            splatTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                            splatTexture.Clear(Color.blue);
                        }
                    }
                    result.Add(new BabylonTerrainSplat(splatTexture, tileSize, tileOffset));
                }
            }
            return (result.Count > 0) ? result.ToArray() : null;
        }
        
        private BabylonTexture DumpTextureFromTerrain(Terrain terrain, int index, bool bump)
        {
            BabylonTexture result = null;
            if (terrain.terrainData.splatPrototypes != null && terrain.terrainData.splatPrototypes.Length > index && terrain.terrainData.splatPrototypes[index] != null) {
                SplatPrototype splat = terrain.terrainData.splatPrototypes[index];
                if (bump) {
                    result = DumpTerrainTexture(splat.normalMap, splat.tileSize, splat.tileOffset);
                } else {
                    result = DumpTerrainTexture(splat.texture, splat.tileSize, splat.tileOffset);
                }
            }
            return result;
        }

        private BabylonTexture DumpTerrainTexture(Texture2D texture, Vector2 tileSize, Vector2 tileOffset)
        {
            if (texture == null)
            {
                return null;
            }

            var texturePath = AssetDatabase.GetAssetPath(texture);
            var textureName = Path.GetFileName(texturePath);
            var babylonTexture = new BabylonTexture { name = textureName };
            babylonTexture.uScale = Tools.GetTerrainScale(tileSize.x);
            babylonTexture.vScale = Tools.GetTerrainScale(tileSize.y);

            babylonTexture.uOffset = tileOffset.x;
            babylonTexture.vOffset = tileOffset.y;

            CopyTexture(texturePath, texture, babylonTexture, false, true, (exportationOptions.ImageEncodingOptions == (int)BabylonImageFormat.JPEG));

            return babylonTexture;
        }
        
        private BabylonTexture DumpTerrainLightmap(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            var texturePath = AssetDatabase.GetAssetPath(texture);
            var babylonTexture = new BabylonTexture();

            CopyTexture(texturePath, texture, babylonTexture, true, true);

            return babylonTexture;
        }
    }
}
