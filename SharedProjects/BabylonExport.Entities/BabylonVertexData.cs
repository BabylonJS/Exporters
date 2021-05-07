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
            colors = data.colors;
            matricesIndices = data.matricesIndices;
            matricesIndicesExtra = data.matricesIndicesExtra;
            matricesWeights = data.matricesWeights;
            matricesWeightsExtra = data.matricesWeightsExtra;
            indices = data.indices;
        }
    }
}
