@echo off

call update_deps.cmd
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