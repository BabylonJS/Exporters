from babylon_js.logger import *
from babylon_js.package_level import *

import bpy
#===============================================================================
# Not intended to be instanced directly
class AbstractMaterial:
    def __init__(self, checkReadyOnlyOnce, maxSimultaneousLights):
        self.checkReadyOnlyOnce = checkReadyOnlyOnce
        self.maxSimultaneousLights = maxSimultaneousLights
        # first pass of textures, either appending image type or recording types of bakes to do
        self.textures = []
# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    def to_scene_file(self, file_handler):
        file_handler.write('{')
        write_string(file_handler, 'name', self.name, True)
        write_string(file_handler, 'id', self.name)
        write_color(file_handler, 'ambient', self.ambient)
        write_color(file_handler, 'diffuse', self.diffuse)
        write_color(file_handler, 'specular', self.specular)
        write_color(file_handler, 'emissive', self.emissive)
        write_float(file_handler, 'specularPower', self.specularPower)
        write_float(file_handler, 'alpha', self.alpha)
        write_bool(file_handler, 'backFaceCulling', self.backFaceCulling)
        write_bool(file_handler, 'checkReadyOnlyOnce', self.checkReadyOnlyOnce)
        write_int(file_handler, 'maxSimultaneousLights', self.maxSimultaneousLights)
        for texSlot in self.textures:
            texSlot.to_scene_file(file_handler)

        file_handler.write('}')