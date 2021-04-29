using System.Text;

namespace Max2Babylon
{
    public static class MaterialScripts
    {
        public static string AsInlineScript( this string src) => src.Replace("\"", "\\\"");

        public static string AddCustomAttribute( this string decl, string container, string def) =>  $@"{decl}
        maxMaterial = sceneMaterials[""{container}""]
        custAttributes.add maxMaterial {def};";

        public static string StandardBabylonCAtDef => Encoding.UTF8.GetString(Properties.Resources.STANDARD_MATERIAL_CAT_DEF);
        public static string AIBabylonCAtDef => Encoding.UTF8.GetString(Properties.Resources.ARNOLD_MATERIAL_CAT_DEF);
        public static string PhysicalBabylonCAtDef => Encoding.UTF8.GetString(Properties.Resources.PHYSICAL_MATERIAL_CAT_DEF);
        public static string AddCallback => $@"addMaterialCallbackScript = ""maxMaterial = callbacks.notificationParam();
        if classof maxMaterial == StandardMaterial then (
            {StandardBabylonCAtDef.AsInlineScript()} 
            custAttributes.add maxMaterial STANDARD_MATERIAL_CAT_DEF;
        ) else  if classof maxMaterial == PhysicalMaterial then (
            {PhysicalBabylonCAtDef.AsInlineScript()}
            custAttributes.add maxMaterial PHYSICAL_MATERIAL_CAT_DEF;
        ) else  if classof maxMaterial == PBRMetalRough then (
            {PhysicalBabylonCAtDef.AsInlineScript()}
            custAttributes.add maxMaterial PHYSICAL_MATERIAL_CAT_DEF;
        ) else  if classof maxMaterial == PBRSpecGloss then (
            {PhysicalBabylonCAtDef.AsInlineScript()}
            custAttributes.add maxMaterial PHYSICAL_MATERIAL_CAT_DEF;
        ) else if classof maxMaterial == ai_standard_surface then (
            {AIBabylonCAtDef.AsInlineScript()}
            custAttributes.add maxMaterial babylonAttributesDataCA;
        )"";

        -- Remove any definition of this callback
        callbacks.removeScripts id:#BabylonAttributesMaterial;

        -- Add a callback triggered when a new material is created
        -- Note: The callback is NOT persistent (default value is false).
        -- This means that it is not linked to a specific file.
        -- Rather, the callback is active for the current run of 3ds Max.
        callbacks.addScript #mtlRefAdded addMaterialCallbackScript id:#BabylonAttributesMaterial;";
    } 
}