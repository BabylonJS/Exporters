using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaAnim;
using BabylonExport.Entities;
using MayaBabylon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        private MFnSkinCluster mFnSkinCluster;          // the skin cluster of the mesh/vertices
        private MFnTransform mFnTransform;              // the transform of the mesh
        private MStringArray allMayaInfluenceNames;     // the joint names that influence the mesh (joint with 0 weight included)
        private MDoubleArray allMayaInfluenceWeights;   // the joint weights for the vertex (0 weight included)
        private Dictionary<string, int> indexByNodeName = new Dictionary<string, int>();    // contains the node (joint and parents of the current skin) fullPathName and its index

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mDagPath">DAG path to the transform</param>
        /// <param name="babylonScene"></param>
        /// <returns></returns>
        private BabylonNode ExportDummy(MDagPath mDagPath, BabylonScene babylonScene)
        {
            RaiseMessage(mDagPath.partialPathName, 1);

            MFnTransform mFnTransform = new MFnTransform(mDagPath);

            Print(mFnTransform, 2, "Print ExportDummy mFnTransform");

            var babylonMesh = new BabylonMesh { name = mFnTransform.name, id = mFnTransform.uuid().asString() };
            babylonMesh.isDummy = true;

            // Position / rotation / scaling / hierarchy
            ExportNode(babylonMesh, mFnTransform, babylonScene);

            // Animations
            if (exportParameters.exportAnimations && MAnimUtil.isAnimated(mDagPath))
            {
                if (exportParameters.bakeAnimationFrames)
                {
                    ExportNodeAnimationFrameByFrame(babylonMesh, mFnTransform);
                }
                else
                {
                    ExportNodeAnimation(babylonMesh, mFnTransform);
                }
            }

            babylonScene.MeshesList.Add(babylonMesh);

            return babylonMesh;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mDagPath">DAG path to the transform above mesh</param>
        /// <param name="babylonScene"></param>
        /// <returns></returns>
        private BabylonNode ExportMesh(MDagPath mDagPath, BabylonScene babylonScene)
        {
            RaiseMessage(mDagPath.partialPathName, 1);

            // Transform above mesh
            mFnTransform = new MFnTransform(mDagPath);

            // Mesh direct child of the transform
            // TODO get the original one rather than the modified?
            MFnMesh mFnMesh = null;         // Shape of the mesh displayed by maya when the export begins. It contains the material, skin, blendShape information.
            MFnMesh meshShapeOrig = null;   // Original shape of the mesh
            for (uint i = 0; i < mFnTransform.childCount; i++)
            {
                MObject childObject = mFnTransform.child(i);
                if (childObject.apiType == MFn.Type.kMesh)
                {
                    var _mFnMesh = new MFnMesh(childObject);
                    if (!_mFnMesh.isIntermediateObject)
                    {
                        mFnMesh = _mFnMesh;
                    }
                    else
                    {
                        meshShapeOrig = _mFnMesh;
                    }
                }
            }

            bool hasMorphTarget = hasBlendShape(mFnMesh.objectProperty);
            if (!hasMorphTarget)
            {
                RaiseMessage("no morph targets", 2);
            }
            else
            {
                RaiseMessage("morph targets", 2);
            }

            if (mFnMesh == null)
            {
                RaiseError("No mesh found has child of " + mDagPath.fullPathName);
                return null;
            }

            RaiseMessage("mFnMesh.fullPathName=" + mFnMesh.fullPathName, 2);

            // --- prints ---
            #region prints

            Action<MFnDagNode> printMFnDagNode = (MFnDagNode mFnDagNode) =>
           {
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.name=" + mFnDagNode.name, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.absoluteName=" + mFnDagNode.absoluteName, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.fullPathName=" + mFnDagNode.fullPathName, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.partialPathName=" + mFnDagNode.partialPathName, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.activeColor=" + mFnDagNode.activeColor.toString(), 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.attributeCount=" + mFnDagNode.attributeCount, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.childCount=" + mFnDagNode.childCount, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.dormantColor=" + mFnDagNode.dormantColor, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.hasUniqueName=" + mFnDagNode.hasUniqueName, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.inUnderWorld=" + mFnDagNode.inUnderWorld, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.isDefaultNode=" + mFnDagNode.isDefaultNode, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.isInstanceable=" + mFnDagNode.isInstanceable, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.isInstanced(true)=" + mFnDagNode.isInstanced(true), 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.isInstanced(false)=" + mFnDagNode.isInstanced(false), 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.isInstanced()=" + mFnDagNode.isInstanced(), 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.instanceCount(true)=" + mFnDagNode.instanceCount(true), 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.instanceCount(false)=" + mFnDagNode.instanceCount(false), 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.isIntermediateObject=" + mFnDagNode.isIntermediateObject, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.isShared=" + mFnDagNode.isShared, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.objectColor=" + mFnDagNode.objectColor, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.parentCount=" + mFnDagNode.parentCount, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.parentNamespace=" + mFnDagNode.parentNamespace, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.uuid().asString()=" + mFnDagNode.uuid().asString(), 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.dagRoot().apiType=" + mFnDagNode.dagRoot().apiType, 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.model.equalEqual(mFnDagNode.objectProperty)=" + mFnDagNode.model.equalEqual(mFnDagNode.objectProperty), 3);
               RaiseVerbose("BabylonExporter.Mesh | mFnDagNode.transformationMatrix.toString()=" + mFnDagNode.transformationMatrix.toString(), 3);
           };

            Action<MFnMesh> printMFnMesh = (MFnMesh _mFnMesh) =>
            {
                printMFnDagNode(mFnMesh);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numVertices=" + _mFnMesh.numVertices, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numEdges=" + _mFnMesh.numEdges, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numPolygons=" + _mFnMesh.numPolygons, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numFaceVertices=" + _mFnMesh.numFaceVertices, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numNormals=" + _mFnMesh.numNormals, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numUVSets=" + _mFnMesh.numUVSets, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numUVsProperty=" + _mFnMesh.numUVsProperty, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.displayColors=" + _mFnMesh.displayColors, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numColorSets=" + _mFnMesh.numColorSets, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.numColorsProperty=" + _mFnMesh.numColorsProperty, 3);
                RaiseVerbose("BabylonExporter.Mesh | _mFnMesh.currentUVSetName()=" + _mFnMesh.currentUVSetName(), 3);

                var _uvSetNames = new MStringArray();
                mFnMesh.getUVSetNames(_uvSetNames);
                foreach (var uvSetName in _uvSetNames)
                {
                    RaiseVerbose("BabylonExporter.Mesh | uvSetName=" + uvSetName, 3);
                    RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numUVs(uvSetName)=" + mFnMesh.numUVs(uvSetName), 4);
                    MFloatArray us = new MFloatArray();
                    MFloatArray vs = new MFloatArray();
                    mFnMesh.getUVs(us, vs, uvSetName);
                    RaiseVerbose("BabylonExporter.Mesh | us.Count=" + us.Count, 4);
                }
            };

            Action<MFnTransform> printMFnTransform = (MFnTransform _mFnMesh) =>
            {
                printMFnDagNode(mFnMesh);
            };

            RaiseVerbose("BabylonExporter.Mesh | mFnMesh data", 2);
            printMFnMesh(mFnMesh);

            RaiseVerbose("BabylonExporter.Mesh | mFnTransform data", 2);
            printMFnTransform(mFnTransform);

            Print(mFnTransform, 2, "Print ExportMesh mFnTransform");

            Print(mFnMesh, 2, "Print ExportMesh mFnMesh");

            //// Geometry
            //MIntArray triangleCounts = new MIntArray();
            //MIntArray trianglesVertices = new MIntArray();
            //mFnMesh.getTriangles(triangleCounts, trianglesVertices);
            //RaiseVerbose("BabylonExporter.Mesh | triangleCounts.ToArray()=" + triangleCounts.ToArray().toString(), 3);
            //RaiseVerbose("BabylonExporter.Mesh | trianglesVertices.ToArray()=" + trianglesVertices.ToArray().toString(), 3);
            //int[] polygonsVertexCount = new int[mFnMesh.numPolygons];
            //for (int polygonId = 0; polygonId < mFnMesh.numPolygons; polygonId++)
            //{
            //    polygonsVertexCount[polygonId] = mFnMesh.polygonVertexCount(polygonId);
            //}
            //RaiseVerbose("BabylonExporter.Mesh | polygonsVertexCount=" + polygonsVertexCount.toString(), 3);

            ////MFloatPointArray points = new MFloatPointArray();
            ////mFnMesh.getPoints(points);
            ////RaiseVerbose("BabylonExporter.Mesh | points.ToArray()=" + points.ToArray().Select(mFloatPoint => mFloatPoint.toString()), 3);

            ////MFloatVectorArray normals = new MFloatVectorArray();
            ////mFnMesh.getNormals(normals);
            ////RaiseVerbose("BabylonExporter.Mesh | normals.ToArray()=" + normals.ToArray().Select(mFloatPoint => mFloatPoint.toString()), 3);

            //for (int polygonId = 0; polygonId < mFnMesh.numPolygons; polygonId++)
            //{
            //    MIntArray verticesId = new MIntArray();
            //    RaiseVerbose("BabylonExporter.Mesh | polygonId=" + polygonId, 3);

            //    int nbTriangles = triangleCounts[polygonId];
            //    RaiseVerbose("BabylonExporter.Mesh | nbTriangles=" + nbTriangles, 3);

            //    for (int triangleIndex = 0; triangleIndex < triangleCounts[polygonId]; triangleIndex++)
            //    {
            //        RaiseVerbose("BabylonExporter.Mesh | triangleIndex=" + triangleIndex, 3);
            //        int[] triangleVertices = new int[3];
            //        mFnMesh.getPolygonTriangleVertices(polygonId, triangleIndex, triangleVertices);
            //        RaiseVerbose("BabylonExporter.Mesh | triangleVertices=" + triangleVertices.toString(), 3);

            //        foreach (int vertexId in triangleVertices)
            //        {
            //            RaiseVerbose("BabylonExporter.Mesh | vertexId=" + vertexId, 3);
            //            MPoint point = new MPoint();
            //            mFnMesh.getPoint(vertexId, point);
            //            RaiseVerbose("BabylonExporter.Mesh | point=" + point.toString(), 3);

            //            MVector normal = new MVector();
            //            mFnMesh.getFaceVertexNormal(polygonId, vertexId, normal);
            //            RaiseVerbose("BabylonExporter.Mesh | normal=" + normal.toString(), 3);
            //        }
            //    }
            //}

            #endregion

            if (IsMeshExportable(mFnMesh, mDagPath) == false)
            {
                return null;
            }

            var babylonMesh = new BabylonMesh{
                                        name = mFnTransform.name,
                                        id = mFnTransform.uuid().asString(),
                                        visibility = Loader.GetVisibility(mFnTransform.fullPathName)
                                    };
            
            // Instance
            // For a mesh with instances, we distinguish between master and instance meshes:
            //      - a master mesh stores all the info of the mesh (transform, hierarchy, animations + vertices, indices, materials, bones...)
            //      - an instance mesh only stores the info of the node (transform, hierarchy, animations)

            // Check if this mesh has already been exported as a master mesh
            BabylonMesh babylonMasterMesh = GetMasterMesh(mFnMesh, babylonMesh);
            if (babylonMasterMesh != null)
            {
                RaiseMessage($"The master mesh {babylonMasterMesh.name} was already exported. This one will be exported as an instance.",2);

                // Export this node as instance
                var babylonInstanceMesh = new BabylonAbstractMesh { name = mFnTransform.name, id = mFnTransform.uuid().asString() };

                //// Add instance to master mesh
                List<BabylonAbstractMesh> instances = babylonMasterMesh.instances != null ? babylonMasterMesh.instances.ToList() : new List<BabylonAbstractMesh>();
                instances.Add(babylonInstanceMesh);
                babylonMasterMesh.instances = instances.ToArray();

                // Export transform / hierarchy / animations
                ExportNode(babylonInstanceMesh, mFnTransform, babylonScene);

                // Extra attributes
                babylonInstanceMesh.metadata = ExportCustomAttributeFromTransform(mFnTransform);

                // Animations
                if (exportParameters.exportAnimations && MAnimUtil.isAnimated(mDagPath))
                {
                    ExportNodeAnimation(babylonInstanceMesh, mFnTransform);
                }

                return babylonInstanceMesh;
            }

            babylonScene.MeshesList.Add(babylonMesh);

            // Position / rotation / scaling / hierarchy
            ExportNode(babylonMesh, mFnTransform, babylonScene);

            // Extra attributes
            babylonMesh.metadata = ExportCustomAttributeFromTransform(mFnTransform);

            if (mFnMesh.numPolygons < 1)
            {
                RaiseError($"Mesh {babylonMesh.name} has no face", 2);
            }

            if (mFnMesh.numVertices < 3)
            {
                RaiseError($"Mesh {babylonMesh.name} has not enough vertices", 2);
            }

            if (mFnMesh.numVertices >= 65536)
            {
                RaiseWarning($"Mesh {babylonMesh.name} has more than 65536 vertices which means that it will require specific WebGL extension to be rendered. This may impact portability of your scene on low end devices.", 2);
            }

            // skin
            if (exportParameters.exportSkins)
            {
                mFnSkinCluster = getMFnSkinCluster(mFnMesh);
            }
            int maxNbBones = 0;
            if (mFnSkinCluster != null)
            {
                RaiseMessage($"mFnSkinCluster.name | {mFnSkinCluster.name}", 2);
                Print(mFnSkinCluster, 3, $"Print {mFnSkinCluster.name}");

                // Get the bones dictionary<name, index> => it represents all the bones in the skeleton
                indexByNodeName = GetIndexByFullPathNameDictionary(mFnSkinCluster);

                // Get the joint names that influence this mesh
                allMayaInfluenceNames = GetBoneFullPathName(mFnSkinCluster, mFnTransform);

                // Get the max number of joints acting on a vertex
                int maxNumInfluences = GetMaxInfluence(mFnSkinCluster, mFnTransform, mFnMesh);

                RaiseMessage($"Max influences : {maxNumInfluences}", 2);
                if (maxNumInfluences > 8)
                {
                    RaiseWarning($"Too many bones influences per vertex: {maxNumInfluences}. Babylon.js only support up to 8 bones influences per vertex.", 2);
                    RaiseWarning("The result may not be as expected.", 2);
                }
                maxNbBones = Math.Min(maxNumInfluences, 8);

                if (indexByNodeName != null && allMayaInfluenceNames != null)
                {
                    int skeletonId = babylonMesh.skeletonId = GetSkeletonIndex(mFnSkinCluster);

                    // Bones with zero scale damage the mesh geometry export
                    // So you need to fid a frame were those bones have a higher scale
                    if (frameBySkeletonID.ContainsKey(skeletonId))
                    {
                        double frame = frameBySkeletonID[skeletonId];
                        RaiseVerbose($"Export the mesh at the same frame as its skeleton {frame}");
                    }
                    else
                    {
                        List<MObject> bones = GetRevelantNodes(mFnSkinCluster);
                        double currentFrame = Loader.GetCurrentTime();
                        frameBySkeletonID[skeletonId] = currentFrame;

                        if (HasNonZeroScale(bones, currentFrame))
                        {
                            RaiseVerbose($"Export the mesh at the current frame {currentFrame}", 2);
                        }
                        else
                        {   // There is at least one bone in the skeleton that has a zero scale
                            IList<double> validFrames = GetValidFrames(mFnSkinCluster);
                            if (validFrames.Count > 0)
                            {
                                RaiseVerbose($"Export the mesh at the frame {validFrames[0]}", 2);
                                Loader.SetCurrentTime(validFrames[0]);
                                frameBySkeletonID[skeletonId] = validFrames[0];
                            }
                            else
                            {
                                RaiseError($"No valid frame found for the mesh export. The bone scales are too close to zero.", 2);
                            }
                        }
                    }
                }
                else
                {
                    mFnSkinCluster = null;
                }
            }

            // Animations
            if (exportParameters.exportAnimations && MAnimUtil.isAnimated(mDagPath))
            {
                if (exportParameters.bakeAnimationFrames)
                {
                    ExportNodeAnimationFrameByFrame(babylonMesh, mFnTransform);
                }
                else
                {
                    ExportNodeAnimation(babylonMesh, mFnTransform);
                }
            }
            if (exportParameters.exportAnimationsOnly)
            {
                // Skip material and geometry export
                RaiseMessage("BabylonExporter.Mesh | done", 2);
                return babylonMesh;
            }

            // Material
            MObjectArray shaders = new MObjectArray();
            mFnMesh.getConnectedShaders(0, shaders, new MIntArray());

            bool isDoubleSided = false;

            if (shaders.Count > 0)
            {
                List<MFnDependencyNode> materials = new List<MFnDependencyNode>();
                foreach (MObject shader in shaders)
                {
                    // Retreive material
                    MFnDependencyNode shadingEngine = new MFnDependencyNode(shader);
                    MPlug mPlugSurfaceShader = shadingEngine.findPlug("surfaceShader");
                    MObject materialObject = mPlugSurfaceShader.source.node;
                    if (materialObject.hasFn(MFn.Type.kSurfaceShader))
                    {
                        isDoubleSided = true;
                    }
                    MFnDependencyNode material = new MFnDependencyNode(materialObject);

                    materials.Add(material);
                }

                if (shaders.Count == 1 && !isDoubleSided)
                {
                    MFnDependencyNode material = materials[0];

                    // Material is referenced by id
                    babylonMesh.materialId = material.uuid().asString();

                    // Register material for export if not already done
                    if (!referencedMaterials.Contains(material, new MFnDependencyNodeEqualityComparer()))
                    {
                        referencedMaterials.Add(material);
                    }
                }
                else if (isDoubleSided)
                {
                    // Get the UUID of the SufaceShader node
                    string uuidMultiMaterial = materials[0].uuid().asString();

                    // DoubleSided material is referenced by id
                    babylonMesh.materialId = uuidMultiMaterial;

                    // Get the textures from the double sided node
                    MPlug connectionOutColor = materials[0].getConnection("outColor");

                    MObject destinationObject = connectionOutColor.source.node;

                    MFnDependencyNode babylonAttributesDependencyNode = new MFnDependencyNode(destinationObject);

                    MPlug connectionMat01 = babylonAttributesDependencyNode.getConnection("colorIfTrue");
                    MPlug connectionMat02 = babylonAttributesDependencyNode.getConnection("colorIfFalse");
                    MPlug connectionFirstTerm = babylonAttributesDependencyNode.getConnection("firstTerm");

                    MObject babylonFirstMaterialSourceNode = new MObject();
                    MObject babylonSecondMaterialSourceNode = new MObject();

                    if (connectionMat01 != null && connectionMat02 != null && connectionFirstTerm.name.Contains("firstTerm"))
                    {
                        babylonFirstMaterialSourceNode = connectionMat01.source.node;
                        babylonSecondMaterialSourceNode = connectionMat02.source.node;

                        MFnDependencyNode babylonFirstMaterial = new MFnDependencyNode(babylonFirstMaterialSourceNode);
                        MFnDependencyNode babylonSecondMaterial = new MFnDependencyNode(babylonSecondMaterialSourceNode);

                        List<MFnDependencyNode> materialsFromDoubleSided = new List<MFnDependencyNode>();
                        materialsFromDoubleSided.Add(babylonFirstMaterial);
                        materialsFromDoubleSided.Add(babylonSecondMaterial);

                        // Register double sided as multi material for export if not already done
                        if (!multiMaterials.ContainsKey(uuidMultiMaterial))
                        {
                            multiMaterials.Add(uuidMultiMaterial, materialsFromDoubleSided);
                        }
                    }
                    else
                    {
                        isDoubleSided = false;
                        RaiseWarning("This material is not supported it will not be exported:'" + materials[0].name + "'", 2);
                    }
                }
                else
                {
                    // Create a new id for the group of sub materials
                    string uuidMultiMaterial = GetMultimaterialUUID(materials);

                    // Multi material is referenced by id
                    babylonMesh.materialId = uuidMultiMaterial;

                    // Register multi material for export if not already done
                    if (!multiMaterials.ContainsKey(uuidMultiMaterial))
                    {
                        multiMaterials.Add(uuidMultiMaterial, materials);
                    }
                }
            }

            var vertices = new List<GlobalVertex>();
            var indices = new List<int>();

            var uvSetNames = new MStringArray();
            mFnMesh.getUVSetNames(uvSetNames);
            bool[] isUVExportSuccess = new bool[Math.Min(uvSetNames.Count, 2)];
            for (int indexUVSet = 0; indexUVSet < isUVExportSuccess.Length; indexUVSet++)
            {
                isUVExportSuccess[indexUVSet] = true;
            }

            // Export tangents if option is checked and mesh have tangents
            bool isTangentExportSuccess = exportParameters.exportTangents;

            // TODO - color, alpha
            //var hasColor = unskinnedMesh.NumberOfColorVerts > 0;
            //var hasAlpha = unskinnedMesh.GetNumberOfMapVerts(-2) > 0;

            //var optimizeVertices = meshNode.MaxNode.GetBoolProperty("babylonjsexportParameters.optimizeVertices");
            var optimizeVertices = exportParameters.optimizeVertices; // global option

            // Compute normals
            var subMeshes = new List<BabylonSubMesh>();

            if (hasMorphTarget) // Remove the morph influence to extract the original geometry
            {
                setEnvelopesToZeros(mFnMesh.objectProperty);
            }

            ExtractGeometry(babylonMesh, mFnMesh, vertices, indices, subMeshes, uvSetNames, ref isUVExportSuccess, ref isTangentExportSuccess, optimizeVertices, isDoubleSided);

            if (vertices.Count >= 65536)
            {
                RaiseWarning($"Mesh {babylonMesh.name} has {vertices.Count} vertices. This may prevent your scene to work on low end devices where 32 bits indice are not supported", 2);

                if (!optimizeVertices)
                {
                    RaiseError("You can try to optimize your object using [Try to optimize vertices] option", 2);
                }
            }

            for (int indexUVSet = 0; indexUVSet < isUVExportSuccess.Length; indexUVSet++)
            {
                string uvSetName = uvSetNames[indexUVSet];
                // If at least one vertex is mapped to an UV coordinate but some have failed to be exported
                if (isUVExportSuccess[indexUVSet] == false && mFnMesh.numUVs(uvSetName) > 0)
                {
                    RaiseWarning($"Failed to export UV set named {uvSetName}. Ensure all vertices are mapped to a UV coordinate.", 2);
                }
            }

            RaiseMessage($"{vertices.Count} vertices, {indices.Count / 3} faces", 2);

            // Buffers
            babylonMesh.positions = vertices.SelectMany(v => v.Position).ToArray();
            babylonMesh.normals = vertices.SelectMany(v => v.Normal).ToArray();

            // Check that the positions of the vertices are different, otherwise raise a warning
            float[] firstPosition = vertices[0].Position;
            bool allEqual = vertices.All(v => v.Position.IsEqualTo(firstPosition, 0.001f));
            if (allEqual)
            {
                RaiseWarning("All the vertices share the same position. Is the mesh invisible? The result may not be as expected.", 2);
            }

            // export the skin
            if (mFnSkinCluster != null)
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

            // Tangent
            if (isTangentExportSuccess)
            {
                babylonMesh.tangents = vertices.SelectMany(v => v.Tangent).ToArray();
            }
            // Color
            string colorSetName;
            mFnMesh.getCurrentColorSetName(out colorSetName);
            if (mFnMesh.numColors(colorSetName) > 0) {
                babylonMesh.colors = vertices.SelectMany(v => v.Color).ToArray();
            }
            // UVs
            if (uvSetNames.Count > 0 && isUVExportSuccess[0])
            {
                
                babylonMesh.uvs = vertices.SelectMany(v => v.UV).ToArray();
            }
            if (uvSetNames.Count > 1 && isUVExportSuccess[1])
            {
                babylonMesh.uvs2 = vertices.SelectMany(v => v.UV2).ToArray();
            }

            babylonMesh.subMeshes = subMeshes.ToArray();

            // Buffers - Indices
            babylonMesh.indices = indices.ToArray();


            // ------------------------
            // ---- Morph targets -----
            // ------------------------
            if (hasMorphTarget)
            {
                restoreEnvelopes(mFnMesh.objectProperty);
                
                // Maya blend shape influencing the mesh
                RaiseMessage("Morph target", 2);
                IList<MFnBlendShapeDeformer> blendShapeDeformers = GetBlendShape(mFnMesh.objectProperty);

                if (exportParameters.exportSkins && mFnSkinCluster != null)
                {
                    RaiseWarning("A mesh with both skinning and morph target is not fully supported. Please set the playhead at the frame you want to choose as the bind pose before exporting.", 3);
                }

                if(blendShapeDeformers.Count > 1)
                {
                    RaiseWarning($"There are {blendShapeDeformers.Count} blend shapes. The exporter currently support one. So all blend shapes will be exported as one.", 3);
                }

                // Morph Target Manager
                BabylonMorphTargetManager babylonMorphTargetManager = new BabylonMorphTargetManager(babylonMesh);
                babylonScene.MorphTargetManagersList.Add(babylonMorphTargetManager);
                babylonMesh.morphTargetManagerId = babylonMorphTargetManager.id;

                IList<BabylonMorphTarget> babylonMorphTargets = GetMorphTargets(babylonMesh, mFnMesh);
                babylonMorphTargetManager.targets = babylonMorphTargets.ToArray();

                if (babylonMorphTargets.Count > 5)
                {
                    RaiseWarning($"There are {babylonMorphTargets.Count} morph targets.", 3);
                    RaiseWarning($"Please be aware that most of the browsers are limited to 16 attributes per mesh. Adding a single morph target to a mesh can add up to 3 new attributes (position + normal + tangent). You can export less attributs by modifying the MorphTarget options.", 3);
                }
            }

            RaiseMessage("BabylonExporter.Mesh | done", 2);

            return babylonMesh;
        }

        /// <summary>
        /// Extract ordered indices on a triangle basis
        /// Extract position and normal of each vertex per face
        /// </summary>
        /// <param name="mFnMesh"></param>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="subMeshes"></param>
        /// <param name="uvSetNames"></param>
        /// <param name="isUVExportSuccess"></param>
        /// <param name="optimizeVertices"></param>
        private void ExtractGeometry(BabylonMesh babylonMesh, MFnMesh mFnMesh, List<GlobalVertex> vertices, List<int> indices, List<BabylonSubMesh> subMeshes, MStringArray uvSetNames, ref bool[] isUVExportSuccess, ref bool isTangentExportSuccess, bool optimizeVertices, bool isDoubleSided)
        {
            Dictionary<GlobalVertex, List<GlobalVertex>> verticesAlreadyExported = null;

            if (optimizeVertices)
            {
                verticesAlreadyExported = new Dictionary<GlobalVertex, List<GlobalVertex>>();
            }

            MIntArray triangleCounts = new MIntArray();
            MIntArray trianglesVertices = new MIntArray();
            mFnMesh.getTriangles(triangleCounts, trianglesVertices);
            
            MObjectArray shaders = new MObjectArray();
            MIntArray faceMatIndices = new MIntArray(); // given a face index => get a shader index
            mFnMesh.getConnectedShaders(0, shaders, faceMatIndices);

            // Export geometry even if an error occured with shaders
            int nbShaders = Math.Max(1, shaders.Count);
            bool checkShader = nbShaders == shaders.Count;
            RaiseVerbose("shaders.Count=" + shaders.Count, 2);

            // For each material of this mesh
            for (int indexShader = 0; indexShader < nbShaders; indexShader++)
            {
                var nbIndicesSubMesh = 0;
                var minVertexIndexSubMesh = int.MaxValue;
                var maxVertexIndexSubMesh = int.MinValue;
                var subMesh = new BabylonSubMesh { indexStart = indices.Count, materialIndex = indexShader };

                // For each polygon of this mesh
                for (int polygonId = 0; polygonId < faceMatIndices.Count; polygonId++)
                {
                    if (checkShader && faceMatIndices[polygonId] != indexShader)
                    {
                        continue;
                    }

                    // The object-relative (mesh-relative/global) vertex indices for this face
                    MIntArray polygonVertices = new MIntArray();
                    mFnMesh.getPolygonVertices(polygonId, polygonVertices);

                    // For each triangle of this polygon
                    for (int triangleId = 0; triangleId < triangleCounts[polygonId]; triangleId++)
                    {
                        int[] polygonTriangleVertices = new int[3];
                        mFnMesh.getPolygonTriangleVertices(polygonId, triangleId, polygonTriangleVertices);

                        /*
                         * Switch coordinate system at global level
                         * 
                         * Piece of code kept just in case
                         * See BabylonExporter for more information
                         */
                        //// Inverse winding order to flip faces
                        //var tmp = triangleVertices[1];
                        //triangleVertices[1] = triangleVertices[2];
                        //triangleVertices[2] = tmp;

                        // For each vertex of this triangle (3 vertices per triangle)
                        foreach (int vertexIndexGlobal in polygonTriangleVertices)
                        {
                            // Get the face-relative (local) vertex id
                            int vertexIndexLocal = 0;
                            for (vertexIndexLocal = 0; vertexIndexLocal < polygonVertices.Count - 1; vertexIndexLocal++) // -1 to stop at vertexIndexLocal=2
                            {
                                if (polygonVertices[vertexIndexLocal] == vertexIndexGlobal)
                                {
                                    break;
                                }
                            }

                            GlobalVertex vertex = ExtractVertex(mFnMesh, polygonId, vertexIndexGlobal, vertexIndexLocal, uvSetNames, ref isUVExportSuccess, ref isTangentExportSuccess);

                            // Optimize vertices
                            if (verticesAlreadyExported != null)
                            {
                                // If a stored vertex is similar to current vertex
                                if (verticesAlreadyExported.ContainsKey(vertex))
                                {
                                    // Use stored vertex instead of current one
                                    verticesAlreadyExported[vertex].Add(vertex);
                                    vertex = verticesAlreadyExported[vertex].ElementAt(0);
                                }
                                else
                                {
                                    // add the stored vertex
                                    verticesAlreadyExported[vertex] = new List<GlobalVertex>();
                                    var modifiedVertex = new GlobalVertex(vertex);
                                    modifiedVertex.CurrentIndex = vertices.Count;
                                    verticesAlreadyExported[vertex].Add(modifiedVertex);
                                    vertex = modifiedVertex;
                                    vertices.Add(vertex);
                                    
                                    // Store vertex data
                                    babylonMesh.VertexDatas.Add(new VertexData(polygonId, vertexIndexGlobal, vertexIndexLocal));
                                }
                            }
                            else
                            {
                                vertex.CurrentIndex = vertices.Count;
                                vertices.Add(vertex);

                                // Store vertex data
                                babylonMesh.VertexDatas.Add(new VertexData(polygonId, vertexIndexGlobal, vertexIndexLocal));
                            }

                            indices.Add(vertex.CurrentIndex);

                            minVertexIndexSubMesh = Math.Min(minVertexIndexSubMesh, vertex.CurrentIndex);
                            maxVertexIndexSubMesh = Math.Max(maxVertexIndexSubMesh, vertex.CurrentIndex);
                            nbIndicesSubMesh++;
                        }
                    }
                }

                if (nbIndicesSubMesh != 0)
                {
                    subMesh.indexCount = nbIndicesSubMesh;
                    subMesh.verticesStart = minVertexIndexSubMesh;
                    subMesh.verticesCount = maxVertexIndexSubMesh - minVertexIndexSubMesh + 1;

                    subMeshes.Add(subMesh);
                }
            }

            if (isDoubleSided)
            {
                List<GlobalVertex> tempVertices = new List<GlobalVertex>();

                int positionsCount = 0;

                for (var i = 0; i < vertices.Count; i++)
                {
                    GlobalVertex newVertex = vertices[i];
                    newVertex.Normal = new float[vertices[i].Normal.Length];
                    newVertex.Position = new float[vertices[i].Position.Length];
                    for (var j = 0; j < vertices[i].Normal.Length; j++)
                    {
                        newVertex.Normal[j] = -vertices[i].Normal[j];
                    }

                    for (var j = 0; j < vertices[i].Position.Length; j++)
                    {
                        newVertex.Position[j] = vertices[i].Position[j];
                    }

                    positionsCount++;

                    tempVertices.Add(newVertex);
                }
                vertices.AddRange(tempVertices);

                int indicesCount = indices.Count;
                indices.AddRange(indices);

                for (var i = 0; i < indicesCount; i += 3)
                {
                    indices[i + indicesCount] = indices[i + 2] + positionsCount;
                    indices[i + 1 + indicesCount] = indices[i + 1] + positionsCount;
                    indices[i + 2 + indicesCount] = indices[i] + positionsCount;
                }

                var subMesh = new BabylonSubMesh { indexStart = indices.Count/2, materialIndex = 1};

                subMesh.indexCount = indicesCount;
                subMesh.verticesStart = vertices.Count/2;
                subMesh.verticesCount = vertices.Count/2;

                subMeshes.Add(subMesh);
            }

        }

        /// <summary>
        /// Extract geometry (position, normal, UVs...) for a specific vertex
        /// </summary>
        /// <param name="mFnMesh"></param>
        /// <param name="polygonId">The polygon (face) to examine</param>
        /// <param name="vertexIndexGlobal">The object-relative (mesh-relative/global) vertex index</param>
        /// <param name="vertexIndexLocal">The face-relative (local) vertex id to examine</param>
        /// <param name="uvSetNames"></param>
        /// <param name="isUVExportSuccess"></param>
        /// <returns></returns>
        private GlobalVertex ExtractVertex(MFnMesh mFnMesh, int polygonId, int vertexIndexGlobal, int vertexIndexLocal, MStringArray uvSetNames, ref bool[] isUVExportSuccess, ref bool isTangentExportSuccess)
        {
            MPoint point = new MPoint();
            mFnMesh.getPoint(vertexIndexGlobal, point);

            MVector normal = new MVector();
            mFnMesh.getFaceVertexNormal(polygonId, vertexIndexGlobal, normal);

            // Switch coordinate system at object level
            point.z *= -1;
            normal.z *= -1;

            // Apply unit conversion factor to meter
            point.x *= scaleFactorToMeters;
            point.y *= scaleFactorToMeters;
            point.z *= scaleFactorToMeters;

            var vertex = new GlobalVertex
            {
                BaseIndex = vertexIndexGlobal,
                Position = point.toArray(),
                Normal = normal.toArray()
            };

            // Tangent
            if (isTangentExportSuccess)
            {
                try
                {
                    MVector tangent = new MVector();
                    mFnMesh.getFaceVertexTangent(polygonId, vertexIndexGlobal, tangent);

                    // Switch coordinate system at object level
                    tangent.z *= -1;

                    int tangentId = mFnMesh.getTangentId(polygonId, vertexIndexGlobal);
                    bool isRightHandedTangent = mFnMesh.isRightHandedTangent(tangentId);

                    // Invert W to switch to left handed system
                    vertex.Tangent = new float[] { (float)tangent.x, (float)tangent.y, (float)tangent.z, isRightHandedTangent ? -1 : 1 };
                }
                catch
                {
                    // Exception raised when mesh don't have tangents
                    isTangentExportSuccess = false;
                }
            }

            // Color
            int colorIndex;
            string colorSetName;
            float[] defaultColor = new float[] { 1, 1, 1, 0 };
            MColor color = new MColor();
            mFnMesh.getCurrentColorSetName(out colorSetName);

            if (mFnMesh.numColors(colorSetName) > 0)
            {
                //Get the color index
                mFnMesh.getColorIndex(polygonId, vertexIndexLocal, out colorIndex);
                
                //if a color is set
                if (colorIndex != -1)
                {
                    mFnMesh.getColor(colorIndex, color);
                    vertex.Color = color.toArray();
                }
                //else set the default color
                else
                {
                    vertex.Color = defaultColor;
                }
            }

            // UV
            int indexUVSet = 0;
            if (uvSetNames.Count > indexUVSet && isUVExportSuccess[indexUVSet])
            {
                try
                {
                    float u = 0, v = 0;
                    mFnMesh.getPolygonUV(polygonId, vertexIndexLocal, ref u, ref v, uvSetNames[indexUVSet]);
                    vertex.UV = new float[] { u, v };
                }
                catch
                {
                    // An exception is raised when a vertex isn't mapped to an UV coordinate
                    isUVExportSuccess[indexUVSet] = false;
                }
            }
            indexUVSet = 1;
            if (uvSetNames.Count > indexUVSet && isUVExportSuccess[indexUVSet])
            {
                try
                {
                    float u = 0, v = 0;
                    mFnMesh.getPolygonUV(polygonId, vertexIndexLocal, ref u, ref v, uvSetNames[indexUVSet]);
                    vertex.UV2 = new float[] { u, v };
                }
                catch
                {
                    // An exception is raised when a vertex isn't mapped to an UV coordinate
                    isUVExportSuccess[indexUVSet] = false;
                }
            }

            // skin
            if (mFnSkinCluster != null)
            {
                // Filter null weights
                Dictionary<int, double> weightByInfluenceIndex = new Dictionary<int, double>(); // contains the influence indices with weight > 0

                // Export Weights and BonesIndices for the vertex
                // Get the weight values of all the influences for this vertex
                allMayaInfluenceWeights = new MDoubleArray();
                MGlobal.executeCommand($"skinPercent -query -value {mFnSkinCluster.name} {mFnTransform.name}.vtx[{vertexIndexGlobal}]",
                                        allMayaInfluenceWeights);
                allMayaInfluenceWeights.get(out double[] allInfluenceWeights);

                for (int influenceIndex = 0; influenceIndex < allInfluenceWeights.Length; influenceIndex++)
                {
                    double weight = allInfluenceWeights[influenceIndex];

                    if (weight > 0)
                    {
                        try
                        {
                            // add indice and weight in the local lists
                            weightByInfluenceIndex.Add(indexByNodeName[allMayaInfluenceNames[influenceIndex]], weight);
                        }
                        catch (Exception e)
                        {
                            RaiseError(e.Message, 2);
                            RaiseError(e.StackTrace, 3);
                        }
                    }
                }

                // normalize weights => Sum weights == 1
                Normalize(ref weightByInfluenceIndex);

                // decreasing sort
                OrderByDescending(ref weightByInfluenceIndex);

                int bonesCount = indexByNodeName.Count; // number total of bones/influences for the mesh
                float weight0 = 0;
                float weight1 = 0;
                float weight2 = 0;
                float weight3 = 0;
                int bone0 = 0;
                int bone1 = 0;
                int bone2 = 0;
                int bone3 = 0;
                int nbBones = weightByInfluenceIndex.Count; // number of bones/influences for this vertex

                if (nbBones == 0)
                {
                    weight0 = 1.0f;
                    bone0 = bonesCount;
                }

                if (nbBones > 0)
                {
                    bone0 = weightByInfluenceIndex.ElementAt(0).Key;
                    weight0 = (float)weightByInfluenceIndex.ElementAt(0).Value;

                    if (nbBones > 1)
                    {
                        bone1 = weightByInfluenceIndex.ElementAt(1).Key;
                        weight1 = (float)weightByInfluenceIndex.ElementAt(1).Value;

                        if (nbBones > 2)
                        {
                            bone2 = weightByInfluenceIndex.ElementAt(2).Key;
                            weight2 = (float)weightByInfluenceIndex.ElementAt(2).Value;

                            if (nbBones > 3)
                            {
                                bone3 = weightByInfluenceIndex.ElementAt(3).Key;
                                weight3 = (float)weightByInfluenceIndex.ElementAt(3).Value;
                            }
                        }
                    }
                }

                float[] weights = { weight0, weight1, weight2, weight3 };
                vertex.Weights = weights;
                vertex.BonesIndices = (bone3 << 24) | (bone2 << 16) | (bone1 << 8) | bone0;

                if (nbBones > 4)
                {
                    bone0 = weightByInfluenceIndex.ElementAt(4).Key;
                    weight0 = (float)weightByInfluenceIndex.ElementAt(4).Value;
                    weight1 = 0;
                    weight2 = 0;
                    weight3 = 0;

                    if (nbBones > 5)
                    {
                        bone1 = weightByInfluenceIndex.ElementAt(5).Key;
                        weight1 = (float)weightByInfluenceIndex.ElementAt(4).Value;

                        if (nbBones > 6)
                        {
                            bone2 = weightByInfluenceIndex.ElementAt(6).Key;
                            weight2 = (float)weightByInfluenceIndex.ElementAt(4).Value;

                            if (nbBones > 7)
                            {
                                bone3 = weightByInfluenceIndex.ElementAt(7).Key;
                                weight3 = (float)weightByInfluenceIndex.ElementAt(7).Value;
                            }
                        }
                    }

                    float[] weightsExtra = { weight0, weight1, weight2, weight3 };
                    vertex.WeightsExtra = weightsExtra;
                    vertex.BonesIndicesExtra = (bone3 << 24) | (bone2 << 16) | (bone1 << 8) | bone0;
                }
            }
            return vertex;
        }
        
        private void ExportNode(BabylonAbstractMesh babylonAbstractMesh, MFnTransform mFnTransform, BabylonScene babylonScene)
        {
            RaiseVerbose("BabylonExporter.Mesh | ExportNode", 2);

            // Position / rotation / scaling
            ExportTransform(babylonAbstractMesh, mFnTransform);

            // Hierarchy
            ExportHierarchy(babylonAbstractMesh, mFnTransform);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mFnDagNode">DAG function set of the node (mesh) below the transform</param>
        /// <param name="mDagPath">DAG path of the transform above the node</param>
        /// <returns></returns>
        private bool IsMeshExportable(MFnDagNode mFnDagNode, MDagPath mDagPath)
        {
            return IsNodeExportable(mFnDagNode, mDagPath);
        }

        private MFnSkinCluster getMFnSkinCluster(MFnMesh mFnMesh)
        {
            MFnSkinCluster mFnSkinCluster = null;

            MPlugArray connections = new MPlugArray();
            mFnMesh.getConnections(connections);
            foreach (MPlug connection in connections)
            {
                MObject source = connection.source.node;
                if (source != null)
                {
                    if (source.hasFn(MFn.Type.kSkinClusterFilter))
                    {
                        mFnSkinCluster = new MFnSkinCluster(source);
                    }

                    if ((mFnSkinCluster == null) && (source.hasFn(MFn.Type.kSet) || source.hasFn(MFn.Type.kPolyNormalPerVertex)))
                    {
                        mFnSkinCluster = getMFnSkinCluster(source);
                    }
                }
            }

            return mFnSkinCluster;
        }

        private MFnSkinCluster getMFnSkinCluster(MObject mObject)
        {
            MFnSkinCluster mFnSkinCluster = null;

            MFnDependencyNode mFnDependencyNode = new MFnDependencyNode(mObject);
            MPlugArray connections = new MPlugArray();
            mFnDependencyNode.getConnections(connections);
            for (int index = 0; index < connections.Count && mFnSkinCluster == null; index++)
            {
                MObject source = connections[index].source.node;
                if (source != null && source.hasFn(MFn.Type.kSkinClusterFilter))
                {
                    mFnSkinCluster = new MFnSkinCluster(source);
                }
            }

            return mFnSkinCluster;
        }

        /// <summary>
        /// Instances manager
        /// </summary>
        private List<MFnMesh> exportedMFnMesh = new List<MFnMesh>();
        private List<BabylonMesh> exportedMasterBabylonMesh = new List<BabylonMesh>();
        private BabylonMesh GetMasterMesh(MFnMesh mFnMesh, BabylonMesh babylonMesh)
        {
            BabylonMesh babylonMasterMesh = null;
            int index = exportedMFnMesh.FindIndex(mesh => mesh.fullPathName.Equals(mFnMesh.fullPathName));

            if(index == -1)
            {
                exportedMFnMesh.Add(mFnMesh);
                exportedMasterBabylonMesh.Add(babylonMesh);
            }
            else
            {
                babylonMasterMesh = exportedMasterBabylonMesh[index];
            }

            return babylonMasterMesh;
        }

        /// <summary>
        /// Check if a Maya object is link to a blend shape by counting its connections to it. 
        /// </summary>
        /// <param name="mObject"></param>
        /// <returns>
        /// True, if there at least one connection to a blend shape
        /// False, otherwise
        /// </returns>
        private bool hasBlendShape(MObject mObject)
        {
            IList<MFnBlendShapeDeformer> blendShapeDeformers = GetBlendShape(mObject);
            return blendShapeDeformers.Count > 0;
        }

        /// <summary>
        /// Search the blend shapes through the connections of the Maya object
        /// </summary>
        /// <param name="mObject"></param>
        /// <returns>A list with all blend shape linked to the object</returns>
        private IDictionary<MObject, IList<MFnBlendShapeDeformer>> blendShapeByMObject = new Dictionary<MObject, IList<MFnBlendShapeDeformer>>();
        private IList<MFnBlendShapeDeformer> GetBlendShape(MObject mObject)
        {
            var pair =  blendShapeByMObject.FirstOrDefault(item => item.Key.equalEqual(mObject));
            if (! pair.Equals(default(KeyValuePair<MObject, IList<MFnBlendShapeDeformer>>)))
            {
                return pair.Value;
            }

            IList<MFnBlendShapeDeformer> blendShapeDeformers = GetBlendShapeSub(mObject);
            
            // uniqueness
            IList <MFnBlendShapeDeformer> uniqBlendShapeDeformers = new List<MFnBlendShapeDeformer>();
            for (int index = 0; index < blendShapeDeformers.Count; index++)
            {
                MFnBlendShapeDeformer blendShapeDeformer = blendShapeDeformers[index];

                if (uniqBlendShapeDeformers.Count(item => item.name.Equals(blendShapeDeformer.name)) == 0)
                {
                    uniqBlendShapeDeformers.Add(blendShapeDeformer);
                }
            }

            blendShapeByMObject[mObject] = uniqBlendShapeDeformers;
            return uniqBlendShapeDeformers;
        }

        private IList<MFnBlendShapeDeformer> GetBlendShapeSub(MObject mObject)
        {
            List<MFnBlendShapeDeformer> blendShapeDeformers = new List<MFnBlendShapeDeformer>();

            MFnDependencyNode dependencyNode = new MFnDependencyNode(mObject);
            MPlugArray connections = new MPlugArray();
            dependencyNode.getConnections(connections);
            foreach (MPlug connection in connections)
            {
                MObject source = connection.source.node;
                if (source != null)
                {
                    if (source.hasFn(MFn.Type.kSet))
                    {
                        blendShapeDeformers.AddRange(GetBlendShapeSub(source));
                    }

                    if (source.hasFn(MFn.Type.kBlendShape))
                    {
                        MFnBlendShapeDeformer blendShapeDeformer = new MFnBlendShapeDeformer(source);
                        blendShapeDeformers.Add(blendShapeDeformer);
                    }
                }
            }

            return blendShapeDeformers;
        }

        /// <summary>
        /// Convert a Maya blendShape influencing a Maya object into a BabylonMorphTarget list
        /// </summary>
        /// <param name="baseObject">The Maya object influenced by the blendShapes</param>
        /// <param name="blendShapeDeformers">List of Maya blendShape. Use GetBlendShape function to get the right one.</param>
        /// <returns>BabylonMorphTarget list</returns>
        private IList<BabylonMorphTarget> GetMorphTargets(BabylonMesh babylonMesh, MFnMesh mesh)
        {
            // Morph Targets
            IList<MFnBlendShapeDeformer> blendShapeDeformers = GetBlendShape(mesh.objectProperty);
            IList <BabylonMorphTarget> babylonMorphTargets = new List<BabylonMorphTarget>();

            for (int index = 0; index < blendShapeDeformers.Count; index++)
            {
                MFnBlendShapeDeformer blendShapeDeformer = blendShapeDeformers[index];

                float envelope = blendShapeDeformer.envelope;

                MIntArray weightIndexList = new MIntArray();    // list of weight. For each weight, there are multiple targets
                blendShapeDeformer.weightIndexList(weightIndexList);

                MPlug plugWeight = blendShapeDeformer.findPlug("weight");

                for (int i = 0; i < weightIndexList.Count; i++)
                {
                    int weightIndex = weightIndexList[i];
                    float weight = blendShapeDeformer.weight((uint)weightIndex);

                    MPlug plugCurrentWeight = plugWeight.elementByLogicalIndex((uint)weightIndex);
                    string currentWeightName = blendShapeDeformer.plugsAlias(plugCurrentWeight);

                    MObjectArray targets = new MObjectArray();  // the targets for the given weight
                    try
                    {
                        blendShapeDeformer.getTargets(mesh.objectProperty, weightIndex, targets);
                        // reach here it's mean the target are still present into the scene. However, it's common that
                        // the target has been removed and the deformation baked into the deformer.
                        // If so, the get target throw an exception OR return no results and we need to address the case differently.
                        for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
                        {
                            MObject target = targets[targetIndex];
                            MFnMesh targetMesh = new MFnMesh(target);

                            BabylonMorphTarget babylonMorphTarget = new BabylonMorphTarget
                            {
                                name = $"{blendShapeDeformer.name}.{currentWeightName}",
                                influence = envelope * weight,
                                id = Guid.NewGuid().ToString()
                            };
                            babylonMorphTargets.Add(babylonMorphTarget);

                            // Target geometry
                            var targetVertices = new List<GlobalVertex>();
                            var uvSetNames = new MStringArray();
                            bool[] isUVExportSuccess = { false, false };
                            bool isTangentExportSuccess = exportParameters.exportTangents;
                            bool optimizeVertices = exportParameters.optimizeVertices;

                            List<VertexData> vertexDatas = babylonMesh.VertexDatas;
                            for (int vertexIndex = 0; vertexIndex < vertexDatas.Count; vertexIndex++)
                            {
                                VertexData vertexData = vertexDatas[vertexIndex];

                                MPoint position = new MPoint();
                                targetMesh.getPoint(vertexData.vertexIndexGlobal, position);
                                MVector normal = new MVector();
                                targetMesh.getFaceVertexNormal(vertexData.polygonId, vertexData.vertexIndexGlobal, normal);

                                // Switch coordinate system at object level
                                position.z *= -1;
                                normal.z *= -1;

                                // Apply unit conversion factor to meter
                                position.x *= scaleFactorToMeters;
                                position.y *= scaleFactorToMeters;
                                position.z *= scaleFactorToMeters;

                                GlobalVertex vertex = new GlobalVertex
                                {
                                    BaseIndex = vertexData.vertexIndexGlobal,
                                    Position = position.toArray(),
                                    Normal = normal.toArray()
                                };

                                if (isTangentExportSuccess)
                                {
                                    try
                                    {
                                        MVector tangent = new MVector();
                                        targetMesh.getFaceVertexTangent(vertexData.polygonId, vertexData.vertexIndexGlobal, tangent);

                                        // Switch coordinate system at object level
                                        tangent.z *= -1;

                                        int tangentId = targetMesh.getTangentId(vertexData.polygonId, vertexData.vertexIndexGlobal);
                                        bool isRightHandedTangent = targetMesh.isRightHandedTangent(tangentId);

                                        // Invert W to switch to left handed system
                                        vertex.Tangent = new float[] { (float)tangent.x, (float)tangent.y, (float)tangent.z, isRightHandedTangent ? -1 : 1 };
                                    }
                                    catch
                                    {
                                        // Exception raised when mesh don't have tangents
                                        isTangentExportSuccess = false;
                                    }
                                }

                                targetVertices.Add(vertex);
                            }

                            babylonMorphTarget.positions = targetVertices.SelectMany(v => v.Position).ToArray();

                            if (exportParameters.exportMorphNormals)
                            {
                                babylonMorphTarget.normals = targetVertices.SelectMany(v => v.Normal).ToArray();
                            }

                            // Tangent
                            if (isTangentExportSuccess && exportParameters.exportMorphTangents)
                            {
                                babylonMorphTarget.tangents = targetVertices.SelectMany(v => v.Tangent).ToArray();
                            }

                            // Animation
                            if (exportParameters.exportAnimations)
                            {
                                babylonMorphTarget.animations = GetAnimationsInfluence(blendShapeDeformer.name, weightIndex).ToArray();
                            }
                        }
                    }
                    catch
                    {
                        // see comment above.
                    }
                    if (targets.Count == 0)
                    {
                        // its common for target to be deleted, then beeing not available anymore. (This is a known problem of all Maya exporter, and seems no-ones find a solution so-far)
                        // Maya is then store the baked "deltas" into plug. We going to retreive these deltas and associated index to build our GlobalVertices list.

                        // 1 - Return the "inputTargetItem" array indices for the specified target. The "inputTargetItem" array indices correspond to the weight where the targets take affect according to the formula: index = wt * 1000 + 5000. 
                        // For example, if you have only a single target, and no in-betweens, the index will typically be 6000 since the default weight for the initial target is 1.0.
                        MIntArray targetItemIndices = new MIntArray();
                        blendShapeDeformer.targetItemIndexList((uint)weightIndex, mesh.objectProperty, targetItemIndices);

                        foreach (int k in targetItemIndices)
                        {
                            BabylonMorphTarget babylonMorphTarget = new BabylonMorphTarget
                            {
                                name = $"{blendShapeDeformer.name}.{currentWeightName}",
                                influence = envelope * weight,
                                id = Guid.NewGuid().ToString()
                            };
                            babylonMorphTargets.Add(babylonMorphTarget);

                            // 2 - The inputPointsTarget holds all the deltas as four component double list [X,Y,Z,W]
                            MDoubleArray deltas = new MDoubleArray();
                            MGlobal.executeCommand($"getAttr {blendShapeDeformer.name}.inputTarget[0].inputTargetGroup[{weightIndex}].inputTargetItem[{k}].inputPointsTarget;", deltas);

                            // 3 - the inputComponentsTarget holds the vertex indices that get each delta
                            // list is string formatted as "vtx[6873:6875]"
                            // this is not an obvious format -> TODO - search for an alternative to access theses values
                            MStringArray indiceStrList = new MStringArray();
                            MGlobal.executeCommand($"getAttr {blendShapeDeformer.name}.inputTarget[0].inputTargetGroup[{weightIndex}].inputTargetItem[{k}].inputComponentsTarget;", indiceStrList);
                            uint[] originalVertexIndices = indiceStrList.SelectMany(str => ParseStrIndice(str)).ToArray();

                            // 4 - within the indices, access the original vertices position and add delta to it.
                            var targetVertices = new List<GlobalVertex>();
                            List<VertexData> vertexDatas = babylonMesh.VertexDatas;
                            for (int vertexIndex = 0; vertexIndex < vertexDatas.Count; vertexIndex++)
                            {
                                VertexData vertexData = vertexDatas[vertexIndex];
                                // retreive the original position
                                MPoint position = new MPoint();
                                mesh.getPoint(vertexData.vertexIndexGlobal, position);
                                MVector normal = new MVector();
                                mesh.getFaceVertexNormal(vertexData.polygonId, vertexData.vertexIndexGlobal, normal);

                                // Switch coordinate system at object level
                                position.z *= -1;
                                normal.z *= -1;

                                // Apply unit conversion factor to meter
                                position.x *= scaleFactorToMeters;
                                position.y *= scaleFactorToMeters;
                                position.z *= scaleFactorToMeters;

                                GlobalVertex vertex = new GlobalVertex
                                {
                                    BaseIndex = vertexData.vertexIndexGlobal,
                                    Position = position.toArray()
                                };

                                for (int j = 0; j != originalVertexIndices.Length; j++)
                                {
                                    int localIndex = (int)originalVertexIndices[j];

                                    if (localIndex == vertexData.vertexIndexGlobal)
                                    {
                                        // we find a vertice to apply delta
                                        // remember that morph formula is : final mesh = original mesh + sum((morph targets - original mesh) * morph targets influences)
                                        int il = j * 4;
                                        vertex.Position[0] += (float)(deltas[il] * scaleFactorToMeters);
                                        vertex.Position[1] += (float)(deltas[il + 1] * scaleFactorToMeters);
                                        vertex.Position[2] += (float)(deltas[il + 2] * -scaleFactorToMeters);
                                        break;
                                    }

                                    if (localIndex > vertexData.vertexIndexGlobal)
                                    {
                                        // optimize because originalVertexIndices is sort ascending
                                        break;
                                    }
                                }
                                targetVertices.Add(vertex);
                            }
                            babylonMorphTarget.positions = targetVertices.SelectMany(v => v.Position).ToArray();

                            // Animation
                            if (exportParameters.exportAnimations)
                            {
                                babylonMorphTarget.animations = GetAnimationsInfluence(blendShapeDeformer.name, weightIndex).ToArray();
                            }
                        }
                    }
                }
            }

            return babylonMorphTargets;
        }

        /// <summary>
        /// utility for GetMorphTargets
        /// </summary>
        /// <param name="str">the index to be parsed in the form of vtx[6873:6875] or vtx[6873]</param>
        /// <returns>All the corresponding indices</returns>
        private IEnumerable<uint> ParseStrIndice(string str)
        {
            str = str.Substring(4, str.Length - 5);
            uint [] indices = str.Split(':').Select(s=>uint.Parse(s)).ToArray();
            uint a = indices[0];
            if (indices.Length == 1)
            {
                yield return a;
            } 
            else
            {
                uint b = indices[1];
                for (uint i = a; i <= b; i++)
                {
                    yield return i;
                }
            }
         }

        /// <summary>
        /// Set the blend shape envelope to 0.
        /// </summary>
        /// <param name="mObject"></param>
        private IDictionary<MObject, IList<float>> envelopeByObject = new Dictionary<MObject, IList<float>>();
        private void setEnvelopesToZeros(MObject mObject)
        {
            IList<MFnBlendShapeDeformer> blendShapeDeformers = GetBlendShape(mObject);
            IList<float> envelopes = new List<float>();

            for (int index = 0; index < blendShapeDeformers.Count; index++)
            {
                MFnBlendShapeDeformer blendShapeDeformer = blendShapeDeformers[index];

                envelopes.Add( blendShapeDeformer.envelope);

                blendShapeDeformer.envelope = 0f;
            }

            envelopeByObject[mObject] = envelopes;
        }

        /// <summary>
        /// Restore the blend shape envelope to its previous value (before the setEnvelopesToZeros call)
        /// </summary>
        /// <param name="mObject"></param>
        private void restoreEnvelopes(MObject mObject)
        {
            try
            {
                IList<MFnBlendShapeDeformer> blendShapeDeformers = GetBlendShape(mObject);
                IList<float> envelopes = envelopeByObject.First(item => item.Key.equalEqual(mObject)).Value;

                for(int index = 0; index < blendShapeDeformers.Count; index++)
                {
                    MFnBlendShapeDeformer blendShapeDeformer = blendShapeDeformers[index];
                    float envelope = envelopes[index];
                    blendShapeDeformer.envelope = envelope;
                }
            }
            catch { }
        }
    }
}
