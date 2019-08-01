using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace Max2Babylon
{
    public static class MeshUtlities
    {
        public static ITriObject GetTriObjectFromNode(this IINode iNode)
        {
            IObject obj = iNode.EvalWorldState(Loader.Core.Time, false).Obj;
            if (obj.CanConvertToType(Loader.Global.TriObjectClassID) == 1)
            {
                return (ITriObject) obj.ConvertToType(Loader.Core.Time, Loader.Global.TriObjectClassID);
            }
            return null;
        }

        public static IPolyObject GetPolyObjectFromNode(this IINode iNode)
        {
            IObject obj = iNode.EvalWorldState(Loader.Core.Time, false).Obj;
            if (obj.CanConvertToType(Loader.Global.PolyObjectClassID) == 1)
            {
                return (IPolyObject) obj.ConvertToType(Loader.Core.Time, Loader.Global.PolyObjectClassID);
            }
            return null;
        }
    }
}
