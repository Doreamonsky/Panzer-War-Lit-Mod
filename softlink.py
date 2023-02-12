import os

PRJ_NAME="UnityProject"

root= os.getcwd() + "/" + PRJ_NAME + "-Softlink"
subFolders=["Assets","Packages","ProjectSettings"]

if not os.path.exists(root):
    os.mkdir(root)

def mkStr(str):
    tmp =  r'"' + str + r'"'
    return tmp

for subFolder in subFolders:
    src = os.getcwd() + "/" + PRJ_NAME +"/" + subFolder
    dstFolder = root + "/" 
    dst = dstFolder + "/" + subFolder

    if not os.path.exists(dstFolder):
        os.mkdir(dstFolder)


    cmd = "mklink /J" + " " + mkStr(dst) + " " + mkStr(src)
    os.system(cmd)

input()
