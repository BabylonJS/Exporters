import c4d
import os
import sys
import json
import math
import re

from c4d import gui, documents, utils, storage, plugins, bitmaps, Vector
from math import pi

PLUGIN_ID = 1054498
__version__ = "1.1"
__plugin_title__ = "BABYLON Scene Control"
__author__ = "Pryme8"

#=================================
#SCENE CONTROL DECLATRIONS
#=================================
BJS_SCENE_AUTO_CLEAR             = 10000
BJS_SCENE_CLEAR_COLOR            = 10001
BJS_SCENE_CLEAR_ALPHA            = 100010
BJS_SCENE_AMBIENT_COLOR          = 10002
BJS_SCENE_GRAVITY                = 10003
BJS_SCENE_ACTIVE_CAMERA          = 10004
BJS_SCENE_COLLISIONS_ENABLED     = 10005
BJS_SCENE_PHYSICS_ENABLED        = 10006
BJS_SCENE_PHYSICS_GRAVITY        = 10007
BJS_SCENE_PHYSICS_ENGINE         = 10008
BJS_SCENE_AUTO_ANIMATE           = 10009
BJS_SCENE_AUTO_ANIMATE_FROM      = 10010
BJS_SCENE_AUTO_ANIMATE_TO        = 10011
BJS_SCENE_AUTO_ANIMATE_LOOP      = 10012
BJS_SCENE_AUTO_ANIMATE_SPEED     = 10013

BJS_SCENE_GLOBAL_SCALE           = 10014

BJS_EXPORT_SCENE_TEMPLATE = 2000
BJS_EXPORT_SCENE_WEBSITE = 2001
#---------------------------------

#=================================
#JSON Encoder Class
#=================================
class ComplexEncoder(json.JSONEncoder):
    def default(self, obj):
        if hasattr(obj,'reprJSON'):
            return obj.reprJSON()
        else:
            return json.JSONEncoder.default(self, obj)
#---------------------------------

#=================================
#quick converters
#=================================
def Vec3Array(v):
    return [ v.x, v.y, v.z ]
    
def Vec3ArrayScaled(v, s):
    return [ v.x * s, v.y * s, v.z * s ]    
 

def getValue(op, v):
    return eval('op[c4d.'+v+']')

def Vec2(v):
    return {'x':v[0],'y':v[1]}

def Vec3(v):
    return {'x':v[0], 'y':v[1], 'z':v[2]}

def rotationAxis(op):
    mat = utils.MatrixToRotAxis(op.GetMg())    
    return {'x':mat[0][0], 'y':mat[0][1], 'z':mat[0][2], 'a': mat[1]}

def sVec3(v):
    return {'x':scale(v[0]),'y':scale(v[1]),'z':scale(v[2])}

def Orientation(axes):
    return ([{'x':1,'y':0,'z':0},{'x':-1,'y':0,'z':0},{'x':0,'y':1,'z':0},{'x':0,'y':-1,'z':0},{'x':0,'y':0,'z':1},{'x':1,'y':0,'z':-1}])[axes]

def LightTypes(t):
    return (["Point", "Spot", "Directional", "Directional"])[t]

def deg2Rad(v):
    return math.pi*v*180

#---------------------------------

#=================================
#BASIC TRANSFORMS CLASS
#=================================
class Transforms:
    def __init__(self):        
        self.position = None
        self.rotation = None
        self.scale = None
        
    def reprJSON(self):
        return dict(position=self.position, rotation=self.rotation, scale=self.scale)    
#--------------------------------- 

#=================================
#Mesh CLASS -- Any Mesh Object.
#=================================
class Mesh:
    def __init__(self, node, scene, parent):
        
        self.name = node.GetName()
        self.id = node.GetName()
        
        nodeData = node.GetDataInstance()
        tags = node.GetTags()
        
        self.tags = ""
        if parent is not False:
            self.parentId = parent.id
        else:
            self.parentId = ""
            
        self.materialId = ""
        
        self.position = Vec3ArrayScaled( node.GetAbsPos(), scene.globalScale )
        
    
        scene.meshes.append( mesh )
        
    
    def reprJSON(self):
        return dict()
        
