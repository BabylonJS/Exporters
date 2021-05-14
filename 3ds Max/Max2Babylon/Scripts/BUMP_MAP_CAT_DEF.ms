-- A script material utilities for Babylon Exporter
-- Modified: 2021-03-22
-- Copyright 2021 Microsoft, All rights reserved. This file is licensed under Apache 2.0 license

-- Bump Map custom attribute definition
BUMP_MAP_CAT_DEF = attributes "Babylon Attributes" attribID:#(0x4f890984, 0x24da8888)
version:1
(
    local description = "Help Export with the definition of the normal map format"
    parameters main rollout:params
    (
        babylonUseMaxTransforms type:#boolean ui:babylonUseMaxTransforms_ui default:false
        --Normal direction : Y+ or Y-, Y+ is OpenGL, Y- is Directx
        babylonNMY type:#integer ui:babylonNMY_ui default:1
        --Coordinate system : Right or Left. OpenGL is Right Handed when DirectX is Left handed
        babylonNMCoordinate type:#integer ui:babylonNMCoordinate_ui default:1
    )
    rollout params "Babylon Attributes"
    (
        edittext lvl readOnly:true text:description height:20
        checkbox babylonUseMaxTransforms_ui "Use 3dsMax Channel Direction"
        group "Normal Map Y" (
           radiobuttons  babylonNMY_ui labels:#("Unknown","Y+", "Y-") 
        ) 
        group "Normal Map Coordinate" (
           radiobuttons  babylonNMCoordinate_ui labels:#("Unknown","Right", "Left")
        )
           
        --instead of declaring the whole input format. We can add a bunch of button for preset values such DirectX, OpenGL, Clear
        group "Preset" (
           button SetDirectXBtn "DirectX" across:3
           button SetOpenGlBtn  "OpenGL"
           button ClearBtn "Clear"
        )
        
        on ClearBtn pressed do
        (
            babylonNMY = 1
            babylonNMCoordinate = 1
        )
        on SetOpenGlBtn pressed do
        (
            babylonNMY = 2
            babylonNMCoordinate = 2
        )
        on SetDirectXBtn pressed do 
        (
            babylonNMY = 3
            babylonNMCoordinate = 3
        )
    )    
)
