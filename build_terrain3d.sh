#!/bin/bash

cd Terrain3D
scons
cd ..
mkdir -p addons
rm -f $(pwd)/addons/terrain_3d
ln -s $(pwd)/Terrain3D/project/addons/terrain_3d $(pwd)/addons/terrain_3d
