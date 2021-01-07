
import pathlib
import pytest

try:
    import maya.standalone as ms
    ms.initialize()
except:
    pass

import libMayaExtended as lme

def test_texturedSphereExport( ):
    lme.loadPlugin("mtoa")
    lme.loadPlugin("Maya2Babylon.nll.dll")

    sphere = lme.sphere("Sphere")
    shader = lme.createAssignShader(sphere, "aiStandardSurface", "Sphere_PBR")

    texturePath = "%s\\resources\\uv_layout.png" % pathlib.Path(__file__).parent.absolute()
    texture = lme.createTextureFile("file", texturePath, "Sphere_Albedo")
    lme.connect("%s.outColor" % texture, "%s.baseColor" % shader)

    lme.select(sphere)

    exportPath = "%s\\temp\\03_SphereTextured" % pathlib.Path(__file__).parent.absolute()
    exportParams = [
        exportPath,         # export path
        "gltf",             # export format
        "",                 # textures path
        "1",                # scale factor
        "True",             # write textures
        "True",             # overwrite textures
        "False",            # export hidden objects
        "True",             # export materials
        "True",             # export selected
        "False",            # bake animation
        "False",            # optimize animation
        "True",             # optimize vertices
        "False",            # animation group non-exported (?)
        "False",            # generate manifest
        "False",            # auto-save scene
        "True",             # export tangents
        "False",            # export skins
        "False",            # export morph targets
        "True",             # export normals
        "100",              # texture quality
        "False",            # create MRAO (RGB) maps
        "False",            # draco compression
        "False",            # enable KHR_LIGHTS_PUNCTUAL
        "True",             # enable KHR_TEXTURE_TRANSFORM
        "False",            # enable KHR_MATERIALS_UNLIT
        "False",            # PBR full
        "False",            # PBR no lights
        "False",            # create default skybox
        "",                 # environment path
        "False",            # export animation
        "False",            # export animation only
        "True"              # export textures
    ]
    lme.evalMelString("ScriptToBabylon -exportParameters %s;" % lme.convertStringsToMelArray(exportParams))