#---------------------------------

#=================================
#Camera CLASS -- Any Camera Object.
#=================================
class Camera:
    def __init__(self, node, scene, parent):
        nData = node.GetDataInstance()
        tags = node.GetTags()
        
        cTag = None
        for tag in tags:
            print tag.GetTypeName()
            if tag.GetTypeName() == "BJS_Camera_Tag":
                cTag = tag
        
        tData = cTag.GetDataInstance()
        
        self.name = node.GetName()
        self.id = node.GetName()            
        
        #BJS_CAMERA_TYPE_FREE
        if tData[c4d.BJS_CAMERA_TYPE] == 100011:
            self.type = 'UniversalCamera'
        #BJS_CAMERA_TYPE_ARC
        elif tData[c4d.BJS_CAMERA_TYPE] == 100012:
            self.type = 'ArcRotateCamera'
        #BJS_CAMERA_TYPE_FOLLOW
        elif tData[c4d.BJS_CAMERA_TYPE] == 100013:
            self.type = 'FollowCamera'
            
        self.tags = ""
        if parent is not False:
            self.parentId = parent.id
        else:
            self.parentId = ""
        
        #lockedTargetId 
        
        self.position = Vec3ArrayScaled( node.GetAbsPos(), scene.globalScale )
        
        tempTarget = c4d.BaseObject(c4d.Ocube)          
        tempTarget.InsertUnder( node )
        tempTarget.SetRelPos(c4d.Vector(0, 0, 2000))
        mat = tempTarget.GetMg() 
        
        self.target =  Vec3ArrayScaled(tempTarget.GetRelPos() * mat , scene.globalScale)
        
        tempTarget.Remove()
        
        #if tData[c4d.BJS_CAMERA_MAKE_ACTIVE] == True:
        if scene.activeCamera is None:
            scene.activeCamera = self.name            
        
        #alpha
        #beta
        #radius
        #eye_space
        
        #heightOffset
        #rotationOffset            
        #cameraRigMode
        
        self.fov = tData[c4d.BJS_CAMERA_FOV]
        self.minZ = tData[c4d.BJS_CAMERA_MINZ]
        self.maxZ = tData[c4d.BJS_CAMERA_MAXZ]
        self.speed = tData[c4d.BJS_CAMERA_SPEED]
        self.inertia = tData[c4d.BJS_CAMERA_INERTIA]
        self.checkCollisions = tData[c4d.BJS_CAMERA_CHECK_COLLISIONS]
        self.applyGravity = tData[c4d.BJS_CAMERA_FOV]
        self.ellipsoid = tData[c4d.BJS_CAMERA_FOV]
        self.attachControls = tData[c4d.BJS_CAMERA_ATTACH_CONTROLS]
        
        self.animations = []
        self.autoAnimate = tData.GetBool( c4d.BJS_CAMERA_AUTO_ANIMATE )
        self.autoAnimateFrom = tData.GetLong( c4d.BJS_CAMERA_AUTO_ANIMATE_FROM )
        self.autoAnimateTo = tData.GetLong( c4d.BJS_CAMERA_AUTO_ANIMATE_TO )
        self.autoAnimateLoop = tData.GetBool( c4d.BJS_CAMERA_AUTO_ANIMATE_LOOP )
        self.autoAnimateSpeed = tData.GetFloat( c4d.BJS_CAMERA_AUTO_ANIMATE_SPEED )
        
        self.inputmgr = []
        scene.cameras.append( self )

    def __getitem__(self, arg):
        return str(arg)

    def reprJSON(self):
        return dict(
                name = self.name,
                id = self.id,
                type = self.type,
                tags = self.tags,
                parentId = self.parentId,
                #lockedTargetId = self.lockedTargetId,
                position = self.position,
                target = self.target,
                #alpha = self.alpha,
                #beta = self.beta,
                #radius = self.radius,
                #eye_space = self.eye_space,
                #heightOffset = self.heightOffset,
                #rotationOffset = self.rotationOffset,
                #cameraRigMode = self.cameraRigMode,
                fov = self.fov,
                minZ = self.minZ,
                maxZ = self.maxZ,
                speed = self.speed,
                inertia = self.inertia,
                checkCollisions = self.checkCollisions,
                applyGravity = self.applyGravity,
                ellipsoid = self.ellipsoid,
                attachControls = self.attachControls,
                animations = self.animations,
                autoAnimate = self.autoAnimate,
                autoAnimateFrom = self.autoAnimateFrom,
                autoAnimateTo = self.autoAnimateTo,
                autoAnimateLoop = self.autoAnimateLoop,
                autoAnimateSpeed = self.autoAnimateSpeed,
                inputmgr = self.inputmgr                
            ) 
