from .logger import *
from .package_level import *

from .f_curve_animatable import *
from .armature import *
from .material import *
from .shape_key_group import *

import bpy
import math
from mathutils import Vector, Quaternion
from random import randint

# used in Mesh & Node constructors, defined in BABYLON.AbstractMesh
BILLBOARDMODE_NONE = 0
#BILLBOARDMODE_X = 1
#BILLBOARDMODE_Y = 2
#BILLBOARDMODE_Z = 4
BILLBOARDMODE_ALL = 7

# used in Mesh constructor, defined in BABYLON.PhysicsImpostor
SPHERE_IMPOSTER = 1
BOX_IMPOSTER = 2
#PLANE_IMPOSTER = 3
MESH_IMPOSTER = 4
CAPSULE_IMPOSTER = 5
CONE_IMPOSTER = 6
CYLINDER_IMPOSTER = 7
PARTICLE_IMPOSTER = 8

SHAPE_KEY_GROUPS_ALLOWED = False

ZERO_V = Vector((0, 0, 0))
ZERO_Q = Quaternion((1, 0, 0, 0))
#===============================================================================
class Mesh(FCurveAnimatable):
    def __init__(self, object, scene, exporter):
        self.scene = scene
        self.name = object.name
        Logger.log('processing begun of mesh:  ' + self.name)
        self.define_animations(object, True, True, True)  #Should animations be done when forcedParent

        self.isVisible = not object.hide_render
        self.isPickable = object.data.isPickable
        self.isEnabled = not object.data.loadDisabled
        if hasattr(object.data, 'useFlatShading') and object.data.useFlatShading:
            hasModifier = False
            # extra checking not really working; all checking going to be pulled in future
            for con in object.constraints:
                if con.name == 'EDGE_SPLIT':
                    hasModifier = True
                    break
            
            if not hasModifier:
                Logger.warn('Found Obsolete "Use Flat Shading" property set True.  Replaced by "Edge Split" modifier instead', 2)
                
        self.checkCollisions = object.data.checkCollisions
        self.receiveShadows = object.data.receiveShadows
        self.castShadows = object.data.castShadows
        self.freezeWorldMatrix = object.data.freezeWorldMatrix
        self.layer = getLayer(object) # used only for lights with 'This Layer Only' checked, not exported
        self.tags = object.data.tags

        # Constraints
        for constraint in object.constraints:
            if constraint.type == 'TRACK_TO':
                self.lockedTargetId = constraint.target.name # does not support; 'to', 'up', 'space' or 'influence'
                break

        # hasSkeleton detection & skeletonID determination
        self.hasSkeleton = False
        objArmature = None      # if there's an armature, this will be the one!
        if len(object.vertex_groups) > 0 and not object.data.ignoreSkeleton:
            objArmature = object.find_armature()
            if objArmature != None:
                # used to get bone index, since could be skipping IK bones
                skeleton = exporter.get_skeleton(objArmature.name)
                self.hasSkeleton = skeleton is not None
                
                if not self.hasSkeleton:
                    Logger.warn('No skeleton with name "' + objArmature.name + '" found skeleton ignored.', 2)
                else:
                    i = 0
                    for obj in scene.objects:
                        if obj.type == "ARMATURE":
                            if obj == objArmature:
                                self.skeletonId = i
                                break
                            else:
                                i += 1

        # determine Position, rotation, & scaling
        # Use local matrix
        locMatrix = object.matrix_local
        if objArmature != None:
            # unless the armature is the parent
            if object.parent and object.parent == objArmature:
                locMatrix = object.matrix_world * object.parent.matrix_world.inverted()

        loc, rot, scale = locMatrix.decompose()
        self.position = loc
        if object.rotation_mode == 'QUATERNION':
            self.rotationQuaternion = rot
        else:
            self.rotation = scale_vector(rot.to_euler('XYZ'), -1)
        self.scaling = scale

        # ensure no unapplied rotation or scale, when there is an armature
        self.hasUnappliedTransforms = (self.scaling.x != 1 or self.scaling.y != 1 or self.scaling.z != 1 or
            (hasattr(self, 'rotation'          ) and not same_vertex    (self.rotation          , ZERO_V, FLOAT_PRECISION_DEFAULT)) or 
            (hasattr(self, 'rotationQuaternion') and not same_quaternion(self.rotationQuaternion, ZERO_Q, FLOAT_PRECISION_DEFAULT))
            )

        # determine parent & dataName
        self.dataName = object.data.name # used to support shared vertex instances in later passed
        if object.parent and object.parent.type != 'ARMATURE':
            self.parentId = object.parent.name

        # Physics
        if object.rigid_body != None:
            shape_items = {'SPHERE'     : SPHERE_IMPOSTER,
                           'BOX'        : BOX_IMPOSTER,
                           'MESH'       : MESH_IMPOSTER,
                           'CAPSULE'    : CAPSULE_IMPOSTER,
                           'CONE'       : CONE_IMPOSTER,
                           'CYLINDER'   : CYLINDER_IMPOSTER,
                           'CONVEX_HULL': PARTICLE_IMPOSTER}

            shape_type = shape_items[object.rigid_body.collision_shape]
            self.physicsImpostor = shape_type
            
            mass = object.rigid_body.mass
            if mass < 0.005:
                mass = 0
            self.physicsMass = mass
            self.physicsFriction = object.rigid_body.friction
            self.physicsRestitution = object.rigid_body.restitution

        # Get if this will be an instance of another, before processing materials, to avoid multi-bakes
        sourceMesh = exporter.getSourceMeshInstance(self.dataName)
        if sourceMesh is not None:
            #need to make sure rotation mode matches, since value initially copied in InstancedMesh constructor
            if hasattr(sourceMesh, 'rotationQuaternion'):
                instRot = None
                instRotq = rot
            else:
                instRot = scale_vector(rot.to_euler('XYZ'), -1)
                instRotq = None

            instance = MeshInstance(self, instRot, instRotq)
            sourceMesh.instances.append(instance)
            Logger.log('mesh is an instance of :  ' + sourceMesh.name + '.  Processing halted.', 2)
            return
        else:
            self.instances = []

        # process all of the materials required
        recipe = BakingRecipe(object)
        self.billboardMode = BILLBOARDMODE_ALL if recipe.isBillboard else BILLBOARDMODE_NONE

        if recipe.needsBaking:
            if recipe.multipleRenders:
                Logger.warn('Mixing of Cycles & Blender Render in same mesh not supported.  No materials exported.', 2)
            else:
                bakedMat = BakedMaterial(exporter, object, recipe)
                exporter.materials.append(bakedMat)
                self.materialId = bakedMat.name

        else:
            bjs_material_slots = []
            for slot in object.material_slots:
                # None will be returned when either the first encounter or must be unique due to baked textures
                material = exporter.getMaterial(slot.name)
                if (material != None):
                    Logger.log('registered as also a user of material:  ' + slot.name, 2)
                else:
                    material = StdMaterial(slot, exporter, object)
                    exporter.materials.append(material)

                bjs_material_slots.append(material)

            if len(bjs_material_slots) == 1:
                self.materialId = bjs_material_slots[0].name

            elif len(bjs_material_slots) > 1:
                multimat = MultiMaterial(bjs_material_slots, len(exporter.multiMaterials), exporter.nameSpace)
                self.materialId = multimat.name
                exporter.multiMaterials.append(multimat)
            else:
                Logger.warn('No materials have been assigned: ', 2)

        # Get mesh
        mesh = object.to_mesh(scene, True, 'PREVIEW')

        # Triangulate mesh if required
        Mesh.mesh_triangulate(mesh)

        # Getting vertices and indices
        self.positions  = []
        self.normals    = []
        self.uvs        = [] # not always used
        self.uvs2       = [] # not always used
        self.colors     = [] # not always used
        self.indices    = []
        self.subMeshes  = []

        hasUV = len(mesh.tessface_uv_textures) > 0
        if hasUV:
            which = len(mesh.tessface_uv_textures) - 1 if recipe.needsBaking else 0
            UVmap = mesh.tessface_uv_textures[which].data

        hasUV2 = len(mesh.tessface_uv_textures) > 1 and not recipe.needsBaking
        if hasUV2:
            UV2map = mesh.tessface_uv_textures[1].data

        hasVertexColor = len(mesh.vertex_colors) > 0
        if hasVertexColor:
            Colormap = mesh.tessface_vertex_colors.active.data

        if self.hasSkeleton:
            weightsPerVertex = []
            indicesPerVertex = []
            influenceCounts = [0, 0, 0, 0, 0, 0, 0, 0, 0] # 9, so accessed orign 1; 0 used for all those greater than 8
            totalInfluencers = 0
            highestInfluenceObserved = 0

        hasShapeKeys = False
        if object.data.shape_keys:
            for block in object.data.shape_keys.key_blocks:
                if (block.name == 'Basis'):
                    hasShapeKeys = True
                    keyOrderMap = []
                    basis = block
                    break

            if not hasShapeKeys:
                Logger.warn('Basis key missing, shape-key processing NOT performed', 2)

        # used tracking of vertices as they are received
        alreadySavedVertices = []
        vertices_Normals = []
        vertices_UVs = []
        vertices_UV2s = []
        vertices_Colors = []
        vertices_indices = []
        vertices_sk_weights = []
        vertices_sk_indices = []

        for v in range(len(mesh.vertices)):
            alreadySavedVertices.append(False)
            vertices_Normals.append([])
            vertices_UVs.append([])
            vertices_UV2s.append([])
            vertices_Colors.append([])
            vertices_indices.append([])
            vertices_sk_weights.append([])
            vertices_sk_indices.append([])

        materialsCount = 1 if recipe.needsBaking else max(1, len(object.material_slots))
        verticesCount = 0
        indicesCount = 0

        for materialIndex in range(materialsCount):
            subMeshVerticesStart = verticesCount
            subMeshIndexStart = indicesCount

            for faceIndex in range(len(mesh.tessfaces)):  # For each face
                face = mesh.tessfaces[faceIndex]

                if face.material_index != materialIndex and not recipe.needsBaking:
                    continue

                for v in range(3): # For each vertex in face
                    vertex_index = face.vertices[v]

                    vertex = mesh.vertices[vertex_index]
                    position = vertex.co

                    normal = vertex.normal

                    #skeletons
                    if self.hasSkeleton:
                        matricesWeights = []
                        matricesIndices = []

                        # Getting influences
                        for group in vertex.groups:
                            index = group.group
                            weight = group.weight

                            for bone in objArmature.pose.bones:
                                if object.vertex_groups[index].name == bone.name:
                                    matricesWeights.append(weight)
                                    matricesIndices.append(skeleton.get_index_of_bone(bone.name))

                    # Texture coordinates
                    if hasUV:
                        vertex_UV = UVmap[face.index].uv[v]

                    if hasUV2:
                        vertex_UV2 = UV2map[face.index].uv[v]

                    # Vertex color
                    if hasVertexColor:
                        if v == 0:
                            vertex_Color = Colormap[face.index].color1
                        if v == 1:
                            vertex_Color = Colormap[face.index].color2
                        if v == 2:
                            vertex_Color = Colormap[face.index].color3

                    # Check if the current vertex is already saved
                    alreadySaved = alreadySavedVertices[vertex_index]
                    if alreadySaved:
                        alreadySaved = False

                        # UV
                        index_UV = 0
                        for savedIndex in vertices_indices[vertex_index]:
                            vNormal = vertices_Normals[vertex_index][index_UV]
                            if not same_vertex(normal, vNormal, scene.normalsPrecision):
                                continue;

                            if hasUV:
                                vUV = vertices_UVs[vertex_index][index_UV]
                                if not same_array(vertex_UV, vUV, scene.UVsPrecision):
                                    continue

                            if hasUV2:
                                vUV2 = vertices_UV2s[vertex_index][index_UV]
                                if not same_array(vertex_UV2, vUV2, scene.UVsPrecision):
                                    continue

                            if hasVertexColor:
                                vColor = vertices_Colors[vertex_index][index_UV]
                                if not same_color(vertex_Color, vColor, scene.vColorsPrecision):
                                    continue

                            if self.hasSkeleton:
                                vSkWeight = vertices_sk_weights[vertex_index]
                                vSkIndices = vertices_sk_indices[vertex_index]
                                if not same_array(vSkWeight[index_UV], matricesWeights, scene.mWeightsPrecision) or not same_array(vSkIndices[index_UV], matricesIndices, 1):
                                    continue

                            if vertices_indices[vertex_index][index_UV] >= subMeshVerticesStart:
                                alreadySaved = True
                                break

                            index_UV += 1

                    if (alreadySaved):
                        # Reuse vertex
                        index = vertices_indices[vertex_index][index_UV]
                    else:
                        # Export new one
                        index = verticesCount
                        alreadySavedVertices[vertex_index] = True

                        vertices_Normals[vertex_index].append(normal)
                        self.normals.append(normal)

                        if hasUV:
                            vertices_UVs[vertex_index].append(vertex_UV)
                            self.uvs.append(vertex_UV[0])
                            self.uvs.append(vertex_UV[1])
                        if hasUV2:
                            vertices_UV2s[vertex_index].append(vertex_UV2)
                            self.uvs2.append(vertex_UV2[0])
                            self.uvs2.append(vertex_UV2[1])
                        if hasVertexColor:
                            vertices_Colors[vertex_index].append(vertex_Color)
                            self.colors.append(vertex_Color.r)
                            self.colors.append(vertex_Color.g)
                            self.colors.append(vertex_Color.b)
                            self.colors.append(1.0)
                        if self.hasSkeleton:
                            vertices_sk_weights[vertex_index].append(matricesWeights)
                            vertices_sk_indices[vertex_index].append(matricesIndices)
                            nInfluencers = len(matricesWeights)
                            totalInfluencers += nInfluencers
                            if nInfluencers <= 8:
                                influenceCounts[nInfluencers] += 1
                            else:
                                influenceCounts[0] += 1
                            highestInfluenceObserved = nInfluencers if nInfluencers > highestInfluenceObserved else highestInfluenceObserved
                            weightsPerVertex.append(matricesWeights)
                            indicesPerVertex.append(matricesIndices)

                        if hasShapeKeys:
                            keyOrderMap.append([vertex_index, len(self.positions)]) # use len positions before it is append to convert from 1 to 0 origin

                        vertices_indices[vertex_index].append(index)

                        self.positions.append(position)

                        verticesCount += 1
                    self.indices.append(index)
                    indicesCount += 1
            self.subMeshes.append(SubMesh(materialIndex, subMeshVerticesStart, subMeshIndexStart, verticesCount - subMeshVerticesStart, indicesCount - subMeshIndexStart))

        BakedMaterial.meshBakingClean(object)

        Logger.log('num positions      :  ' + str(len(self.positions)), 2)
        Logger.log('num normals        :  ' + str(len(self.normals  )), 2)
        Logger.log('num uvs            :  ' + str(len(self.uvs      )), 2)
        Logger.log('num uvs2           :  ' + str(len(self.uvs2     )), 2)
        Logger.log('num colors         :  ' + str(len(self.colors   )), 2)
        Logger.log('num indices        :  ' + str(len(self.indices  )), 2)

        if self.hasSkeleton:
            Logger.log('Skeleton stats:  ', 2)
            self.toFixedInfluencers(weightsPerVertex, indicesPerVertex, object.data.maxInfluencers, highestInfluenceObserved)

            self.skeletonIndices = Mesh.packSkeletonIndices(self.skeletonIndices)
            if (self.numBoneInfluencers > 4):
                self.skeletonIndicesExtra = Mesh.packSkeletonIndices(self.skeletonIndicesExtra)

            Logger.log('Total Influencers:  ' + format_f(totalInfluencers), 3)
            Logger.log('Avg # of influencers per vertex:  ' + format_f(totalInfluencers / len(self.positions)), 3)
            Logger.log('Highest # of influencers observed:  ' + str(highestInfluenceObserved) + ', num vertices with this:  ' + format_int(influenceCounts[highestInfluenceObserved if highestInfluenceObserved < 9 else 0]), 3)
            Logger.log('exported as ' + str(self.numBoneInfluencers) + ' influencers', 3)
            nWeights = len(self.skeletonWeights) + (len(self.skeletonWeightsExtra) if hasattr(self, 'skeletonWeightsExtra') else 0)
            Logger.log('num skeletonWeights and skeletonIndices:  ' + str(nWeights), 3)

        numZeroAreaFaces = self.find_zero_area_faces()
        if numZeroAreaFaces > 0:
            Logger.warn('# of 0 area faces found:  ' + str(numZeroAreaFaces), 2)

        # shape keys for mesh
        if hasShapeKeys:
            Mesh.sort(keyOrderMap)
            self.rawShapeKeys = []
            self.morphTargetManagerId = randint(0, 1000000) # not used for TOB implementation
            groupNames = []
            Logger.log('Shape Keys:', 2)

            # process the keys in the .blend
            for block in object.data.shape_keys.key_blocks:
                # perform name format validation, before processing
                keyName = block.name

                # the Basis shape key is a member of all groups, processed in 2nd pass
                if keyName == 'Basis': continue

                if keyName.find('-') <= 0 and SHAPE_KEY_GROUPS_ALLOWED:
                    if object.data.defaultShapeKeyGroup != DEFAULT_SHAPE_KEY_GROUP:
                        keyName = object.data.defaultShapeKeyGroup + '-' + keyName
                    else: continue

                group = None
                state = keyName
                if SHAPE_KEY_GROUPS_ALLOWED:
                    temp = keyName.upper().partition('-')
                    group = temp[0]
                    state = temp[2]
                self.rawShapeKeys.append(RawShapeKey(block, group, state, keyOrderMap, basis, self.scene.positionsPrecision))

                if SHAPE_KEY_GROUPS_ALLOWED:
                    # check for a new group, add to groupNames if so
                    newGroup = True
                    for group in groupNames:
                        if temp[0] == group:
                            newGroup = False
                            break
                    if newGroup:
                       groupNames.append(temp[0])

            # process into ShapeKeyGroups, when rawShapeKeys found and groups allowed (implied)
            if len(groupNames) > 0:
                self.shapeKeyGroups = []
                basis = RawShapeKey(basis, None, 'BASIS', keyOrderMap, basis, self.scene.positionsPrecision)
                for group in groupNames:
                    self.shapeKeyGroups.append(ShapeKeyGroup(group,self.rawShapeKeys, basis.vertices, self.scene.positionsPrecision))
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def find_zero_area_faces(self):
        nFaces = int(len(self.indices) / 3)
        nZeroAreaFaces = 0
        for f in range(nFaces):
            faceOffset = f * 3
            p1 = self.positions[self.indices[faceOffset    ]]
            p2 = self.positions[self.indices[faceOffset + 1]]
            p3 = self.positions[self.indices[faceOffset + 2]]

            if same_vertex(p1, p2) or same_vertex(p1, p3) or same_vertex(p2, p3): nZeroAreaFaces += 1

        return nZeroAreaFaces
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    @staticmethod
    # ShapeKeyGroup depends on AffectedIndices being in asending order, so sort it, probably nothing to do
    def sort(keyOrderMap):
        notSorted = True
        while(notSorted):
            notSorted = False
            for idx in range(1, len(keyOrderMap)):
                if keyOrderMap[idx - 1][1] > keyOrderMap[idx][1]:
                    tmp = keyOrderMap[idx]
                    keyOrderMap[idx    ] = keyOrderMap[idx - 1]
                    keyOrderMap[idx - 1] = tmp
                    notSorted = True
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    @staticmethod
    def mesh_triangulate(mesh):
        try:
            import bmesh
            bm = bmesh.new()
            bm.from_mesh(mesh)
            bmesh.ops.triangulate(bm, faces = bm.faces)
            bm.to_mesh(mesh)
            mesh.calc_tessface()
            bm.free()
        except:
            pass
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def toFixedInfluencers(self, weightsPerVertex, indicesPerVertex, maxInfluencers, highestObserved):
        if (maxInfluencers > 8 or maxInfluencers < 1):
            maxInfluencers = 8
            Logger.warn('Maximum # of influencers invalid, set to 8', 3)

        self.numBoneInfluencers = maxInfluencers if maxInfluencers < highestObserved else highestObserved
        needExtras = self.numBoneInfluencers > 4

        maxInfluencersExceeded = 0

        fixedWeights = []
        fixedIndices = []

        fixedWeightsExtra = []
        fixedIndicesExtra = []

        for i in range(len(weightsPerVertex)):
            weights = weightsPerVertex[i]
            indices = indicesPerVertex[i]
            nInfluencers = len(weights)

            if (nInfluencers > self.numBoneInfluencers):
                maxInfluencersExceeded += 1
                Mesh.sortByDescendingInfluence(weights, indices)

            for j in range(4):
                fixedWeights.append(weights[j] if nInfluencers > j else 0.0)
                fixedIndices.append(indices[j] if nInfluencers > j else 0  )

            if needExtras:
                for j in range(4, 8):
                    fixedWeightsExtra.append(weights[j] if nInfluencers > j else 0.0)
                    fixedIndicesExtra.append(indices[j] if nInfluencers > j else 0  )

        self.skeletonWeights = fixedWeights
        self.skeletonIndices = fixedIndices

        if needExtras:
            self.skeletonWeightsExtra = fixedWeightsExtra
            self.skeletonIndicesExtra = fixedIndicesExtra

        if maxInfluencersExceeded > 0:
            Logger.warn('Maximum # of influencers exceeded for ' + format_int(maxInfluencersExceeded) + ' vertices, extras ignored', 3)
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    # sorts one set of weights & indices by descending weight, by reference
    # not shown to help with MakeHuman, but did not hurt.  In just so it is not lost for future.
    @staticmethod
    def sortByDescendingInfluence(weights, indices):
        notSorted = True
        while(notSorted):
            notSorted = False
            for idx in range(1, len(weights)):
                if weights[idx - 1] < weights[idx]:
                    tmp = weights[idx]
                    weights[idx    ] = weights[idx - 1]
                    weights[idx - 1] = tmp

                    tmp = indices[idx]
                    indices[idx    ] = indices[idx - 1]
                    indices[idx - 1] = tmp

                    notSorted = True
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    # assume that toFixedInfluencers has already run, which ensures indices length is a multiple of 4
    @staticmethod
    def packSkeletonIndices(indices):
        compressedIndices = []

        for i in range(math.floor(len(indices) / 4)):
            idx = i * 4
            matricesIndicesCompressed  = indices[idx    ]
            matricesIndicesCompressed += indices[idx + 1] <<  8
            matricesIndicesCompressed += indices[idx + 2] << 16
            matricesIndicesCompressed += indices[idx + 3] << 24

            compressedIndices.append(matricesIndicesCompressed)

        return compressedIndices
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_scene_file(self, file_handler):
        file_handler.write('{')
        write_string(file_handler, 'name', self.name, True)
        write_string(file_handler, 'id', self.name)
        if hasattr(self, 'parentId'): write_string(file_handler, 'parentId', self.parentId)

        if hasattr(self, 'materialId'): write_string(file_handler, 'materialId', self.materialId)
        write_int(file_handler, 'billboardMode', self.billboardMode)
        write_vector(file_handler, 'position', self.position)

        if hasattr(self, "rotationQuaternion"):
            write_quaternion(file_handler, 'rotationQuaternion', self.rotationQuaternion)
        else:
            write_vector(file_handler, 'rotation', self.rotation)

        write_vector(file_handler, 'scaling', self.scaling)
        write_bool(file_handler, 'isVisible', self.isVisible)
        write_bool(file_handler, 'freezeWorldMatrix', self.freezeWorldMatrix)
        write_bool(file_handler, 'isEnabled', self.isEnabled)
        write_bool(file_handler, 'checkCollisions', self.checkCollisions)
        write_bool(file_handler, 'receiveShadows', self.receiveShadows)
        write_bool(file_handler, 'pickable', self.isPickable)
        write_string(file_handler, 'tags', self.tags)

        if hasattr(self, 'physicsImpostor'):
            write_int(file_handler, 'physicsImpostor', self.physicsImpostor)
            write_float(file_handler, 'physicsMass', self.physicsMass)
            write_float(file_handler, 'physicsFriction', self.physicsFriction)
            write_float(file_handler, 'physicsRestitution', self.physicsRestitution)

        # Geometry
        if self.hasSkeleton:
            write_int(file_handler, 'skeletonId', self.skeletonId)
            write_int(file_handler, 'numBoneInfluencers', self.numBoneInfluencers)

        write_vector_array(file_handler, 'positions', self.positions, self.scene.positionsPrecision)
        write_vector_array(file_handler, 'normals'  , self.normals, self.scene.normalsPrecision)

        if len(self.uvs) > 0:
            write_array(file_handler, 'uvs', self.uvs, self.scene.UVsPrecision)

        if len(self.uvs2) > 0:
            write_array(file_handler, 'uvs2', self.uvs2, self.scene.UVsPrecision)

        if len(self.colors) > 0:
            write_array(file_handler, 'colors', self.colors, self.scene.vColorsPrecision)

        if hasattr(self, 'skeletonWeights'):
            write_array(file_handler, 'matricesWeights', self.skeletonWeights, self.scene.mWeightsPrecision)
            write_array(file_handler, 'matricesIndices', self.skeletonIndices)

        if hasattr(self, 'skeletonWeightsExtra'):
            write_array(file_handler, 'matricesWeightsExtra', self.skeletonWeightsExtra, self.scene.mWeightsPrecision)
            write_array(file_handler, 'matricesIndicesExtra', self.skeletonIndicesExtra)

        write_array(file_handler, 'indices', self.indices)

        # Constraint
        if hasattr(self, 'lockedTargetId'):
            file_handler.write('\n,"metadata":{')
            write_string(file_handler, 'lookAt', self.lockedTargetId, True)
            file_handler.write('}')

        # Sub meshes
        file_handler.write('\n,"subMeshes":[')
        first = True
        for subMesh in self.subMeshes:
            if first == False:
                file_handler.write(',')
            subMesh.to_scene_file(file_handler)
            first = False
        file_handler.write(']')

        super().to_scene_file(file_handler) # Animations

        # Instances
        first = True
        file_handler.write('\n,"instances":[')
        for instance in self.instances:
            if first == False:
                file_handler.write(',')

            instance.to_scene_file(file_handler)

            first = False
        file_handler.write(']')
        
        # Shape Keys
        if hasattr(self, 'morphTargetManagerId'):
            write_int(file_handler, 'morphTargetManagerId', self.morphTargetManagerId)

        # Close mesh
        file_handler.write('}\n')
        self.alreadyExported = True
