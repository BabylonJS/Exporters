@echo off
SETLOCAL enabledelayedexpansion

SET versions_to_build=2022 2023 2024 2025 2026 2027
SET build_config=Release

IF NOT "%~1"=="" (
    if /I "%~1"=="Debug" (
        set build_config=Debug
    )
)

SET batch_error=0
for %%v in (%versions_to_build%) do (
    echo ################################
	echo Building for Max version: %%v
    echo ################################
	msbuild -noLogo -verbosity:minimal Max2Babylon.sln /t:Restore;Build /p:Configuration=!build_config!_MAX%%v /p:PostBuildEvent= /p:PreBuildEvent=
	echo.
	if %errorlevel% == 0 (
		echo   SUCCESS
	) else (
		set batch_error=%errorlevel%
		echo   FAILED!!!
	)
	echo.
)
exit /b %batch_error%
	