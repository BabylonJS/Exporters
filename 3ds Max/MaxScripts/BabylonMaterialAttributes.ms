-- Create Babylon material attributes
CAT_DEF = attributes Babylon_Attributes 
version:0
(
	Parameters main rollout:params
	(
		'babylonUnlit' Type:#BOOLEAN UI:'babylonUnlit' Default:False
		'babylonAlphaTest' Type:#BOOLEAN UI:'babylonAlphaTest' Default:False
	)
	Rollout Params "Babylon Attributes"
	(
		checkbox 'babylonUnlit' "Unlit" Width:70 Height:15 Align:#Left Offset:[0,0] Type:#BOOLEAN
		checkbox 'babylonAlphaTest' "Alpha Test" Width:70 Height:15 Align:#Left Offset:[0,0] Type:#BOOLEAN
	)
)


-- Add Babylon Attributes to all materials used in the scene
for mat in scenematerials do
(
	defs = custAttributes.getDefs mat;
	if defs != undefined then
	(
		--Check if babylon material attributes are present
		alreadyAdded = false;
		for def in defs do
		(
			if def.name == #Babylon_Attributes then
			(
				alreadyAdded = true;	
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