#===============================================================================
    def write_morphing_file(self, file_handler):
        first = True
        file_handler.write('{')
        write_int(file_handler, 'id', self.morphTargetManagerId, True)
        file_handler.write('\n,"targets":[')
        for key in self.rawShapeKeys:
            if first == False:
                file_handler.write(',')

            key.to_scene_file(file_handler)

            first = False
        file_handler.write(']}')
#===============================================================================
class MeshInstance:
     def __init__(self, instancedMesh, rotation, rotationQuaternion):
        self.name = instancedMesh.name
        if hasattr(instancedMesh, 'parentId'): self.parentId = instancedMesh.parentId
        self.position = instancedMesh.position
        if rotation is not None:
            self.rotation = rotation
        if rotationQuaternion is not None:
            self.rotationQuaternion = rotationQuaternion
        self.scaling = instancedMesh.scaling
        self.freezeWorldMatrix = instancedMesh.freezeWorldMatrix
        self.tags = instancedMesh.tags
        
        if hasattr(instancedMesh, 'physicsImpostor'):
            self.physicsImpostor = instancedMesh.physicsImpostor
            self.physicsMass = instancedMesh.physicsMass
            self.physicsFriction = instancedMesh.physicsFriction
            self.physicsRestitution = instancedMesh.physicsRestitution
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
     def to_scene_file(self, file_handler):
        file_handler.write('{')
        write_string(file_handler, 'name', self.name, True)
        if hasattr(self, 'parentId'): write_string(file_handler, 'parentId', self.parentId)
        write_vector(file_handler, 'position', self.position)
        if hasattr(self, 'rotation'):
            write_vector(file_handler, 'rotation', self.rotation)
        else:
            write_quaternion(file_handler, 'rotationQuaternion', self.rotationQuaternion)

        write_vector(file_handler, 'scaling', self.scaling)
        # freeze World Matrix currently ignored for instances
        write_bool(file_handler, 'freezeWorldMatrix', self.freezeWorldMatrix)
        write_string(file_handler, 'tags', self.tags)

        if hasattr(self, 'physicsImpostor'):
            write_int(file_handler, 'physicsImpostor', self.physicsImpostor)
            write_float(file_handler, 'physicsMass', self.physicsMass)
            write_float(file_handler, 'physicsFriction', self.physicsFriction)
            write_float(file_handler, 'physicsRestitution', self.physicsRestitution)
            
        file_handler.write('}')
