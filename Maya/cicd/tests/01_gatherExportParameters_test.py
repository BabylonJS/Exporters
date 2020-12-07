
import unittest

try:
    import maya.standalone as ms
    ms.initialize()
except:
    pass

import libMayaExtended as lme

class Test(unittest.TestCase):
    def test_gatherExportParameters(self):
        lme.loadPlugin("Maya2Babylon.nll.dll")
        exportParams = lme.evalMelString("GenerateExportersParameter;")

if __name__ == "__main__":
    unittest.main()