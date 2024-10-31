(
    function createBabylonMenuCB =
    (
        local actionTableId=47368
        local menuMgr= callbacks.notificationParam() 
        local mainMenu=menuMgr.mainMenuBar

        local babMenu = mainMenu.CreateSubMenu "01840AAF-46C6-49F3-9234-9FB704BE4278" "Babylon"
        babMenu.CreateAction "f801f1c3-1e38-48e6-a0b8-551866315985" actionTableId "1"
    )

    function createBabylonQuadMenuCB =
    (
        local actionTableId=47368
        local CuiParentQuadMenuId="f1af2d63-86f8-4f38-bb9e-e5a371a3f863"
        local quadMgr = callbacks.notificationParam() 
        local parent = quadMgr.GetMenuById CuiParentQuadMenuId
        local quadMenu=parent.CreateSubMenu "ffd098bb-9064-4d7b-9309-24936af41994" "Babylon..."

        quadMenu.CreateAction "b573c490-d450-4449-ac39-23294e6122a3" actionTableId "2"
        quadMenu.CreateAction "0dfe0096-3c09-4865-9254-11471dc9c3b4" actionTableId "3"
        quadMenu.CreateAction "e4d2053e-eb33-4541-a11a-8357bbdbe1a8" actionTableId "4"
        quadMenu.CreateAction "31ccb597-2be7-4768-830b-f040f5850474" actionTableId "5"
        quadMenu.CreateAction "b9637ae0-6118-46f8-85f6-2e019e108fb3" actionTableId "6"
    )
    callbacks.removeScripts id:#BabylonMenus

    callbacks.addScript #cuiRegisterMenus createBabylonMenuCB id:#BabylonMenus
    callbacks.addScript #cuiRegisterQuadMenus createBabylonQuadMenuCB id:#BabylonMenus
)