#===============================================================================
class Node(FCurveAnimatable):
    def __init__(self, node):
        Logger.log('processing begun of node:  ' + node.name)
        self.define_animations(node, True, True, True)  #Should animations be done when forcedParent
        self.name = node.name

        if node.parent and node.parent.type != 'ARMATURE':
            self.parentId = node.parent.name

        loc, rot, scale = node.matrix_local.decompose()

        self.position = loc
        if node.rotation_mode == 'QUATERNION':
            self.rotationQuaternion = rot
        else:
            self.rotation = scale_vector(rot.to_euler('XYZ'), -1)
        self.scaling = scale
        self.isVisible = False
        self.isEnabled = True
        self.checkCollisions = False
        self.billboardMode = BILLBOARDMODE_NONE
        self.castShadows = False
        self.receiveShadows = False
        self.tags = ''
        self.layer = -1 # nodes do not have layers attribute
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_scene_file(self, file_handler):
        file_handler.write('{')
        write_string(file_handler, 'name', self.name, True)
        write_string(file_handler, 'id', self.name)
        if hasattr(self, 'parentId'): write_string(file_handler, 'parentId', self.parentId)

        write_vector(file_handler, 'position', self.position)
        if hasattr(self, "rotationQuaternion"):
            write_quaternion(file_handler, "rotationQuaternion", self.rotationQuaternion)
        else:
            write_vector(file_handler, 'rotation', self.rotation)
        write_vector(file_handler, 'scaling', self.scaling)
        write_bool(file_handler, 'isVisible', self.isVisible)
        write_bool(file_handler, 'isEnabled', self.isEnabled)
        write_bool(file_handler, 'checkCollisions', self.checkCollisions)
        write_int(file_handler, 'billboardMode', self.billboardMode)
        write_bool(file_handler, 'receiveShadows', self.receiveShadows)
        write_string(file_handler, 'tags', self.tags)

        super().to_scene_file(file_handler) # Animations
        file_handler.write('}')
