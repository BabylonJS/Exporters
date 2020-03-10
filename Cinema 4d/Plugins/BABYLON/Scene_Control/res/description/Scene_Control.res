CONTAINER Scene_Control
{
	NAME Scene_Control;
	INCLUDE Obase;

	GROUP ID_OBJECTPROPERTIES
	{   
        GROUP
        {
            LAYOUTGROUP; COLUMNS 2;

            GROUP
            {
                COLOR       BJS_SCENE_CLEAR_COLOR        { PARENTCOLLAPSE; }
                COLOR       BJS_SCENE_AMBIENT_COLOR      { PARENTCOLLAPSE; }
            }

            GROUP
            {   
                REAL        BJS_SCENE_CLEAR_ALPHA        { MIN 0; MAX 1; STEP 0.01; }               
            }             
        }       
        
        BOOL BJS_SCENE_AUTO_CLEAR {} 
        
        REAL BJS_SCENE_GLOBAL_SCALE { MIN 0.0001; }
        
        SEPARATOR { LINE; }
        
        BOOL BJS_SCENE_COLLISIONS_ENABLED {}
        VECTOR BJS_SCENE_GRAVITY {}
        BOOL BJS_SCENE_PHYSICS_ENABLED {}
        VECTOR BJS_SCENE_PHYSICS_GRAVITY {}
        STRING BJS_SCENE_PHYSICS_ENGINE {}
        
        SEPARATOR { LINE; }
        
        BOOL BJS_SCENE_AUTO_ANIMATE {}
        LONG BJS_SCENE_AUTO_ANIMATE_FROM {}
        LONG BJS_SCENE_AUTO_ANIMATE_TO {}
        BOOL BJS_SCENE_AUTO_ANIMATE_LOOP {}
        REAL BJS_SCENE_AUTO_ANIMATE_SPEED {}
        
        SEPARATOR { LINE; }
        
        GROUP
        {
            BUTTON BJS_EXPORT_SCENE_TEMPLATE { }
            BUTTON BJS_EXPORT_SCENE_WEBSITE { }
        }       
	}
}