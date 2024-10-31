using Autodesk.Max;
using Autodesk.Max.Plugins;

#if MAX2025 || MAX2026
using UiViewModels.Actions;
#else
using Autodesk.Max.IQuadMenuContext;
#endif

using System;
using System.Collections.Generic;

namespace Max2Babylon
{
    static class CuiTitles
    {
        public const string MenuTitle = "Babylon...";
        public const string GlobalTitle = "Babylon";
        public const string PropertiesTitle = "Babylon Properties";
        public const string AnimationGroupsTitle = "Babylon Animation Groups";
        public const string ActionsBuilderTitle = "Babylon Actions Builder";
        public const string SkipFlattenTitle = "Babylon Toggle Skip Flatten Status";
        public const string LoadAnimationTitle = "Babylon Load Animation From Containers";
        public const string SaveAnimationTitle = "Babylon Save Animation To Containers";
        public const string FileExporterTitle = "File Exporter";
    }

#if MAX2025 || MAX2026
    public class DummyCommandAdapter : CuiActionCommandAdapter
    {
        public const string DummyActionTitle = "BabylonDummyAction";
        public override string InternalActionText => DummyActionTitle;
        public override string InternalCategory => CuiTitles.GlobalTitle;
        public override string ActionText => InternalActionText;
        public override string Category => InternalCategory;
        public override void Execute(object parameter)
        {
            Loader.Global.COREInterface.DisplayTempPrompt("Babylon Dummy Action Adapter", 10);
        }

        // Clear and return ActionTable registered by the DummyAdapter
        public static IActionTable GetDummyActionTable()
        {
            var actionManager = Loader.Core.ActionManager;
            //actionManager.FindTable();
            for(int actionTableIndex = 0; actionTableIndex < actionManager.NumActionTables; ++actionTableIndex)
            {
                var theTable = actionManager.GetTable(actionTableIndex);

                for(int i = 0; i < theTable.Count; ++i)
                {
                    // if we found our known dummy action, remove it and return the table
                    var action = theTable[i];
                    if(action?.DescriptionText == DummyActionTitle)
                    {
                        theTable.DeleteOperation(action);
                        return theTable;
                    }
                }
            }

            return null;
        }
    }

#endif

    class GlobalUtility : GUP
    {
        public const string ActionTableName = "Babylon Actions";
        public const string GUIDPropertyName = "babylonjs_GUID";


#if MAX2025 || MAX2026
        // Placeholder
        public static readonly string CreateMenuScript= System.Text.Encoding.UTF8.GetString(Properties.Resources.CreateBabylonMenus);

        private static bool registerMenusCallback = false;
        private GlobalDelegates.Delegate5 m_registerMenuDelegate;
        private GlobalDelegates.Delegate5 m_registerQuadMenuDelegate;
#else
        IIMenu menu;
        IIMenuItem menuItem;
        IIMenuItem menuItemBabylon;
#endif
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
            AddCallbacks();
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
                string guid = n.GetStringProperty( GUIDPropertyName, string.Empty);
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

#if MAX2025 || MAX2026

        /// <summary>
        /// Force 3ds Max 2025.0 menusystem refresh, required for pre 2025.3 versions
        /// </summary>
        public static void ForceICuiMenuRefresh()
        {
            string script = "(\n" +
                                "local menuMgr = maxops.GetICuiMenuMgr()\n" +
                                "menuMgr.LoadConfiguration(menuMgr.GetCurrentConfiguration())\n" +

                                "local quadMenuMgr = maxOps.GetICuiQuadMenuMgr()\n" +
                                "quadMenuMgr.LoadConfiguration(quadMenuMgr.GetCurrentConfiguration())\n" +
                            ")\n";

            ScriptsUtilities.ExecuteMaxScriptCommand(script);
        }
#endif