#===============================================================================
class SubMesh:
    def __init__(self, materialIndex, verticesStart, indexStart, verticesCount, indexCount):
        self.materialIndex = materialIndex
        self.verticesStart = verticesStart
        self.indexStart = indexStart
        self.verticesCount = verticesCount
        self.indexCount = indexCount
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_scene_file(self, file_handler):
        file_handler.write('{')
        write_int(file_handler, 'materialIndex', self.materialIndex, True)
        write_int(file_handler, 'verticesStart', self.verticesStart)
        write_int(file_handler, 'verticesCount', self.verticesCount)
        write_int(file_handler, 'indexStart'   , self.indexStart)
        write_int(file_handler, 'indexCount'   , self.indexCount)
        file_handler.write('}')
#===============================================================================
bpy.types.Mesh.autoAnimate = bpy.props.BoolProperty(
    name='Auto launch animations',
    description='',
    default = False
)
bpy.types.Mesh.useFlatShading = bpy.props.BoolProperty(
    name='Use Flat Shading',
    description='Obsolete feature.  Only being kept for generating warning message',
    default = False
)
bpy.types.Mesh.isPickable = bpy.props.BoolProperty(
    name='Pickable',
    description='Disable picking for a mesh.',
    default = True
)
bpy.types.Mesh.checkCollisions = bpy.props.BoolProperty(
    name='Check Collisions',
    description='Indicates mesh should be checked that it does not run into anything.',
    default = False
)
bpy.types.Mesh.castShadows = bpy.props.BoolProperty(
    name='Cast Shadows',
    description='',
    default = False
)
bpy.types.Mesh.receiveShadows = bpy.props.BoolProperty(
    name='Receive Shadows',
    description='',
    default = False
)
bpy.types.Mesh.tags = bpy.props.StringProperty(
    name='Tags',
    description='Add meta-data to mesh (space delimited for multiples)',
    default = ''
)
# not currently in use
bpy.types.Mesh.forceBaking = bpy.props.BoolProperty(
    name='Combine Multi-textures / resize',
    description='Also good to adjust single texture\'s size /compression.',
    default = False
)
# not currently in use
bpy.types.Mesh.usePNG = bpy.props.BoolProperty(
    name='Need Alpha',
    description='Saved as PNG when alpha is required, else JPG.',
    default = False
)
bpy.types.Mesh.bakeSize = bpy.props.IntProperty(
    name='Texture Size',
    description='Final dimensions of texture(s).  Not required to be a power of 2, but recommended.',
    default = 1024
)
bpy.types.Mesh.bakeQuality = bpy.props.IntProperty(
    name='Quality 1-100',
    description='For JPG: The trade-off between Quality - File size(100 highest quality)\nFor PNG: amount of time spent for compression',
    default = 50, min = 1, max = 100
)
bpy.types.Mesh.materialNameSpace = bpy.props.StringProperty(
    name='Name Space',
    description='Prefix to use for materials for sharing across .blends.',
    default = DEFAULT_MATERIAL_NAMESPACE
)
bpy.types.Mesh.maxSimultaneousLights = bpy.props.IntProperty(
    name='Max Simultaneous Lights 0 - 32',
    description='BJS property set on each material of this mesh.\nSet higher for more complex lighting.\nSet lower for armatures on mobile',
    default = 4, min = 0, max = 32
)
bpy.types.Mesh.checkReadyOnlyOnce = bpy.props.BoolProperty(
    name='Check Ready Only Once',
    description='When checked better CPU utilization.  Advanced user option.',
    default = False
)
bpy.types.Mesh.freezeWorldMatrix = bpy.props.BoolProperty(
    name='Freeze World Matrix',
    description='Indicate the position, rotation, & scale do not change for performance reasons',
    default = False
)
bpy.types.Mesh.loadDisabled = bpy.props.BoolProperty(
    name='Load Disabled',
    description='Indicate this mesh & children should not be active until enabled by code.',
    default = False
)
bpy.types.Mesh.attachedSound = bpy.props.StringProperty(
    name='Sound',
    description='',
    default = ''
)
bpy.types.Mesh.loopSound = bpy.props.BoolProperty(
    name='Loop sound',
    description='',
    default = True
)
bpy.types.Mesh.autoPlaySound = bpy.props.BoolProperty(
    name='Auto play sound',
    description='',
    default = True
)
bpy.types.Mesh.maxSoundDistance = bpy.props.FloatProperty(
    name='Max sound distance',
    description='',
    default = 100
)
bpy.types.Mesh.ignoreSkeleton = bpy.props.BoolProperty(
    name='Ignore',
    description='Do not export assignment to a skeleton',
    default = False
)
bpy.types.Mesh.maxInfluencers = bpy.props.IntProperty(
    name='Max bone Influencers / Vertex',
    description='When fewer than this are observed, the lower value is used.',
    default = 8, min = 1, max = 8
)

