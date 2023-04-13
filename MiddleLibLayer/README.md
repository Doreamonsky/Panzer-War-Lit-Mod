# NPM 版 装甲纷争 Mod 开发工具

原文档： <https://www.yuque.com/chaojiduolajiang/panzerwar/ehh8ukt16xkf99zw>


好处：

1. 通过 Package Manger 管理版本，无需自己下载解压新的工具
2. 可以切任意旧版本工具，方便回退与更新

坏处：

1. 国内部分地方可能访问会比较慢
2. 无法修改模组工具中的任何内容 
# 使用方式
先需要在项目的Packages/manifest.json中，添加middlelayer的源信息，在这个文件的dependencies节点前增加以下代码

"scopedRegistries": [
  {
    "name": "middlelayer",
    "url": "https://registry.npmjs.org",
    "scopes": [
      "com.shanghaiwindy"
    ]
  }
],

如下所示：


![image.png](https://cdn.nlark.com/yuque/0/2023/png/23154724/1681296705156-da1a713a-0da6-4bcc-b400-cb59be96cc01.png#averageHue=%23201f1f&clientId=ue821d961-8f10-4&from=paste&height=625&id=u0174fa70&name=image.png&originHeight=625&originWidth=918&originalType=binary&ratio=1&rotation=0&showTitle=false&size=66728&status=done&style=none&taskId=ue318e930-a196-427e-8ba5-08971563982&title=&width=918)
然后通过Unity的Window->Package Manager菜单，打开Package Manager.
如果已安装本地的** Panzer War MiddleLayer (Mod Libs)，**则需要先把本地的从 Package Manager 中移除。然后再安装 NPM 版。

点击 Add package by name...
![image.png](https://cdn.nlark.com/yuque/0/2023/png/23154724/1681297240891-a6f4203c-eecc-4c13-9997-36af86b46f44.png#averageHue=%23585353&clientId=uc0d34bb3-3b47-4&from=paste&height=199&id=u47413db8&name=image.png&originHeight=199&originWidth=502&originalType=binary&ratio=1&rotation=0&showTitle=false&size=17391&status=done&style=none&taskId=ue13e4852-ddd1-46af-be1a-21483933080&title=&width=502)
然后输入：
**com.shanghaiwindy.middlelayer**
就会自动开始下载并安装。

以上是核心的 Mod 工具的安装说明，还有如下包名可以在需要的时候进行安装：
安装方式也是点击 Add package by name...，然后输入包名

1. 共享的配置和共享的内构模型

包名：[com.shanghaiwindy.middlesharedresources](https://www.npmjs.com/package/com.shanghaiwindy.middlesharedresources)

# 更新版本
当存在新版本时候，会在 remove 旁边出现 update 的按钮
![image.png](https://cdn.nlark.com/yuque/0/2023/png/23154724/1681297447628-57d6a7ff-87d9-47e8-9be4-826cd9633d18.png#averageHue=%23393939&clientId=uc0d34bb3-3b47-4&from=paste&height=553&id=u8c8b397c&name=image.png&originHeight=553&originWidth=1142&originalType=binary&ratio=1&rotation=0&showTitle=false&size=87546&status=done&style=none&taskId=ua456de8d-f064-4067-aaf0-4641cce87ad&title=&width=1142)
# 回退版本
点击 See other version，然后选择旧版本进行安装
![image.png](https://cdn.nlark.com/yuque/0/2023/png/23154724/1681297399131-c72f4b6c-c86d-45ec-bef1-2f460fe2ae3f.png#averageHue=%23453f3f&clientId=uc0d34bb3-3b47-4&from=paste&height=97&id=uf3011054&name=image.png&originHeight=97&originWidth=510&originalType=binary&ratio=1&rotation=0&showTitle=false&size=15189&status=done&style=none&taskId=ufe23a11d-e46b-439e-be28-101768f8ee7&title=&width=510)
