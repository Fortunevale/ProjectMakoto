@echo off

call update_deps.cmd

for /D %%i in (*) do (
	if /I "%%i" neq "deps" (
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