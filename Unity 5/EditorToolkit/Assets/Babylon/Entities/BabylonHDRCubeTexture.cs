using System;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonHDRCubeTexture : BabylonTexture
    {
        [DataMember]
        public string customType { get; private set; }
        
        [DataMember]
        public int size { get; set; }

        [DataMember]
        public float? rotationY { get; set; }

        [DataMember]
        public bool useInGammaSpace { get; set; }

        [DataMember]
        public bool generateHarmonics { get; set; }

        [DataMember]
        public bool usePMREMGenerator { get; set; }

        [DataMember]
        public bool isBABYLONPreprocessed { get; set; }

        [DataMember]
        public float[] boundingBoxSize { get; set; }

        [DataMember]
        public float[] boundingBoxPosition { get; set; }

        public BabylonHDRCubeTexture()
        {
            SetCustomType("BABYLON.HDRCubeTexture");
            size = 0;
            isCube = true;
            noMipmap = false;
            rotationY = 90.0f * ((float)Math.PI / 180.0f);
            boundingBoxSize = null;
            boundingBoxPosition = null;
            useInGammaSpace = false;
            generateHarmonics = true;
            usePMREMGenerator = false;
            isBABYLONPreprocessed = false;
        }

        public void SetCustomType(string type)
        {
            customType = type;
        }
    }
}
