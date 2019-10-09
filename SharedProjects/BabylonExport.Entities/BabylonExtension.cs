using System;
using System.Collections.Generic;
using System.Text;

namespace BabylonExport.Entities
{
    public class BabylonExtension
    {
        public Type ExtensionType;
        public string ExtensionName;
        public object ExtensionObject;

        public BabylonExtension(string extensionName,object extensionObject,Type extensionType)
        {
            ExtensionName = extensionName;
            ExtensionObject = extensionObject;
            ExtensionType = extensionType;
        }
    }
}
