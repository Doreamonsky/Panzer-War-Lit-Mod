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
mkdir $releaseName/UnityProject

cp -r  MiddleLibLayer $releaseName/MiddleLibLayer
cp -r  UnityProject/Assets $releaseName/UnityProject/Assets
cp -r  UnityProject/ProjectSettings $releaseName/UnityProject/ProjectSettings
cp -r  UnityProject/Packages $releaseName/UnityProject/Packages
cp -r  UnityProject/BuildPiplineCI $releaseName/UnityProject/BuildPiplineCI
cp -r  PanzerWar-FMod-UGC-Project $releaseName/PanzerWar-FMod-UGC-Project
cp -r  更新工程必看.txt $releaseName/更新工程必看.txt

# URP Proj
mkdir $releaseName/UnityURPProject
cp -r  UnityURPProject/Assets $releaseName/UnityURPProject/Assets
cp -r  UnityURPProject/ProjectSettings $releaseName/UnityURPProject/ProjectSettings
cp -r  UnityURPProject/Packages $releaseName/UnityURPProject/Packages

./zip.exe -r $output $releaseName

rm -r -f $releaseName
mv $output archive/$output