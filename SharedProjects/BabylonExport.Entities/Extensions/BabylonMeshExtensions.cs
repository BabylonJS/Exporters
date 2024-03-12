using System.Linq;

namespace BabylonExport.Entities
{
    public static class BabylonMeshExtensions
    {
        public static BabylonMesh Clone(this BabylonMesh mesh, BabylonScene scene)
        {
            string gid = mesh.geometryId;
            if (gid == null)
            {
                // we may create the geometry into the scene
                gid = mesh.id;
                // ensure geometries object exist
                scene.geometries = scene.geometries??new BabylonGeometries();
                // add new geometry object to the scene
                if (!scene.geometries.Contains(gid))
                {
                    scene.geometries.Add(new BabylonVertexData(gid, mesh));
                }
                // and update the mesh to ref this geometry.
                mesh.geometryId = gid;
                mesh.ClearLocalGeometry();
            }

            BabylonMesh newMesh = new BabylonMesh() {
                materialId = mesh.materialId,
                geometryId = mesh.geometryId,
                subMeshes = mesh.subMeshes
            };

            return newMesh;
        } 

        public static IBabylonMeshData ClearLocalGeometry(this IBabylonMeshData data)
        {
            data.positions = null;
            data.normals = null;
            data.tangents = null;
            data.uvs = null;
            data.uvs2 = null;
            data.uvs3 = null;
            data.uvs4 = null;
            data.uvs5 = null;
            data.uvs6 = null;
            data.uvs7 = null;
            data.uvs8 = null;
            data.colors = null;
            data.matricesIndices = null;
            data.matricesWeights = null;
            data.indices = null;
            return data;
        }

        public static void Add(this BabylonGeometries geometries, BabylonVertexData data)
        {
            geometries.vertexData = geometries.vertexData == null ? new[] { data } : geometries.vertexData.Where(v => v.id != data.id).Concat(new[] { data }).ToArray();
        }

        public static BabylonVertexData Get(this BabylonGeometries geometries, string id)
        {
            return geometries.vertexData?.First(v => v.id == id);
        }

        public static bool Contains(this BabylonGeometries geometries, string id)
        {
            return geometries.vertexData == null ? false : geometries.vertexData.Any(v => v.id == id);
        }
    }
}

