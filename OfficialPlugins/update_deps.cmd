@echo off

rmdir /S /Q deps
dotnet publish ..\ProjectMakoto\ProjectMakoto.sln --configuration RELEASE --runtime linux-x64 --no-self-contained --property:PublishDir="deps" --framework net8.0
move ..\ProjectMakoto\deps deps

for /D %%i in (*) do (
	if /I "%%i" neq "deps" (
		if exist "%%i\.build.cmd" (
			echo Creating symlink in %%i to deps
			if not exist "%%i\deps" (
				mklink /d "%%i\deps" "..\deps"
			)
			
			pushd "%%i"
			echo Syncing git submodules in %%i
			git submodule update --init --recursive
			popd
		)
	)
)