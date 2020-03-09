import c4d
import os
import sys
from c4d import gui, plugins, bitmaps, Vector

PLUGIN_ID = 1054494
__version__ = "1.1"
__plugin_title__ = "BABYLON Camera Tag"
__author__ = "Pryme8"

#=================================
# DECLATRIONS
#=================================
#BJS_CAMERA_SETTINGS                = 1000
BJS_CAMERA_TYPE                     = 10001
BJS_CAMERA_TYPE_COMBO               = 100010
BJS_CAMERA_TYPE_FREE                = 100011
BJS_CAMERA_TYPE_ARC                 = 100012
BJS_CAMERA_TYPE_FOLLOW              = 100013
BJS_CAMERA_FOV                      = 10002
BJS_CAMERA_MINZ                     = 10003
BJS_CAMERA_MAXZ                     = 10004
BJS_CAMERA_SPEED                    = 10005
BJS_CAMERA_INERTIA                  = 10006
BJS_CAMERA_MAKE_ACTIVE              = 10007

#BJS_ARC_CAMERA_SETTINGS            = 2000
BJS_CAMERA_ALPHA                    = 20001
BJS_CAMERA_BETA                     = 20002
BJS_CAMERA_RADIUS                   = 20003
BJS_CAMERA_EYE_SPACE                = 20004

#BJS_FOLLOW_CAMERA_SETTINGS         = 3000
BJS_FOLLOW_CAMERA_HEIGHT_OFFSET     = 30001
BJS_FOLLOW_CAMERA_ROTATION_OFFSET   = 30002

#BJS_CAMERA_MISC_SETTINGS           = 4000
BJS_CAMERA_ATTACH_CONTROLS          = 40000
BJS_CAMERA_RIG_MODE                 = 40001
BJS_CAMERA_CHECK_COLLISIONS         = 40002
BJS_CAMERA_APPLY_GRAVITY            = 40003
BJS_CAMERA_ELLIPSOID                = 40004

#BJS_CAMERA_ANIMAION_SETTINGS       = 5000
BJS_CAMERA_AUTO_ANIMATE             = 50001
BJS_CAMERA_AUTO_ANIMATE_FROM        = 50002
BJS_CAMERA_AUTO_ANIMATE_TO          = 50003
BJS_CAMERA_AUTO_ANIMATE_LOOP        = 50004
BJS_CAMERA_AUTO_ANIMATE_SPEED       = 50005
#---------------------------------
#"tags": string,
#"parentId": string,
#"lockedTargetId": string,
#"position": vector3,
#"target": vector3,
#"animations": array of Animations (see below, can be omitted),
#"inputmgr" : map of camera inputs (can be omitted, see below)

class BJS_Camera_Tag(plugins.TagData):
    pass #code
    
    def Init(self, node):
        data = node.GetDataInstance()        
        data.SetLong( BJS_CAMERA_TYPE, BJS_CAMERA_TYPE_FREE )
        data.SetFloat( BJS_CAMERA_FOV, 0.8 )
        data.SetFloat( BJS_CAMERA_MINZ, 1 )
        data.SetFloat( BJS_CAMERA_MAXZ, 10000 )
        data.SetFloat( BJS_CAMERA_SPEED, 2 )
        data.SetFloat( BJS_CAMERA_INERTIA, 0.9 )        
        data.SetBool(  BJS_CAMERA_MAKE_ACTIVE, True)        
        data.SetFloat( BJS_CAMERA_ALPHA, 1 )
        data.SetFloat( BJS_CAMERA_BETA, 0.5 )
        data.SetFloat( BJS_CAMERA_RADIUS, 2 )
        data.SetFloat( BJS_CAMERA_EYE_SPACE, 0.1 )        
        data.SetFloat( BJS_FOLLOW_CAMERA_HEIGHT_OFFSET, 0.0 )
        data.SetFloat( BJS_FOLLOW_CAMERA_ROTATION_OFFSET, 0.0 )        
        data.SetBool( BJS_CAMERA_ATTACH_CONTROLS, False)
        data.SetLong( BJS_CAMERA_RIG_MODE, 0 )
        data.SetBool( BJS_CAMERA_CHECK_COLLISIONS, False)
        data.SetBool( BJS_CAMERA_APPLY_GRAVITY, False)
        data.SetVector( BJS_CAMERA_ELLIPSOID, Vector(0.5,1,0.5)) 
        data.SetBool( BJS_CAMERA_AUTO_ANIMATE, False)
        data.SetLong( BJS_CAMERA_AUTO_ANIMATE_FROM, 0)
        data.SetLong( BJS_CAMERA_AUTO_ANIMATE_TO, 0)
        data.SetBool( BJS_CAMERA_AUTO_ANIMATE_LOOP, False)
        data.SetFloat( BJS_CAMERA_AUTO_ANIMATE_SPEED, 1)
        
        return True
    
    def Execute(self, node, doc, op, bt, priority, flags):

        return c4d.EXECUTIONRESULT_OK

if __name__ == "__main__":
    bmp = bitmaps.BaseBitmap()
    dir, file = os.path.split(__file__)
    bitmapfile = os.path.join(dir, "res", "icon.png")
    
    result = bmp.InitWith(bitmapfile)
    
    if not result:
        print "Error loading Icon!"
    
    okyn = plugins.RegisterTagPlugin(id=PLUGIN_ID, str="BJS_Camera_Tag", info=c4d.TAG_VISIBLE|c4d.TAG_EXPRESSION, g=BJS_Camera_Tag, description="BJS_Camera_Tag", icon=bmp)
    print "BJS_Camera_Tag Initialized", okyn