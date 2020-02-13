using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonTexture
    {
        public enum AddressMode
        {
            CLAMP_ADDRESSMODE = 0,
            WRAP_ADDRESSMODE = 1,
            MIRROR_ADDRESSMODE = 2
        }

        public enum CoordinatesMode
        {
            EXPLICIT_MODE = 0,
            SPHERICAL_MODE = 1,
            PLANAR_MODE = 2
        }

        public enum SamplingMode
        {
            // Constants
            NEAREST_NEAREST_MIPLINEAR = 1, // nearest is mag = nearest and min = nearest and mip = linear
            LINEAR_LINEAR_MIPNEAREST = 2, // Bilinear is mag = linear and min = linear and mip = nearest
            LINEAR_LINEAR_MIPLINEAR = 3, // Trilinear is mag = linear and min = linear and mip = linear
            NEAREST_NEAREST_MIPNEAREST = 4,
            NEAREST_LINEAR_MIPNEAREST = 5,
            NEAREST_LINEAR_MIPLINEAR = 6,
            NEAREST_LINEAR = 7,
            NEAREST_NEAREST = 8,
            LINEAR_NEAREST_MIPNEAREST = 9,
            LINEAR_NEAREST_MIPLINEAR = 10,
            LINEAR_LINEAR = 11,
            LINEAR_NEAREST = 12
        }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public float level { get; set; }

        [DataMember]
        public bool hasAlpha { get; set; }

        [DataMember]
        public bool getAlphaFromRGB { get; set; }

        [DataMember]
        public CoordinatesMode coordinatesMode { get; set; }
        
        [DataMember]
        public bool isCube { get; set; }

        [DataMember]
        public float uOffset { get; set; }

        [DataMember]
        public float vOffset { get; set; }

        [DataMember]
        public float uScale { get; set; }

        [DataMember]
        public float vScale { get; set; }

        [DataMember]
        public float uRotationCenter { get; set; }

        [DataMember]
        public float vRotationCenter { get; set; }

        [DataMember]
        public float wRotationCenter { get; set; }

        [DataMember]
        public bool invertY { get; set; }


        [DataMember]
        public float uAng { get; set; }

        [DataMember]
        public float vAng { get; set; }

        [DataMember]
        public float wAng { get; set; }

        [DataMember]
        public AddressMode wrapU { get; set; }

        [DataMember]
        public AddressMode wrapV { get; set; }

        [DataMember]
        public int coordinatesIndex { get; set; }

        [DataMember]
        public bool isRenderTarget { get; set; }

        [DataMember]
        public int renderTargetSize { get; set; }

        [DataMember]
        public float[] mirrorPlane { get; set; }

        [DataMember]
        public string[] renderList { get; set; }

        [DataMember]
        public BabylonAnimation[] animations { get; set; }

        [DataMember]
        public string[] extensions { get; set; }

        [DataMember]
        public SamplingMode samplingMode { get; set; }

        // Used for gltf export
        public string originalPath;
        // Used for gltf export
        public Bitmap bitmap;

        public string Id { get; }

        public BabylonTexture(string id)
        {
            this.Id = id;
            level = 1.0f;
            uOffset = 0;
            vOffset = 0;
            uScale = 1.0f;
            vScale = 1.0f;
            uRotationCenter = 0.5f;
            vRotationCenter = 0.5f;
            wRotationCenter = 0.5f;
            invertY = true;
            uAng = 0;
            vAng = 0;
            wAng = 0;
            wrapU = AddressMode.WRAP_ADDRESSMODE;
            wrapV = AddressMode.WRAP_ADDRESSMODE;
            hasAlpha = false;
            coordinatesIndex = 0;
            samplingMode = SamplingMode.LINEAR_LINEAR_MIPLINEAR;
        }

        public BabylonTexture(BabylonTexture original)
        {
            Id = Guid.NewGuid().ToString();
            name = original.name;
            level = original.level;
            hasAlpha = original.hasAlpha;
            getAlphaFromRGB = original.getAlphaFromRGB;
            coordinatesMode = original.coordinatesMode;
            isCube = original.isCube;
            uOffset = original.uOffset;
            vOffset = original.vOffset;
            uScale = original.uScale;
            vScale = original.vScale;
            uRotationCenter = original.uRotationCenter;
            vRotationCenter = original.vRotationCenter;
            wRotationCenter = original.wRotationCenter;
            invertY = original.invertY;
            uAng = original.uAng;
            vAng = original.vAng;
            wAng = original.wAng;
            wrapU = original.wrapU;
            wrapV = original.wrapV;
            coordinatesIndex = original.coordinatesIndex;
            isRenderTarget = original.isRenderTarget;
            renderTargetSize = original.renderTargetSize;
            mirrorPlane = original.mirrorPlane;
            renderList = original.renderList;
            animations = original.animations;
            extensions = original.extensions;
            samplingMode = original.samplingMode;
            originalPath = original.originalPath;
            bitmap = original.bitmap;
        }
    }
}
