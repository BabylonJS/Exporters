using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            data.positions = default;
            data.normals = default;
            data.tangents = default;
            data.uvs = default;
            data.uvs2 = default;
            data.uvs3 = default;
            data.uvs4 = default;
            data.uvs5 = default;
            data.uvs6 = default;
            data.colors = default;
            data.matricesIndices = default;
            data.matricesWeights = default;
            data.indices = default;
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

