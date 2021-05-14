using System.Collections.Generic;
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
        public static string BumpMapCAtDef => Encoding.UTF8.GetString(Properties.Resources.BUMP_MAP_CAT_DEF);

        public static string HasBabylonAttribute => @"fn hasBabylonCustomAttribute mat attName = ( 
          l = custAttributes.count mat; 
          for i = 1 to l+1 do ( 
            if(mat.custAttributes[i] != undefined) then (
                if(mat.custAttributes[i].name == attName) then return true
            ) ) return false )";

        /// <summary>
        /// Note : We may run a check on all Slate view, because race condition into script execution 
        /// do not ensure the link beetwen material and bump are correctly set when material is updated.
        /// </summary>
        public static string CheckBumpMap = $@"fn checkBumpMap = (
			local babylonAttName = ""Babylon Attributes""
			for v=1 to sme.GetNumViews() do 
			(
				viewNode = sme.GetView(v)
				for i=1 to viewNode.GetNumNodes() do
				(
					n = viewNode.GetNode i;
					if(classof n.reference == Normal_Bump ) then
				    (
					   nbump = n.reference
					   if(hasBabylonCustomAttribute nbump babylonAttName == false ) then
					   (
                            {BumpMapCAtDef}
							custAttributes.add nbump BUMP_MAP_CAT_DEF
					   )
					) 
				)	
			)        
        )";

        /// There is NO WAY to get an even on BumpMap creation into the Slate editor. Only Material creation is triggered.
        /// This is the reason we listen for parameters Material change
        private static string AddPhysicalBabylonUI =
        $@"if ( hasBabylonCustomAttribute maxMaterial ""Babylon Attributes"" == false) then ( 
              {PhysicalBabylonCAtDef}
              custAttributes.add maxMaterial PHYSICAL_MATERIAL_CAT_DEF 
              when parameters maxMaterial change do checkBumpMap()
        )";

        public static string AddCallback => $@"addMaterialCallbackScript = ""maxMaterial = callbacks.notificationParam();
        if classof maxMaterial == StandardMaterial then (
            {StandardBabylonCAtDef.AsInlineScript()} 
            custAttributes.add maxMaterial STANDARD_MATERIAL_CAT_DEF;
        ) else  if classof maxMaterial == PhysicalMaterial then (
            {AddPhysicalBabylonUI.AsInlineScript()}
        ) else  if classof maxMaterial == PBRMetalRough then (
            {AddPhysicalBabylonUI.AsInlineScript()}
        ) else  if classof maxMaterial == PBRSpecGloss then (
            {AddPhysicalBabylonUI.AsInlineScript()}
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

        public static IEnumerable<string> AddCallbacks()
        {
            yield return HasBabylonAttribute;
            yield return CheckBumpMap;
            yield return AddCallback;
        }
    } 
}