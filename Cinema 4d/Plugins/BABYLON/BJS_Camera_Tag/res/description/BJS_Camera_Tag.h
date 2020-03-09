#ifndef _BJS_Camera_Tag_H_
#define _BJS_Camera_Tag_H_

enum
{   
    BJS_CAMERA_SETTINGS                 = 1000,
    BJS_CAMERA_TYPE                     = 10001,
    BJS_CAMERA_TYPE_COMBO               = 100010,
    BJS_CAMERA_TYPE_FREE                = 100011,
    BJS_CAMERA_TYPE_ARC                 = 100012,
    BJS_CAMERA_TYPE_FOLLOW              = 100013,
    BJS_CAMERA_FOV                      = 10002,
    BJS_CAMERA_MINZ                     = 10003,
    BJS_CAMERA_MAXZ                     = 10004,
    BJS_CAMERA_SPEED                    = 10005,
    BJS_CAMERA_INERTIA                  = 10006,
    BJS_CAMERA_MAKE_ACTIVE              = 10007,

    BJS_ARC_CAMERA_SETTINGS             = 2000,
    BJS_CAMERA_ALPHA                    = 20001,
    BJS_CAMERA_BETA                     = 20002,
    BJS_CAMERA_RADIUS                   = 20003,
    BJS_CAMERA_EYE_SPACE                = 20004,

    BJS_FOLLOW_CAMERA_SETTINGS          = 3000,
    BJS_FOLLOW_CAMERA_HEIGHT_OFFSET     = 30001,
    BJS_FOLLOW_CAMERA_ROTATION_OFFSET   = 30002,

    BJS_CAMERA_MISC_SETTINGS            = 4000,
    BJS_CAMERA_ATTACH_CONTROLS          = 40000,
    BJS_CAMERA_RIG_MODE                 = 40001,
    BJS_CAMERA_CHECK_COLLISIONS         = 40002,
    BJS_CAMERA_APPLY_GRAVITY            = 40003,
    BJS_CAMERA_ELLIPSOID                = 40004,

    BJS_CAMERA_ANIMAION_SETTINGS        = 5000,
    BJS_CAMERA_AUTO_ANIMATE             = 50001,
    BJS_CAMERA_AUTO_ANIMATE_FROM        = 50002,
    BJS_CAMERA_AUTO_ANIMATE_TO          = 50003,
    BJS_CAMERA_AUTO_ANIMATE_LOOP        = 50004,
    BJS_CAMERA_AUTO_ANIMATE_SPEED       = 50005
};

#endif