import maya.mel as mm

def evalMelString(pyString):
    return mm.eval(pyString)
    
def convertStringsToMelArray(pyStrings):
    return str([str(x) for x in pyStrings]).replace("'","\"").replace("[","{").replace("]", "}")