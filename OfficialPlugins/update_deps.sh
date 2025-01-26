#!/bin/bash

# Remove existing deps directory
rm -rf deps

# Publish ProjectMakoto
dotnet publish ../ProjectMakoto/ProjectMakoto.csproj --configuration RELEASE --runtime linux-x64 --no-self-contained --property:PublishDir="deps" --framework net9.0
mv ../ProjectMakoto/deps deps

# Change to Dependencies directory
original_dir=$(pwd)
cd ../Dependencies

# Remove all directories with "*deps*" recursively
find . -type d -name "*deps*" -exec rm -rf {} \;

# Change back to the original directory
cd "$original_dir"

# Update git submodules in the current directory
git submodule update --init --depth 0

# Iterate over subdirectories
for i in */; do
  # Exclude 'deps' directory
  if [ "$i" != "deps/" ]; then
    # Check if .build.cmd file exists
    if [ -f "$i.build.cmd" ]; then
      echo "Creating symlink in $i to deps"
      
      # Create symlink to deps directory
      if [ ! -e "$i/deps" ]; then
		  # Check the operating system type
		  if [[ "$OSTYPE" == "msys" ]]; then
			# MSYS (Git Bash) system
			echo "mklink /d \"$i\\deps\" \"..\\deps\"" > temp.bat
			c:/windows/system32/cmd.exe //c temp.bat
			rm temp.bat
		  else
			# Assume Linux
			ln -s "../deps" "$i/deps"
		  fi
      fi
      
      # Change to subdirectory
      cd "$i"
      
      echo "Syncing git submodules in $i"
      git submodule update --init --depth 0
      
      # Change back to the original directory
      cd "$original_dir"
    fi
  fi
done