        private void OnNodeAdded(IntPtr objPtr, INotifyInfo infoPtr)
        {
            try
            {
                IINode n = (IINode)infoPtr.CallParam;
                //todo replace this with something like isXREFNODE
                //to have a distinction between added xref node and max node
                string guid = n.GetStringProperty( GUIDPropertyName, string.Empty);
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
#if MAX2025 || MAX2026
                // Placeholder
                // no cleanup necessary for the new menu system
#else
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
#endif
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
                
#if MAX2025 || MAX2026
                actionTable = DummyCommandAdapter.GetDummyActionTable();

                if(actionTable != null)
                    idActionTable = actionTable.Id_;

#elif MAX2022 || MAX2023 || MAX2024
                actionTable = Loader.Global.ActionTable.Create(idActionTable, 0 , ActionTableName);
#else
                string actionTableName = ActionTableName;
                actionTable = Loader.Global.ActionTable.Create(idActionTable, 0, ref actionTableName);
#endif
                // prevent null exceptions 
                if(actionTable == null)
                    return 0;

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
                AddCallbacks();
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
                GlobalInterface.Instance.RegisterNotification(this.m_FilePreOpenDelegate, null, SystemNotificationCode.FilePreOpen);

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
                GlobalInterface.Instance.RegisterNotification(this.m_NodeAddedDelegate, null, SystemNotificationCode.SceneAddedNode);
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
#if MAX2025 || MAX2026
            var maxVer = Tools.GetMaxVersion();

            // with 2025.3 and up we could also use ICUIMenuManager ( Loader.Core.ICuiMenuManager / ICuiQuadMEnuManager )
            ScriptsUtilities.ExecuteMaxScriptCommand(CreateMenuScript);
            
            // force menu refresh, as this is broken in 3dsMax versions 2025.0, 2025.1 and 2025.2
            if( maxVer.Major==27 && maxVer.Minor < 3 )
            {
                ForceICuiMenuRefresh();
                ScriptsUtilities.ExecuteMaxScriptCommand($"format \"Max2Babylon: forced menu refresh...\n\"");
            }
#else
            IIMenuManager menuManager = Loader.Core.MenuManager;

            // Set up menu
            menu = menuManager.FindMenu( CuiTitles.GlobalTitle ); // "Babylon"

            if (menu != null)
            {
                menuManager.UnRegisterMenu(menu);
                Loader.Global.ReleaseIMenu(menu);
                menu = null;
            }

            // Main menu
            menu = Loader.Global.IMenu;
            menu.Title = CuiTitles.GlobalTitle; // "Babylon"
            menuManager.RegisterMenu(menu, 0);

            // Launch option
            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = CuiTitles.FileExporterTitle; // "&File Exporter";
            menuItemBabylon.ActionItem = actionTable[0];
            menu.AddItem(menuItemBabylon, -1);

            menuItem = Loader.Global.IMenuItem;
            menuItem.SubMenu = menu;

            menuManager.MainMenuBar.AddItem(menuItem, -1);

            // Quad
            var rootQuadMenu = menuManager.GetViewportRightClickMenu(RightClickContext.NonePressed);
            var quadMenu = rootQuadMenu.GetMenu(0);

            menu = menuManager.FindMenu( CuiTitles.MenuTitle ) ; // "Babylon...");

            if (menu != null)
            {
                menuManager.UnRegisterMenu(menu);
                Loader.Global.ReleaseIMenu(menu);
                menu = null;
            }

            menu = Loader.Global.IMenu;
            menu.Title = CuiTitles.MenuTitle; // "Babylon...";
            menuManager.RegisterMenu(menu, 0);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = CuiTitles.PropertiesTitle; // "Babylon Properties";
            menuItemBabylon.ActionItem = actionTable[1];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = CuiTitles.AnimationGroupsTitle; // "Babylon Animation Groups";
            menuItemBabylon.ActionItem = actionTable[2];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = CuiTitles.SaveAnimationTitle; // "Babylon Save Animation To Containers";
            menuItemBabylon.ActionItem = actionTable[3];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = CuiTitles.LoadAnimationTitle; // "Babylon Load Animation From Containers";
            menuItemBabylon.ActionItem = actionTable[4];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = CuiTitles.SkipFlattenTitle; // "Babylon Toggle Skip Flatten Status";
            menuItemBabylon.ActionItem = actionTable[5];
            menu.AddItem(menuItemBabylon, -1);

            menuItemBabylon = Loader.Global.IMenuItem;
            menuItemBabylon.Title = CuiTitles.ActionsBuilderTitle; // "Babylon Actions Builder";
            menuItemBabylon.ActionItem = actionTable[6];
            menu.AddItem(menuItemBabylon, -1);

            menuItem = Loader.Global.IMenuItem;
            menuItem.SubMenu = menu;

            quadMenu.AddItem(menuItem, -1);

            Loader.Global.COREInterface.MenuManager.UpdateMenuBar();
#endif
            }

        private void AddCallbacks() 
        {
            foreach (var s in MaterialScripts.AddCallbacks())
#if MAX2022 || MAX2023 || MAX2024 || MAX2025|| MAX2026
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(s,ManagedServices.MaxscriptSDK.ScriptSource.NotSpecified);
#else
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(s);
#endif
       }
    }
}
