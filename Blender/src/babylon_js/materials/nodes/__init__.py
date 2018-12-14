if "bpy" in locals():
    import imp
    if 'abstract' in locals():
        imp.reload(abstract)
    if 'ambient_occlusion' in locals():
        imp.reload(ambient_occlusion)
    if 'background' in locals():
        imp.reload(background)
    if 'diffuse' in locals():
        imp.reload(diffuse)
    if 'emission' in locals():
        imp.reload(emission)
    if 'fresnel' in locals():
        imp.reload(fresnel)
    if 'glossy' in locals():
        imp.reload(glossy)
    if 'gltf' in locals():
        imp.reload(gltf)
    if 'normal_map' in locals():
        imp.reload(normal_map)
    if 'mapping' in locals():
        imp.reload(mapping)
    if 'passthru' in locals():
        imp.reload(passthru)
    if 'principled' in locals():
        imp.reload(principled)
    if 'refraction' in locals():
        imp.reload(refraction)
    if 'tex_coord' in locals():
        imp.reload(tex_coord)
    if 'tex_environment' in locals():
        imp.reload(tex_environment)
    if 'tex_image' in locals():
        imp.reload(tex_image)
    if 'transparency' in locals():
        imp.reload(transparency)
    if 'unsupported' in locals():
        imp.reload(unsupported)
    if 'uv_map' in locals():
        imp.reload(uv_map)

import bpy