using Autodesk.Max;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        private int bonesCount;

        readonly Dictionary<IIGameSkin, List<int>> skinSortedBones = new Dictionary<IIGameSkin, List<int>>();

        private bool IsMeshExportable(IIGameNode meshNode)
        {
            return IsNodeExportable(meshNode);
        }

        private BabylonNode ExportDummy(IIGameScene scene, IIGameNode meshNode, BabylonScene babylonScene)
        {
            RaiseMessage(meshNode.Name, 1);

            var gameMesh = meshNode.IGameObject.AsGameMesh();
            bool initialized = gameMesh.InitializeData; // needed, the property is in fact a method initializing the exporter that has wrongly been auto 
                                                        // translated into a property because it has no parameters

            var babylonMesh = new BabylonMesh { name = meshNode.Name, id = meshNode.MaxNode.GetGuid().ToString() };
            babylonMesh.isDummy = true;

            // Position / rotation / scaling / hierarchy
            exportNode(babylonMesh, meshNode, scene, babylonScene);

            // Animations
            exportAnimation(babylonMesh, meshNode);

            babylonScene.MeshesList.Add(babylonMesh);

            return babylonMesh;
        }

        private BabylonNode ExportMesh(IIGameScene scene, IIGameNode meshNode, BabylonScene babylonScene)
        {
            if (IsMeshExportable(meshNode) == false)
            {
                return null;
            }

            RaiseMessage(meshNode.Name, 1);

            // Instances
            var tabs = Loader.Global.NodeTab.Create();
            Loader.Global.IInstanceMgr.InstanceMgr.GetInstances(meshNode.MaxNode, tabs);
            if (tabs.Count > 1)
            {
                // For a mesh with instances, we distinguish between master and instance meshes:
                //      - a master mesh stores all the info of the mesh (transform, hierarchy, animations + vertices, indices, materials, bones...)
                //      - an instance mesh only stores the info of the node (transform, hierarchy, animations)

                // Check if this mesh has already been exported
                BabylonMesh babylonMasterMesh = null;
                var index = 0;
                while (babylonMasterMesh == null &&
                       index < tabs.Count)
                {
#if MAX2017 || MAX2018
                    var tab = tabs[index];
#else
                    var tab = tabs[new IntPtr(index)];
#endif

                    babylonMasterMesh = babylonScene.MeshesList.Find(_babylonMesh => {
                        // Same id
                        return _babylonMesh.id == tab.GetGuid().ToString() &&
                               // Mesh is not a dummy
                               _babylonMesh.isDummy == false;
                    });

                    index++;
                }

                if (babylonMasterMesh != null)
                {
                    // Mesh already exported
                    // Export this node as instance

                    meshNode.MaxNode.MarkAsInstance();

                    var babylonInstanceMesh = new BabylonAbstractMesh { name = meshNode.Name, id = meshNode.MaxNode.GetGuid().ToString() };

                    // Add instance to master mesh
                    List<BabylonAbstractMesh> list = babylonMasterMesh.instances != null ? babylonMasterMesh.instances.ToList() : new List<BabylonAbstractMesh>();
                    list.Add(babylonInstanceMesh);
                    babylonMasterMesh.instances = list.ToArray();

                    // Export transform / hierarchy / animations
                    exportNode(babylonInstanceMesh, meshNode, scene, babylonScene);

                    // Animations
                    exportAnimation(babylonInstanceMesh, meshNode);

                    return babylonInstanceMesh;
                }
            }

            var gameMesh = meshNode.IGameObject.AsGameMesh();
            bool initialized = gameMesh.InitializeData; // needed, the property is in fact a method initializing the exporter that has wrongly been auto 
                                                        // translated into a property because it has no parameters

            var babylonMesh = new BabylonMesh { name = meshNode.Name, id = meshNode.MaxNode.GetGuid().ToString() };

            // Position / rotation / scaling / hierarchy
            exportNode(babylonMesh, meshNode, scene, babylonScene);

            // Sounds
            var soundName = meshNode.MaxNode.GetStringProperty("babylonjs_sound_filename", "");
            if (!string.IsNullOrEmpty(soundName))
            {
                var filename = Path.GetFileName(soundName);

                var meshSound = new BabylonSound
                {
                    name = filename,
                    autoplay = meshNode.MaxNode.GetBoolProperty("babylonjs_sound_autoplay", 1),
                    loop = meshNode.MaxNode.GetBoolProperty("babylonjs_sound_loop", 1),
                    volume = meshNode.MaxNode.GetFloatProperty("babylonjs_sound_volume", 1.0f),
                    playbackRate = meshNode.MaxNode.GetFloatProperty("babylonjs_sound_playbackrate", 1.0f),
                    connectedMeshId = babylonMesh.id,
                    isDirectional = false,
                    spatialSound = false,
                    distanceModel = meshNode.MaxNode.GetStringProperty("babylonjs_sound_distancemodel", "linear"),
                    maxDistance = meshNode.MaxNode.GetFloatProperty("babylonjs_sound_maxdistance", 100f),
                    rolloffFactor = meshNode.MaxNode.GetFloatProperty("babylonjs_sound_rolloff", 1.0f),
                    refDistance = meshNode.MaxNode.GetFloatProperty("babylonjs_sound_refdistance", 1.0f),
                };

                var isDirectional = meshNode.MaxNode.GetBoolProperty("babylonjs_sound_directional");

                if (isDirectional)
                {
                    meshSound.isDirectional = true;
                    meshSound.coneInnerAngle = meshNode.MaxNode.GetFloatProperty("babylonjs_sound_coneinnerangle", 360f);
                    meshSound.coneOuterAngle = meshNode.MaxNode.GetFloatProperty("babylonjs_sound_coneouterangle", 360f);
                    meshSound.coneOuterGain = meshNode.MaxNode.GetFloatProperty("babylonjs_sound_coneoutergain", 1.0f);
                }

                babylonScene.SoundsList.Add(meshSound);

                if (isBabylonExported)
                {
                    try
                    {
                        File.Copy(soundName, Path.Combine(babylonScene.OutputPath, filename), true);
                    }
                    catch
                    {
                    }
                }
            }

            // Misc.
#if MAX2017 || MAX2018
            babylonMesh.isVisible = meshNode.MaxNode.Renderable;
            babylonMesh.receiveShadows = meshNode.MaxNode.RcvShadows;
            babylonMesh.applyFog = meshNode.MaxNode.ApplyAtmospherics;
#else
            babylonMesh.isVisible = meshNode.MaxNode.Renderable == 1;
            babylonMesh.receiveShadows = meshNode.MaxNode.RcvShadows == 1;
            babylonMesh.applyFog = meshNode.MaxNode.ApplyAtmospherics == 1;
#endif
            babylonMesh.pickable = meshNode.MaxNode.GetBoolProperty("babylonjs_checkpickable");
            babylonMesh.showBoundingBox = meshNode.MaxNode.GetBoolProperty("babylonjs_showboundingbox");
            babylonMesh.showSubMeshesBoundingBox = meshNode.MaxNode.GetBoolProperty("babylonjs_showsubmeshesboundingbox");
            babylonMesh.alphaIndex = (int)meshNode.MaxNode.GetFloatProperty("babylonjs_alphaindex", 1000);

            // Collisions
            babylonMesh.checkCollisions = meshNode.MaxNode.GetBoolProperty("babylonjs_checkcollisions");

            var isSkinned = gameMesh.IsObjectSkinned;
            var skin = gameMesh.IGameSkin;
            var unskinnedMesh = gameMesh;
            IGMatrix skinInitPoseMatrix = Loader.Global.GMatrix.Create(Loader.Global.Matrix3.Create(true));
            List<int> boneIds = null;
            int maxNbBones = 0;
            if (isSkinned)
            {
                bonesCount = skin.TotalSkinBoneCount;
                var skinAlreadyStored = skins.Find(_skin => IsSkinEqualTo(_skin, skin));
                if (skinAlreadyStored == null)
                {
                    skins.Add(skin);
                }
                else
                {
                    skin = skinAlreadyStored;
                }

                skinnedNodes.Add(meshNode);
                babylonMesh.skeletonId = skins.IndexOf(skin);
                skin.GetInitSkinTM(skinInitPoseMatrix);
                boneIds = SortBones(skin);
                skinSortedBones[skin] = boneIds;
            }

            // Mesh

            if (unskinnedMesh.IGameType == Autodesk.Max.IGameObject.ObjectTypes.Mesh && unskinnedMesh.MaxMesh != null)
            {
                if (unskinnedMesh.NumberOfFaces < 1)
                {
                    RaiseError($"Mesh {babylonMesh.name} has no face", 2);
                }

                if (unskinnedMesh.NumberOfVerts < 3)
                {
                    RaiseError($"Mesh {babylonMesh.name} has not enough vertices", 2);
                }

                if (unskinnedMesh.NumberOfVerts >= 65536)
                {
                    RaiseWarning($"Mesh {babylonMesh.name} has tmore than 65536 vertices which means that it will require specific WebGL extension to be rendered. This may impact portability of your scene on low end devices.", 2);
                }

                if (skin != null)
                {
                    for (var vertexIndex = 0; vertexIndex < unskinnedMesh.NumberOfVerts; vertexIndex++)
                    {
                        maxNbBones = Math.Max(maxNbBones, skin.GetNumberOfBones(vertexIndex));
                    }
                }

                // Physics
                var impostorText = meshNode.MaxNode.GetStringProperty("babylonjs_impostor", "None");

                if (impostorText != "None")
                {
                    switch (impostorText)
                    {
                        case "Sphere":
                            babylonMesh.physicsImpostor = 1;
                            break;
                        case "Box":
                            babylonMesh.physicsImpostor = 2;
                            break;
                        case "Plane":
                            babylonMesh.physicsImpostor = 3;
                            break;
                        default:
                            babylonMesh.physicsImpostor = 0;
                            break;
                    }

                    babylonMesh.physicsMass = meshNode.MaxNode.GetFloatProperty("babylonjs_mass");
                    babylonMesh.physicsFriction = meshNode.MaxNode.GetFloatProperty("babylonjs_friction", 0.2f);
                    babylonMesh.physicsRestitution = meshNode.MaxNode.GetFloatProperty("babylonjs_restitution", 0.2f);
                }

                // Material
                var mtl = meshNode.NodeMaterial;
                var multiMatsCount = 1;

                if (mtl != null)
                {
                    babylonMesh.materialId = mtl.MaxMaterial.GetGuid().ToString();

                    if (!referencedMaterials.Contains(mtl))
                    {
                        referencedMaterials.Add(mtl);
                    }

                    multiMatsCount = Math.Max(mtl.SubMaterialCount, 1);
                }

                babylonMesh.visibility = meshNode.MaxNode.GetVisibility(0, Tools.Forever);

                var vertices = new List<GlobalVertex>();
                var indices = new List<int>();
                var mappingChannels = unskinnedMesh.ActiveMapChannelNum;
                bool hasUV = false;
                bool hasUV2 = false;
                for (int i = 0; i < mappingChannels.Count; ++i)
                {
#if MAX2017 || MAX2018
                    var channelNum = mappingChannels[i];
#else
                    var channelNum = mappingChannels[new IntPtr(i)];
#endif
                    if (channelNum == 1)
                    {
                        hasUV = true;
                    }
                    else if (channelNum == 2)
                    {
                        hasUV2 = true;
                    }
                }
                var hasColor = unskinnedMesh.NumberOfColorVerts > 0;
                var hasAlpha = unskinnedMesh.GetNumberOfMapVerts(-2) > 0;

                var optimizeVertices = meshNode.MaxNode.GetBoolProperty("babylonjs_optimizevertices");

                var invertedWorldMatrix = GetInvertWorldTM(meshNode, 0);

                // Compute normals
                var subMeshes = new List<BabylonSubMesh>();
                List<int> faceIndexes = null;
                ExtractGeometry(vertices, indices, subMeshes, boneIds, skin, unskinnedMesh, invertedWorldMatrix, hasUV, hasUV2, hasColor, hasAlpha, optimizeVertices, multiMatsCount, meshNode, ref faceIndexes);

                if (vertices.Count >= 65536)
                {
                    RaiseWarning($"Mesh {babylonMesh.name} has {vertices.Count} vertices. This may prevent your scene to work on low end devices where 32 bits indice are not supported", 2);

                    if (!optimizeVertices)
                    {
                        RaiseError("You can try to optimize your object using [Try to optimize vertices] option", 2);
                    }
                }

                RaiseMessage($"{vertices.Count} vertices, {indices.Count / 3} faces", 2);

                // Buffers
                babylonMesh.positions = vertices.SelectMany(v => new[] { v.Position.X, v.Position.Y, v.Position.Z }).ToArray();

                // flip normals depending on parity
                var parityObject = meshNode.GetObjectTM(0).ExtractMatrix3().Parity;

                // for cesium, threejs and babylonjs (all the same)
                if (parityObject)
                {
                    // flipped case: reverse normals
                    babylonMesh.normals = vertices.SelectMany(v => new[] { -v.Normal.X, -v.Normal.Y, -v.Normal.Z }).ToArray();
                }
                else
                {
                    // normal case
                    babylonMesh.normals = vertices.SelectMany(v => new[] { v.Normal.X, v.Normal.Y, v.Normal.Z }).ToArray();
                }

                if (hasUV)
                {
                    babylonMesh.uvs = vertices.SelectMany(v => new[] { v.UV.X, 1 - v.UV.Y }).ToArray();
                }
                if (hasUV2)
                {
                    babylonMesh.uvs2 = vertices.SelectMany(v => new[] { v.UV2.X, 1 - v.UV2.Y }).ToArray();
                }

                if (skin != null)
                {
                    babylonMesh.matricesWeights = vertices.SelectMany(v => v.Weights.ToArray()).ToArray();
                    babylonMesh.matricesIndices = vertices.Select(v => v.BonesIndices).ToArray();

                    babylonMesh.numBoneInfluencers = maxNbBones;
                    if (maxNbBones > 4)
                    {
                        babylonMesh.matricesWeightsExtra = vertices.SelectMany(v => v.WeightsExtra != null ? v.WeightsExtra.ToArray() : new[] { 0.0f, 0.0f, 0.0f, 0.0f }).ToArray();
                        babylonMesh.matricesIndicesExtra = vertices.Select(v => v.BonesIndicesExtra).ToArray();
                    }
                }

                if (hasColor)
                {
                    babylonMesh.colors = vertices.SelectMany(v => v.Color.ToArray()).ToArray();
                    babylonMesh.hasVertexAlpha = hasAlpha;
                }

                babylonMesh.subMeshes = subMeshes.ToArray();

                // Buffers - Indices
                babylonMesh.indices = indices.ToArray();

                // ------------------------
                // ---- Morph targets -----
                // ------------------------

                // Retreive modifiers with morpher flag
                List<IIGameModifier> modifiers = new List<IIGameModifier>();
                for (int i = 0; i < meshNode.IGameObject.NumModifiers; i++)
                {
                    var modifier = meshNode.IGameObject.GetIGameModifier(i);
                    if (modifier.ModifierType == Autodesk.Max.IGameModifier.ModType.Morpher)
                    {
                        modifiers.Add(modifier);
                    }
                }

                // Cast modifiers to morphers
                List<IIGameMorpher> morphers = modifiers.ConvertAll(new Converter<IIGameModifier, IIGameMorpher>(modifier => modifier.AsGameMorpher()));

                var hasMorphTarget = false;
                morphers.ForEach(morpher =>
                {
                    if (morpher.NumberOfMorphTargets > 0)
                    {
                        hasMorphTarget = true;
                    }
                });

                if (hasMorphTarget)
                {
                    RaiseMessage("Export morph targets", 2);

                    // Morph Target Manager
                    var babylonMorphTargetManager = new BabylonMorphTargetManager();
                    babylonScene.MorphTargetManagersList.Add(babylonMorphTargetManager);
                    babylonMesh.morphTargetManagerId = babylonMorphTargetManager.id;

                    // Morph Targets
                    var babylonMorphTargets = new List<BabylonMorphTarget>();
                    // All morphers are considered identical
                    // Their targets are concatenated
                    morphers.ForEach(morpher =>
                    {
                        for (int i = 0; i < morpher.NumberOfMorphTargets; i++)
                        {
                            // Morph target
                            var maxMorphTarget = morpher.GetMorphTarget(i);

                            // Ensure target still exists (green color legend)
                            if (maxMorphTarget != null)
                            {
                                var babylonMorphTarget = new BabylonMorphTarget
                                {
                                    name = maxMorphTarget.Name
                                };
                                babylonMorphTargets.Add(babylonMorphTarget);

                                // TODO - Influence
                                babylonMorphTarget.influence = 0f;

                                // Target geometry
                                var targetVertices = ExtractVertices(maxMorphTarget, optimizeVertices, faceIndexes);
                                babylonMorphTarget.positions = targetVertices.SelectMany(v => new[] { v.Position.X, v.Position.Y, v.Position.Z }).ToArray();
                                babylonMorphTarget.normals = targetVertices.SelectMany(v => new[] { v.Normal.X, v.Normal.Y, v.Normal.Z }).ToArray();

                                // Animations
                                var animations = new List<BabylonAnimation>();
                                var morphWeight = morpher.GetMorphWeight(i);
                                ExportFloatGameController(morphWeight, "influence", animations);
                                if (animations.Count > 0)
                                {
                                    babylonMorphTarget.animations = animations.ToArray();
                                }
                            }
                        }
                    });

                    babylonMorphTargetManager.targets = babylonMorphTargets.ToArray();
                }
            }

            // Animations
            // Done last to avoid '0 vertex found' error (unkown cause)
            exportAnimation(babylonMesh, meshNode);

            babylonScene.MeshesList.Add(babylonMesh);

            return babylonMesh;
        }

        private List<GlobalVertex> ExtractVertices(IIGameNode maxMorphTarget, bool optimizeVertices, List<int> faceIndexes)
        {
            var gameMesh = maxMorphTarget.IGameObject.AsGameMesh();
            bool initialized = gameMesh.InitializeData; // needed, the property is in fact a method initializing the exporter that has wrongly been auto 
                                                        // translated into a property because it has no parameters

            var mtl = maxMorphTarget.NodeMaterial;
            var multiMatsCount = 1;

            if (mtl != null)
            {
                multiMatsCount = Math.Max(mtl.SubMaterialCount, 1);
            }

            var invertedWorldMatrix = GetInvertWorldTM(maxMorphTarget, 0);

            var vertices = new List<GlobalVertex>();
            ExtractGeometry(vertices, new List<int>(), new List<BabylonSubMesh>(), null, null, gameMesh, invertedWorldMatrix, false, false, false, false, optimizeVertices, multiMatsCount, maxMorphTarget, ref faceIndexes);
            return vertices;
        }

        private void ExtractGeometry(List<GlobalVertex> vertices, List<int> indices, List<BabylonSubMesh> subMeshes, List<int> boneIds, IIGameSkin skin, IIGameMesh unskinnedMesh, IMatrix3 invertedWorldMatrix, bool hasUV, bool hasUV2, bool hasColor, bool hasAlpha, bool optimizeVertices, int multiMatsCount, IIGameNode meshNode, ref List<int> faceIndexes)
        {
            List<GlobalVertex>[] verticesAlreadyExported = null;

            if (optimizeVertices)
            {
                verticesAlreadyExported = new List<GlobalVertex>[unskinnedMesh.NumberOfVerts];
            }

            var indexStart = 0;

            // Whether or not to store order in which faces are exported
            // Storage is used when exporting Morph Targets geometry
            // To ensure face order is identical, especially with multimaterials involved
            bool storeFaceIndexes = faceIndexes == null;
            if (storeFaceIndexes)
            {
                faceIndexes = new List<int>();
            }
            int indexInFaceIndexesArray = 0;
            for (int i = 0; i < multiMatsCount; ++i)
            {
                int materialId = meshNode.NodeMaterial?.GetMaterialID(i) ?? 0;
                var indexCount = 0;
                var minVertexIndex = int.MaxValue;
                var maxVertexIndex = int.MinValue;
                var subMesh = new BabylonSubMesh { indexStart = indexStart, materialIndex = i };

                if (multiMatsCount == 1)
                {
                    for (int j = 0; j < unskinnedMesh.NumberOfFaces; ++j)
                    {
                        IFaceEx face = null;
                        if (storeFaceIndexes)
                        {
                            face = unskinnedMesh.GetFace(j);
                            // Store face index (j = face.MeshFaceIndex)
                            faceIndexes.Add(j);
                        }
                        else
                        {
                            face = unskinnedMesh.GetFace(faceIndexes[indexInFaceIndexesArray++]);
                        }
                        ExtractFace(skin, unskinnedMesh, invertedWorldMatrix, vertices, indices, hasUV, hasUV2, hasColor, hasAlpha, verticesAlreadyExported, ref indexCount, ref minVertexIndex, ref maxVertexIndex, face, boneIds);
                    }
                }
                else
                {
                    ITab<IFaceEx> materialFaces = unskinnedMesh.GetFacesFromMatID(materialId);
                    for (int j = 0; j < materialFaces.Count; ++j)
                    {
                        IFaceEx face = null;
                        if (storeFaceIndexes)
                        {
                            // Retreive face
#if MAX2017 || MAX2018
                            face = materialFaces[j];
#else
                            face = materialFaces[new IntPtr(j)];
#endif

                            // Store face index
                            faceIndexes.Add(face.MeshFaceIndex);
                        }
                        else
                        {
                            face = unskinnedMesh.GetFace(faceIndexes[indexInFaceIndexesArray++]);
                        }
                        ExtractFace(skin, unskinnedMesh, invertedWorldMatrix, vertices, indices, hasUV, hasUV2, hasColor, hasAlpha, verticesAlreadyExported, ref indexCount, ref minVertexIndex, ref maxVertexIndex, face, boneIds);
                    }
                }

                if (indexCount != 0)
                {

                    subMesh.indexCount = indexCount;
                    subMesh.verticesStart = minVertexIndex;
                    subMesh.verticesCount = maxVertexIndex - minVertexIndex + 1;

                    indexStart += indexCount;

                    subMeshes.Add(subMesh);
                }
            }
        }

        private void ExtractFace(IIGameSkin skin, IIGameMesh unskinnedMesh, IMatrix3 invertedWorldMatrix, List<GlobalVertex> vertices, List<int> indices, bool hasUV, bool hasUV2, bool hasColor, bool hasAlpha, List<GlobalVertex>[] verticesAlreadyExported, ref int indexCount, ref int minVertexIndex, ref int maxVertexIndex, IFaceEx face, List<int> boneIds)
        {
            int a, b, c;
            // parity is TRUE, if determinant negative ( counter-intuitive convention of 3ds max, see docs... :/ )

            // fix for cesium: currently, cesium does not expect a reversed winding order for negative scales
            //if (false)

            // for threejs and babylonjs (handle negative scales correctly (reversed winding order expected)
            if (invertedWorldMatrix.Parity)
            {
                // flipped case: reverse winding order
                a = CreateGlobalVertex(unskinnedMesh, invertedWorldMatrix, face, 0, vertices, hasUV, hasUV2, hasColor, hasAlpha, verticesAlreadyExported, skin, boneIds);
                b = CreateGlobalVertex(unskinnedMesh, invertedWorldMatrix, face, 1, vertices, hasUV, hasUV2, hasColor, hasAlpha, verticesAlreadyExported, skin, boneIds);
                c = CreateGlobalVertex(unskinnedMesh, invertedWorldMatrix, face, 2, vertices, hasUV, hasUV2, hasColor, hasAlpha, verticesAlreadyExported, skin, boneIds);
            }
            else
            {
                // normal case
                a = CreateGlobalVertex(unskinnedMesh, invertedWorldMatrix, face, 0, vertices, hasUV, hasUV2, hasColor, hasAlpha, verticesAlreadyExported, skin, boneIds);
                b = CreateGlobalVertex(unskinnedMesh, invertedWorldMatrix, face, 2, vertices, hasUV, hasUV2, hasColor, hasAlpha, verticesAlreadyExported, skin, boneIds);
                c = CreateGlobalVertex(unskinnedMesh, invertedWorldMatrix, face, 1, vertices, hasUV, hasUV2, hasColor, hasAlpha, verticesAlreadyExported, skin, boneIds);
            }

            indices.Add(a);
            indices.Add(b);
            indices.Add(c);

            if (a < minVertexIndex)
            {
                minVertexIndex = a;
            }

            if (b < minVertexIndex)
            {
                minVertexIndex = b;
            }

            if (c < minVertexIndex)
            {
                minVertexIndex = c;
            }

            if (a > maxVertexIndex)
            {
                maxVertexIndex = a;
            }

            if (b > maxVertexIndex)
            {
                maxVertexIndex = b;
            }

            if (c > maxVertexIndex)
            {
                maxVertexIndex = c;
            }


            indexCount += 3;
            CheckCancelled();
        }

        List<int> SortBones(IIGameSkin skin)
        {
            var boneIds = new List<int>();
            var boneIndex = new Dictionary<int, IIGameNode>();
            for (var index = 0; index < skin.TotalSkinBoneCount; index++)
            {
                var bone = skin.GetIGameBone(index, false);
                if (bone == null)
                {
                    // non bone in skeletton
                    boneIds.Add(-2);

                }
                else
                {
                    boneIds.Add(bone.NodeID);
                    boneIndex[bone.NodeID] = bone;
                }
            }
            while (true)
            {
                bool foundMisMatch = false;
                for (int i = 0; i < boneIds.Count; ++i)
                {
                    var id = boneIds[i];
                    if (id == -2)
                    {
                        continue;
                    }
                    var parent = boneIndex[id].NodeParent;
                    if (parent != null)
                    {
                        var parentId = parent.NodeID;
                        if (boneIds.IndexOf(parentId) > i)
                        {
                            boneIds.RemoveAt(i);
                            boneIds.Insert(boneIds.IndexOf(parentId) + 1, id);
                            foundMisMatch = true;
                            break;
                        }
                    }
                }
                if (!foundMisMatch)
                {
                    break;
                }
            }
            return boneIds;

        }

        int CreateGlobalVertex(IIGameMesh mesh, IMatrix3 invertedWorldMatrix, IFaceEx face, int facePart, List<GlobalVertex> vertices, bool hasUV, bool hasUV2, bool hasColor, bool hasAlpha, List<GlobalVertex>[] verticesAlreadyExported, IIGameSkin skin, List<int> boneIds)
        {
            var vertexIndex = (int)face.Vert[facePart];

            // Position can by retreived in world space or object space
            // Unfortunately, this value can't be retreived in local space
            var vertex = new GlobalVertex
            {
                BaseIndex = vertexIndex,
                Position = mesh.GetVertex(vertexIndex, false), // world space
                Normal = mesh.GetNormal((int)face.Norm[facePart], false) // world space
            };

            // Convert position and normal to local space
            vertex.Position = invertedWorldMatrix.PointTransform(vertex.Position);
            vertex.Normal = invertedWorldMatrix.VectorTransform(vertex.Normal).Normalize;

            if (hasUV)
            {
                var indices = new int[3];
                unsafe
                {
                    fixed (int* indicesPtr = indices)
                    {
                        mesh.GetMapFaceIndex(1, face.MeshFaceIndex, new IntPtr(indicesPtr));
                    }
                }
                var texCoord = mesh.GetMapVertex(1, indices[facePart]);
                vertex.UV = Loader.Global.Point2.Create(texCoord.X, -texCoord.Y);
            }

            if (hasUV2)
            {
                var indices = new int[3];
                unsafe
                {
                    fixed (int* indicesPtr = indices)
                    {
                        mesh.GetMapFaceIndex(2, face.MeshFaceIndex, new IntPtr(indicesPtr));
                    }
                }
                var texCoord = mesh.GetMapVertex(2, indices[facePart]);
                vertex.UV2 = Loader.Global.Point2.Create(texCoord.X, -texCoord.Y);
            }

            if (hasColor)
            {
                var vertexColorIndex = (int)face.Color[facePart];
                var vertexColor = mesh.GetColorVertex(vertexColorIndex);
                float alpha = 1;
                if (hasAlpha)
                {
                    var indices = new int[3];
                    unsafe
                    {
                        fixed (int* indicesPtr = indices)
                        {
                            mesh.GetMapFaceIndex(-2, face.MeshFaceIndex, new IntPtr(indicesPtr));
                        }
                    }
                    var color = mesh.GetMapVertex(-2, indices[facePart]);

                    alpha = color.X;
                }

                vertex.Color = new[] { vertexColor.X, vertexColor.Y, vertexColor.Z, alpha };
            }

            if (skin != null)
            {
                float weight0 = 0;
                float weight1 = 0;
                float weight2 = 0;
                float weight3 = 0;
                int bone0 = bonesCount;
                int bone1 = bonesCount;
                int bone2 = bonesCount;
                int bone3 = bonesCount;
                var nbBones = skin.GetNumberOfBones(vertexIndex);

                if (nbBones > 0)
                {
                    bone0 = boneIds.IndexOf(skin.GetIGameBone(vertexIndex, 0).NodeID);
                    weight0 = skin.GetWeight(vertexIndex, 0);
                }

                if (nbBones > 1)
                {
                    bone1 = boneIds.IndexOf(skin.GetIGameBone(vertexIndex, 1).NodeID);
                    weight1 = skin.GetWeight(vertexIndex, 1);
                }

                if (nbBones > 2)
                {
                    bone2 = boneIds.IndexOf(skin.GetIGameBone(vertexIndex, 2).NodeID);
                    weight2 = skin.GetWeight(vertexIndex, 2);
                }

                if (nbBones > 3)
                {
                    bone3 = boneIds.IndexOf(skin.GetIGameBone(vertexIndex, 3).NodeID);
                    weight3 = skin.GetWeight(vertexIndex, 3);
                }

                if (nbBones == 0)
                {
                    weight0 = 1.0f;
                    bone0 = bonesCount;
                }

                vertex.Weights = Loader.Global.Point4.Create(weight0, weight1, weight2, weight3);
                vertex.BonesIndices = (bone3 << 24) | (bone2 << 16) | (bone1 << 8) | bone0;

                if (nbBones > 4)
                {
                    bone0 = boneIds.IndexOf(skin.GetIGameBone(vertexIndex, 4).NodeID);
                    weight0 = skin.GetWeight(vertexIndex, 4);

                    weight1 = 0;
                    weight2 = 0;
                    weight3 = 0;

                    if (nbBones > 5)
                    {
                        bone1 = boneIds.IndexOf(skin.GetIGameBone(vertexIndex, 5).NodeID);
                        weight1 = skin.GetWeight(vertexIndex, 5);
                    }

                    if (nbBones > 6)
                    {
                        bone2 = boneIds.IndexOf(skin.GetIGameBone(vertexIndex, 6).NodeID);
                        weight2 = skin.GetWeight(vertexIndex, 6);
                    }

                    if (nbBones > 7)
                    {
                        bone3 = boneIds.IndexOf(skin.GetIGameBone(vertexIndex, 7).NodeID);
                        weight3 = skin.GetWeight(vertexIndex, 7);
                    }

                    vertex.WeightsExtra = Loader.Global.Point4.Create(weight0, weight1, weight2, weight3);
                    vertex.BonesIndicesExtra = (bone3 << 24) | (bone2 << 16) | (bone1 << 8) | bone0;

                    if (nbBones > 8)
                    {
                        RaiseError("Too many bones influences per vertex: " + nbBones + ". Babylon.js only support 8 bones influences per vertex.", 2);
                    }
                }

            }

            if (verticesAlreadyExported != null)
            {
                if (verticesAlreadyExported[vertexIndex] != null)
                {
                    var index = verticesAlreadyExported[vertexIndex].IndexOf(vertex);

                    if (index > -1)
                    {
                        return verticesAlreadyExported[vertexIndex][index].CurrentIndex;
                    }
                }
                else
                {
                    verticesAlreadyExported[vertexIndex] = new List<GlobalVertex>();
                }

                vertex.CurrentIndex = vertices.Count;
                verticesAlreadyExported[vertexIndex].Add(vertex);
            }

            vertices.Add(vertex);

            return vertices.Count - 1;
        }

        private void exportNode(BabylonAbstractMesh babylonAbstractMesh, IIGameNode maxGameNode, IIGameScene maxGameScene, BabylonScene babylonScene)
        {
            // Position / rotation / scaling
            exportTransform(babylonAbstractMesh, maxGameNode);

            // Hierarchy
            if (maxGameNode.NodeParent != null)
            {
                babylonAbstractMesh.parentId = maxGameNode.NodeParent.MaxNode.GetGuid().ToString();
            }
        }

        private void exportTransform(BabylonAbstractMesh babylonAbstractMesh, IIGameNode maxGameNode)
        {
            // Position / rotation / scaling
            var localTM = maxGameNode.GetLocalTM(0);
            
            // use babylon decomposition, as 3ds max built-in values are no correct
            var tm_babylon = new BabylonMatrix();
            tm_babylon.m = localTM.ToArray();

            var s_babylon = new BabylonVector3();
            var q_babylon = new BabylonQuaternion();
            var t_babylon = new BabylonVector3();

            tm_babylon.decompose(s_babylon, q_babylon, t_babylon);

            if (ExportQuaternionsInsteadOfEulers)
            {
                babylonAbstractMesh.rotationQuaternion = q_babylon.ToArray();
            }
            else
            {
                babylonAbstractMesh.rotation = q_babylon.toEulerAngles().ToArray();
            }

            // normalize quaternion
            var q = q_babylon;
            float q_length = (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
            babylonAbstractMesh.rotationQuaternion = new[] { q_babylon.X / q_length, q_babylon.Y / q_length, q_babylon.Z / q_length, q_babylon.W / q_length };
            babylonAbstractMesh.scaling = new[] { s_babylon.X, s_babylon.Y, s_babylon.Z };
            babylonAbstractMesh.position = new[] { t_babylon.X, t_babylon.Y, t_babylon.Z };

        }

        private void exportAnimation(BabylonNode babylonNode, IIGameNode maxGameNode)
        {
            var animations = new List<BabylonAnimation>();

            GenerateCoordinatesAnimations(maxGameNode, animations);

            if (!ExportFloatController(maxGameNode.MaxNode.VisController, "visibility", animations))
            {
                ExportFloatAnimation("visibility", animations, key => new[] { maxGameNode.MaxNode.GetVisibility(key, Tools.Forever) });
            }

            babylonNode.animations = animations.ToArray();

            if (maxGameNode.MaxNode.GetBoolProperty("babylonjs_autoanimate", 1))
            {
                babylonNode.autoAnimate = true;
                babylonNode.autoAnimateFrom = (int)maxGameNode.MaxNode.GetFloatProperty("babylonjs_autoanimate_from");
                babylonNode.autoAnimateTo = (int)maxGameNode.MaxNode.GetFloatProperty("babylonjs_autoanimate_to", 100);
                babylonNode.autoAnimateLoop = maxGameNode.MaxNode.GetBoolProperty("babylonjs_autoanimateloop", 1);
            }
        }

        public void GenerateCoordinatesAnimations(IIGameNode meshNode, List<BabylonAnimation> animations)
        {
            GeneratePositionAnimation(meshNode, animations);
            GenerateRotationAnimation(meshNode, animations);
            GenerateScalingAnimation(meshNode, animations);
        }
    }
}
