
import pytest

try:
    import maya.standalone as ms
    ms.initialize()
except:
    pass

import libMayaExtended as lme

def test_gatherExportParameters( ):
    lme.loadPlugin("Maya2Babylon.nll.dll")
    exportParams = lme.evalMelString("GenerateExportersParameter;")