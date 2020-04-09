using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonLight : BabylonNode
    {
        public enum Type
        {
            Point, Directional, Spot, Hemispheric
        }

        [DataMember]
        public float[] direction { get; set; }

        [DataMember]
        public int type { get; set; }

        [DataMember]
        public float[] diffuse { get; set; }

        [DataMember]
        public float[] specular { get; set; }

        [DataMember]
        public float intensity { get; set; }

        [DataMember]
        public float range { get; set; }

        [DataMember]
        public float exponent { get; set; }

        /// <summary>
        /// In radians
        /// </summary>
        [DataMember]
        public float angle { get; set; }

        [DataMember]
        public float[] groundColor { get; set; }

        [DataMember]
        public string[] excludedMeshesIds { get; set; }

        [DataMember]
        public string[] includedOnlyMeshesIds { get; set; }

        [DataMember]
        public int? lightmapMode { get; set; }

        [DataMember]
        public int? falloffType { get; set; }

        public bool? hasDummy { get; set; }

        public BabylonLight()
        {
            diffuse = new[] {1.0f, 1.0f, 1.0f};
            specular = new[] { 1.0f, 1.0f, 1.0f };
            intensity = 1.0f;
            range = float.MaxValue;

            lightmapMode = null;
            falloffType = null;

            position = new float[] { 0, 0, 0 };
            rotation = new float[] { 0, 0, 0 };
            rotationQuaternion = new float[] { 0, 0, 0, 1 };
            scaling = new float[] { 1, 1, 1 };
            name = "";
        }
    }
}
