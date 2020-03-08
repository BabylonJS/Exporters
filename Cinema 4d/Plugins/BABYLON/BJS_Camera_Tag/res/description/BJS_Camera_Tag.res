CONTAINER BJS_Camera_Tag
{
    NAME BJS_Camera_Tag; 
    
    GROUP BJS_CAMERA_SETTINGS
    {
    
        SEPARATOR { LINE; }
    
        LONG BJS_CAMERA_TYPE
        {
            PARENTCOLLAPSE BJS_CAMERA_TYPE_COMBO;
            CYCLE
            {
                BJS_CAMERA_TYPE_FREE;
                BJS_CAMERA_TYPE_ARC;
                BJS_CAMERA_TYPE_FOLLOW;
            }
        }
        
        SEPARATOR { LINE; }
        
        REAL  BJS_CAMERA_FOV { MINSLIDER 0.0001; MAXSLIDER 2; STEP 0.01; CUSTOMGUI REALSLIDER;} 
        
        GROUP
        {
            LAYOUTGROUP; COLUMNS 2;

            GROUP
            {
                REAL BJS_CAMERA_MINZ        { MIN 0.0001; STEP 0.01; }
            }

            GROUP
            {
                REAL BJS_CAMERA_MAXZ        { MIN 0.0002; STEP 0.01; }
            }             
        }
        
        REAL BJS_CAMERA_SPEED       { MIN 0.0001; STEP 0.01; }
        REAL BJS_CAMERA_INERTIA     { MIN 0.0001; STEP 0.01; }
        
        BOOL BJS_CAMERA_MAKE_ACTIVE    {  }
        
        SEPARATOR { LINE; }        
        
        GROUP BJS_ARC_CAMERA_SETTINGS
        {
            REAL BJS_CAMERA_ALPHA        {}
            REAL BJS_CAMERA_BETA         {}
            REAL BJS_CAMERA_RADIUS       {}
            REAL BJS_CAMERA_EYE_SPACE    {}
        }
        
        GROUP BJS_FOLLOW_CAMERA_SETTINGS
        {
            VECTOR BJS_FOLLOW_CAMERA_HEIGHT_OFFSET     {}
            VECTOR BJS_FOLLOW_CAMERA_ROTATION_OFFSET   {}
        }
        
        GROUP BJS_CAMERA_MISC_SETTINGS
        {
            BOOL BJS_CAMERA_ATTACH_CONTROLS          {}
            LONG BJS_CAMERA_RIG_MODE                 {}
            BOOL BJS_CAMERA_CHECK_COLLISIONS         {}
            BOOL BJS_CAMERA_APPLY_GRAVITY            {}
            VECTOR BJS_CAMERA_ELLIPSOID              {}
        }
        
        GROUP BJS_CAMERA_ANIMAION_SETTINGS
        {
            BOOL BJS_CAMERA_AUTO_ANIMATE            {}
            LONG BJS_CAMERA_AUTO_ANIMATE_FROM       {}
            LONG BJS_CAMERA_AUTO_ANIMATE_TO         {}
            BOOL BJS_CAMERA_AUTO_ANIMATE_LOOP       {}
            REAL BJS_CAMERA_AUTO_ANIMATE_SPEED      {}
        }
        
    }
}