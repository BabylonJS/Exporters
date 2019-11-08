using Autodesk.Max;
using Autodesk.Max.IQuadMenuContext;
using Autodesk.Max.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Autodesk.Max.MaxSDK.AssetManagement;
using Object = Autodesk.Max.Plugins.Object;

namespace Max2Babylon
{
    class GlobalUtility : GUP
    {
        IIMenu menu;
        IIMenuItem menuItem;
        IIMenuItem menuItemBabylon;
        uint idActionTable;
        IActionTable actionTable;
        IActionCallback actionCallback;

        /// <summary>
        /// Store reference of exporter form to close it manually when exiting 3ds max
        /// </summary>
        BabylonExportActionItem babylonExportActionItem;

#if MAX2018 || MAX2019
        GlobalDelegates.Delegate5 m_SystemStartupDelegate;
#endif
        private static bool filePreOpenCallback = false;
        private GlobalDelegates.Delegate5 m_FilePreOpenDelegate;

        private static bool postSceneResetCallback = false;
        private GlobalDelegates.Delegate5 m_PostSceneResetCallback;

        private static bool nodeAddedCallback = false;
        private GlobalDelegates.Delegate5 m_NodeAddedDelegate;

        private static bool nodeDeleteCallback = false;
        private GlobalDelegates.Delegate5 m_NodeDeleteDelegate;


        private void MenuSystemStartupHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            InstallMenus();
        }

        private void InitializeBabylonGuids(IntPtr param0, IntPtr param1)
        {
            Tools.guids = new Dictionary<Guid, IAnimatable>();
        }

        private void InitializeBabylonGuids(IntPtr objPtr, INotifyInfo infoPtr)
        {
            Tools.guids = new Dictionary<Guid, IAnimatable>();
        }

#if MAX2015
        private void OnNodeAdded(IntPtr param0, IntPtr param1)
        {
            try
            {
                INotifyInfo obj = Loader.Global.NotifyInfo.Marshal(param1);

                IINode n = (IINode) obj.CallParam;
                //todo replace this with something like isXREFNODE
                //to have a distinction between added xref node and max node
                string guid = n.GetStringProperty("babylonjs_GUID", string.Empty);
                if (string.IsNullOrEmpty(guid))
                {
                    n.GetGuid(); // force to assigne a new guid if not exist yet for this node
                }

                IIContainerObject contaner = Loader.Global.ContainerManagerInterface.IsContainerNode(n);
                if (contaner != null)
                {
                    // a generic operation on a container is done (open/inherit)
                    contaner.ResolveContainer();
                }
            }
            catch
            {
                // Fails silently
            }
        }
#endif

#if MAX2015
        private void OnNodeDeleted(IntPtr objPtr, IntPtr param1)
        {
            try
            {
                INotifyInfo obj = Loader.Global.NotifyInfo.Marshal(param1);

                IINode n = (IINode) obj.CallParam;
                Tools.guids.Remove(n.GetGuid());
            }
            catch
            {
                // Fails silently
            }
        }
#endif

        private void OnNodeAdded(IntPtr objPtr, INotifyInfo infoPtr)
        {
            try
            {
                IINode n = (IINode)infoPtr.CallParam;
                //todo replace this with something like isXREFNODE
                //to have a distinction between added xref node and max node
                string guid = n.GetStringProperty("babylonjs_GUID", string.Empty);
                if (string.IsNullOrEmpty(guid))
                {
                    n.GetGuid(); // force to assigne a new guid if not exist yet for this node
                }

                IIContainerObject container = Loader.Global.ContainerManagerInterface.IsContainerNode(n);
                if (container != null)
                {
                    // a generic operation on a container is done (open/inherit)
                    container.ResolveContainer();
                }
            }
            catch
            {
                // Fails silently
            }
        }

        private void OnNodeDeleted(IntPtr objPtr, INotifyInfo infoPtr)
        {
            try
            {
                IINode n = (IINode)infoPtr.CallParam;
                Tools.guids.Remove(n.GetGuid());
            }
            catch
            {
                // Fails silently
            }
        }

        public override void Stop()
        {
            try
            {
                // Close exporter form manually
                if (babylonExportActionItem != null)
                {
                    babylonExportActionItem.Close();
                }

                if (actionTable != null)
                {
                    Loader.Global.COREInterface.ActionManager.DeactivateActionTable(actionCallback, idActionTable);
                }

                // Clean up menu
                if (menu != null)
                {
                    Loader.Global.COREInterface.MenuManager.UnRegisterMenu(menu);
                    Loader.Global.ReleaseIMenu(menu);
                    Loader.Global.ReleaseIMenuItem(menuItemBabylon);
                    Loader.Global.ReleaseIMenuItem(menuItem);

                    menu = null;
                    menuItem = null;
                }
            }
            catch
            {
                // Fails silently
            }
        }

