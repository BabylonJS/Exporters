from .animation import *
from .armature import *
from .camera import *
from .light_shadow import *
from .logging import *
from .materials.material import *
from .mesh import *
from .package_level import *
from .sound import *
from .world import *

import bpy
from io import open
from os import path, makedirs

# JSON specific, for manifest file
import time
import calendar

#===============================================================================
class JsonExporter:
    nameSpace   = None  # assigned in execute
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def execute(self, context, filepath):
        scene = context.scene
        self.scene = scene # reference for passing
        self.settings = scene.world
        self.fatalError = None

        self.cameras = []
        self.lights = []
        self.shadowGenerators = []
        self.skeletons = []
        skeletonId = 0
        self.meshesAndNodes = []
        self.morphTargetMngrs = []
        self.materials = []
        self.multiMaterials = []
        self.sounds = []
        self.needPhysics = False

        try:
            self.filepathMinusExtension = filepath.rpartition('.')[0]
            JsonExporter.nameSpace = getNameSpace(self.filepathMinusExtension)

            log = Logger(self.filepathMinusExtension + '.log')

            if bpy.ops.object.mode_set.poll():
                bpy.ops.object.mode_set(mode = 'OBJECT')

            self.inlineTextures = self.settings.inlineTextures

            # assign texture location, purely temporary if in-lining
            self.textureFullPathDir = path.dirname(filepath)
            if not self.inlineTextures:
                self.textureFullPathDir = path.join(self.textureFullPathDir, self.settings.textureDir)
                if not path.isdir(self.textureFullPathDir):
                    makedirs(self.textureFullPathDir)
                    Logger.warn('Texture sub-directory did not already exist, created: ' + self.textureFullPathDir)

            Logger.log('========= Conversion from Blender to Babylon.js =========', 0)
            Logger.log('Scene settings used :', 1)
            Logger.log('inline textures     :  ' + format_bool(self.inlineTextures), 2)
            Logger.log('Material Type       :  ' + ('PBR' if self.settings.usePBRMaterials else 'STD'), 2)
            Logger.log('Positions Precision :  ' + format_int(self.settings.positionsPrecision), 2)
            Logger.log('Normals Precision   :  ' + format_int(self.settings.normalsPrecision), 2)
            Logger.log('UVs Precision       :  ' + format_int(self.settings.UVsPrecision), 2)
            Logger.log('Vert Color Precision:  ' + format_int(self.settings.vColorsPrecision), 2)
            Logger.log('Mat Weight Precision:  ' + format_int(self.settings.mWeightsPrecision), 2)
            if not self.inlineTextures:
                Logger.log('texture directory   :  ' + self.textureFullPathDir, 2)
            self.world = World(scene)

            bpy.ops.screen.animation_cancel()
            currentFrame = bpy.context.scene.frame_current

            # Active camera
            if scene.camera != None:
                self.activeCamera = scene.camera.name
            else:
                Logger.warn('No active camera has been assigned, or is not in a currently selected Blender layer')

            # Scene level sound
            if self.settings.attachedSound != '':
                self.sounds.append(Sound(self.settings.attachedSound, self.settings.autoPlaySound, self.settings.loopSound))

            # separate loop doing all skeletons, so available in Mesh to make skipping IK bones possible
            for object in scene.objects:
                if self.shouldBeCulled(object): continue

                scene.frame_set(currentFrame)
                if object.type == 'ARMATURE':
                    if object.visible_get():
                        self.skeletons.append(Skeleton(object, context, skeletonId, self.settings.ignoreIKBones))
                        skeletonId += 1
                    else:
                        Logger.warn('The following armature not visible in scene thus ignored: ' + object.name)

            # exclude light in this pass, so ShadowGenerator constructor can be passed meshesAnNodes
            for object in scene.objects:
                if self.shouldBeCulled(object): continue

                scene.frame_set(currentFrame)
                if object.type == 'CAMERA':
                    if object.visible_get():
                        self.cameras.append(Camera(object, self))
                    else:
                        Logger.warn('The following camera not visible in scene thus ignored: ' + object.name)

                elif object.type == 'MESH':
                    mesh = Mesh(object, scene, self)
                    if mesh.hasUnappliedTransforms and hasattr(mesh, 'skeletonWeights'):
                        self.fatalError = 'Mesh: ' + mesh.name + ' has un-applied transformations.  This will never work for a mesh with an armature.  Export cancelled'
                        Logger.log(self.fatalError)
                        return

                    if hasattr(mesh, 'positions') and len(mesh.positions) == 0:  # instances will have no positions assigned
                        Logger.warn('mesh, ' + mesh.name + ', has 0 vertices; ignored')
                        continue

                    if hasattr(mesh, 'physicsImpostor'): self.needPhysics = True

                    if hasattr(mesh, 'instances'):
                        self.meshesAndNodes.append(mesh)
                        if hasattr(mesh, 'morphTargetManagerId'):
                            self.morphTargetMngrs.append(mesh)

                    if object.data.attachedSound != '':
                        self.sounds.append(Sound(object.data.attachedSound, object.data.autoPlaySound, object.data.loopSound, object))

                elif object.type == 'EMPTY':
                    self.meshesAndNodes.append(Node(object))

                elif object.type != 'LIGHT' and object.type != 'ARMATURE':
                    Logger.warn('The following object (type - ' +  object.type + ') is not currently exportable thus ignored: ' + object.name)

            # Lamp / shadow Generator pass; meshesAnNodes complete & forceParents included
            for object in scene.objects:
                if self.shouldBeCulled(object): continue

                if object.type == 'LIGHT':
                    bulb = Light(object, self, self.settings.usePBRMaterials)
                    self.lights.append(bulb)
                    if object.data.shadowMap != 'NONE':
                        if bulb.light_type == DIRECTIONAL_LIGHT or bulb.light_type == SPOT_LIGHT:
                            self.shadowGenerators.append(ShadowGenerator(object, self.meshesAndNodes, scene))
                        else:
                            Logger.warn('Only directional (sun) and spot types of lamp are valid for shadows thus ignored: ' + object.name)

            bpy.context.scene.frame_set(currentFrame)

            # output file
            if log.nErrors == 0:
                self.to_json_file()
            else:
                Logger.log('Output cancelled due to data error')

        except:# catch *all* exceptions
            log.log_error_stack()
            raise

        finally:
            log.close()

        self.nWarnings = log.nWarnings
        self.nErrors = log.nErrors
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_json_file(self):
        Logger.log('========= Writing of JSON file started =========', 0)
        file_handler = open(self.filepathMinusExtension + '.babylon', 'w', encoding='utf8')
        file_handler.write('{')
        file_handler.write('"producer":{"name":"Blender","version":"' + bpy.app.version_string + '","exporter_version":"' + format_exporter_version() + '","file":"' + JsonExporter.nameSpace + '.babylon"},\n')
        self.world.to_json_file(file_handler, self)

        # Materials
        file_handler.write(',\n"materials":[')
        first = True
        for material in self.materials:
            if first != True:
                file_handler.write(',\n')

            first = False
            material.to_json_file(file_handler)
        file_handler.write(']')

        # Multi-materials
        file_handler.write(',\n"multiMaterials":[')
        first = True
        for multimaterial in self.multiMaterials:
            if first != True:
                file_handler.write(',')

            first = False
            multimaterial.to_json_file(file_handler)
        file_handler.write(']')

        # Armatures/Bones
        file_handler.write(',\n"skeletons":[')
        first = True
        for skeleton in self.skeletons:
            if first != True:
                file_handler.write(',')

            first = False
            skeleton.to_json_file(file_handler)
        file_handler.write(']')

        # Meshes
        file_handler.write(',\n"meshes":[')
        first = True
        for mesh in self.meshesAndNodes:
            if first != True:
                file_handler.write(',')

            first = False
            mesh.to_json_file(file_handler)
        file_handler.write(']')

        # Morph targets
        file_handler.write(',\n"morphTargetManagers":[')
        first = True
        for mesh in self.morphTargetMngrs:
            if first != True:
                file_handler.write(',')

            first = False
            mesh.write_morphing_file(file_handler)
        file_handler.write(']')

        # Cameras
        file_handler.write(',\n"cameras":[')
        first = True
        for camera in self.cameras:
            if hasattr(camera, 'fatalProblem'): continue
            if first != True:
                file_handler.write(',')

            first = False
            camera.update_for_target_attributes(self.meshesAndNodes)
            camera.to_json_file(file_handler)
        file_handler.write(']')

        # Active camera
        if hasattr(self, 'activeCamera'):
            write_string(file_handler, 'activeCamera', self.activeCamera)

        # Lights
        file_handler.write(',\n"lights":[')
        first = True
        for light in self.lights:
            if first != True:
                file_handler.write(',')

            first = False
            light.to_json_file(file_handler)
        file_handler.write(']')

        # Shadow generators
        file_handler.write(',\n"shadowGenerators":[')
        first = True
        for shadowGen in self.shadowGenerators:
            if first != True:
                file_handler.write(',')

            first = False
            shadowGen.to_json_file(file_handler)
        file_handler.write(']')

        # Sounds
        if len(self.sounds) > 0:
            file_handler.write('\n,"sounds":[')
            first = True
            for sound in self.sounds:
                if first != True:
                    file_handler.write(',')

                first = False
                sound.to_json_file(file_handler)

            file_handler.write(']')

        # Closing
        file_handler.write('\n}')
        file_handler.close()

        # Create or update .manifest file
        if self.settings.writeManifestFile:
            file_handler = open(self.filepathMinusExtension + '.babylon.manifest', 'w', encoding='utf8')
            file_handler.write('{\n')
            file_handler.write('\t"version" : ' + str(calendar.timegm(time.localtime())) + ',\n')
            file_handler.write('\t"enableSceneOffline" : true,\n')
            file_handler.write('\t"enableTextureOffline" : true\n')
            file_handler.write('}')
            file_handler.close()

        Logger.log('========= Writing of JSON file completed =========', 0)
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def getMaterial(self, baseMaterialId):
        for material in self.materials:
            if material.name == baseMaterialId:
                return material

        return None
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def getSourceMeshInstance(self, dataName):
        for mesh in self.meshesAndNodes:
            # nodes have no 'dataName', cannot be instanced in any case
            if hasattr(mesh, 'dataName') and mesh.dataName == dataName:
                return mesh

        return None
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def get_skeleton(self, name):
        for skeleton in self.skeletons:
            if skeleton.name == name:
                return skeleton
        #really cannot happen, will cause exception in caller
        return None
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def get_skeletonIndex(self, name):
        for idx,skeleton in enumerate(self.skeletons):
            if skeleton.name == name:
                return idx
        #really cannot happen, will cause exception in caller
        return None
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def shouldBeCulled(self, object):
        return object.hide_viewport or object.users_collection[0].hide_viewport
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    # only return a parent, when is has not been culled
    def getExportedParent(self, childObject):
        cand = childObject.parent
        if cand is None or self.shouldBeCulled(cand):
            return None

        # lights & cameras must also be visible to be exported, so check if parent a light or camera
        if cand.type == 'CAMERA' or cand.type == 'LIGHT':
            if not cand.visible_get():
                return None

        return cand