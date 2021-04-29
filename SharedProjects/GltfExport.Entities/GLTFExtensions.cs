using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [CollectionDataContract]
    public class GLTFExtensions : Dictionary<string, object>
    {
        public void AddExtension(GLTF context, string key, object ext)
        {
            context.extensionsUsed  = context.extensionsUsed ?? new System.Collections.Generic.List<string>();
            if (!context.extensionsUsed.Contains(key))
            {
                context.extensionsUsed.Add(key);
            }
            this[key] = ext;
        }
    }
}
