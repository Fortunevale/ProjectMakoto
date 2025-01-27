@echo off

set "current_dir=%CD%"
call update_deps.cmd
cd /d "%current_dir%"

del /q *.pmpl

for /D %%i in (*) do (
	if /I "%%i" neq "deps" (
		if /I "%%i" neq "Example" (
			if exist "%%i\.build.cmd" (			
				pushd "%%i"
				echo Running .build.cmd in %%i
				call .\.build.cmd
				popd
				
				rem Move pmpl files to parent directory
				move "%%i\*.pmpl" .
			)
		)
	)
)

rmdir /s /q trusted_manifests
mkdir trusted_manifests

cd deps
dotnet ProjectMakoto.dll --build-manifests .. --output-manifests ../trusted_manifests