#===============================================================================
class MeshPanel(bpy.types.Panel):
    bl_label = get_title()
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = 'data'
    
    @classmethod
    def poll(cls, context):
        ob = context.object
        return ob is not None and isinstance(ob.data, bpy.types.Mesh)

    def draw(self, context):
        ob = context.object
        layout = self.layout  
        row = layout.row()
        row.prop(ob.data, 'isPickable')
        row.prop(ob.data, 'checkCollisions')
        
        row = layout.row()
        row.prop(ob.data, 'castShadows')
        row.prop(ob.data, 'receiveShadows')
        
        row = layout.row()
        row.prop(ob.data, 'freezeWorldMatrix')
        row.prop(ob.data, 'loadDisabled')
        
        layout.prop(ob.data, 'autoAnimate')
        
        layout.prop(ob.data, 'tags')
        
        box = layout.box()
        box.label(text='Skeleton:')
        box.prop(ob.data, 'ignoreSkeleton')
        row = box.row()
        row.enabled = not ob.data.ignoreSkeleton
        row.prop(ob.data, 'maxInfluencers')
        
        box = layout.box()
        box.label('Materials')
        box.prop(ob.data, 'materialNameSpace')
        box.prop(ob.data, 'maxSimultaneousLights')
        box.prop(ob.data, 'checkReadyOnlyOnce')
        
        box = layout.box()
        box.label(text='Procedural Texture / Cycles Baking')
#        box.prop(ob.data, 'forceBaking')
#        box.prop(ob.data, 'usePNG')
        box.prop(ob.data, 'bakeSize')
        box.prop(ob.data, 'bakeQuality')
        # - - - - - - - - - - - - - - - - - - - - - - - - -
        box = layout.box()
        box.prop(ob.data, 'attachedSound')
        row = box.row()

        row.prop(ob.data, 'autoPlaySound')
        row.prop(ob.data, 'loopSound')
        box.prop(ob.data, 'maxSoundDistance')
