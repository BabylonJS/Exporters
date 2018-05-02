@echo off
if not "%1" == "" goto SwitchSandbox

:Usage 
echo =================================================================================================
echo %~nx0
echo Usage:
echo.
echo %~nx0 ^<sandbox^>
echo.
echo.
echo =================================================================================================

goto :EOF

:SwitchSandbox

set sandbox=%1

SETLOCAL

echo Setting regkey.
reg add hklm\software\microsoft\XboxLive /v Sandbox /d %sandbox% /f

echo Restarting XblAuthManager.
net stop XblAuthManager & net start XblAuthManager

echo Done.
echo.
