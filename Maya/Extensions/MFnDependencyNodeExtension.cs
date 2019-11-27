using Autodesk.Maya.OpenMaya;

namespace Maya2Babylon
{
    static class MFnDependencyNodeExtension
    {
        public static MPlug getConnection(this MFnDependencyNode mFnDependencyNode, string name)
        {
            MPlugArray connections = new MPlugArray();
            mFnDependencyNode.getConnections(connections);
            foreach (MPlug connection in connections)
            {
                if (connection.name == (mFnDependencyNode.name + "." + name))
                {
                    return connection;
                }
            }
            return null;
        }
    }
}
