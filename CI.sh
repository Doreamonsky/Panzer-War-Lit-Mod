time=$(date "+%Y%m%d%H%M%S")
# 创建缓存工程
releaseName=UnityProject-Release
output=$time-UnityProject.zip

mkdir $releaseName

cp -r  UnityProject/Assets $releaseName/Assets
cp -r  UnityProject/ProjectSettings $releaseName/ProjectSettings
cp -r  UnityProject/Packages $releaseName/Packages
cp -r  UnityProject/BuildPiplineCI $releaseName/BuildPiplineCI
cp -r  UnityProject/RuntimeRes $releaseName/RuntimeRes

./zip.exe -r $output $releaseName

rm -r -f $releaseName
mv $output archive/$output

git add .
git commit -m "Auto Submit"
git push
git push github