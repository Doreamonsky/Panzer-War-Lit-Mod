time=$(date "+%Y%m%d%H%M%S")
./zip.exe -r $time-UnityProject.zip UnityProject
git add .
git commit -m "Auto Submit"
git push
git push github