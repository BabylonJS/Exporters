
import unittest

try:
    import maya.standalone as ms
    ms.initialize()
except:
    pass

import libMayaExtended as lme

class Test(unittest.TestCase):
    def test_simpleSphereExport(self):
        lme.loadPlugin("mtoa")
        lme.loadPlugin("Maya2Babylon.nll.dll")

        sphere = lme.sphere("Sphere")
        lme.createAssignShader(sphere, "aiStandardSurface", "Sphere_PBR")
        lme.select(sphere)

        exportPath = "D:\\Home\\Documents\\maya\\projects\\default\\exports\\Sphere"
        exportParams = [
            exportPath,         # export path
            "gltf",             # export format
            "",                 # textures path
            "1",                # scale factor
            "False",            # write textures
            "False",            # overwrite textures
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
            "False",            # enable KHR_TEXTURE_TRANSFORM
            "False",            # enable KHR_MATERIALS_UNLIT
            "False",            # PBR full
            "False",            # PBR no lights
            "False",            # create default skybox
            "",                 # environment path
            "False",            # export animation
            "False",            # export animation only
            "False"             # export textures
        ]
        #------------------------------------------------------------------------------------------
        # this fails on the command line every time, but succeeds in editor
        # the dotnet command can be accessed via `GenerateExportersParameter` as in test 01
        #------------------------------------------------------------------------------------------
        # lme.evalMelString("ScriptToBabylon -exportParameters %s;" % lme.convertStringsToMelArray(exportParams))

if __name__ == "__main__":
    unittest.main()