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

        /// <summary>
        /// Gets the NodeLayerProperties interface
        /// </summary>
        public static IInterface_ID NodeLayerProperties
        {
            get
            {
                return Global.Interface_ID.Create(0x44e025f8, 0x6b071e44);

            }
        }

        /// <summary>
        /// Gets the Function-Published layer manager.
        /// </summary>
        public static IIFPLayerManager IIFPLayerManager
        {
            get 
            {
                IInterface_ID iIFPLayerManagerID = Global.Interface_ID.Create((uint)BuiltInInterfaceIDA.LAYERMANAGER_INTERFACE,(uint)BuiltInInterfaceIDB.LAYERMANAGER_INTERFACE);
                return (IIFPLayerManager) Global.GetCOREInterface(iIFPLayerManagerID);
            }
        }

        public static IIObjXRefManager8 IIObjXRefManager
        {
            get
            {
                return Loader.Global.IObjXRefManager8.Instance;

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
