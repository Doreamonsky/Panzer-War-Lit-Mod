#!/bin/bash

time=$(date "+%Y%m%d%H%M%S")

echo "Start to build the project from CI tool."
echo "Input the modified comment:"
read comment

# 提交 Commit
git add .
git commit -m "$comment"
git push
git push github

# 创建缓存工程
releaseName=UnityProject-Release
output=$time-UnityProject.zip

mkdir $releaseName

cp -r  UnityProject/Assets $releaseName/Assets
cp -r  UnityProject/ProjectSettings $releaseName/ProjectSettings
cp -r  UnityProject/Packages $releaseName/Packages
cp -r  UnityProject/BuildPiplineCI $releaseName/BuildPiplineCI
cp -r  UnityProject/RuntimeRes $releaseName/RuntimeRes
cp -r  UnityProject/PanzerWar-FMod-UGC-Project $releaseName/PanzerWar-FMod-UGC-Project
cp -r  更新工程必看.txt $releaseName/更新工程必看.txt

./zip.exe -r $output $releaseName

rm -r -f $releaseName
mv $output archive/$output