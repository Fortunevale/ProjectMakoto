#!/bin/bash

current_dir=$(pwd)
./update_deps.sh

if [ "$1" -ne 1 ]; then
    ./update_deps.sh
fi

if [ $? -ne 0 ]; then
  echo "Error: update_deps.sh script failed. Exiting."
  exit 1
fi

cd "$current_dir"
rm -f *.pmpl

for i in */; do
  # Exclude 'deps' and 'Example' directories
  if [ "$i" != "deps/" ] && [ "$i" != "Example/" ]; then
    # Check if .build.sh file exists and is executable
    if [ -x "$i.build.sh" ]; then
      cd "$i"
      echo "Running .build.sh in $i"
      ./.build.sh
	  
	  if [ $? -ne 0 ]; then
	    echo "Error: Build failed."
	    exit 1
	  fi
	  
      cd ..
      
      # Move pmpl files to parent directory
      mv "$i"/*.pmpl .
    fi
  fi
done

rm -rf trusted_manifests
mkdir trusted_manifests

cd deps
dotnet ProjectMakoto.dll --build-manifests .. --output-manifests ../trusted_manifests