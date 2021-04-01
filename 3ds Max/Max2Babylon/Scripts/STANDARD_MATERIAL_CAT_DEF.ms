-- A script material utilities for Babylon Exporter
-- Modified: 2021-03-22
-- Copyright 2021 Microsoft, All rights reserved. This file is licensed under Apache 2.0 license

-- Physical material custom attribute definition
STANDARD_MATERIAL_CAT_DEF = attributes "Babylon Attributes" attribID:#(0x360393c4, 0x6cfefa59)
version:2
(
    parameters main rollout:params
    (
        babylonUnlit type:#boolean ui:babylonUnlit_ui
        babylonMaxSimultaneousLights type:#integer ui:babylonMaxSimultaneousLights_ui default:4
        babylonTransparencyMode type:#integer default:0
    )
    rollout params "Babylon Attributes"
    (
        checkbox babylonUnlit_ui "Unlit" across:3
        label babylonMaxSimultaneousLights_label "Max Simultaneous Lights" Align: #Left across:2
        spinner babylonMaxSimultaneousLights_ui Align: #Right type: #integer Range:[0,100,4]
        label babylonTransparencyMode_label "Transparency Mode " Align: #Left across:2
        dropdownlist babylonTransparencyMode_dd  items:# ("Opaque","Cutoff","Blend") selection:(babylonTransparencyMode+1) Align: #Right
        on babylonTransparencyMode_dd selected i do babylonTransparencyMode = i-1
    )    
)
