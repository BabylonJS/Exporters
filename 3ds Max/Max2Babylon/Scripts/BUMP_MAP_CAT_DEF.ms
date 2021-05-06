-- A script material utilities for Babylon Exporter
-- Modified: 2021-03-22
-- Copyright 2021 Microsoft, All rights reserved. This file is licensed under Apache 2.0 license

-- Bump Map custom attribute definition
BUMP_MAP_CAT_DEF = attributes "Babylon Attributes" attribID:#(0x4f890716, 0x24da1760)
version:2
(
    parameters main rollout:params
    (
        babylonUseMaxTransforms type:#boolean ui:babylonUseMaxTransforms_ui default:false
        --Normal direction : Y+ or Y-, Y+ is OpenGL, Y- is Directx
        babylonNMY type:#integer ui:babylonBumpFormat_ui default:0
        --Coordinate system : Right or Left. OpenGL is Right Handed when DirectX is Left handed
        babylonNMCoordinate type:#integer ui:babylonBumpFormat_ui default:0
    )
    rollout params "Babylon Attributes"
    (
        checkbox babylonUseMaxTransforms_ui "Use 3dsMax Channel directions"
        group "Normal Map Setting" (
            radiobuttons  babylonNMY labels:#("Unknown","Y+", "Y-") 
            radiobuttons  babylonNMCoordinate labels:#("Unknown","Right", "Left") 
            --instead of declaring the whole input format. We can add a bunch of button for preset values such DirectX, OpenGL, Clear
            button SetDirectXBtn "DirectX"
            button SetOpenGlBtn  "OpenGL"
            button ClearBtn "Clear"
        )
        on ClearBtn pressed do
        (
            babylonNMY = 0
            babylonNMCoordinate = 0
        )
        on SetOpenGlBtn pressed do
        (
            babylonNMY = 1
            babylonNMCoordinate = 1
        )
        on SetDirectXBtn pressed do 
        (
            babylonNMY = 2
            babylonNMCoordinate = 2
        )
    )    
)
