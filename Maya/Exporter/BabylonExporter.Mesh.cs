using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using MayaBabylon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
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

            var babylonMesh = new BabylonMesh { name = mFnTransform.name, id = mFnTransform.uuid().asString() };
            babylonMesh.isDummy = true;

            // Position / rotation / scaling / hierarchy
            ExportNode(babylonMesh, mFnTransform, babylonScene);

            // TODO - Animations
            //exportAnimation(babylonMesh, meshNode);

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
            MFnTransform mFnTransform = new MFnTransform(mDagPath);

            // Mesh direct child of the transform
            MFnMesh mFnMesh = null;
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
                }
            }
            if (mFnMesh == null)
            {
                RaiseError("No mesh found has child of " + mDagPath.fullPathName);
                return null;
            }

            RaiseMessage("mFnMesh.fullPathName=" + mFnMesh.fullPathName, 2);

            // --- prints ---
            #region prints

            Action <MFnDagNode> printMFnDagNode = (MFnDagNode mFnDagNode) =>
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

            var babylonMesh = new BabylonMesh { name = mFnTransform.name, id = mFnTransform.uuid().asString() };

            // Position / rotation / scaling / hierarchy
            ExportNode(babylonMesh, mFnTransform, babylonScene);

            // Misc.
            // TODO - Retreive from Maya
            // TODO - What is the difference between isVisible and visibility?
            // TODO - Fix fatal error: Attempting to save in C:/Users/Fabrice/AppData/Local/Temp/Fabrice.20171205.1613.ma
            //babylonMesh.isVisible = mDagPath.isVisible;
            //babylonMesh.visibility = meshNode.MaxNode.GetVisibility(0, Tools.Forever);
            //babylonMesh.receiveShadows = meshNode.MaxNode.RcvShadows == 1;
            //babylonMesh.applyFog = meshNode.MaxNode.ApplyAtmospherics == 1;

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

            // Material
            MObjectArray shaders = new MObjectArray();
            mFnMesh.getConnectedShaders(0, shaders, new MIntArray());
            if (shaders.Count > 0)
            {
                List<MFnDependencyNode> materials = new List<MFnDependencyNode>();
                foreach (MObject shader in shaders)
                {
                    // Retreive material
                    MFnDependencyNode shadingEngine = new MFnDependencyNode(shader);
                    MPlug mPlugSurfaceShader = shadingEngine.findPlug("surfaceShader");
                    MObject materialObject = mPlugSurfaceShader.source.node;
                    MFnDependencyNode material = new MFnDependencyNode(materialObject);

                    materials.Add(material);
                }

                if (shaders.Count == 1)
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

            bool hasUV = mFnMesh.numUVSets > 0 && mFnMesh.numUVsProperty > 0;

            // TODO - color, alpha
            //var hasColor = unskinnedMesh.NumberOfColorVerts > 0;
            //var hasAlpha = unskinnedMesh.GetNumberOfMapVerts(-2) > 0;

            // TODO - Add custom properties
            var optimizeVertices = false; // meshNode.MaxNode.GetBoolProperty("babylonjs_optimizevertices");

            // Compute normals
            var subMeshes = new List<BabylonSubMesh>();
            ExtractGeometry(mFnMesh, vertices, indices, subMeshes, hasUV, optimizeVertices);

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
            babylonMesh.positions = vertices.SelectMany(v => v.Position).ToArray();
            babylonMesh.normals = vertices.SelectMany(v => v.Normal).ToArray();
            if (hasUV)
            {
                babylonMesh.uvs = vertices.SelectMany(v => v.UV).ToArray();
            }

            babylonMesh.subMeshes = subMeshes.ToArray();

            // Buffers - Indices
            babylonMesh.indices = indices.ToArray();


            babylonScene.MeshesList.Add(babylonMesh);
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
        /// <param name="hasUV"></param>
        /// <param name="optimizeVertices"></param>
        private void ExtractGeometry(MFnMesh mFnMesh, List<GlobalVertex> vertices, List<int> indices, List<BabylonSubMesh> subMeshes, bool hasUV, bool optimizeVertices)
        {
            // TODO - optimizeVertices
            MIntArray triangleCounts = new MIntArray();
            MIntArray trianglesVertices = new MIntArray();
            mFnMesh.getTriangles(triangleCounts, trianglesVertices);
            
            MObjectArray shaders = new MObjectArray();
            MIntArray faceMatIndices = new MIntArray(); // given a face index => get a shader index
            mFnMesh.getConnectedShaders(0, shaders, faceMatIndices);

            // Export geometry even if an error occured with shaders
            // This is a fix for Maya test files
            // TODO - Find the reason why shaders.count = 0
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
                            for (vertexIndexLocal = 0; vertexIndexLocal < polygonVertices.Count; vertexIndexLocal++)
                            {
                                if (polygonVertices[vertexIndexLocal] == vertexIndexGlobal)
                                {
                                    break;
                                }
                            }

                            GlobalVertex vertex = ExtractVertex(mFnMesh, polygonId, vertexIndexGlobal, vertexIndexLocal, hasUV);
                            vertex.CurrentIndex = vertices.Count;

                            indices.Add(vertex.CurrentIndex);
                            vertices.Add(vertex);

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
        }

        /// <summary>
        /// Extract geometry (position, normal, UVs...) for a specific vertex
        /// </summary>
        /// <param name="mFnMesh"></param>
        /// <param name="polygonId">The polygon (face) to examine</param>
        /// <param name="vertexIndexGlobal">The object-relative (mesh-relative/global) vertex index</param>
        /// <param name="vertexIndexLocal">The face-relative (local) vertex id to examine</param>
        /// <param name="hasUV"></param>
        /// <returns></returns>
        private GlobalVertex ExtractVertex(MFnMesh mFnMesh, int polygonId, int vertexIndexGlobal, int vertexIndexLocal, bool hasUV)
        {
            MPoint point = new MPoint();
            mFnMesh.getPoint(vertexIndexGlobal, point);

            MVector normal = new MVector();
            mFnMesh.getFaceVertexNormal(polygonId, vertexIndexGlobal, normal);

            // Switch coordinate system at object level
            point.z *= -1;
            normal.z *= -1;

            var vertex = new GlobalVertex
            {
                BaseIndex = vertexIndexGlobal,
                Position = point.toArray(),
                Normal = normal.toArray()
            };

            // UV
            if (hasUV)
            {
                float u = 0, v = 0;
                mFnMesh.getPolygonUV(polygonId, vertexIndexLocal, ref u, ref v);
                vertex.UV = new float[] { u, v };
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

        private void ExportTransform(BabylonAbstractMesh babylonAbstractMesh, MFnTransform mFnTransform)
        {
            // Position / rotation / scaling
            RaiseVerbose("BabylonExporter.Mesh | ExportTransform", 2);
            float[] position = null;
            float[] rotationQuaternion = null;
            float[] rotation = null;
            float[] scaling = null;
            GetTransform(mFnTransform, ref position, ref rotationQuaternion, ref rotation, ref scaling);

            babylonAbstractMesh.position = position;
            if (_exportQuaternionsInsteadOfEulers)
            {
                babylonAbstractMesh.rotationQuaternion = rotationQuaternion;
            }
            else
            {
                babylonAbstractMesh.rotation = rotation;
            }
            babylonAbstractMesh.scaling = scaling;
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
    }
}
