
===============================
FreeImage_TmoDrago03
===============================
DLL_API FIBITMAP* DLL_CALLCONV FreeImage_TmoDrago03(FIBITMAP *src, double gamma FI_DEFAULT(2.2), double exposure FI_DEFAULT(0));

Converts a High Dynamic Range image to a 24-bit RGB image using a global operator based on logarithmic compression of luminance values, imitating the human response to light. A bias power function is introduced to adaptively vary logarithmic bases, resulting in good preservation of details and contrast.
Upon entry, gamma (where gamma > 0) is a gamma correction that is applied after the tone mapping. A value of 1 means no correction. The default 2.2 value, used in the original author’s paper, is recommended as a good starting value.
The exposure parameter, in the range [-8, 8], is an exposure scale factor allowing users to adjust the brightness of the output image to their displaying conditions. The default value (0) means that no correction is applied. Higher values will make the image lighter whereas lower values make the image darker.



===============================
FreeImage_TmoFattal02
===============================
DLL_API FIBITMAP *DLL_CALLCONV FreeImage_TmoFattal02(FIBITMAP *src, double color_saturation FI_DEFAULT(0.5), double attenuation FI_DEFAULT(0.85));

Converts a High Dynamic Range image to a 24-bit RGB image using a local operator that manipulate the gradient field of the luminance image by attenuating the magnitudes of large gradients.
A new, low dynamic range image is then obtained by solving a Poisson equation on the modified gradient field.
Upon entry, the color_saturation parameter, in the range [0.4, 0.6], controls color saturation in the resulting image.
The attenuation parameter, in the range [0.8, 0.9], controls the amount of attenuation.
The algorithm works by solving as many Partial Differential Equations as there are pixels in the image, using a Poisson solver based on a multigrid algorithm. Thus, the algorithm may take many minutes (up to 5 or more) before to complete.



===============================
FreeImage_TmoReinhard05
===============================
DLL_API FIBITMAP* DLL_CALLCONV FreeImage_TmoReinhard05(FIBITMAP *src, double intensity FI_DEFAULT(0), double contrast FI_DEFAULT(0));

Converts a High Dynamic Range image to a 24-bit RGB image using a global operator inspired by photoreceptor physiology of the human visual system.
Upon entry, the intensity parameter, in the range [-8, 8], controls the overall image intensity. The default value 0 means no correction. Higher values will make the image lighter whereas lower values make the image darker.
The contrast parameter, in the range [0.3, 1.0[, controls the overall image contrast. When using the default value (0), this parameter is calculated automatically.
