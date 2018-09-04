using Autodesk.Max;
using Autodesk.Max.IQuadMenuContext;
using Autodesk.Max.Plugins;
using System;

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

        private void MenuSystemStartupHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            InstallMenus();
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
#else
                InstallMenus();
#endif

                return 0;
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