#---------------------------------

#=================================
#Light CLASS -- Any Light Object.
#=================================
class Light:
    def __init__(self, node, scene, parent):
        nData = node.GetDataInstance()
        tags = node.GetTags()
        
        lightTag = False
        lData = None
        for tag in tags:
            if tag.GetTypeName() == "BJS Light Tag":
                lightTag = tag            
        
        self.name = node.GetName()
        self.id = node.GetName()
        
        self.specular  = [1,1,1]
        self.diffuse   = [1,1,1]
        self.intensity      = 1
        self.range          = None
        self.radius         = None           
        self.direction      = None
        self.angle          = None
        self.exponent       = None
        self.groundColor    = None
        
        self.excludedMeshesIds = None
        self.includedOnlyMeshesIds = None
        
        if lightTag:
            lData = lightTag.GetDataInstance()
            self.specular = Vec3Array(lightTag[c4d.BJS_LIGHT_SPECULAR])
        
        self.type = node[c4d.LIGHT_TYPE]
        print self.type
        #Establish what type.
        #C4D :
        #0  = Point Light
        #1  = Spot Light
        #3  = Directional
        #BJS :
        #LIGHTTYPEID_POINTLIGHT = 0;
        #LIGHTTYPEID_DIRECTIONALLIGHT = 1;
        #LIGHTTYPEID_SPOTLIGHT = 2;
        #LIGHTTYPEID_HEMISPHERICLIGHT = 3;            
        if self.type == 1:
            #Spot Light
            self.type = 2
            if lightTag is not False:
                self.exponent = lData[c4d.BJS_LIGHT_EXPONENT]
            
            tempTarget = c4d.BaseObject(c4d.Ocube)          
            tempTarget.InsertUnder( node )
            tempTarget.SetRelPos(c4d.Vector(0, 0, 1))
            gPos = tempTarget.GetRelPos() * tempTarget.GetMg()            
            tempTarget.Remove()
            normal = gPos - node.GetAbsPos()
            self.direction  = Vec3Array(normal.GetNormalized())
            
            print deg2Rad( node[c4d.LIGHT_DETAILS_OUTERANGLE] )
            self.angle = node[c4d.LIGHT_DETAILS_OUTERANGLE]
            
        elif self.type == 3:
            if lightTag:
                print "Make HEMI?" 
                print lData[ c4d.BJS_LIGHT_MAKE_HEMISPHERIC ]  
                if lData[ c4d.BJS_LIGHT_MAKE_HEMISPHERIC ] is True:
                    self.type = 3
                    self.groundColor = Vec3Array(lData[c4d.BJS_LIGHT_GROUND_COLOR])                    
                    tempTarget = c4d.BaseObject(c4d.Ocube)          
                    tempTarget.InsertUnder( node )
                    tempTarget.SetRelPos(c4d.Vector(0, 0, -1))
                    gPos = tempTarget.GetRelPos() * tempTarget.GetMg()            
                    tempTarget.Remove()
                    normal = gPos - node.GetAbsPos()
                    self.direction  = Vec3Array(normal.GetNormalized())
                else:
                    self.type = 1
                    tempTarget = c4d.BaseObject(c4d.Ocube)          
                    tempTarget.InsertUnder( node )
                    tempTarget.SetRelPos(c4d.Vector(0, 0, 1))
                    gPos = tempTarget.GetRelPos() * tempTarget.GetMg()            
                    tempTarget.Remove()
                    normal = gPos - node.GetAbsPos()
                    self.direction  = Vec3Array(normal.GetNormalized())                    
            else:
                self.type = 1
                tempTarget = c4d.BaseObject(c4d.Ocube)          
                tempTarget.InsertUnder( node )
                tempTarget.SetRelPos(c4d.Vector(0, 0, 1))
                gPos = tempTarget.GetRelPos() * tempTarget.GetMg()            
                tempTarget.Remove()
                normal = gPos - node.GetAbsPos()
                self.direction  = Vec3Array(normal.GetNormalized())
        else:
            self.type = 0
            #Area light not supported.

        self.diffuse = Vec3Array(node[c4d.LIGHT_COLOR])
        self.intensity = node[c4d.LIGHT_BRIGHTNESS]
           
        self.tags = ""
        if parent is not False:
            self.parentId = parent.id
        else:
            self.parentId = ""
        
        self.position = Vec3ArrayScaled( node.GetAbsPos(), scene.globalScale )

        if node[c4d.LIGHT_DETAILS_FALLOFF] == 0:
            self.range = float("inf")
        else:
            self.range = node[c4d.LIGHT_DETAILS_OUTERDISTANCE]
        
        
        inExlist = node[c4d.LIGHT_EXCLUSION_LIST]
        inExStringList = []
        
        for i in range(inExlist.GetObjectCount()):
            obj = inExlist.ObjectFromIndex( documents.GetActiveDocument(), i)
            inExStringList.append( obj.GetName() )

        if len(inExStringList) > 0:
            if node[c4d.LIGHT_EXCLUSION_MODE] == 0:
                #include list
                self.includedOnlyMeshesIds = inExStringList
            else:
                #exclude list
                self.excludedMeshesIds = inExStringList
        
        self.animations = []
        if lightTag:
            #Need to fix this...
            self.autoAnimate = False
            self.autoAnimateFrom = 0
            self.autoAnimateTo = 0
            self.autoAnimateLoop = False
            self.autoAnimateSpeed = 1  
            #self.autoAnimate = lData[ c4d.BJS_LIGHT_AUTO_ANIMATE ]
            #self.autoAnimateFrom = lData[  c4d.BJS_LIGHT_AUTO_ANIMATE_FROM ]
            #self.autoAnimateTo = lData[  c4d.BJS_LIGHT_AUTO_ANIMATE_TO ]
            #self.autoAnimateLoop = lData[  c4d.BJS_LIGHT_AUTO_ANIMATE_LOOP ]
            #self.autoAnimateSpeed = lData[  c4d.BJS_LIGHT_AUTO_ANIMATE_SPEED ]
        else:
            self.autoAnimate = False
            self.autoAnimateFrom = 0
            self.autoAnimateTo = 0
            self.autoAnimateLoop = False
            self.autoAnimateSpeed = 1            
        
        scene.lights.append( self )
            
    def __getitem__(self, arg):
        return str(arg)
    
    def reprJSON(self):
        return dict(
                name =              self.name,
                id =                self.id,
                type =              self.type,
                tags =              self.tags,
                parentId =          self.parentId,
                position =          self.position,
                direction =         self.direction,
                diffuse =           self.diffuse,
                specular =          self.specular,
                groundColor =       self.groundColor,
                intensity =         self.intensity,
                angle =             self.angle,
                exponent =          self.exponent,
                animations =        self.animations,
                autoAnimate =       self.autoAnimate,
                autoAnimateFrom =   self.autoAnimateFrom,
                autoAnimateTo =     self.autoAnimateTo,
                autoAnimateLoop =   self.autoAnimateLoop,
                autoAnimateSpeed =  self.autoAnimateSpeed            
            ) 

