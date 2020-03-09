import c4d
import os
import sys
from c4d import gui, plugins, bitmaps, Vector

PLUGIN_ID = 1054685
__version__ = "1.0"
__plugin_title__ = "BJS Ignore Tag"
__author__ = "Pryme8"

#BJS_IGNORE_ELEMENT_SETTINGS = 1000
BJS_IGNORE_BOOL     = 10000

class BJS_Ignore_Tag(plugins.TagData):
    pass #code 
    
    def Init(self, node):
        data = node.GetDataInstance()        
        data.SetBool(BJS_IGNORE_BOOL,   True)
        
        return True
    
    def Execute(self, node, doc, op, bt, priority, flags):
        data = node.GetDataInstance()        
        isExposed = data.GetBool(BJS_IGNORE_BOOL)
        
        return c4d.EXECUTIONRESULT_OK

if __name__ == "__main__":
    bmp = bitmaps.BaseBitmap()
    dir, file = os.path.split(__file__)
    bitmapfile = os.path.join(dir, "res", "icon.png")
    
    result = bmp.InitWith(bitmapfile)
    
    if not result:
        print "Error loading Icon!"
    
    okyn = plugins.RegisterTagPlugin(id=PLUGIN_ID, str=__plugin_title__, info=c4d.TAG_VISIBLE|c4d.TAG_EXPRESSION, g=BJS_Ignore_Tag, description="BJS_Ignore_Tag", icon=bmp)
    print "BJS_Ignore_Tag Initialized", okyn