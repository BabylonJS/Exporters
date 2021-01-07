@echo off
rem
rem  testBabylon2Maya.bat  -- Test dot net plugin using mayapy
rem 
rem  usage:  testBabylon2Maya
rem  where:  run all python code in test directory:
rem          located at: .\Tests\*.py
rem

if "%1" == "" goto nomayaver

set mayaversion=%1
set mayalocation=%ProgramFiles%\Autodesk\Maya%mayaversion%

set mayapy=%mayalocation%\bin\mayapy.exe
if not exist "%mayapy%" goto nomayapy

"%mayapy%" -m pip install --upgrade pip --quiet
"%mayapy%" -m pip install virtualenv --quiet

"%mayapy%" -m virtualenv "%~dp0..\venv" -p "%mayapy%" --quiet
if not exist "%~dp0..\venv" goto novenv
call %~dp0..\venv\Scripts\activate.bat

"%mayapy%" -m pip install -r "%~dp0..\python\.requirements" --quiet
"%mayapy%" -m pip install "%~dp0..\python\libMayaExtended" --quiet

"%mayapy%" -m pytest -s -W ignore::DeprecationWarning "%~dp0..\tests"
call %~dp0..\venv\Scripts\deactivate.bat

echo tests completed
goto end

:nomayaver
echo no maya version specified: re-run command with 2019|2020|2021 as argument
goto end

:nomayapy
echo no mayapy executable found at location [%mayapy%]
goto end

:novenv
echo no virtualenv created or found for python execution
goto end

:end
echo exiting bat file