#=================================
#Recursion Function
#=================================
def recurse_hierarchy(op, scene, parent):    
    while op:
        element = None
        ignore = False
        for tag in op.GetTags():
            print tag.GetTypeName()
            if tag.GetTypeName() == "BJS Ignore Tag":
               ignore = tag.GetDataInstance()[c4d.BJS_IGNORE_BOOL]
               break
        if ignore is False:        
            type = op.GetTypeName()
            
            if type == "Camera":
                camera = Camera(op, scene, parent)
                element = camera
                
            elif type == "Light":
                light = Light(op, scene, parent)
                element = light
                
            elif type == "Polygon":
                mesh = Mesh(op, scene, parent)
                element = mesh             
                    
            if(op.GetDown()):
                recurse_hierarchy(op.GetDown(), scene, element)
        
        op = op.GetNext()
    return scene
#---------------------------------

#=================================
#Scene CLASS -- Core Scene Object.
#=================================
class Scene:
    def __init__(self, node):
        data = node.GetDataInstance()
        self.autoClear = data.GetBool( BJS_SCENE_AUTO_CLEAR )
        self.globalScale = 1/data.GetFloat( BJS_SCENE_GLOBAL_SCALE )
        self.clearColor = Vec3Array(data.GetVector( BJS_SCENE_CLEAR_COLOR ))
        self.clearColor = [ self.clearColor[0], self.clearColor[1], self.clearColor[2], data.GetFloat( BJS_SCENE_CLEAR_ALPHA ) ]
        self.ambientColor = Vec3Array(data.GetVector( BJS_SCENE_AMBIENT_COLOR ))
        self.gravity = Vec3Array(data.GetVector( BJS_SCENE_GRAVITY ))
        self.cameras = []
        self.activeCamera = None
        self.lights = []
        self.reflectionProbes = []
        self.materials = []
        self.geometries = []
        self.meshes = []
        self.multiMaterials = []
        self.shadowGenerators = []
        self.skeletons = []
        self.particleSystems = []
        self.lensFlareSystems = []
        self.actions = []
        self.sounds = []
        self.collisionsEnabled = data.GetBool( BJS_SCENE_COLLISIONS_ENABLED )
        self.physicsEnabled = data.GetBool( BJS_SCENE_PHYSICS_ENABLED )
        self.physicsGravity = Vec3Array(data.GetVector( BJS_SCENE_PHYSICS_GRAVITY ))
        self.physicsEngine = data.GetString( BJS_SCENE_PHYSICS_ENGINE )
        self.animations = []
        self.autoAnimate = data.GetBool( BJS_SCENE_AUTO_ANIMATE )
        self.autoAnimateFrom = data.GetLong( BJS_SCENE_AUTO_ANIMATE_FROM )
        self.autoAnimateTo = data.GetLong( BJS_SCENE_AUTO_ANIMATE_TO )
        self.autoAnimateLoop = data.GetBool( BJS_SCENE_AUTO_ANIMATE_LOOP )
        self.autoAnimateSpeed = data.GetFloat( BJS_SCENE_AUTO_ANIMATE_SPEED )        
    
    
    def getMeshByID(self, id):
        for mesh in self.meshes:
            if mesh.id == id:
                return mesh
        
        return None
        
    
    def __getitem__(self, arg):
        return str(arg)
    
    def reprJSON(self):
        return dict( 
            autoClear=self.autoClear,
            clearColor=self.clearColor,
            ambientColor=self.ambientColor,
            gravity=self.gravity,
            cameras=self.cameras,
            activeCamera=self.activeCamera,
            lights=self.lights,
            reflectionProbes=self.reflectionProbes,
            materials=self.materials,
            geometries=self.geometries,
            meshes=self.meshes,
            multiMaterials=self.multiMaterials,
            shadowGenerators=self.shadowGenerators,
            skeletons=self.skeletons,
            particleSystems=self.particleSystems,
            lensFlareSystems=self.lensFlareSystems,
            actions=self.actions,
            sounds=self.sounds,
            collisionsEnabled=self.collisionsEnabled,
            physicsEnabled=self.physicsEnabled,
            physicsGravity=self.physicsGravity,
            physicsEngine=self.physicsEngine,
            animations=self.animations,
            autoAnimate=self.autoAnimate,
            autoAnimateFrom=self.autoAnimateFrom,
            autoAnimateTo=self.autoAnimateTo,
            autoAnimateLoop=self.autoAnimateLoop,
            autoAnimateSpeed=self.autoAnimateSpeed
            ) 
