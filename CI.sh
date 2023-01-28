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

# 归档
output=$time-UnityProject.zip
git archive --format=zip--output=$output
mv $output archive/$output