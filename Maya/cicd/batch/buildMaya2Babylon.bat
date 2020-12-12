@echo off
rem
rem  buildBabylon2Maya.bat  -- Build and install dot net plugin
rem 
rem  usage:  buildBabylon2Maya
rem  where:  install dll and nll assemblies to 2021 plug-ins directory:
rem          located at: C:\Program Files\Autodesk\Maya(Version)\bin\plug-ins
rem

if "%1" == "" goto nomayaver

set mayaversion=%1
set mayalocation=%ProgramFiles%\Autodesk\Maya%mayaversion%

set solutionfile=%~dp0..\..\Maya2Babylon.sln
if not exist %solutionfile% goto nosolution

set installdir=%mayalocation%\bin\plug-ins
if not exist "%installdir%" goto noplugsdir

set installeddll=%installdir%\Maya2babylon.dll
if exist "%installeddll%" del /f "%installeddll%"
set installedassembly=%installdir%\Maya2babylon.nll.dll
if exist "%installedassembly%" del /f "%installedassembly%"

set releasedir=%~dp0..\..\bin\Release\2020
if exist "%releasedir%" del /f /q "%releasedir%"

set assemblydir=%~dp0..\..\assemblies\2020
if exist "%assemblydir%" del /f /q "%assemblydir%"

msbuild %solutionfile% /v:quiet /t:Rebuild /p:Configuration=Release

set releasedll=%releasedir%\Maya2Babylon.dll
copy /y "%releasedll%" "%installdir%\Maya2Babylon.dll"

set releaseassembly=%assemblydir%\Maya2Babylon.nll.dll
copy /y "%releaseassembly%" "%installdir%\Maya2Babylon.nll.dll"

echo build suceeded
goto end

:nomayaver
echo no maya version specified: re-run command with 2019|2020|2021 as argument
goto end

:nosolution
echo no solution file found [%solutionfile%]
goto end

:noplugsdir
echo no maya plug-ins directory found for install [%installdir%]
goto end

:nomsbuild
echo no msbuild command found on system path [%path%]
goto end

:end
echo exiting bat file