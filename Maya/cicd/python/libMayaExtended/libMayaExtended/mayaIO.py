import inspect

def getCallingFunctionName():
    try:
        return inspect.stack()[2][3]
    except:
        return "UndeterminedFunction"

import maya.OpenMaya as OpenMaya

def debug(message):
    fullMsg = "Debug: %s: %s" % (getCallingFunctionName(), message)
    OpenMaya.MGlobal.displayInfo(fullMsg)

def info(message):
    fullMsg = "%s: %s" % (getCallingFunctionName(), message)
    OpenMaya.MGlobal.displayInfo(fullMsg)

def warn(message):
    fullMsg = "%s: %s" % (getCallingFunctionName(), message)
    OpenMaya.MGlobal.displayWarning(fullMsg)

def error(message):
    fullMsg = "%s: %s" % (getCallingFunctionName(), message)
    OpenMaya.MGlobal.displayError(fullMsg)