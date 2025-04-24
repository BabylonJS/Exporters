using System.Linq;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{

    [DataContract]
    public class BabylonVertexData : IBabylonMeshData
    {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public bool updatable { get; set; }
   
        [DataMember]
        public string tags { get; set; }

        #region IBabylonMeshData
        [DataMember]
        public float[] positions { get; set; }

        [DataMember]
        public float[] normals { get; set; }

        [DataMember]
        public float[] tangents { get; set; }

        [DataMember]
        public float[] uvs { get; set; }

        [DataMember]
        public float[] uvs2 { get; set; }

        [DataMember]
        public float[] uvs3 { get; set; }
        
        [DataMember]
        public float[] uvs4 { get; set; }
        
        [DataMember]
        public float[] uvs5 { get; set; }
        
        [DataMember]
        public float[] uvs6 { get; set; }
        
        [DataMember]
        public float[] uvs7 { get; set; }

        [DataMember]
        public float[] uvs8 { get; set; }

        [DataMember]
        public float[] colors { get; set; }

        [DataMember]
        public int[] matricesIndices { get; set; }

        [DataMember]
        public int[] matricesIndicesExtra { get; set; }

        [DataMember]
        public float[] matricesWeights { get; set; }

        [DataMember]
        public float[] matricesWeightsExtra { get; set; }

        [DataMember]
        public int[] indices { get; set; }

        [DataMember]
        public bool matricesIndicesExpanded { get; set; }

        [DataMember]
        public bool matricesIndicesExtraExpanded { get; set; }

        #endregion

        public BabylonVertexData() { }

        public BabylonVertexData(string identifier, IBabylonMeshData data): this(data)
        {
            id = identifier;
        }

        public BabylonVertexData(IBabylonMeshData data)
        {
            positions = data.positions;
            normals = data.normals;
            tangents = data.tangents;
            uvs = data.uvs;
            uvs2 = data.uvs2;
            uvs3 = data.uvs3;
            uvs4 = data.uvs4;
            uvs5 = data.uvs5;
            uvs6 = data.uvs6;
            uvs7 = data.uvs7;
            uvs8 = data.uvs8;
            colors = data.colors;
            matricesIndices = data.matricesIndices;
            matricesIndicesExtra = data.matricesIndicesExtra;
            matricesWeights = data.matricesWeights;
            matricesWeightsExtra = data.matricesWeightsExtra;
            indices = data.indices;
            matricesIndicesExpanded = false;
            matricesIndicesExtraExpanded = false;
        }

        private int[] CreatePackedArray(int[] rawArray)
        {
            var arrayReplacement = new int[rawArray.Length / 4];

            for (int i = 0; i < arrayReplacement.Length; i++)
            {
                int rawIndex = i * 4;
                int bone0 = rawArray[rawIndex];
                int bone1 = rawArray[rawIndex + 1];
                int bone2 = rawArray[rawIndex + 2];
                int bone3 = rawArray[rawIndex + 3];
                arrayReplacement[i] = (bone3 << 24) | (bone2 << 16) | (bone1 << 8) | bone0;
            }

            return arrayReplacement;
        }

        public bool TryPackIndexArrays()
        {
            bool result = true;

            if (matricesIndices != null && matricesIndices.Length != 0)
            {
                if (matricesIndices != null && matricesIndices.Any(a => a > 255))
                {
                    matricesIndicesExpanded = true;
                    result = false;
                }
                else
                {
                    matricesIndices = CreatePackedArray(matricesIndices);
                }
            }

            if (matricesIndicesExtra != null && matricesIndicesExtra.Length != 0)
            {
                if (matricesIndicesExtra != null && matricesIndicesExtra.Any(a => a > 255))
                {
                    matricesIndicesExtraExpanded = true;
                    result = false;
                }
                else
                {
                    matricesIndicesExtra = CreatePackedArray(matricesIndicesExtra);
                }
            }

            return result;
        }
    }
}
