Assembly = dotNetClass "System.Reflection.Assembly"
-- UPDATE YOUR PATH TO BABYLON DLL
Assembly.loadfrom "C:\Program Files\Autodesk\3ds Max 2017\bin\assemblies\Max2Babylon.dll"
maxScriptManager = dotNetObject "Max2Babylon.MaxScriptManager"


----------- METHOD 1 : custom parameters -----------

-- UPDATE YOUR OUTPUT PATH
-- Use \\ or / to separate folder.
-- Use \\\ or // for network path. Ex: "\\\192.168.0.1\\my\\docs\\test.babylon"
param = maxScriptManager.InitParameters "C:\\test.babylon"

-- Print all parameters
print "Export parameters:"
showproperties param

-- UPDATE PARAMETERS
--param.exportOnlySelected = true

maxScriptManager.Export param


----------- METHOD 2 : default parameters -----------

-- UPDATE YOUR OUTPUT PATH
-- Use \\ or / to separate folder.
-- Use \\\ or // for network path. Ex: "\\\192.168.0.1\\my\\docs\\test.babylon"
-- maxScriptManager.Export "C:\\test.babylon"