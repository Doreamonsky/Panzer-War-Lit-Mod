using System.IO;
using System.Text;

namespace StorageGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            var directory = new DirectoryInfo("./");

            var authorDirs = directory.GetDirectories();

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("# Mod Download / 模组下载一览");
            stringBuilder.AppendLine();

            stringBuilder.AppendLine("[How to install the mod? Click Here](https://github.com/Doreamonsky/Panzer-War-Lit-Mod)");
            stringBuilder.AppendLine("[如何安装模组? 点击这里](https://github.com/Doreamonsky/Panzer-War-Lit-Mod/wiki/%E6%A8%A1%E7%BB%84%E4%B8%8B%E8%BD%BD%E6%8C%87%E5%8D%97)");

            foreach (var authorDir in authorDirs)
            {
                if (authorDir.Name == ".git")
                {
                    continue;
                }

                stringBuilder.AppendLine($"## {authorDir.Name}");
                stringBuilder.AppendLine();

                foreach (var file in authorDir.GetFiles("*.modpack"))
                {
                    stringBuilder.AppendLine($"### {Path.GetFileNameWithoutExtension(file.Name)}");
                    stringBuilder.AppendLine();

                    var modRMStream = new FileStream($"{authorDir}/{Path.GetFileNameWithoutExtension(file.Name)}.md", FileMode.OpenOrCreate);
                    var streamReader = new StreamReader(modRMStream);
                    stringBuilder.Append(streamReader.ReadToEnd());
                    stringBuilder.AppendLine();

                    var picPath = $"{authorDir}/{Path.GetFileNameWithoutExtension(file.Name)}.jpg";
                    var pic = new FileInfo(picPath);

                    var size = file.Length / 1024f / 1024f;
                    stringBuilder.AppendLine("Size/大小:" + size.ToString("f2") + "MB");

                    if (pic.Exists)
                    {
                        stringBuilder.AppendLine($"![pic]({picPath})");
                        stringBuilder.AppendLine();
                    }

                    if (file.Name.Contains("Android"))
                    {
                        stringBuilder.AppendLine($"Platform:Android / 支持平台:安卓");
                    }

                    if (file.Name.Contains("Windows"))
                    {
                        stringBuilder.AppendLine($"Platform:Windows / 支持平台:电脑");
                    }

                    stringBuilder.AppendLine($"[Click To Download/点击下载](https://github.com/Doreamonsky/Panzer-War-Mod-Storage/blob/master/{authorDir.Name}/{file.Name}?raw=true)");
                    stringBuilder.AppendLine();
                }
            }

            var fileStream = new FileStream("ReadMe.md", FileMode.Create);

            var bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());

            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();
        }
    }
}
