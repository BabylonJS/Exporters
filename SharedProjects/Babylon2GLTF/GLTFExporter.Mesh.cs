using BabylonExport.Entities;
using GLTFExport.Entities;
using GLTFExport.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Babylon2GLTF
{
    partial class GLTFExporter
    {
        private List<BabylonMesh> alreadyExportedSkinnedMeshes = new List<BabylonMesh>();

        // Meshes that share skinning information, indexed by the exported mesh with the original skinning information.
        private Dictionary<GLTFMesh, List<GLTFMesh>> sharedSkinnedMeshesByOriginal = new Dictionary<GLTFMesh, List<GLTFMesh>>();
        private Dictionary<string, List<GLTFMeshPrimitive>> primitivesCache = new Dictionary<string, List<GLTFMeshPrimitive>>();

        private GLTFMesh ExportMesh(BabylonMesh babylonMesh, GLTF gltf, BabylonScene babylonScene)
        {
            logger.RaiseMessage("GLTFExporter.Mesh | Export mesh named: " + babylonMesh.name, 1);

            // --------------------------
            // --- Mesh from babylon ----
            // --------------------------
            // get direct access to mesh data or use geometryId to do so
            IBabylonMeshData meshData = babylonMesh.geometryId == null ? babylonMesh : (IBabylonMeshData)babylonScene.geometries.Get(babylonMesh.geometryId)?? babylonMesh;

            if (meshData.positions == null || meshData.positions.Length == 0)
            {
                logger.RaiseMessage("GLTFExporter.Mesh | Mesh is a dummy", 2);
                return null;
            }

            logger.RaiseMessage("GLTFExporter.Mesh | Mesh from babylon", 2);
            // Retreive general data from babylon mesh
            int nbVertices = meshData.positions.Length / 3;
            bool hasUV = meshData.uvs != null && meshData.uvs.Length > 0;
            bool hasUV2 = meshData.uvs2 != null && meshData.uvs2.Length > 0;
            bool hasColor = meshData.colors != null && meshData.colors.Length > 0;
            bool hasBones = meshData.matricesIndices != null && meshData.matricesIndices.Length > 0;
            bool hasTangents = meshData.tangents != null && meshData.tangents.Length > 0;
            bool hasNormals = meshData.normals != null && meshData.normals.Length > 0;

            bool hasBonesExtra = babylonMesh.matricesIndicesExtra != null && babylonMesh.matricesIndicesExtra.Length > 0;
            bool hasMetadata = babylonMesh.metadata != null && babylonMesh.metadata.Count > 0;

            logger.RaiseMessage("GLTFExporter.Mesh | nbVertices=" + nbVertices, 3);
            logger.RaiseMessage("GLTFExporter.Mesh | hasUV=" + hasUV, 3);
            logger.RaiseMessage("GLTFExporter.Mesh | hasUV2=" + hasUV2, 3);
            logger.RaiseMessage("GLTFExporter.Mesh | hasColor=" + hasColor, 3);
            logger.RaiseMessage("GLTFExporter.Mesh | hasBones=" + hasBones, 3);
            logger.RaiseMessage("GLTFExporter.Mesh | hasBonesExtra=" + hasBonesExtra, 3);
            logger.RaiseMessage("GLTFExporter.Mesh | hasMetadata=" + hasMetadata, 3);
            logger.RaiseMessage("GLTFExporter.Mesh | hasNormals=" + hasNormals, 3);

            // Retreive vertices data from babylon mesh
            List<GLTFGlobalVertex> globalVertices = new List<GLTFGlobalVertex>();
            for (int indexVertex = 0; indexVertex < nbVertices; indexVertex++)
            {
                GLTFGlobalVertex globalVertex = new GLTFGlobalVertex();
                globalVertex.Position = BabylonVector3.FromArray(meshData.positions, indexVertex);

                // Switch coordinate system at object level
                globalVertex.Position.Z *= -1;

                if (hasNormals)
                {
                    globalVertex.Normal = BabylonVector3.FromArray(meshData.normals, indexVertex);
                    globalVertex.Normal.Z *= -1;
                }

                if (hasTangents)
                {
                    globalVertex.Tangent = BabylonQuaternion.FromArray(meshData.tangents, indexVertex);

                    // Switch coordinate system at object level
                    globalVertex.Tangent.Z *= -1;

                    // Invert W to switch to right handed system
                    globalVertex.Tangent.W *= -1;
                }

                if (hasUV)
                {
                    globalVertex.UV = BabylonVector2.FromArray(meshData.uvs, indexVertex);
                    // For glTF, the origin of the UV coordinates (0, 0) corresponds to the upper left corner of a texture image
                    // While for Babylon, it corresponds to the lower left corner of a texture image
                    globalVertex.UV.Y = 1 - globalVertex.UV.Y;
                }
                if (hasUV2)
                {
                    globalVertex.UV2 = BabylonVector2.FromArray(meshData.uvs2, indexVertex);
                    // For glTF, the origin of the UV coordinates (0, 0) corresponds to the upper left corner of a texture image
                    // While for Babylon, it corresponds to the lower left corner of a texture image
                    globalVertex.UV2.Y = 1 - globalVertex.UV2.Y;
                }
                if (hasColor)
                {
                    globalVertex.Color = ArrayExtension.SubArrayFromEntity(meshData.colors, indexVertex, 4);
                }
                if (hasBones)
                {
                    // In babylon, the 4 bones indices are stored in a single int
                    // Each bone index is 8-bit offset from the next
                    ushort[] unpackBabylonBonesToArray (uint babylonBoneIndices)
                    {
                        uint bone3 = babylonBoneIndices >> 24;
                        uint bone2 = (babylonBoneIndices << 8) >> 24;
                        uint bone1 = (babylonBoneIndices << 16) >> 24;
                        uint bone0 = (babylonBoneIndices << 24) >> 24;
                        babylonBoneIndices -= bone0 << 0;
                        return new ushort[] { (ushort)bone0, (ushort)bone1, (ushort)bone2, (ushort)bone3 };
                    }

                    uint bonesIndicesMerged = (uint)meshData.matricesIndices[indexVertex];
                    globalVertex.BonesIndices = unpackBabylonBonesToArray(bonesIndicesMerged);
                    globalVertex.BonesWeights = ArrayExtension.SubArrayFromEntity(meshData.matricesWeights, indexVertex, 4);
                    void clearBoneUnusedIndices(ushort[] indices, float[] weights)
                    {
                        for (int i = 0; i < indices.Length; ++i)
                        {
                            // Zero out indices of unused joint weights to avoid ACCESSOR_JOINTS_USED_ZERO_WEIGHT.
                            if (MathUtilities.IsAlmostEqualTo(weights[i], 0, float.Epsilon))
                            {
                                indices[i] = 0;
                            }
                        }
                    }

                    clearBoneUnusedIndices(globalVertex.BonesIndices, globalVertex.BonesWeights);

                    if (hasBonesExtra)
                    {
                        uint bonesIndicesExtraMerged = (uint)meshData.matricesIndicesExtra[indexVertex];
                        globalVertex.BonesIndicesExtra = unpackBabylonBonesToArray(bonesIndicesExtraMerged);
                        globalVertex.BonesWeightsExtra = ArrayExtension.SubArrayFromEntity(meshData.matricesWeightsExtra, indexVertex, 4);
                        
                        clearBoneUnusedIndices(globalVertex.BonesIndicesExtra, globalVertex.BonesWeightsExtra);
                    }
                }

                globalVertices.Add(globalVertex);
            }

            var babylonMorphTargetManager = GetBabylonMorphTargetManager(babylonScene, babylonMesh);

            // Retreive indices from babylon mesh
            List<int> babylonIndices = meshData.indices.ToList();

            // --------------------------
            // ------- Init glTF --------
            // --------------------------

            logger.RaiseMessage("GLTFExporter.Mesh | Init glTF", 2);
            // Mesh
            var gltfMesh = new GLTFMesh { name = babylonMesh.name };
            gltfMesh.index = gltf.MeshesList.Count;
            gltf.MeshesList.Add(gltfMesh);
            gltfMesh.idGroupInstance = babylonMesh.idGroupInstance;
            if (hasBones)
            {
                gltfMesh.idBabylonSkeleton = babylonMesh.skeletonId;
            }

            // --------------------------
            // ---- glTF primitives -----
            // --------------------------
            logger.RaiseMessage("GLTFExporter.Mesh | glTF primitives", 2);
            List<GLTFMeshPrimitive> meshPrimitives = new List<GLTFMeshPrimitive>();
            if (meshData != babylonMesh && primitivesCache.TryGetValue(babylonMesh.geometryId, out List<GLTFMeshPrimitive> result))
            {
                // this is a clone, indexing the same geometry
                // so we just need to retreive the primitive object and copy attributes and indices
                meshPrimitives.AddRange(result.Select((p,i) =>
                {
                    var meshPrimitive = new GLTFMeshPrimitive()
                    {
                        indices = p.indices,
                        attributes = p.attributes
                    };

                    // Material
                    if (babylonMesh.materialId != null)
                    {
                        logger.RaiseMessage("GLTFExporter.Mesh | Material", 3);
                        // according we have a clone, then submeshes are of the same count and order.
                        SetBabylonMaterial(babylonMesh, babylonMesh.subMeshes[i], meshPrimitive);
                    }
                    return meshPrimitive;
                }));
            }
            else
            {
                foreach (BabylonSubMesh babylonSubMesh in babylonMesh.subMeshes)
                {
                    // --------------------------
                    // ------ SubMesh data ------
                    // --------------------------

                    List<GLTFGlobalVertex> globalVerticesSubMesh = globalVertices.GetRange(babylonSubMesh.verticesStart, babylonSubMesh.verticesCount);

                    var gltfIndices = babylonIndices.GetRange(babylonSubMesh.indexStart, babylonSubMesh.indexCount);
                    // In gltf, indices of each mesh primitive are 0-based (ie: min value is 0)
                    // Thus, the gltf indices list is a concatenation of sub lists all 0-based
                    // Example for 2 triangles, each being a submesh:
                    // babylonIndices = {0,1,2, 3,4,5} gives as result gltfIndicies = {0,1,2, 0,1,2}
                    var minIndiceValue = gltfIndices.Min(); // Should be equal to babylonSubMesh.indexStart
                    if (minIndiceValue != 0)
                    {
                        for (int indexIndice = 0; indexIndice < gltfIndices.Count; indexIndice++)
                        {
                            gltfIndices[indexIndice] -= minIndiceValue;
                        }
                    }

                    // --------------------------
                    // ----- Mesh primitive -----
                    // --------------------------
                    // MeshPrimitive
                    var meshPrimitive = new GLTFMeshPrimitive
                    {
                        attributes = new Dictionary<string, int>()
                    };
                    meshPrimitives.Add(meshPrimitive);

                    // Material
                    if (babylonMesh.materialId != null)
                    {
                        logger.RaiseMessage("GLTFExporter.Mesh | Material", 3);
                        SetBabylonMaterial(babylonMesh, babylonSubMesh, meshPrimitive);
                    }

                    // --------------------------
                    // ------- Accessors --------
                    // --------------------------

                    logger.RaiseMessage("GLTFExporter.Mesh | Geometry", 3);

                    // Buffer
                    var buffer = GLTFBufferService.Instance.GetBuffer(gltf);

                    // --- Indices ---
                    var componentType = GLTFAccessor.ComponentType.UNSIGNED_SHORT;
                    if (nbVertices >= 65536)
                    {
                        componentType = GLTFAccessor.ComponentType.UNSIGNED_INT;
                    }
                    var accessorIndices = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewScalar(gltf, buffer),
                        "accessorIndices",
                        componentType,
                        GLTFAccessor.TypeEnum.SCALAR
                    );
                    meshPrimitive.indices = accessorIndices.index;
                    // Populate accessor
                    if (componentType == GLTFAccessor.ComponentType.UNSIGNED_INT)
                    {
                        gltfIndices.ForEach(n => accessorIndices.bytesList.AddRange(BitConverter.GetBytes(n)));
                    }
                    else
                    {
                        var gltfIndicesShort = gltfIndices.ConvertAll(new Converter<int, ushort>(n => (ushort)n));
                        gltfIndicesShort.ForEach(n => accessorIndices.bytesList.AddRange(BitConverter.GetBytes(n)));
                    }
                    accessorIndices.count = gltfIndices.Count;

                    // --- Positions ---
                    var accessorPositions = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewFloatVec3(gltf, buffer),
                        "accessorPositions",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.VEC3
                    );
                    meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.POSITION.ToString(), accessorPositions.index);
                    // Populate accessor
                    accessorPositions.min = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
                    accessorPositions.max = new float[] { float.MinValue, float.MinValue, float.MinValue };
                    globalVerticesSubMesh.ForEach((globalVertex) =>
                    {
                        var positions = globalVertex.Position.ToArray();
                    // Store values as bytes
                    foreach (var position in positions)
                        {
                            accessorPositions.bytesList.AddRange(BitConverter.GetBytes(position));
                        }
                    // Update min and max values
                    GLTFBufferService.UpdateMinMaxAccessor(accessorPositions, positions);
                    });
                    accessorPositions.count = globalVerticesSubMesh.Count;

                    // --- Tangents ---
                    if (hasTangents)
                    {
                        var accessorTangents = GLTFBufferService.Instance.CreateAccessor(
                            gltf,
                            GLTFBufferService.Instance.GetBufferViewFloatVec4(gltf, buffer),
                            "accessorTangents",
                            GLTFAccessor.ComponentType.FLOAT,
                            GLTFAccessor.TypeEnum.VEC4
                        );
                        meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.TANGENT.ToString(), accessorTangents.index);
                        // Populate accessor
                        List<float> tangents = globalVerticesSubMesh.SelectMany(v => v.Tangent.ToArray()).ToList();
                        tangents.ForEach(n => accessorTangents.bytesList.AddRange(BitConverter.GetBytes(n)));
                        accessorTangents.count = globalVerticesSubMesh.Count;
                    }

                    // --- Normals ---
                    if (hasNormals)
                    {
                        var accessorNormals = GLTFBufferService.Instance.CreateAccessor(
                          gltf,
                          GLTFBufferService.Instance.GetBufferViewFloatVec3(gltf, buffer),
                          "accessorNormals",
                          GLTFAccessor.ComponentType.FLOAT,
                          GLTFAccessor.TypeEnum.VEC3
                        );
                        meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.NORMAL.ToString(), accessorNormals.index);

                        // Populate accessor
                        List<float> normals = globalVerticesSubMesh.SelectMany(v => v.Normal.ToArray()).ToList();
                        normals.ForEach(n => accessorNormals.bytesList.AddRange(BitConverter.GetBytes(n)));
                        accessorNormals.count = globalVerticesSubMesh.Count;
                    }

                    // --- Colors ---
                    if (hasColor)
                    {
                        var accessorColors = GLTFBufferService.Instance.CreateAccessor(
                            gltf,
                            GLTFBufferService.Instance.GetBufferViewFloatVec4(gltf, buffer),
                            "accessorColors",
                            GLTFAccessor.ComponentType.FLOAT,
                            GLTFAccessor.TypeEnum.VEC4
                        );
                        meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.COLOR_0.ToString(), accessorColors.index);
                        // Populate accessor
                        List<float> colors = globalVerticesSubMesh.SelectMany(v => new[] { v.Color[0], v.Color[1], v.Color[2], v.Color[3] }).ToList();
                        colors.ForEach(n => accessorColors.bytesList.AddRange(BitConverter.GetBytes(n)));
                        accessorColors.count = globalVerticesSubMesh.Count;
                    }

                    // --- UV ---
                    if (hasUV)
                    {
                        var accessorUVs = GLTFBufferService.Instance.CreateAccessor(
                            gltf,
                            GLTFBufferService.Instance.GetBufferViewFloatVec2(gltf, buffer),
                            "accessorUVs",
                            GLTFAccessor.ComponentType.FLOAT,
                            GLTFAccessor.TypeEnum.VEC2
                        );
                        meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.TEXCOORD_0.ToString(), accessorUVs.index);
                        // Populate accessor
                        List<float> uvs = globalVerticesSubMesh.SelectMany(v => v.UV.ToArray()).ToList();
                        uvs.ForEach(n => accessorUVs.bytesList.AddRange(BitConverter.GetBytes(n)));
                        accessorUVs.count = globalVerticesSubMesh.Count;
                    }

                    // --- UV2 ---
                    if (hasUV2)
                    {
                        var accessorUV2s = GLTFBufferService.Instance.CreateAccessor(
                            gltf,
                            GLTFBufferService.Instance.GetBufferViewFloatVec2(gltf, buffer),
                            "accessorUV2s",
                            GLTFAccessor.ComponentType.FLOAT,
                            GLTFAccessor.TypeEnum.VEC2
                        );
                        meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.TEXCOORD_1.ToString(), accessorUV2s.index);
                        // Populate accessor
                        List<float> uvs2 = globalVerticesSubMesh.SelectMany(v => v.UV2.ToArray()).ToList();
                        uvs2.ForEach(n => accessorUV2s.bytesList.AddRange(BitConverter.GetBytes(n)));
                        accessorUV2s.count = globalVerticesSubMesh.Count;
                    }

                    // --- Bones ---
                    if (hasBones)
                    {
                        logger.RaiseMessage("GLTFExporter.Mesh | Bones", 3);

                        // if we've already exported this mesh's skeleton, check if the skins match,
                        // if so then export this mesh primitive to share joint and weight accessors.
                        var matchingSkinnedMesh = alreadyExportedSkinnedMeshes.FirstOrDefault(skinnedMesh => skinnedMesh.skeletonId == babylonMesh.skeletonId);
                        if (matchingSkinnedMesh != null && BabylonMesh.MeshesShareSkin(matchingSkinnedMesh, babylonMesh))
                        {
                            var tmpGltfMesh = gltf.MeshesList.FirstOrDefault(mesh => matchingSkinnedMesh.name == mesh.name);
                            var tmpGltfMeshPrimitive = tmpGltfMesh.primitives.First();

                            meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.JOINTS_0.ToString(), tmpGltfMeshPrimitive.attributes[GLTFMeshPrimitive.Attribute.JOINTS_0.ToString()]);
                            meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.WEIGHTS_0.ToString(), tmpGltfMeshPrimitive.attributes[GLTFMeshPrimitive.Attribute.WEIGHTS_0.ToString()]);
                            if (hasBonesExtra)
                            {
                                meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.JOINTS_1.ToString(), tmpGltfMeshPrimitive.attributes[GLTFMeshPrimitive.Attribute.JOINTS_1.ToString()]);
                                meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.WEIGHTS_1.ToString(), tmpGltfMeshPrimitive.attributes[GLTFMeshPrimitive.Attribute.WEIGHTS_1.ToString()]);
                            }
                            sharedSkinnedMeshesByOriginal[tmpGltfMesh].Add(gltfMesh);
                        }
                        else
                        {
                            // Create new joint and weight accessors for this mesh's skinning.
                            // --- Joints ---
                            sharedSkinnedMeshesByOriginal[gltfMesh] = new List<GLTFMesh>();
                            var accessorJoints = GLTFBufferService.Instance.CreateAccessor(
                                gltf,
                                GLTFBufferService.Instance.GetBufferViewUnsignedShortVec4(gltf, buffer),
                                "accessorJoints",
                                GLTFAccessor.ComponentType.UNSIGNED_SHORT,
                                GLTFAccessor.TypeEnum.VEC4
                            );
                            meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.JOINTS_0.ToString(), accessorJoints.index);
                            // Populate accessor
                            List<ushort> joints = globalVerticesSubMesh.SelectMany(v => new[] { v.BonesIndices[0], v.BonesIndices[1], v.BonesIndices[2], v.BonesIndices[3] }).ToList();
                            joints.ForEach(n => accessorJoints.bytesList.AddRange(BitConverter.GetBytes(n)));
                            accessorJoints.count = globalVerticesSubMesh.Count;

                            if (hasBonesExtra)
                            {
                                // --- Joints Extra ---
                                var accessorJointsExtra = GLTFBufferService.Instance.CreateAccessor(
                                    gltf,
                                    GLTFBufferService.Instance.GetBufferViewUnsignedShortVec4(gltf, buffer),
                                    "accessorJointsExtra",
                                    GLTFAccessor.ComponentType.UNSIGNED_SHORT,
                                    GLTFAccessor.TypeEnum.VEC4
                                );
                                meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.JOINTS_1.ToString(), accessorJointsExtra.index);
                                // Populate accessor
                                List<ushort> jointsExtra = globalVerticesSubMesh.SelectMany(v => new[] { v.BonesIndicesExtra[0], v.BonesIndicesExtra[1], v.BonesIndicesExtra[2], v.BonesIndicesExtra[3] }).ToList();
                                jointsExtra.ForEach(n => accessorJointsExtra.bytesList.AddRange(BitConverter.GetBytes(n)));
                                accessorJointsExtra.count = globalVerticesSubMesh.Count;
                            }

                            // --- Weights ---
                            var accessorWeights = GLTFBufferService.Instance.CreateAccessor(
                                gltf,
                                GLTFBufferService.Instance.GetBufferViewFloatVec4(gltf, buffer),
                                "accessorWeights",
                                GLTFAccessor.ComponentType.FLOAT,
                                GLTFAccessor.TypeEnum.VEC4
                            );
                            meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.WEIGHTS_0.ToString(), accessorWeights.index);
                            // Populate accessor
                            List<float> weightBones = globalVerticesSubMesh.SelectMany(v => new[] { v.BonesWeights[0], v.BonesWeights[1], v.BonesWeights[2], v.BonesWeights[3] }).ToList();
                            weightBones.ForEach(n => accessorWeights.bytesList.AddRange(BitConverter.GetBytes(n)));
                            accessorWeights.count = globalVerticesSubMesh.Count;

                            if (hasBonesExtra)
                            {
                                // --- Weights Extra ---
                                var accessorWeightsExtra = GLTFBufferService.Instance.CreateAccessor(
                                    gltf,
                                    GLTFBufferService.Instance.GetBufferViewFloatVec4(gltf, buffer),
                                    "accessorWeightsExtra",
                                    GLTFAccessor.ComponentType.FLOAT,
                                    GLTFAccessor.TypeEnum.VEC4
                                );
                                meshPrimitive.attributes.Add(GLTFMeshPrimitive.Attribute.WEIGHTS_1.ToString(), accessorWeightsExtra.index);
                                // Populate accessor
                                List<float> weightBonesExtra = globalVerticesSubMesh.SelectMany(v => new[] { v.BonesWeightsExtra[0], v.BonesWeightsExtra[1], v.BonesWeightsExtra[2], v.BonesWeightsExtra[3] }).ToList();
                                weightBonesExtra.ForEach(n => accessorWeightsExtra.bytesList.AddRange(BitConverter.GetBytes(n)));
                                accessorWeightsExtra.count = globalVerticesSubMesh.Count;
                            }
                        }
                    }

                    // Morph targets positions and normals
                    if (babylonMorphTargetManager != null)
                    {
                        logger.RaiseMessage("GLTFExporter.Mesh | Morph targets", 3);
                        _exportMorphTargets(babylonMesh, babylonSubMesh, babylonMorphTargetManager, gltf, buffer, meshPrimitive);
                    }
                }
                
                // this is a "master" mesh so save the primitives into dictionary
                if(babylonMesh.geometryId != null && meshPrimitives != null)
                {
                    primitivesCache.Add(babylonMesh.geometryId, meshPrimitives);
                }
            }
            gltfMesh.primitives = meshPrimitives.ToArray();

            // Morph targets weights and names
            if (babylonMorphTargetManager != null && babylonMorphTargetManager.targets != null)
            {
                var weights = new List<float>();
                var targetNames = new List<String>();
                foreach (BabylonMorphTarget babylonMorphTarget in babylonMorphTargetManager.targets)
                {
                    weights.Add(babylonMorphTarget.influence);
                    targetNames.Add(babylonMorphTarget.name);
                }
                gltfMesh.weights = weights.ToArray();

                if (gltfMesh.extras == null) {
                    gltfMesh.extras = new Dictionary<string, object>();
                }
                gltfMesh.extras["targetNames"] = targetNames.ToArray();
            }

            if (hasBones)
            {
                alreadyExportedSkinnedMeshes.Add(babylonMesh);
            }

            ExportGLTFExtension(babylonMesh, ref gltfMesh,gltf);

            return gltfMesh;
        }

        private void SetBabylonMaterial(BabylonMesh babylonMesh, BabylonSubMesh babylonSubMesh, GLTFMeshPrimitive meshPrimitive)
        {
            // Retreive the babylon material
            BabylonMaterial babylonMaterial;
            var babylonMaterialId = babylonMesh.materialId;
            // From multi materials first, if any
            // Loop recursively even though it shouldn't be a real use case
            var babylonMultiMaterials = new List<BabylonMultiMaterial>(babylonScene.multiMaterials);
            BabylonMultiMaterial babylonMultiMaterial;
            do
            {
                babylonMultiMaterial = babylonMultiMaterials.Find(_babylonMultiMaterial => _babylonMultiMaterial.id == babylonMaterialId);
                if (babylonMultiMaterial != null)
                {
                    babylonMaterialId = babylonMultiMaterial.materials[babylonSubMesh.materialIndex];
                }
            }
            while (babylonMultiMaterial != null);
            // Then from materials
            var babylonMaterials = new List<BabylonMaterial>(babylonScene.materials);
            babylonMaterial = babylonMaterials.Find(_babylonMaterial => _babylonMaterial.id == babylonMaterialId);

            meshPrimitive.mode = GLTFMeshPrimitive.FillMode.TRIANGLES;

            // If babylon material was exported successfully
            if (babylonMaterial != null)
            {
                // Update primitive material index
                var indexMaterial = babylonMaterialsToExport.FindIndex(_babylonMaterial => _babylonMaterial == babylonMaterial);
                if (indexMaterial == -1)
                {
                    // Store material for exportation
                    indexMaterial = babylonMaterialsToExport.Count;
                    babylonMaterialsToExport.Add(babylonMaterial);
                }
                meshPrimitive.material = indexMaterial;

                // TODO - Add and retreive info from babylon material
                if (babylonMaterial.wireframe)
                {
                    meshPrimitive.mode = GLTFMeshPrimitive.FillMode.LINE_STRIP;
                }
            }
        }
        private BabylonMorphTargetManager GetBabylonMorphTargetManager(BabylonScene babylonScene, BabylonMesh babylonMesh)
        {
            if (babylonMesh.morphTargetManagerId.HasValue)
            {
                if (babylonScene.morphTargetManagers == null)
                {
                    logger.RaiseWarning("GLTFExporter.Mesh | morphTargetManagers is not defined", 3);
                }
                else
                {
                    var babylonMorphTargetManager = babylonScene.morphTargetManagers.ElementAtOrDefault(babylonMesh.morphTargetManagerId.Value);

                    if (babylonMorphTargetManager == null)
                    {
                        logger.RaiseWarning($"GLTFExporter.Mesh | morphTargetManager with index {babylonMesh.morphTargetManagerId.Value} not found", 3);
                        return null;
                    }
                    
                    if (babylonMorphTargetManager.targets == null )
                    {
                        logger.RaiseWarning($"GLTFExporter.Mesh | morphTargetManager with index {babylonMesh.morphTargetManagerId.Value} empty", 3);
                        return null;
                    }

                    return babylonMorphTargetManager;
                }
            }
            return null;
        }

        private void _exportMorphTargets(BabylonMesh babylonMesh, BabylonSubMesh babylonSubMesh, BabylonMorphTargetManager babylonMorphTargetManager, GLTF gltf, GLTFBuffer buffer, GLTFMeshPrimitive meshPrimitive)
        {
            var morphTargets = babylonMorphTargetManager.targets;
            if (morphTargets != null)
            {
                var gltfMorphTargets = new List<GLTFMorphTarget>();
                foreach (var babylonMorphTarget in morphTargets)
                {
                    var gltfMorphTarget = new GLTFMorphTarget();

                    // Positions
                    if (babylonMorphTarget.positions != null)
                    {
                        var accessorTargetPositions = GLTFBufferService.Instance.CreateAccessor(
                            gltf,
                            GLTFBufferService.Instance.GetBufferViewFloatVec3(gltf, buffer),
                            "accessorTargetPositions",
                            GLTFAccessor.ComponentType.FLOAT,
                            GLTFAccessor.TypeEnum.VEC3
                        );
                        gltfMorphTarget.Add(GLTFMorphTarget.Attribute.POSITION.ToString(), accessorTargetPositions.index);
                        // Populate accessor
                        int nbComponents = 3; // Vector3
                        int startIndex = babylonSubMesh.verticesStart * nbComponents;
                        int endIndex = startIndex + babylonSubMesh.verticesCount * nbComponents;
                        accessorTargetPositions.min = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
                        accessorTargetPositions.max = new float[] { float.MinValue, float.MinValue, float.MinValue };
                        for (int indexPosition = startIndex; indexPosition < endIndex; indexPosition += 3)
                        {
                            var positionTarget = ArrayExtension.SubArray(babylonMorphTarget.positions, indexPosition, 3);

                            // Babylon stores morph target information as final data while glTF expects deltas from mesh primitive
                            var positionMesh = ArrayExtension.SubArray(babylonMesh.positions, indexPosition, 3);
                            for (int indexCoordinate = 0; indexCoordinate < positionTarget.Length; indexCoordinate++)
                            {
                                positionTarget[indexCoordinate] = positionTarget[indexCoordinate] - positionMesh[indexCoordinate];
                            }

                            positionTarget[2] *= -1;

                            // Store values as bytes
                            foreach (var coordinate in positionTarget)
                            {
                                accessorTargetPositions.bytesList.AddRange(BitConverter.GetBytes(coordinate));
                            }
                            // Update min and max values
                            GLTFBufferService.UpdateMinMaxAccessor(accessorTargetPositions, positionTarget);
                        }
                        accessorTargetPositions.count = babylonSubMesh.verticesCount;
                    }

                    // Normals
                    if (babylonMorphTarget.normals != null && exportParameters.exportMorphNormals)
                    {
                        var accessorTargetNormals = GLTFBufferService.Instance.CreateAccessor(
                            gltf,
                            GLTFBufferService.Instance.GetBufferViewFloatVec3(gltf, buffer),
                            "accessorTargetNormals",
                            GLTFAccessor.ComponentType.FLOAT,
                            GLTFAccessor.TypeEnum.VEC3
                        );
                        gltfMorphTarget.Add(GLTFMorphTarget.Attribute.NORMAL.ToString(), accessorTargetNormals.index);
                        // Populate accessor
                        int nbComponents = 3; // Vector3
                        int startIndex = babylonSubMesh.verticesStart * nbComponents;
                        int endIndex = startIndex + babylonSubMesh.verticesCount * nbComponents;
                        for (int indexNormal = startIndex; indexNormal < endIndex; indexNormal += 3)
                        {
                            var normalTarget = ArrayExtension.SubArray(babylonMorphTarget.normals, indexNormal, 3);

                            // Babylon stores morph target information as final data while glTF expects deltas from mesh primitive
                            var normalMesh = ArrayExtension.SubArray(babylonMesh.normals, indexNormal, 3);
                            for (int indexCoordinate = 0; indexCoordinate < normalTarget.Length; indexCoordinate++)
                            {
                                normalTarget[indexCoordinate] = normalTarget[indexCoordinate] - normalMesh[indexCoordinate];
                            }

                            normalTarget[2] *= -1;

                            // Store values as bytes
                            foreach (var coordinate in normalTarget)
                            {
                                accessorTargetNormals.bytesList.AddRange(BitConverter.GetBytes(coordinate));
                            }
                        }
                        accessorTargetNormals.count = babylonSubMesh.verticesCount;
                    }

                    // Tangents
                    if (babylonMorphTarget.tangents != null && exportParameters.exportTangents)
                    {
                        var accessorTargetTangents = GLTFBufferService.Instance.CreateAccessor(
                            gltf,
                            GLTFBufferService.Instance.GetBufferViewFloatVec3(gltf, buffer),
                            "accessorTargetTangents",
                            GLTFAccessor.ComponentType.FLOAT,
                            GLTFAccessor.TypeEnum.VEC3
                        );
                        gltfMorphTarget.Add(GLTFMeshPrimitive.Attribute.TANGENT.ToString(), accessorTargetTangents.index);
                        // Populate accessor
                        // Note that the w component for handedness is omitted when targeting TANGENT data since handedness cannot be displaced.
                        int nbComponents = 4; // Vector4
                        int startIndex = babylonSubMesh.verticesStart * nbComponents;
                        int endIndex = startIndex + babylonSubMesh.verticesCount * nbComponents;
                        for (int indexTangent = startIndex; indexTangent < endIndex; indexTangent += 4)
                        {
                            var tangentTarget = ArrayExtension.SubArray(babylonMorphTarget.tangents, indexTangent, 3);

                            // Babylon stores morph target information as final data while glTF expects deltas from mesh primitive
                            var tangentMesh = ArrayExtension.SubArray(babylonMesh.tangents, indexTangent, 3);
                            for (int indexCoordinate = 0; indexCoordinate < tangentTarget.Length; indexCoordinate++)
                            {
                                tangentTarget[indexCoordinate] = tangentTarget[indexCoordinate] - tangentMesh[indexCoordinate];
                            }

                            tangentTarget[2] *= -1;

                            // Store values as bytes
                            foreach (var coordinate in tangentTarget)
                            {
                                accessorTargetTangents.bytesList.AddRange(BitConverter.GetBytes(coordinate));
                            }
                        }
                        accessorTargetTangents.count = babylonSubMesh.verticesCount;
                    }

                    gltfMorphTargets.Add(gltfMorphTarget);
                }
                if (gltfMorphTargets.Count > 0)
                {
                    meshPrimitive.targets = gltfMorphTargets.ToArray();
                }
            }
        }
    }
}
