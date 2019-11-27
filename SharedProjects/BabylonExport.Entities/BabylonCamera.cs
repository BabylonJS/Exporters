using System.Runtime.Serialization;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonCamera : BabylonNode
    {
        public enum CameraMode
        {
            PERSPECTIVE_CAMERA = 0,
            ORTHOGRAPHIC_CAMERA = 1
        }

        public enum Type
        {
            AnaglyphArcRotateCamera,
            AnaglyphFreeCamera,
            ArcRotateCamera,
            DeviceOrientationCamera,
            FollowCamera,
            FreeCamera,
            GamepadCamera,
            TouchCamera,
            VirtualJoysticksCamera,
            WebVRFreeCamera,
            VRDeviceOrientationFreeCamera,
            UniversalCamera
        }

        [DataMember(EmitDefaultValue = false)]
        public string lockedTargetId { get; set; }

        [DataMember]
        public string type { get; set; }

        override public float[] scaling { get; set; }

        [DataMember]
        override public float[] rotation { get; set; }

        [DataMember]
        override public float[] rotationQuaternion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float[] target { get; set; }

        [DataMember]
        public float fov { get; set; }

        [DataMember]
        public float minZ { get; set; }

        [DataMember]
        public float maxZ { get; set; }

        [DataMember]
        public float speed { get; set; }

        [DataMember]
        public float inertia { get; set; }

        [DataMember]
        public float interaxialDistance { get; set; }

        [DataMember]
        public bool checkCollisions { get; set; }

        [DataMember]
        public bool applyGravity { get; set; }

        [DataMember]
        public float[] ellipsoid { get; set; }

        [DataMember]
        public CameraMode mode { get; set; }

        [DataMember]
        public float? orthoLeft { get; set; }

        [DataMember]
        public float? orthoRight { get; set; }

        [DataMember]
        public float? orthoBottom { get; set; }

        [DataMember]
        public float? orthoTop { get; set; }

        [DataMember]
        public bool isStereoscopicSideBySide;

        public BabylonCamera()
        {
            // Default values
            fov = 0.8f;
            minZ = 0.1f;
            maxZ = 5000.0f;
            speed = 1.0f;
            inertia = 0.9f;
            interaxialDistance = 0.0637f;

            mode = CameraMode.PERSPECTIVE_CAMERA;
            orthoLeft = null;
            orthoRight = null;
            orthoBottom = null;
            orthoTop = null;

            type = "FreeCamera";
        }
    }
}
