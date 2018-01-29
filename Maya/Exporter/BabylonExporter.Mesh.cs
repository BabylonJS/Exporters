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
            mDagPath.extendToShape();
            MFnMesh mFnMesh = new MFnMesh(mDagPath);

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

            // Geometry
            MIntArray triangleCounts = new MIntArray();
            MIntArray trianglesVertices = new MIntArray();
            mFnMesh.getTriangles(triangleCounts, trianglesVertices);
            RaiseVerbose("BabylonExporter.Mesh | triangleCounts.ToArray()=" + triangleCounts.ToArray().toString(), 3);
            RaiseVerbose("BabylonExporter.Mesh | trianglesVertices.ToArray()=" + trianglesVertices.ToArray().toString(), 3);
            int[] polygonsVertexCount = new int[mFnMesh.numPolygons];
            for (int polygonId = 0; polygonId < mFnMesh.numPolygons; polygonId++)
            {
                polygonsVertexCount[polygonId] = mFnMesh.polygonVertexCount(polygonId);
            }
            RaiseVerbose("BabylonExporter.Mesh | polygonsVertexCount=" + polygonsVertexCount.toString(), 3);

            //MFloatPointArray points = new MFloatPointArray();
            //mFnMesh.getPoints(points);
            //RaiseVerbose("BabylonExporter.Mesh | points.ToArray()=" + points.ToArray().Select(mFloatPoint => mFloatPoint.toString()), 3);

            //MFloatVectorArray normals = new MFloatVectorArray();
            //mFnMesh.getNormals(normals);
            //RaiseVerbose("BabylonExporter.Mesh | normals.ToArray()=" + normals.ToArray().Select(mFloatPoint => mFloatPoint.toString()), 3);

            for (int polygonId = 0; polygonId < mFnMesh.numPolygons; polygonId++)
            {
                MIntArray verticesId = new MIntArray();
                RaiseVerbose("BabylonExporter.Mesh | polygonId=" + polygonId, 3);

                int nbTriangles = triangleCounts[polygonId];
                RaiseVerbose("BabylonExporter.Mesh | nbTriangles=" + nbTriangles, 3);

                for (int triangleIndex = 0; triangleIndex < triangleCounts[polygonId]; triangleIndex++)
                {
                    RaiseVerbose("BabylonExporter.Mesh | triangleIndex=" + triangleIndex, 3);
                    int[] triangleVertices = new int[3];
                    mFnMesh.getPolygonTriangleVertices(polygonId, triangleIndex, triangleVertices);
                    RaiseVerbose("BabylonExporter.Mesh | triangleVertices=" + triangleVertices.toString(), 3);

                    foreach (int vertexId in triangleVertices)
                    {
                        RaiseVerbose("BabylonExporter.Mesh | vertexId=" + vertexId, 3);
                        MPoint point = new MPoint();
                        mFnMesh.getPoint(vertexId, point);
                        RaiseVerbose("BabylonExporter.Mesh | point=" + point.toString(), 3);

                        MVector normal = new MVector();
                        mFnMesh.getFaceVertexNormal(polygonId, vertexId, normal);
                        RaiseVerbose("BabylonExporter.Mesh | normal=" + normal.toString(), 3);
                    }
                }
            }

            #endregion


            if (IsMeshExportable(mFnMesh) == false)
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

            // TODO - Material

            var vertices = new List<GlobalVertex>();
            var indices = new List<int>();
            // TODO - UV, color, alpha
            //var mappingChannels = unskinnedMesh.ActiveMapChannelNum;
            //bool hasUV = false;
            //bool hasUV2 = false;
            //for (int i = 0; i < mappingChannels.Count; ++i)
            //{
            //    var indexer = i;
            //    var channelNum = mappingChannels[indexer];
            //    if (channelNum == 1)
            //    {
            //        hasUV = true;
            //    }
            //    else if (channelNum == 2)
            //    {
            //        hasUV2 = true;
            //    }
            //}
            //var hasColor = unskinnedMesh.NumberOfColorVerts > 0;
            //var hasAlpha = unskinnedMesh.GetNumberOfMapVerts(-2) > 0;

            // TODO - Add custom properties
            var optimizeVertices = false; // meshNode.MaxNode.GetBoolProperty("babylonjs_optimizevertices");

            // Compute normals
            var subMeshes = new List<BabylonSubMesh>();
            ExtractGeometry(mFnMesh, vertices, indices, subMeshes, optimizeVertices);

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
        /// <param name="optimizeVertices"></param>
        private void ExtractGeometry(MFnMesh mFnMesh, List<GlobalVertex> vertices, List<int> indices, List<BabylonSubMesh> subMeshes, bool optimizeVertices)
        {
            // TODO - Multimaterials: create a BabylonSubMesh per submaterial
            // TODO - optimizeVertices
            MIntArray triangleCounts = new MIntArray();
            MIntArray trianglesVertices = new MIntArray();
            mFnMesh.getTriangles(triangleCounts, trianglesVertices);
            
            // For each polygon of this mesh
            for (int polygonId = 0; polygonId < mFnMesh.numPolygons; polygonId++)
            {
                MIntArray verticesId = new MIntArray();
                int nbTriangles = triangleCounts[polygonId];

                // For each triangle of this polygon
                for (int triangleIndex = 0; triangleIndex < triangleCounts[polygonId]; triangleIndex++)
                {
                    int[] triangleVertices = new int[3];
                    mFnMesh.getPolygonTriangleVertices(polygonId, triangleIndex, triangleVertices);

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
                    foreach (int vertexId in triangleVertices)
                    {
                        MPoint point = new MPoint();
                        mFnMesh.getPoint(vertexId, point);

                        MVector normal = new MVector();
                        mFnMesh.getFaceVertexNormal(polygonId, vertexId, normal);

                        // Switch coordinate system at object level
                        point.z *= -1;
                        normal.z *= -1;

                        var vertex = new GlobalVertex
                        {
                            BaseIndex = vertexId,
                            Position = point.toArray(),
                            Normal = normal.toArray()
                        };

                        indices.Add(vertices.Count);
                        vertices.Add(vertex);
                    }
                }
            }

            // BabylonSubMesh
            var subMesh = new BabylonSubMesh { indexStart = 0, materialIndex = 0 };
            subMeshes.Add(subMesh);

            subMesh.indexCount = indices.Count;
            subMesh.verticesStart = 0;
            subMesh.verticesCount = vertices.Count;
        }
        
        private void ExportNode(BabylonAbstractMesh babylonAbstractMesh, MFnTransform mFnTransform, BabylonScene babylonScene)
        {
            RaiseVerbose("BabylonExporter.Mesh | ExportNode", 2);

            // Position / rotation / scaling
            ExportTransform(babylonAbstractMesh, mFnTransform);

            // Hierarchy
            if (mFnTransform.parentCount != 0)
            {
                RaiseVerbose("BabylonExporter.Mesh | Hierarchy", 2);

                MObject parentMObject = mFnTransform.parent(0);
                // Children of World node don't have parent in Babylon
                if (parentMObject.apiType != MFn.Type.kWorld)
                {
                    MFnDagNode mFnTransformParent = new MFnDagNode(parentMObject);
                    babylonAbstractMesh.parentId = mFnTransformParent.uuid().asString();
                }
            }
        }

        private void ExportTransform(BabylonAbstractMesh babylonAbstractMesh, MFnTransform mFnTransform)
        {
            RaiseVerbose("BabylonExporter.Mesh | ExportTransform", 2);

            // Position / rotation / scaling
            var transformationMatrix = new MTransformationMatrix(mFnTransform.transformationMatrix);
            
            babylonAbstractMesh.position = transformationMatrix.getTranslation();

            if (_exportQuaternionsInsteadOfEulers)
            {
                babylonAbstractMesh.rotationQuaternion = transformationMatrix.getRotationQuaternion();
            }
            else
            {
                babylonAbstractMesh.rotation = transformationMatrix.getRotation();
            }

            babylonAbstractMesh.scaling = transformationMatrix.getScale();

            // Switch coordinate system at object level
            babylonAbstractMesh.position[2] *= -1;
            if (_exportQuaternionsInsteadOfEulers)
            {
                babylonAbstractMesh.rotationQuaternion[0] *= -1;
                babylonAbstractMesh.rotationQuaternion[1] *= -1;
            }
            else
            {
                babylonAbstractMesh.rotation[0] *= -1;
                babylonAbstractMesh.rotation[1] *= -1;
            }
        }

        private bool IsMeshExportable(MFnDagNode mFnDagNode)
        {
            return IsNodeExportable(mFnDagNode);
        }
    }
}
