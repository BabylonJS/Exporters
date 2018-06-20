using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [DataContract]
    public class GLTFExtensions
    {
        [DataMember(EmitDefaultValue = false)]
        public KHR_lightsExtension KHR_lights {get; set;}


        public GLTFExtensions()
        {
            KHR_lights = new KHR_lightsExtension();
        }

        public void Prepare()
        {
            // Do not export empty arrays
            KHR_lights.Prepare();
        }


        [DataContract]
        public class KHR_lightsExtension
        {
            [DataMember(EmitDefaultValue = false)]
            public GLTFLight[] lights { get; set; }

            public List<GLTFLight> lightsList { get; set; }

            public KHR_lightsExtension()
            {
                lightsList = new List<GLTFLight>();
            }

            public void Prepare()
            {
                // Do not export empty arrays
                if (lightsList.Count > 0)
                {
                    lights = lightsList.ToArray();
                }
            }
        }
    }
}
