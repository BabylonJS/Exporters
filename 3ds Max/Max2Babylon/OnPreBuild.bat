setlocal enabledelayedexpansion

SET configurationName=%1
ECHO %configurationName%

IF "%configurationName%"=="Debug" GOTO OnDebug

:OnDebug
taskkill /f /fi "pid gt 0" /im 3dsmax.exe


pause
exit
