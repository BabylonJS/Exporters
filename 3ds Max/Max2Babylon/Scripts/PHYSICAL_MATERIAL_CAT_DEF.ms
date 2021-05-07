-- A script material utilities for Babylon Exporter
-- Modified: 2021-03-22
-- Copyright 2021 Microsoft, All rights reserved. This file is licensed under Apache 2.0 license

-- Physical material custom attribute definition
PHYSICAL_MATERIAL_CAT_DEF = attributes "Babylon Attributes" attribID:#(0x4f890715, 0x24da1759)
version:2
(
    local description = "Babylon attributes support Babylon-specific parameters."
    parameters main rollout:params
    (
        babylonUnlit type:#boolean ui:babylonUnlit_ui
        babylonBackfaceCulling type:#boolean ui:babylonBackfaceCulling_ui default:false
        babylonSeparateCullingPass type:#boolean ui:babylonSeparateCullingPass_ui default:false
        babylonUseFactors type:#boolean ui:babylonUseFactors_ui default:false
        babylonMaxSimultaneousLights type:#integer ui:babylonMaxSimultaneousLights_ui default:4
        babylonDirectIntensity type:#float ui:babylonDirectIntensity_ui default:1.0
        babylonEmissiveIntensity type:#float ui:babylonEmissiveIntensity_ui default:1.0
        babylonEnvironmentIntensity type:#float ui:babylonEnvironmentIntensity_ui default:1.0
        babylonSpecularIntensity type:#float ui:babylonSpecularIntensity_ui default:1.0
        babylonTransparencyMode type:#integer default:0
    )
    rollout params "Babylon Attributes"
    (
        edittext lvl readOnly:true text:description height:20
        checkbox babylonUnlit_ui "Unlit" across:3
        checkbox babylonBackfaceCulling_ui "Backface Culling"
        checkbox babylonSeparateCullingPass_ui "Separate Culling Pass"
        checkbox babylonUseFactors_ui "Use Scalar Factors"

        label babylonMaxSimultaneousLights_label "Max Simultaneous Lights" Align: #Left across:2
        spinner babylonMaxSimultaneousLights_ui Align: #Right type: #integer Range:[0,100,4]
        group "PBR" (
            label babylonDirectIntensity_label "Direct Intensity " Align: #Left across:2
            spinner babylonDirectIntensity_ui  Align: #Right type: #float Range:[0.0,1.0,1.0]

            label babylonEmissiveIntensity_label "Emissive Intensity " Align: #Left across:2
            spinner babylonEmissiveIntensity_ui Align: #Right type: #float Range:[0.0,1.0,1.0]

            label babylonEnvironmentIntensity_label "Environment Intensity " Align: #Left across:2
            spinner babylonEnvironmentIntensity_ui Align: #Right type: #float Range:[0.0,1.0,1.0]

            label babylonSpecularIntensity_label "Specular Intensity " Align: #Left across:2
            spinner babylonSpecularIntensity_ui Align: #Right type: #float Range:[0.0,1.0,1.0]
        )

        label babylonTransparencyMode_label "Transparency Mode " Align: #Left across:2
        dropdownlist babylonTransparencyMode_dd  items:# ("Opaque","Cutoff","Blend") selection:(babylonTransparencyMode+1) Align: #Right
        on babylonTransparencyMode_dd selected i do babylonTransparencyMode = i-1
    )    
)