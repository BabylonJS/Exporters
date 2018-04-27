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
        readonly List<MFnSkinCluster> skins = new List<MFnSkinCluster>();
        //readonly list<> skinnednodes = new list<>();

        private void ExportSkin(MFnSkinCluster skin, BabylonScene babylonScene)
        {
            //Print(skin, 1, "skin cluster");
           
            int skinIndex = skins.IndexOf(skin);
            BabylonSkeleton babylonSkeleton = new BabylonSkeleton { id = skinIndex };
            babylonSkeleton.name = "skeleton #" + babylonSkeleton.id;

            //RaiseMessage(babylonSkeleton.name, 1);

            // get the root node
            MObject rootNode = getRootNode(skin);
            // print the DAG from this joint
            //PrintDAG(true, rootNode);

            MFnTransform rootTransform = new MFnTransform(rootNode);

            //RaiseWarning( string.Join(",",rootTransform.transformation.asMatrixProperty.toArray()) );
            //RaiseWarning( string.Join(",", rootTransform.transformationMatrix.toArray()) );
            //var transformationMatrix = new MTransformationMatrix(rootTransform.transformationMatrix);

            //RaiseWarning( string.Join(",", transformationMatrix.asMatrixProperty.toArray()) );
        }


        private MObject getRootNode(MFnSkinCluster skin)
        {
            MObject rootJoint = null;

            MPlugArray connections = new MPlugArray();
            try
            {
                skin.getConnections(connections);
                // Search a joint in the connections
                int connectionsCount = connections.Count;
                int connectionIndex = 0;
                while(connectionIndex < connectionsCount && rootJoint == null)
                {
                    MObject source = connections[connectionIndex].source.node;
                    if (source != null && source.hasFn(MFn.Type.kJoint))
                    {
                        rootJoint = source;
                    }
                    connectionIndex++;
                }

                // Check the joint parent until it's a kWorld
                MFnDagNode mFnDagNode = new MFnDagNode(rootJoint);
                MObject firstParent = mFnDagNode.parent(0);
                MFnDependencyNode node = new MFnDependencyNode(firstParent);
                while(!firstParent.apiType.Equals(MFn.Type.kWorld))
                {
                    rootJoint = firstParent;
                    mFnDagNode = new MFnDagNode(rootJoint);
                    firstParent = mFnDagNode.parent(0);
                    node = new MFnDependencyNode(firstParent);
                }
            }
            catch (Exception e)
            {
            }

            return rootJoint;
        }
    }
}