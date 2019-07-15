using Autodesk.Max;
using Autodesk.Max.IQuadMenuContext;
using Autodesk.Max.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        private static bool filePreOpenCallback = false;
        private GlobalDelegates.Delegate5 m_FilePreOpenDelegate;

        private static bool nodeAddedCallback = false;
        private GlobalDelegates.Delegate5 m_NodeAddedDelegate;
#endif

        private void MenuSystemStartupHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            InstallMenus();
        }

        private void InitializeBabylonGuids(IntPtr objPtr, INotifyInfo infoPtr)
        {
            Tools.guids = new Dictionary<Guid, IAnimatable>();
        }

        private void OnNodeAdded(IntPtr objPtr, INotifyInfo infoPtr)
        {
            try
            {
                IINode n = (IINode)infoPtr.CallParam;
                n.GetGuid(); // force to assigne a new guid if not exist yet for this node

                IIContainerObject contaner = Loader.Global.ContainerManagerInterface.IsContainerNode(n);
                if (contaner!=null)
                {
                    // a generic operation on a container is done (open/inherit)
                    Tools.guids = new Dictionary<Guid, IAnimatable>();
                }
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
                actionCallback = new BabylonActionCallback();

                actionManager.RegisterActionTable(actionTable);
                actionManager.ActivateActionTable(actionCallback as ActionCallback, idActionTable);

                // Set up menus
#if MAX2018 || MAX2019
                var global = GlobalInterface.Instance;
                m_SystemStartupDelegate = new GlobalDelegates.Delegate5(MenuSystemStartupHandler);
                global.RegisterNotification(m_SystemStartupDelegate, null, SystemNotificationCode.SystemStartup);
                RegisterFilePreOpen();
                RegisterNodeAddedCallback();
#else
                InstallMenus();
#endif
                return 0;
            }
        }

#if MAX2018 || MAX2019
        public void RegisterFilePreOpen()
        {
            if (!filePreOpenCallback)
            {
                m_FilePreOpenDelegate = new GlobalDelegates.Delegate5(this.InitializeBabylonGuids);
                GlobalInterface.Instance.RegisterNotification(this.m_FilePreOpenDelegate, null, SystemNotificationCode.FilePreOpen );

                filePreOpenCallback = true;
            }
        }

        public void RegisterNodeAddedCallback()
        {
            if (!nodeAddedCallback)
            {
                m_NodeAddedDelegate = new GlobalDelegates.Delegate5(this.OnNodeAdded);
                GlobalInterface.Instance.RegisterNotification(this.m_NodeAddedDelegate, null, SystemNotificationCode.SceneAddedNode );

                nodeAddedCallback = true;
            }
        }
#endif

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
            menuItemBabylon.Title = "Babylon Actions Builder";
            menuItemBabylon.ActionItem = actionTable[3];
            menu.AddItem(menuItemBabylon, -1);

            menuItem = Loader.Global.IMenuItem;
            menuItem.SubMenu = menu;

            quadMenu.AddItem(menuItem, -1);

            Loader.Global.COREInterface.MenuManager.UpdateMenuBar();
        }
    }
}
