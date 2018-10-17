from babylon_js.logger import *
from babylon_js.package_level import *

from .abstract_material import *

import bpy
#===============================================================================
class SimpleMaterial(AbstractMaterial):
    def __init__(self, material_slot, exporter, mesh):
        super().__init__(mesh.data.checkReadyOnlyOnce, mesh.data.maxSimultaneousLights)
        nameSpace = exporter.nameSpace if mesh.data.materialNameSpace == DEFAULT_MATERIAL_NAMESPACE or len(mesh.data.materialNameSpace) == 0 else mesh.data.materialNameSpace
        self.name = nameSpace + '.' + material_slot.name

        Logger.log('processing begun of Standard material:  ' +  material_slot.name, 2)

        # a material slot is not a reference to an actual material; need to look up
        material = material_slot.material

        self.ambient = material.ambient * material.diffuse_color
        self.diffuse = material.diffuse_intensity * material.diffuse_color
        self.specular = material.specular_intensity * material.specular_color
        self.emissive = material.emit * material.diffuse_color
        self.specularPower = material.specular_hardness
        self.alpha = material.alpha

        self.backFaceCulling = material.game_settings.use_backface_culling

        textures = [mtex for mtex in material.texture_slots if mtex and mtex.texture]
        for mtex in textures:
            # test should be un-neccessary, since should be a BakedMaterial; just for completeness
            if (mtex.texture.type != 'IMAGE'):
                continue
            elif not mtex.texture.image:
                Logger.warn('Material has un-assigned image texture:  "' + mtex.name + '" ignored', 3)
                continue
            elif len(mesh.data.uv_textures) == 0:
                Logger.warn('Mesh has no UV maps, material:  "' + mtex.name + '" ignored', 3)
                continue

            if mtex.use_map_diffuse or mtex.use_map_color_diffuse:
                if mtex.texture_coords == 'REFLECTION':
                    Logger.log('Reflection texture found "' + mtex.name + '"', 3)
                    self.textures.append(Texture('reflectionTexture', mtex.diffuse_color_factor, mtex, mesh, exporter))
                else:
                    Logger.log('Diffuse texture found "' + mtex.name + '"', 3)
                    self.textures.append(Texture('diffuseTexture', mtex.diffuse_color_factor, mtex, mesh, exporter))

            if mtex.use_map_ambient:
                Logger.log('Ambient texture found "' + mtex.name + '"', 3)
                self.textures.append(Texture('ambientTexture', mtex.ambient_factor, mtex, mesh, exporter))

            if mtex.use_map_alpha:
                if self.alpha > 0:
                    Logger.log('Opacity texture found "' + mtex.name + '"', 3)
                    self.textures.append(Texture('opacityTexture', mtex.alpha_factor, mtex, mesh, exporter))
                else:
                    Logger.warn('Opacity non-std way to indicate opacity, use material alpha to also use Opacity texture', 4)
                    self.alpha = 1

            if mtex.use_map_emit:
                Logger.log('Emissive texture found "' + mtex.name + '"', 3)
                self.textures.append(Texture('emissiveTexture', mtex.emit_factor, mtex, mesh, exporter))

            if mtex.use_map_normal:
                Logger.log('Bump texture found "' + mtex.name + '"', 3)
                self.textures.append(Texture('bumpTexture', mtex.normal_factor, mtex, mesh, exporter))

            if mtex.use_map_color_spec:
                Logger.log('Specular texture found "' + mtex.name + '"', 3)
                self.textures.append(Texture('specularTexture', mtex.specular_color_factor, mtex, mesh, exporter))