-- Create Babylon material attributes
CAT_DEF = attributes Babylon_Attributes 
version:1
(
	Parameters main rollout:params
	(
		'babylonUnlit' Type:#BOOLEAN UI:'babylonUnlit' Default:False
		'babylonAlphaTest' Type:#BOOLEAN UI:'babylonAlphaTest' Default:False
		'babylonAlphaTestPercent' Type:#FLOAT UI:'babylonAlphaTestPercent' Default:0.50
	)
	Rollout Params "Babylon Attributes"
	(
		checkbox 'babylonUnlit' "Unlit" Width:70 Height:15 Align:#Left Offset:[0,0] Type:#BOOLEAN
		checkbox 'babylonAlphaTest' "Alpha Test" Width:70 Height:15 Align:#Left Offset:[0,0] Type:#BOOLEAN
		spinner 'babylonAlphaTestPercent' "Alpha Test Cutoff Percent" Width:140 Height:15 Align:#Left Offset:[0,0] Type:#FLOAT Range:[0,1.0, 0]
	)
)


-- Add Babylon Attributes to all materials used in the scene
for mat in scenematerials do
(
	print ("Adding attributes to " + mat.name)
	defs = custAttributes.getDefs mat;
	if defs != undefined then
	(
		--Check if babylon material attributes are present
		alreadyAdded = false;
		for def in defs do
		(
			-- if attribute already exists, update the definition to use this version.
			if def.name == #Babylon_Attributes then
			(
				alreadyAdded = true;
				CustAttributes.redefine def CAT_DEF.source
			)
		)
		
		-- Add babylon material attributes if not present
		if alreadyAdded == false then
		(
			CustAttributes.add mat CAT_DEF;
		)
	)
	else
	(
		CustAttributes.add mat CAT_DEF;
	)
)