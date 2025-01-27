#!/bin/bash
echo "Cleaning up old versions.."
rm -rf "../ProjectMakoto/bin/x64/Debug/net8.0/Plugins/"
mkdir -p "../ProjectMakoto/bin/x64/Debug/net8.0/Plugins/"

echo "Moving new plugins.."
mv *.pmpl "../ProjectMakoto/bin/x64/Debug/net8.0/Plugins/"