#---------------------------------

#=================================
#PARSED DATA CLASS
#=================================
class parsedScene:
    def __init__(self, op):
        self.nodes = []
        self.attributes = {}        
        description = op.GetDescription(c4d.DESCFLAGS_DESC_0)
        
        for bc, paramid, groupid in description:
            if bc[c4d.DESC_IDENT] != None:
                 if (
                    bc[c4d.DESC_IDENT] == "BJS_SCENE_CLEARCOLOR" or
                    bc[c4d.DESC_IDENT] == "BJS_SCENE_AMBIENTCOLOR" 
                 ):
                    self.attributes[bc[c4d.DESC_IDENT]] = Vec3(getValue(op, bc[c4d.DESC_IDENT]))
                 elif (
                    bc[c4d.DESC_IDENT] == "BJS_SCENE_CLEARALPHA" or
                    bc[c4d.DESC_IDENT] == "BJS_SCENE_MAX_LIGHTS" 
                 ):
                    self.attributes[bc[c4d.DESC_IDENT]] = getValue(op, bc[c4d.DESC_IDENT])
    
    def __getitem__(self, arg):
        return str(arg)
    
    def reprJSON(self):
        return dict( 
            nodes=self.nodes,
            attributes=self.attributes
            )    
#---------------------------------

#=================================
#Parsing Function
#=================================
def startParse(op):
    scene = Scene(op)
    scene = recurse_hierarchy(op.GetDown(), scene, False)
    return scene
