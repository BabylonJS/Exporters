import c4d
import os
import sys
from c4d import gui, plugins, bitmaps, Vector

PLUGIN_ID = 1054500
__version__ = "1.0"
__plugin_title__ = "BJS Expose Variable Tag"
__author__ = "Pryme8"

BJS_EXPOSE_VARIABLE_TRUE     = 10001
BJS_EXPOSE_GLOBAL       = 10002
BJS_VARIABLE_NAME       = 10003

class BJS_Expose_Variable(plugins.TagData):
    pass #code 
    
    def Init(self, node):
        data = node.GetDataInstance()        
        data.SetBool(BJS_EXPOSE_VARIABLE_TRUE,   True)
        data.SetBool(BJS_EXPOSE_GLOBAL,     False)
        data.SetString(BJS_VARIABLE_NAME,   "New_Variable")
        
        return True
    
    def Execute(self, node, doc, op, bt, priority, flags):
        data = node.GetDataInstance()        
        isExposed = data.GetBool(BJS_EXPOSE_VARIABLE_TRUE)
        isGlobal = data.GetBool(BJS_EXPOSE_GLOBAL)
        
        return c4d.EXECUTIONRESULT_OK

if __name__ == "__main__":
    bmp = bitmaps.BaseBitmap()
    dir, file = os.path.split(__file__)
    bitmapfile = os.path.join(dir, "res", "icon.png")
    
    result = bmp.InitWith(bitmapfile)
    
    if not result:
        print "Error loading Icon!"
    
    okyn = plugins.RegisterTagPlugin(id=PLUGIN_ID, str=__plugin_title__, info=c4d.TAG_VISIBLE|c4d.TAG_EXPRESSION, g=BJS_Expose_Variable, description="BJS_Expose_Variable", icon=bmp)
    print "BJS_Expose_Variable Initialized", okyn