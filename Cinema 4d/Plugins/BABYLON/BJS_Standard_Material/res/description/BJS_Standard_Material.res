CONTAINER BJS_Standard_Material
{
    NAME BJS_Standard_Material;   
    
    GROUP BJS_MATERIAL_SETTINGS
    {
        GROUP
        {
            STRING       BJS_MATERIAL_NAME        { }
        }
        
        SEPARATOR { LINE; }
        STATICTEXT { NAME BJS_MATERIAL_COLOR_SECTION; }
        GROUP
        {
            LAYOUTGROUP; COLUMNS 2;

            GROUP
            {
                COLOR       BJS_MATERIAL_COLOR_AMBIENT          { PARENTCOLLAPSE; }                
            }

            GROUP
            {
                COLOR       BJS_MATERIAL_COLOR_DIFFUSE          { PARENTCOLLAPSE; }
            }
        }
        GROUP
        {
            LAYOUTGROUP; COLUMNS 2;
            GROUP
            {
                COLOR       BJS_MATERIAL_COLOR_EMISSIVE         { PARENTCOLLAPSE; }
            }
            
            GROUP
            {
                COLOR       BJS_MATERIAL_COLOR_SPECULAR         { PARENTCOLLAPSE; }
            } 
        }
     
    }
}