#---------------------------------

def cleanArrays(data):
    regex = r"""
	(?:\[(?:[^]]+)\])
	"""
    matches = re.finditer(regex, data, re.MULTILINE | re.VERBOSE)    
    diff = 0    
    for i, m in enumerate(matches, start=0):
        s = m.start(0) - diff
        e = m.end(0) - diff        
        clean = m.group(0).replace(' ', '').replace('\n', '').replace('\r', '')        
        diff = diff + (len(m.group(0)) - len(clean))
        a = data[0:s]
        b = data[e:len(data)]        
        data = a+clean+b
    return data

#=================================
#SCENE CONTROL CLASS
#=================================
class Scene_Control(plugins.ObjectData):
    
    def Init(self, node):
        #Scene Defaults
        data = node.GetDataInstance()
        data.SetBool( BJS_SCENE_AUTO_CLEAR, True)
        data.SetVector( BJS_SCENE_CLEAR_COLOR, Vector(0.2,0.2,0.5))
        data.SetFloat( BJS_SCENE_CLEAR_ALPHA, 1)
        data.SetVector( BJS_SCENE_AMBIENT_COLOR, Vector(0,0,0))
        data.SetVector( BJS_SCENE_GRAVITY, Vector(0,-9.81,0))
        data.SetBool( BJS_SCENE_COLLISIONS_ENABLED, False)
        data.SetBool( BJS_SCENE_PHYSICS_ENABLED, False)
        data.SetVector( BJS_SCENE_PHYSICS_GRAVITY, Vector(0,-9.81,0))
        data.SetString( BJS_SCENE_PHYSICS_ENGINE, 'oimo')
        data.SetBool( BJS_SCENE_AUTO_ANIMATE, False)
        data.SetLong( BJS_SCENE_AUTO_ANIMATE_FROM, 0)
        data.SetLong( BJS_SCENE_AUTO_ANIMATE_TO, 0)
        data.SetBool( BJS_SCENE_AUTO_ANIMATE_LOOP, False)
        data.SetFloat( BJS_SCENE_AUTO_ANIMATE_SPEED, 1)        
        data.SetFloat( BJS_SCENE_GLOBAL_SCALE, 200 )        
         
        return True
        
    def Execute(self, node, doc, bt, priority, flags):
        return c4d.EXECUTIONRESULT_OK


    def Message(self, node, type, data):
        if type == c4d.MSG_DESCRIPTION_CHECKDRAGANDDROP:
            print data
            
        if type ==  c4d.MSG_DESCRIPTION_COMMAND:
            if data['id'][0].id == BJS_EXPORT_SCENE_TEMPLATE:
                self.ExportTemplate(node)
            elif data['id'][0].id == BJS_EXPORT_SCENE_WEBSITE:
                self.ExportWebsite(node)
        
        
        return True        

    def GetSceneJSON(self, node):
        data = json.dumps((startParse(node)).reprJSON(), sort_keys=True, indent=4, separators=(',', ': '), cls=ComplexEncoder)
        return cleanArrays(data)  

    def ExportTemplate(self, node):        
        data = self.GetSceneJSON(node)
        #print data
        filePath = storage.LoadDialog(title="Save as Babylon File", flags=c4d.FILESELECT_SAVE, force_suffix="babylon")
        if filePath is None:
            return
        #open file
        f = open(filePath,"w")
        f.write(data)
        f.close()        
        c4d.CopyStringToClipboard("KEEEYAH!")
        gui.MessageDialog(".babylon file exported")
        
        return
        
    def ExportWebsite(self, node): 
        data = self.GetSceneJSON(node)
        filePath = storage.LoadDialog(title="Save as Website", flags=c4d.FILESELECT_DIRECTORY)
        
        assetPath = os.path.join(filePath, "Assets") 
        if os.path.exists(assetPath) == False:
            os.mkdir(assetPath)
        
        babylonFile = open(assetPath+"/scene.babylon","w")
        babylonFile.write(data)
        babylonFile.close()

        html =  """<meta http-equiv="Content-Type" content="text/html" charset="utf-8"/>
            <title>Babylon - Getting Started</title>
            <!--- Link to the last version of BabylonJS --->
            <script src="https://cdn.babylonjs.com/babylon.js"></script>
            <style>
                html, body {
                    overflow: hidden;
                    width   : 100%;
                    height  : 100%;
                    margin  : 0;
                    padding : 0;
                }

                #renderCanvas {
                    width   : 100%;
                    height  : 100%;
                    touch-action: none;
                }
            </style>
        </head>
        <body>
            <canvas id="renderCanvas"></canvas>
            <script>
                window.addEventListener('DOMContentLoaded', ()=>{
                    var canvas = document.getElementById('renderCanvas')
                    var engine = new BABYLON.Engine(canvas, true)
                    BABYLON.SceneLoader.Load('./Assets/', "scene.babylon", engine, (scene)=>{    
                        
                        scene.debugLayer.show(true)
                        
                        engine.runRenderLoop(()=>{
                            scene.render();
                        })
                        window.addEventListener('resize', ()=>{
                            engine.resize()
                        })                        
                    })
                })
            </script>
        </body>
        </html>"""

        siteFile = open(filePath+"/index.html", "w")
        siteFile.write(html)
        siteFile.close()
        gui.MessageDialog("Files exported!")
     
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
    
    okyn = plugins.RegisterObjectPlugin( 
        id=PLUGIN_ID,
        str="Scene_Control",
        info=c4d.OBJECT_GENERATOR,
        g=Scene_Control,
        description="Scene_Control",
        icon=bmp
    )    
    print "BJS_Scene_Control Initialized", okyn
    
#---------------------------------