using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFLight : GLTFIndexedChildRootProperty
    {
        public enum LightType
        {
            point,      // 0
            directional,// 1
            spot,       // 2
            ambient     // 3
        }


        // Property used by GLTFNodeExtension
        [DataMember(EmitDefaultValue = false)]
        public int? light { get; set; }


        // Properties used by GLTFextension

        [DataMember(EmitDefaultValue = false)]
        public float[] color { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float intensity { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public LightType type { get; set; }         // ambient, directional, point or spot

        [DataMember(EmitDefaultValue = false)]
        public float range { get; set; }            // point or spot

        [DataMember(EmitDefaultValue = false)]
        public Spot spot { get; set; }              // spot



        [DataContract]
        public class Spot
        {
            [DataMember(EmitDefaultValue = false)]
            public float? innerConeAngle { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public float? outerConeAngle { get; set; }
        }
    }

    //Loader
    //module BABYLON.GLTF2.Extensions
    //{
    //const NAME = "KHR_lights";

    //enum LightType
    //{
    //    AMBIENT = "ambient",
    //    DIRECTIONAL = "directional",
    //    POINT = "point",
    //    SPOT = "spot"
    //}

    //interface ILightReference
    //{
    //    light: number;
    //}

    //interface ILight
    //{
    //    type: LightType;
    //    color?: number[];
    //    intensity?: number;
    //}

    //interface ISpotLight extends ILight
    //{
    //    innerConeAngle?: number;
    //    outerConeAngle?: number;
    //}

    //interface ILights
    //{
    //    lights: ILight[];
    //}

}