        public override uint Start
        {
            get
            {
                IIActionManager actionManager = Loader.Core.ActionManager;

                // Set up global actions
                idActionTable = (uint)actionManager.NumActionTables;

                string actionTableName = "Babylon Actions";
                actionTable = Loader.Global.ActionTable.Create(idActionTable, 0, ref actionTableName);
                babylonExportActionItem = new BabylonExportActionItem();
                actionTable.AppendOperation(babylonExportActionItem);
                actionTable.AppendOperation(new BabylonPropertiesActionItem()); // Babylon Properties forms are modals => no need to store reference
                actionTable.AppendOperation(new BabylonAnimationActionItem());
                actionTable.AppendOperation(new BabylonSaveAnimations());
                actionTable.AppendOperation(new BabylonLoadAnimations());
                actionTable.AppendOperation(new BabylonSkipFlattenToggle());

                actionCallback = new BabylonActionCallback();

                actionManager.RegisterActionTable(actionTable);
                actionManager.ActivateActionTable(actionCallback as ActionCallback, idActionTable);

                

                // Set up menus
#if MAX2018 || MAX2019
                var global = GlobalInterface.Instance;
                m_SystemStartupDelegate = new GlobalDelegates.Delegate5(MenuSystemStartupHandler);
                global.RegisterNotification(m_SystemStartupDelegate, null, SystemNotificationCode.SystemStartup);

                
#else
                InstallMenus();
#endif
                RegisterFilePreOpen();
                RegisterPostSceneReset();
                RegisterNodeAddedCallback();
                RegisterNodeDeletedCallback();
                return 0;
            }
        }

        public void RegisterFilePreOpen()
        {
            if (!filePreOpenCallback)
            {
                m_FilePreOpenDelegate = new GlobalDelegates.Delegate5(this.InitializeBabylonGuids);
                GlobalInterface.Instance.RegisterNotification(this.m_FilePreOpenDelegate, null, SystemNotificationCode.FilePreOpen );

                filePreOpenCallback = true;
            }
        }

        public void RegisterPostSceneReset()
        {
            if (!postSceneResetCallback)
            {
                m_PostSceneResetCallback = new GlobalDelegates.Delegate5(this.InitializeBabylonGuids);
                GlobalInterface.Instance.RegisterNotification(this.m_PostSceneResetCallback, null, SystemNotificationCode.PostSceneReset);

                postSceneResetCallback = true;
            }
        }
        

        public void RegisterNodeAddedCallback()
        {
            if (!nodeAddedCallback)
            {
                m_NodeAddedDelegate = new GlobalDelegates.Delegate5(this.OnNodeAdded);
#if MAX2015
                //bug on Autodesk API  SystemNotificationCode.SceneAddedNode doesn't work for max 2015-2016
                GlobalInterface.Instance.RegisterNotification(this.m_NodeAddedDelegate, null, SystemNotificationCode.NodeLinked );
#else
                GlobalInterface.Instance.RegisterNotification(this.m_NodeAddedDelegate, null, SystemNotificationCode.SceneAddedNode );
#endif
                nodeAddedCallback = true;
            }
        }

        public void RegisterNodeDeletedCallback()
        {
            if (!nodeDeleteCallback)
            {
                m_NodeDeleteDelegate = new GlobalDelegates.Delegate5(this.OnNodeDeleted);
                GlobalInterface.Instance.RegisterNotification(this.m_NodeDeleteDelegate, null, SystemNotificationCode.ScenePreDeletedNode);
                nodeDeleteCallback = true;
            }
        }

        private void InstallMenus()
        {
            IIMenuManager menuManager = Loader.Core.MenuManager;

            // Set up menu
            menu = menuManager.FindMenu("Babylon");

            if (menu != null)
            {
                menuManager.UnRegisterMenu(menu);
                Loader.Global.ReleaseIMenu(menu);
                menu = null;
            }

            // Main menu
            menu = Loader.Global.IMenu;
            menu.Title = "Babylon";
            menuManager.RegisterMenu(menu, 0);

            // Launch option
            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "&File Exporter";
            menuItemBabylon.ActionItem = actionTable[0];
            menu.AddItem(menuItemBabylon, -1);

            menuItem = Loader.Global.IMenuItem;
            menuItem.SubMenu = menu;

            menuManager.MainMenuBar.AddItem(menuItem, -1);

            // Quad
            var rootQuadMenu = menuManager.GetViewportRightClickMenu(RightClickContext.NonePressed);
            var quadMenu = rootQuadMenu.GetMenu(0);

            menu = menuManager.FindMenu("Babylon...");

            if (menu != null)
            {
                menuManager.UnRegisterMenu(menu);
                Loader.Global.ReleaseIMenu(menu);
                menu = null;
            }

            menu = Loader.Global.IMenu;
            menu.Title = "Babylon...";
            menuManager.RegisterMenu(menu, 0);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Properties";
            menuItemBabylon.ActionItem = actionTable[1];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Animation Groups";
            menuItemBabylon.ActionItem = actionTable[2];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Save Animation To Containers";
            menuItemBabylon.ActionItem = actionTable[3];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Load Animation From Containers";
            menuItemBabylon.ActionItem = actionTable[4];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Toggle Skip Flatten Status";
            menuItemBabylon.ActionItem = actionTable[5];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = "Babylon Actions Builder";
            menuItemBabylon.ActionItem = actionTable[6];
            menu.AddItem(menuItemBabylon, -1);

            menuItem = Loader.Global.IMenuItem;
            menuItem.SubMenu = menu;

            quadMenu.AddItem(menuItem, -1);

            Loader.Global.COREInterface.MenuManager.UpdateMenuBar();
        }
    }
}
