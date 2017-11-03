using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine
{
    public static class MeshExtensions
    {
        public static Mesh Clone(this Mesh source, bool recalculate = false, Matrix4x4? matrix = null, bool lightmaps = true, Mesh[] mergeMeshes = null)
        {
            Mesh clone = new Mesh();
            clone.name = source.name;
            clone.vertices = source.vertices;
            clone.normals = source.normals;
            clone.tangents = source.tangents;
            clone.triangles = source.triangles;
            clone.uv = source.uv;
            clone.uv2 = source.uv2;
            clone.uv3 = source.uv3;
            clone.uv4 = source.uv4;
            clone.colors = source.colors;
            clone.colors32 = source.colors32;
            clone.bindposes = source.bindposes;
            clone.boneWeights = source.boneWeights;
            clone.subMeshCount = source.subMeshCount;
            clone.bounds = source.bounds;
            if (recalculate) clone.RecalculateBounds();
            if (matrix != null) {
                Mesh regen = new Mesh();
                regen.name = clone.name;
                // Setup Source Combine
                CombineInstance combine = new CombineInstance();
                combine.transform = matrix.Value;
                combine.mesh = clone;
                List<CombineInstance> combines = new List<CombineInstance>();
                combines.Add(combine);
                // Merge Other Combines
                if (mergeMeshes != null && mergeMeshes.Length > 0) {
                    foreach (var mergeMesh in mergeMeshes) {
                        combines.Add(new CombineInstance() { transform = matrix.Value, mesh = mergeMesh });
                    }
                }
                regen.CombineMeshes(combines.ToArray(), true, true, lightmaps);
                return regen;
            } else  {
                return clone;
            }
        }

        public static Mesh Copy(this Mesh source, bool recalculate = false, Matrix4x4? matrix = null, bool lightmaps = true, Mesh[] mergeMeshes = null)
        {
            Mesh copy = new Mesh();
            copy.name = source.name;
            if (source.vertices != null) copy.SetVertices(new List<Vector3>(source.vertices));
            if (source.uv != null) copy.SetUVs(0, new List<Vector2>(source.uv));
            if (source.uv2 != null) copy.SetUVs(1, new List<Vector2>(source.uv2));
            if (source.uv3 != null) copy.SetUVs(2, new List<Vector2>(source.uv3));
            if (source.uv4 != null) copy.SetUVs(3, new List<Vector2>(source.uv4));
            if (source.normals != null) copy.SetNormals(new List<Vector3>(source.normals));
            if (source.tangents != null) copy.SetTangents(new List<Vector4>(source.tangents));
            if (source.colors != null) copy.SetColors(new List<Color>(source.colors));
            if (source.colors32 != null && source.colors32.Length > 0) copy.colors32 = new List<Color32>(source.colors32).ToArray();
            if (source.bindposes != null && source.bindposes.Length > 0) copy.bindposes = new List<Matrix4x4>(source.bindposes).ToArray();
            if (source.boneWeights != null && source.boneWeights.Length > 0) copy.boneWeights = new List<BoneWeight>(source.boneWeights).ToArray();
            if (source.triangles != null && source.triangles.Length > 0) copy.triangles = new List<int>(source.triangles).ToArray();
            copy.subMeshCount = source.subMeshCount;
            copy.bounds = source.bounds;
            if (recalculate) copy.RecalculateBounds();
            if (matrix != null) {
                Mesh regen = new Mesh();
                regen.name = copy.name;
                // Setup Source Combine
                CombineInstance combine = new CombineInstance();
                combine.transform = matrix.Value;
                combine.mesh = copy;
                List<CombineInstance> combines = new List<CombineInstance>();
                combines.Add(combine);
                // Merge Other Combines
                if (mergeMeshes != null && mergeMeshes.Length > 0) {
                    foreach (var mergeMesh in mergeMeshes) {
                        combines.Add(new CombineInstance() { transform = matrix.Value, mesh = mergeMesh });
                    }
                }
                regen.CombineMeshes(combines.ToArray(), true, true, lightmaps);
                return regen;
            } else  {
                return copy;
            }
        }

        public static Mesh Scale(this Mesh source, Vector3 scale)
        {
            Mesh mesh = source.Copy();
            Vector3[] origVerts = mesh.vertices;
            Vector3[] newVerts = new Vector3[origVerts.Length];
            Matrix4x4 m = Matrix4x4.Scale(scale);
            int i = 0;
            while (i < origVerts.Length) {
                newVerts[i] = m.MultiplyPoint3x4(origVerts[i]);
                i++;
            }
            mesh.vertices = newVerts;
            return mesh;
        }        

        public static Mesh Rotate(this Mesh source, Vector3 rotation)
        {
            Mesh mesh = source.Copy();
            Vector3[] origVerts = mesh.vertices;
            Vector3[] newVerts = new Vector3[origVerts.Length];
            Quaternion rotationq = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            Matrix4x4 m = Matrix4x4.Rotate(rotationq);
            int i = 0;
            while (i < origVerts.Length) {
                newVerts[i] = m.MultiplyPoint3x4(origVerts[i]);
                i++;
            }
            mesh.vertices = newVerts;
            return mesh;
        }        

        public static Mesh Translate(this Mesh source, Vector3 position)
        {
            Mesh mesh = source.Copy();
            Vector3[] origVerts = mesh.vertices;
            Vector3[] newVerts = new Vector3[origVerts.Length];
            Matrix4x4 m = Matrix4x4.Translate(position);
            int i = 0;
            while (i < origVerts.Length) {
                newVerts[i] = m.MultiplyPoint3x4(origVerts[i]);
                i++;
            }
            mesh.vertices = newVerts;
            return mesh;
        }        

        public static Mesh Transform(this Mesh source, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Mesh mesh = source.Copy();
            Vector3[] origVerts = mesh.vertices;
            Vector3[] newVerts = new Vector3[origVerts.Length];
            Quaternion rotationq = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            Matrix4x4 m = Matrix4x4.TRS(position, rotationq, scale);
            int i = 0;
            while (i < origVerts.Length) {
                newVerts[i] = m.MultiplyPoint3x4(origVerts[i]);
                i++;
            }
            mesh.vertices = newVerts;
            return mesh;
        }        

        public static Mesh GetSubmesh(this Mesh source, int subMeshIndex)
        {
            if (subMeshIndex < 0 || subMeshIndex >= source.subMeshCount) return null;
            int index = subMeshIndex + 1;
            int[] indices = source.GetTriangles(subMeshIndex);
            Vertices dest = new Vertices();
            Vertices copy = new Vertices(source);
            int[] newIndices = new int[indices.Length];
            Dictionary<int, int> map = new Dictionary<int, int>();
            for (int i = 0; i < indices.Length; i++) {
                int o = indices[i];
                int n;
                if (!map.TryGetValue(o, out n)) {
                    n = dest.Add(copy, o);
                    map.Add(o, n);
                }
                newIndices[i] = n;
            }
            // Create new sub mesh
            Mesh sub = new Mesh();
            dest.AssignTo(sub);
            sub.subMeshCount = 1;
            sub.triangles = newIndices;
            sub.name = source.name + "Sub" + index.ToString();
            sub.RecalculateBounds();
            return sub;
        }
        
        public static void ReverseNormals(this Mesh mesh)
        {
            Vector3[] normals = mesh.normals;
			for (int i=0;i<normals.Length;i++) {
				normals[i] = -normals[i];
            }
			mesh.normals = normals;
			for (int m=0;m<mesh.subMeshCount;m++) {
				int[] triangles = mesh.GetTriangles(m);
				for (int i=0;i<triangles.Length;i+=3) {
					int temp = triangles[i + 0];
					triangles[i + 0] = triangles[i + 1];
					triangles[i + 1] = temp;
				}
				mesh.SetTriangles(triangles, m);
			}            
        }
        
        ////////////////////////////////////////////
        // Private Sub Mesh Triangle Helper Class //
        ////////////////////////////////////////////
        
        private class Vertices
        {
            List<Vector3> verts = null;
            List<Vector2> uv1 = null;
            List<Vector2> uv2 = null;
            List<Vector2> uv3 = null;
            List<Vector2> uv4 = null;
            List<Vector3> normals = null;
            List<Vector4> tangents = null;
            List<Color> colors = null;
            List<Color32> colors32 = null;
            List<BoneWeight> boneWeights = null;
            List<Matrix4x4> bindPoses = null;

            public Vertices()
            {
                verts = new List<Vector3>();
            }
            public Vertices(Mesh source)
            {
                verts = CreateList(source.vertices);
                uv1 = CreateList(source.uv);
                uv2 = CreateList(source.uv2);
                uv3 = CreateList(source.uv3);
                uv4 = CreateList(source.uv4);
                normals = CreateList(source.normals);
                tangents = CreateList(source.tangents);
                colors = CreateList(source.colors);
                colors32 = CreateList(source.colors32);
                bindPoses = CreateList(source.bindposes);
                boneWeights = CreateList(source.boneWeights);
            }

            private List<T> CreateList<T>(T[] aSource)
            {
                if (aSource == null)
                    return null;
                return new List<T>(aSource);
            }

            private void Copy<T>(ref List<T> aDest, List<T> aSource, int aIndex)
            {
                if (aSource == null)
                    return;
                if (aDest == null)
                    aDest = new List<T>();
                int counter = aIndex + 1;
                if (aSource.Count >= counter)
                    aDest.Add(aSource[aIndex]);
            }

            public int Add(Vertices aOther, int aIndex)
            {
                int i = verts.Count;
                Copy(ref verts, aOther.verts, aIndex);
                Copy(ref uv1, aOther.uv1, aIndex);
                Copy(ref uv2, aOther.uv2, aIndex);
                Copy(ref uv3, aOther.uv3, aIndex);
                Copy(ref uv4, aOther.uv4, aIndex);
                Copy(ref normals, aOther.normals, aIndex);
                Copy(ref tangents, aOther.tangents, aIndex);
                Copy(ref colors, aOther.colors, aIndex);
                Copy(ref colors32, aOther.colors32, aIndex);
                Copy(ref bindPoses, aOther.bindPoses, aIndex);
                Copy(ref boneWeights, aOther.boneWeights, aIndex);
                return i;
            }

            public void AssignTo(Mesh aTarget)
            {
                aTarget.SetVertices(verts);
                if (uv1 != null) aTarget.SetUVs(0, uv1);
                if (uv2 != null) aTarget.SetUVs(1, uv2);
                if (uv3 != null) aTarget.SetUVs(2, uv3);
                if (uv4 != null) aTarget.SetUVs(3, uv4);
                if (normals != null) aTarget.SetNormals(normals);
                if (tangents != null) aTarget.SetTangents(tangents);
                if (colors != null) aTarget.SetColors(colors);
                if (colors32 != null && colors32.Count > 0) aTarget.colors32 = colors32.ToArray();
                if (bindPoses != null && bindPoses.Count > 0) aTarget.bindposes = bindPoses.ToArray();
                if (boneWeights != null && boneWeights.Count > 0) aTarget.boneWeights = boneWeights.ToArray();
            }
        }
    }
}