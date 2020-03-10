import c4d
import os
import sys
from c4d import gui, plugins, bitmaps, Vector

PLUGIN_ID = 1054495
__version__ = "1.1"
__plugin_title__ = "BABYLON Light Tag"
__author__ = "Pryme8"

#=================================
# DECLATRIONS
#=================================

#BJS_LIGHT_SETTINGS                     = 1000
BJS_LIGHT_SPECULAR                      = 10001     
BJS_LIGHT_EXPONENT                      = 10002
BJS_LIGHT_EXPONENT_SIMULATE_PREVIEW     = 10003

BJS_LIGHT_MAKE_HEMISPHERIC              = 10004
BJS_LIGHT_GROUND_COLOR                  = 10005

BJS_LIGHT_ANIMAION_SETTINGS             = 2000
BJS_LIGHT_AUTO_ANIMATE                  = 20000
BJS_LIGHT_AUTO_ANIMATE_FROM             = 20001
BJS_LIGHT_AUTO_ANIMATE_TO               = 20002
BJS_LIGHT_AUTO_ANIMATE_LOOP             = 20003
BJS_LIGHT_AUTO_ANIMATE_SPEED            = 20004

#---------------------------------
class BJS_Light_Tag(plugins.TagData):
    pass #code
    
    def Init(self, node):
        self.dirty = True
        data = node.GetDataInstance()        
        data.SetVector(BJS_LIGHT_SPECULAR, Vector(1,1,1))
        data.SetReal(BJS_LIGHT_EXPONENT, 16)        
        data.SetBool(BJS_LIGHT_MAKE_HEMISPHERIC, False)
        data.SetVector(BJS_LIGHT_GROUND_COLOR, Vector(0,0,0))
        
        data.SetBool( BJS_LIGHT_AUTO_ANIMATE, False)
        data.SetLong( BJS_LIGHT_AUTO_ANIMATE_FROM, 0)
        data.SetLong( BJS_LIGHT_AUTO_ANIMATE_TO, 0)
        data.SetBool( BJS_LIGHT_AUTO_ANIMATE_LOOP, False)
        data.SetFloat( BJS_LIGHT_AUTO_ANIMATE_SPEED, 1)
        
        return True
    
    def checkHemi(self, node):
        data = node.GetDataInstance()  
        hemi = data.GetBool(BJS_LIGHT_MAKE_HEMISPHERIC)
        
        groundLight = node.GetObject().GetDown()
        print groundLight
        if hemi == True:
            if groundLight == None:
                groundLight = node.GetObject().GetClone()
                lightTag = groundLight.GetFirstTag()
                lightTag.Remove()                           
                groundLight.SetName(node.GetObject().GetName()+'_GroundLight')
                doc = c4d.documents.GetActiveDocument()
                doc.InsertObject(groundLight, parent=node.GetObject())
                rl = groundLight.GetAbsRot()
                groundLight.SetAbsRot(Vector(rl[0], rl[1]-1.57079632679, rl[2]))
            
            groundLight[c4d.LIGHT_COLOR] = data.GetVector(BJS_LIGHT_GROUND_COLOR)
        else:
            if groundLight is not None:
                groundLight.Remove()        

        
    def Execute(self, node, doc, op, bt, priority, flags):
        data = node.GetDataInstance()  
        data.SetBool( BJS_LIGHT_AUTO_ANIMATE, False)
        data.SetLong( BJS_LIGHT_AUTO_ANIMATE_FROM, 0)
        data.SetLong( BJS_LIGHT_AUTO_ANIMATE_TO, 0)
        data.SetBool( BJS_LIGHT_AUTO_ANIMATE_LOOP, False)
        data.SetFloat( BJS_LIGHT_AUTO_ANIMATE_SPEED, 1)
    
        return c4d.EXECUTIONRESULT_OK
        
    def Message(self, node, type, data):       
        if type ==  c4d.MSG_DESCRIPTION_POSTSETPARAMETER:
            if data['descid'][0].id == BJS_LIGHT_MAKE_HEMISPHERIC:
                self.checkHemi( node )
                
            if data['descid'][0].id == BJS_LIGHT_GROUND_COLOR:
                if node.GetDataInstance().GetBool(BJS_LIGHT_MAKE_HEMISPHERIC) == True:
                    groundLight = node.GetObject().GetDown()
                    groundLight[c4d.LIGHT_COLOR] = node.GetDataInstance().GetVector(BJS_LIGHT_GROUND_COLOR)                
            
        
        return True

if __name__ == "__main__":
    bmp = bitmaps.BaseBitmap()
    dir, file = os.path.split(__file__)
    bitmapfile = os.path.join(dir, "res", "icon.png")
    
    result = bmp.InitWith(bitmapfile)
    
    if not result:
        print "Error loading Icon!"
    
    okyn = plugins.RegisterTagPlugin(id=PLUGIN_ID, str="BJS Light Tag", info=c4d.TAG_VISIBLE|c4d.TAG_EXPRESSION, g=BJS_Light_Tag, description="BJS_Light_Tag", icon=bmp)
    print "BJS_Light_Tag Initialized", okyn