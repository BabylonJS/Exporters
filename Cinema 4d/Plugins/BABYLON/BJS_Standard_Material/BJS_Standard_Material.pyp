import c4d
import os
import sys
from c4d import gui, plugins, bitmaps, Vector

PLUGIN_ID = 1054508
__version__ = "1.0"
__plugin_title__ = "BABYLON Standard Material Tag"
__author__ = "Pryme8"


#=================================
#SCENE CONTROL DECLATRIONS
#=================================
BJS_MATERIAL_NAME               =   100001
BJS_MATERIAL_COLOR_AMBIENT      =   100002
BJS_MATERIAL_COLOR_DIFFUSE      =   100003
BJS_MATERIAL_COLOR_EMISSIVE     =   100004
BJS_MATERIAL_COLOR_SPECULAR     =   100005

#---------------------------------


def FindLastTag(op):
    ttag = op.GetFirstTag()
    ttag1 = None
    while(ttag):
        ttag1 = ttag;
        ttag = ttag.GetNext()
    if ttag1 is not None:
        return ttag1
    else:
        return None

#=================================
#CLASS BJS_Standard_Material
#=================================
class BJS_Standard_Material(plugins.TagData):
    pass #code
    
    def Init(self, node):
        self.dirty = True
        data = node.GetDataInstance()
        data.SetString(BJS_MATERIAL_NAME, "New_Standard_Material")
        data.SetVector(BJS_MATERIAL_COLOR_AMBIENT, Vector(0, 0, 0))
        data.SetVector(BJS_MATERIAL_COLOR_DIFFUSE, Vector(0.6, 0.6, 0.6))
        data.SetVector(BJS_MATERIAL_COLOR_EMISSIVE, Vector(0, 0, 0))
        data.SetVector(BJS_MATERIAL_COLOR_SPECULAR, Vector(1, 1, 1))        
        self.mat = c4d.Material(c4d.Mbase)
        
        self.mat[c4d.MATERIAL_USE_COLOR] = True
        self.mat[c4d.MATERIAL_USE_LUMINANCE] = True
        self.mat[c4d.MATERIAL_USE_REFLECTION] = True
        self.mat[c4d.MATERIAL_USE_ENVIRONMENT] = True
        
        return True
    
    def updateInteralMat(self, node, doc):
        if self.dirty:
            data = node.GetDataInstance()
            mName = data.GetString(BJS_MATERIAL_NAME)
            self.mat.SetName(mName)
            colorAmbient = data.GetVector(BJS_MATERIAL_COLOR_AMBIENT)
            colorDiffuse = data.GetVector(BJS_MATERIAL_COLOR_DIFFUSE)
            colorEmissive = data.GetVector(BJS_MATERIAL_COLOR_EMISSIVE)
            colorSpecular = data.GetVector(BJS_MATERIAL_COLOR_SPECULAR)            
            mat = self.mat
            
            op = node.GetObject()            
            tag = op.GetTag(c4d.Ttexture)
           
            if tag is not None:
                tag.Remove()                 
            
            tag = c4d.TextureTag()
            tt = FindLastTag(op)
            
            if tt is not None:
                op.InsertTag(tag, tt)
            else:
                op.InsertTag(tag)
            
            tag.SetMaterial(mat)
            
            m = None
            for _m in doc.GetMaterials():
                if _m.GetName() == mName:
                    m = _m
                    break;
            
            #print m
            
            if m is None:              
                doc.InsertMaterial(mat)
                m = mat
                
            tag.SetMaterial(m)
            
            m[c4d.MATERIAL_COLOR_COLOR] = colorDiffuse
            
            rLayer = m.GetReflectionLayerIndex(0).GetDataID()
            m[rLayer + c4d.REFLECTION_LAYER_COLOR_COLOR] = colorSpecular
            m[c4d.MATERIAL_LUMINANCE_COLOR] = colorEmissive
            m[c4d.MATERIAL_ENVIRONMENT_COLOR] = colorAmbient  
            
            print "UPDATE"
            self.dirty = False

    
    def Execute(self, node, doc, op, bt, priority, flags):
        data = node.GetDataInstance()
        mName = data.GetString(BJS_MATERIAL_NAME)

        colorAmbient = data.GetVector(BJS_MATERIAL_COLOR_AMBIENT)
        colorDiffuse = data.GetVector(BJS_MATERIAL_COLOR_DIFFUSE)
        colorEmissive = data.GetVector(BJS_MATERIAL_COLOR_EMISSIVE)
        colorSpecular = data.GetVector(BJS_MATERIAL_COLOR_SPECULAR)        
        self.updateInteralMat(node, doc)
        
        
    
        return c4d.EXECUTIONRESULT_OK
    
    
    def Message(self, node, type, data):
        if type == c4d.MSG_DESCRIPTION_COMMAND:
            if data["id"][0].id == 1:
                tag = node.GetObject().GetTag(c4d.Ttexture)
                tag.SetMaterial(self.mat)            
                return True 
        
        if type == c4d.MSG_DESCRIPTION_CHECKUPDATE:    
            self.dirty = True
            self.updateInteralMat(node, node.GetDocument())            
            
    
        return True
        
#---------------------------------

#=================================
# __main__
#=================================
if __name__ == "__main__":
    bmp = bitmaps.BaseBitmap()
    dir, file = os.path.split(__file__)
    bitmapfile = os.path.join(dir, "res", "icon.png")
    
    result = bmp.InitWith(bitmapfile)
    
    if not result:
        print "Error loading Icon!"
    
    okyn = plugins.RegisterTagPlugin(id=PLUGIN_ID, str="BJS_Standard_Material", info=c4d.TAG_VISIBLE|c4d.TAG_EXPRESSION, g=BJS_Standard_Material, description="BJS_Standard_Material", icon=bmp)
    
    #print "BJS_Standard_Material Initialized", okyn
#---------------------------------