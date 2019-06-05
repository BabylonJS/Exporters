setlocal enabledelayedexpansion

SET configurationName=%1
ECHO %configurationName%

IF "%configurationName%"=="Debug" GOTO OnDebug

:OnDebug
taskkill  /im 3dsmax.exe /f /fi "STATUS eq RUNNING"


pause
exit
