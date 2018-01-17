using System;
using System.ComponentModel;
using System.Timers;
using Autodesk.Max;
using Autodesk.Max.Plugins;

namespace Max2Babylon
{
    public class Loader
    {
        static IGlobal global;
        public static IGlobal Global
        {
            get
            {
                return global;
            }
        }

        public static IInterface14 core;
        public static IInterface14 Core
        {
            get
            {
                return core;
            }
        }
        public static IClass_ID Class_ID;

        static void Initialize()
        {
            if (global == null)
            {
                global = GlobalInterface.Instance;
                core = global.COREInterface14;
                Class_ID = global.Class_ID.Create(0x8217f123, 0xef980456);
                core.AddClass(new Descriptor());
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
