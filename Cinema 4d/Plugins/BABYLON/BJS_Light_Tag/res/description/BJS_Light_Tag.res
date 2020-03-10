CONTAINER BJS_Light_Tag
{
    NAME BJS_Light_Tag;
        
    GROUP BJS_LIGHT_SETTINGS
    {        
        COLOR BJS_LIGHT_SPECULAR { PARENTCOLLAPSE; }
        
        SEPARATOR { LINE; }
        
        REAL BJS_LIGHT_EXPONENT { MIN 0; STEP 0.1; }
        
        SEPARATOR { LINE; }
        
        BOOL BJS_LIGHT_MAKE_HEMISPHERIC {}
        COLOR BJS_LIGHT_GROUND_COLOR { PARENTCOLLAPSE; }
        
        GROUP BJS_LIGHT_ANIMAION_SETTINGS
        {
            BOOL BJS_LIGHT_AUTO_ANIMATE            {}
            LONG BJS_LIGHT_AUTO_ANIMATE_FROM       {}
            LONG BJS_LIGHT_AUTO_ANIMATE_TO         {}
            BOOL BJS_LIGHT_AUTO_ANIMATE_LOOP       {}
            REAL BJS_LIGHT_AUTO_ANIMATE_SPEED      {}
        }
        
    }
}