@echo off
echo Cleaning up old versions..
rmdir \s \q "..\ProjectMakoto\bin\x64\Debug\net8.0\Plugins\"
mkdir "..\ProjectMakoto\bin\x64\Debug\net8.0\Plugins\"

echo Moving new plugins..
move "*.pmpl" "..\ProjectMakoto\bin\x64\Debug\net8.0\Plugins\"