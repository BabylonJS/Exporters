SETLOCAL enabledelayedexpansion
@ECHO off


SET max_version=%2
SET max_location=!ADSK_3DSMAX_x64_%max_version%!

SET source_dir="%~dp0%max_version%\bin\%1"
ECHO %source_dir%

IF %1=="Debug" GOTO OnDebug
IF %1=="Release" GOTO OnRelease



:OnDebug
SET dest_dir="%max_location%bin\assemblies"
GOTO CopyFiles

:OnRelease
SET dest_dir="%max_location%bin\assemblies"
GOTO CopyFiles

:CopyFiles
ECHO :: Copying plug-in files
ECHO :: From: %source_dir%
ECHO :: To: %dest_dir%

if exist %dest_dir%\GDImageLibrary.dll del /f /q %dest_dir%\GDImageLibrary.dll
COPY %source_dir%\GDImageLibrary.dll %dest_dir%\GDImageLibrary.dll

if exist %dest_dir%\Newtonsoft.Json.dll del /f /q %dest_dir%\Newtonsoft.Json.dll
COPY %source_dir%\Newtonsoft.Json.dll %dest_dir%\Newtonsoft.Json.dll

if exist %dest_dir%\SharpDX.dll del /f /q %dest_dir%\SharpDX.dll
COPY %source_dir%\SharpDX.dll %dest_dir%\SharpDX.dll

if exist %dest_dir%\SharpDX.Mathematics.dll del /f /q %dest_dir%\SharpDX.Mathematics.dll
COPY %source_dir%\SharpDX.Mathematics.dll %dest_dir%\SharpDX.Mathematics.dll

if exist %dest_dir%\TargaImage.dll del /f /q %dest_dir%\TargaImage.dll
COPY %source_dir%\TargaImage.dll %dest_dir%\TargaImage.dll

if exist %dest_dir%\TQ.Texture.dll del /f /q %dest_dir%\TQ.Texture.dll
COPY %source_dir%\TQ.Texture.dll %dest_dir%\TQ.Texture.dll

if exist %dest_dir%\Max2Babylon.dll del /f /q %dest_dir%\Max2Babylon.dll
COPY %source_dir%\Max2Babylon.dll %dest_dir%\Max2Babylon.dll



IF %1=="Debug" GOTO DebugOnMax
GOTO Close

:NoConfiguration
ECHO "No Configuaration"
GOTO Close

:DebugOnMax
START /d "%max_location%" 3dsmax.exe
GOTO Close

:Close
PAUSE
EXIT