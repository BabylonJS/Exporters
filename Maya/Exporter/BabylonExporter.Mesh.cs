using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using MayaBabylon;
using System.Collections.Generic;
using System.Linq;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        private BabylonNode ExportMesh(MObject mObject, BabylonScene babylonScene)
        {
            MFnMesh mFnMesh = new MFnMesh(mObject);
            MFnDagNode mFnDagNode = new MFnDagNode(mObject);

            var mObjectParent = mFnMesh.parent(0);
            MFnDagNode mFnDagNodeParent = new MFnDagNode(mObjectParent);

            // --- prints ---
            #region prints

            RaiseVerbose("BabylonExporter.Mesh | mFnMesh data", 2);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.name=" + mFnMesh.name, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.absoluteName=" + mFnMesh.absoluteName, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.fullPathName=" + mFnMesh.fullPathName, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.partialPathName=" + mFnMesh.partialPathName, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numVertices=" + mFnMesh.numVertices, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numEdges=" + mFnMesh.numEdges, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numPolygons=" + mFnMesh.numPolygons, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numFaceVertices=" + mFnMesh.numFaceVertices, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numNormals=" + mFnMesh.numNormals, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numUVSets=" + mFnMesh.numUVSets, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numUVsProperty=" + mFnMesh.numUVsProperty, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.activeColor=" + mFnMesh.activeColor.toString(), 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.attributeCount=" + mFnMesh.attributeCount, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.childCount=" + mFnMesh.childCount, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.displayColors=" + mFnMesh.displayColors, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.dormantColor=" + mFnMesh.dormantColor, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.hasUniqueName=" + mFnMesh.hasUniqueName, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.inUnderWorld=" + mFnMesh.inUnderWorld, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.isDefaultNode=" + mFnMesh.isDefaultNode, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.isInstanceable=" + mFnMesh.isInstanceable, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.isInstanced(true)=" + mFnMesh.isInstanced(true), 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.isInstanced(false)=" + mFnMesh.isInstanced(false), 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.isInstanced()=" + mFnMesh.isInstanced(), 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.isIntermediateObject=" + mFnMesh.isIntermediateObject, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.isShared=" + mFnMesh.isShared, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numColorSets=" + mFnMesh.numColorSets, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.numColorsProperty=" + mFnMesh.numColorsProperty, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.objectColor=" + mFnMesh.objectColor, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.parentCount=" + mFnMesh.parentCount, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.parentNamespace=" + mFnMesh.parentNamespace, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.uuid().asString()=" + mFnMesh.uuid().asString(), 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.dagRoot().apiType=" + mFnMesh.dagRoot().apiType, 3);
            RaiseVerbose("BabylonExporter.Mesh | mFnMesh.model.equalEqual(mFnMesh.objectProperty)=" + mFnMesh.model.equalEqual(mFnMesh.objectProperty), 3);
            RaiseVerbose("BabylonExporter.Mesh | ToString(mFnMesh.transformationMatrix)=" + mFnMesh.transformationMatrix.toString(), 3);
            var transformationMatrix = new MTransformationMatrix(mFnMesh.transformationMatrix);
            RaiseVerbose("BabylonExporter.Mesh | transformationMatrix.getTranslation().toString()=" + transformationMatrix.getTranslation().toString(), 3);
            var transformationMatrixParent = new MTransformationMatrix(mFnDagNodeParent.transformationMatrix);
            RaiseVerbose("BabylonExporter.Mesh | transformationMatrixParent.getTranslation().toString()=" + transformationMatrixParent.getTranslation().toString(), 3);

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

            RaiseMessage(mFnMesh.name, 1);

            var babylonMesh = new BabylonMesh { name = mFnMesh.name, id = mFnMesh.uuid().asString() };

            // Position / rotation / scaling / hierarchy
            ExportNode(babylonMesh, mFnDagNode, babylonScene);

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
            RaiseMessage("BabylonExporter.Mesh | done", 3);

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

                    // Inverse winding order
                    var tmp = triangleVertices[1];
                    triangleVertices[1] = triangleVertices[2];
                    triangleVertices[2] = tmp;

                    // For each vertex of this triangle (3 vertices per triangle)
                    foreach (int vertexId in triangleVertices)
                    {
                        MPoint point = new MPoint();
                        mFnMesh.getPoint(vertexId, point);

                        MVector normal = new MVector();
                        mFnMesh.getFaceVertexNormal(polygonId, vertexId, normal);

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
        
        private void ExportNode(BabylonAbstractMesh babylonAbstractMesh, MFnDagNode mFnDagNode, BabylonScene babylonScene)
        {
            if (mFnDagNode.parentCount != 0)
            {
                MObject parentMObject = mFnDagNode.parent(0);
                MFnDagNode mFnDagNodeParent = new MFnDagNode(parentMObject);

                // Position / rotation / scaling
                ExportTransform(babylonAbstractMesh, mFnDagNodeParent);

                // TODO - Hierarchy
                //babylonAbstractMesh.parentId = mFnDagNodeParent.uuid().asString();
            }
        }

        private void ExportTransform(BabylonAbstractMesh babylonAbstractMesh, MFnDagNode mFnDagNode)
        {
            // Position / rotation / scaling
            var transformationMatrix = new MTransformationMatrix(mFnDagNode.transformationMatrix);
            
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
        }

        private bool IsMeshExportable(MFnDagNode mFnDagNode)
        {
            return IsNodeExportable(mFnDagNode);
        }
    }
}
