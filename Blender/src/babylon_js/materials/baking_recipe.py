from ..logging import *

from .material import *

# need to evaluate the need to bake a mesh before even starting; class also stores specific types of bakes
#===============================================================================
class BakingRecipe:
    def __init__(self, bpyMesh, exporter):
        forceBaking = bpyMesh.data.forceBaking
        self.needsBaking = forceBaking  # need externally by Mesh

        # bakeable channels
        self.diffuseChannel  = False
        self.ambientChannel  = False
        self.emissiveChannel = False
        self.specularChannel = False
        self.bumpChannel     = False

        # un-bakeable Channels
        opacityChannel    = False
        metalChannel      = False
        refractionChannel = False

        # need to get the node trees of each material using nodes, if baking ends up required, so temp node can be added
        self.node_trees = []

        # only used, when mesh does not end up being baked
        self.bjsMaterials = []

        # when only one material, can selectively only bake the channel which really need it
        # force baking also ignores any image textures
        isMultiMaterial = len(bpyMesh.material_slots) > 1 or forceBaking
        firstMatUsingNodes = None

        for material_slot in bpyMesh.material_slots:
            # a material slot is not a reference to an actual material; need to look up
            bpyMaterial = material_slot.material

            bjsMaterial = BJSMaterial(bpyMaterial, exporter)
            self.bjsMaterials.append(bjsMaterial)

            if bpyMaterial.use_nodes == True:
                if firstMatUsingNodes is None:
                    firstMatUsingNodes = bjsMaterial
                    if isMultiMaterial:
                        firstMatUsingNodes.name = bpyMesh.name + '_baked'

                self.node_trees.append(bpyMaterial.node_tree)

                bjsNodeTree = bjsMaterial.bjsNodeTree
                self.needsBaking |= bjsNodeTree.mustBake

                if isMultiMaterial:
                    self.diffuseChannel  |= bjsNodeTree.usesDiffuseChannel()
                    self.ambientChannel  |= bjsNodeTree.usesAmbientChannel()
                    self.emissiveChannel |= bjsNodeTree.usesEmissiveChannel()
                    self.specularChannel |= bjsNodeTree.usesSpecularChannel()
                    self.bumpChannel     |= bjsNodeTree.usesBumpChannel()

                    # un-bakeable channels
                    opacityChannel    |= bjsNodeTree.usesOpacityChannel()
                    metalChannel      |= bjsNodeTree.usesMetalChannel()
                    refractionChannel |= bjsNodeTree.usesRefractionChannel()
                else:
                    self.diffuseChannel  = bjsNodeTree.mustBakeDiffuse
                    self.ambientChannel  = bjsNodeTree.mustBakeAmbient
                    self.emissiveChannel = bjsNodeTree.mustBakeEmissive
                    self.specularChannel = bjsNodeTree.mustBakeSpecular
                    self.bumpChannel     = bjsNodeTree.mustBakeNormal

                    # un-bakeable channels
                    opacityChannel    = bjsNodeTree.mustBakeOpacity
                    metalChannel      = bjsNodeTree.mustBakeMetal
                    refractionChannel = bjsNodeTree.mustBakeRefraction

        if self.needsBaking:
            if opacityChannel:
                Logger.warn('opacity channel baking required, but not possible, ignored', 3)
            if metalChannel:
                Logger.warn('Metal channel baking required, but not possible, ignored', 3)
            if refractionChannel:
                Logger.warn('Refraction channel baking required, but not possible, ignored', 3)

            # when baking let the values of any custom properties come from the first node material
            self.bakedMaterial = BJSMaterial(firstMatUsingNodes, exporter)
            if not isMultiMaterial:
                # there may be other image textures, which could be transferred when not multi material
                self.bakedMaterial.processImageTextures(bpyMesh)

            self.bakedMaterial.bake(bpyMesh, self)

            exporter.materials.append(self.bakedMaterial)
            exporter.hasTextures = True