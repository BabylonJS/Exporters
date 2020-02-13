using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GLTFExport.Entities
{
    [CollectionDataContract]
    public class GLTFExtensions : Dictionary<string, object>
    {
    }
}
