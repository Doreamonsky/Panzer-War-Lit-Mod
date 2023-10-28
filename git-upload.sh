#!/bin/bash
echo "Diff ==========================="
git status
echo "Diff End ==========================="

time=$(date "+%Y%m%d%H%M%S")

echo "Start to build the project from CI tool."
echo "Input the modified comment:"
read comment

# 提交 Commit
git add .
git commit -m "$comment"
git push
