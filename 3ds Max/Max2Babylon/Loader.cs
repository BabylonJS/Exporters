using System;
using System.ComponentModel;
using System.Timers;
using Autodesk.Max;
using Autodesk.Max.Plugins;

namespace Max2Babylon
{
    public class Loader
    {

        public static IGlobal Global
        {
            get
            {
                return GlobalInterface.Instance;
            }
        }

        public static IInterface_ID EditablePoly
        {
            get
            {
                return  Global.Interface_ID.Create(0x092779, 0x634020);
            }
        }

        public static IInterface14 Core
        {
            get
            {
                return Global.COREInterface14;
            }
        }
        public static IClass_ID Class_ID;

        static void Initialize()
        {
            if (Class_ID == null)
            {
                Class_ID = Global.Class_ID.Create(0x8217f123, 0xef980456);
                Core.AddClass(new Descriptor());
            }
        }

        public static void AssemblyMain()
        {
            Initialize();
        }

        public static void AssemblyInitializationCleanup()
        {

        }

        public static void AssemblyShutdown()
        {

        }
    }
}
