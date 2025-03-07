@echo off

rmdir /S /Q deps
dotnet publish ..\ProjectMakoto\ProjectMakoto.csproj --configuration RELEASE --runtime linux-x64 --no-self-contained --property:PublishDir="deps" --framework net9.0
move ..\ProjectMakoto\deps deps

set "original_dir=%CD%"
cd /d "..\Dependencies"

for /d /r %%i in (*deps*) do (
    rd /s /q "%%i"
)

cd /d "%original_dir%"

git submodule update --init --depth 0

for /D %%i in (*) do (
	if /I "%%i" neq "deps" (
		if exist "%%i\.build.cmd" (
			echo Creating symlink in %%i to deps
			if not exist "%%i\deps" (
				mklink /d "%%i\deps" "..\deps"
			)
			
			cd "%%i"
			
			echo Syncing git submodules in %%i
			git submodule update --init --depth 0
			
			cd ..
		)
	)
)
