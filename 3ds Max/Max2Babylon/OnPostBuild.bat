SETLOCAL enabledelayedexpansion
@ECHO off

SET config=%~1
SET build_config=NoConfiguration

if /i "%config:~0,5%"=="Debug" (
	SET build_config=Debug
) else if /i "%config:~0,7%"=="Release" (
	SET build_config=Release
)
SET max_version=%2
SET exporter_version=%max_version%
SET max_location=!ADSK_3DSMAX_x64_%max_version%!

IF %exporter_version%==2016 SET exporter_version=2015

ECHO "Max version is %max_version%"
ECHO "Exporter version is %exporter_version%"
ECHO "Build config is %build_config%"

SET source_dir="%~dp0bin\%build_config%\%exporter_version%"
ECHO %source_dir%

IF "%max_location%"=="" (
	ECHO 3DS Max %max_version% not installed. Skipping copy.
	GOTO Close
)

IF build_config=="Debug" GOTO OnDebug
IF build_config=="Release" GOTO OnRelease

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

if exist %dest_dir%\Microsoft.WindowsAPICodePack.dll del /f /q %dest_dir%\Microsoft.WindowsAPICodePack.dll
COPY %source_dir%\Microsoft.WindowsAPICodePack.dll %dest_dir%\Microsoft.WindowsAPICodePack.dll

if exist %dest_dir%\Microsoft.WindowsAPICodePack.Shell.dll del /f /q %dest_dir%\Microsoft.WindowsAPICodePack.Shell.dll
COPY %source_dir%\Microsoft.WindowsAPICodePack.Shell.dll %dest_dir%\Microsoft.WindowsAPICodePack.Shell.dll

if exist %dest_dir%\Microsoft.WindowsAPICodePack.ShellExtensions.dll del /f /q %dest_dir%\Microsoft.WindowsAPICodePack.ShellExtensions.dll
COPY %source_dir%\Microsoft.WindowsAPICodePack.ShellExtensions.dll %dest_dir%\Microsoft.WindowsAPICodePack.ShellExtensions.dll

if exist %dest_dir%\Max2Babylon.dll del /f /q %dest_dir%\Max2Babylon.dll
COPY %source_dir%\Max2Babylon.dll %dest_dir%\Max2Babylon.dll

IF %build_config%==Debug GOTO DebugOnMax
GOTO Close

:NoConfiguration
ECHO "No Configuration"
GOTO Close

:DebugOnMax
ECHO "Launching 3ds Max"
START /d "%max_location%" 3dsmax.exe /Language=ENU
GOTO Close

:Close
PAUSE
EXIT
