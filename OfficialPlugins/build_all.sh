#!/bin/bash

current_dir=$(pwd)
./update_deps.sh

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
      cd ..
      
      # Move pmpl files to parent directory
      mv "$i"/*.pmpl .
    fi
  fi
done
