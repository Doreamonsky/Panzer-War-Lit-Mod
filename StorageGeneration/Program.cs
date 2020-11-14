using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StorageGeneration
{
    class Program
    {
        [System.Serializable]
        public class Links
        {
            public List<DownloadLink> downloadLinks = new List<DownloadLink>();
        }

        [System.Serializable]
        public class DownloadLink
        {
            public string packName;

            public string link;

            public string editTime;

            public string size;

            public string description;

            public string platform;

            public string author;

            [System.NonSerialized]
            public DateTime date;
        }

        public static Links links = new Links();

        private static void Main(string[] args)
        {
            var directory = new DirectoryInfo("./");

            var authorDirs = directory.GetDirectories();

            var readMeBuilder = new StringBuilder();

            readMeBuilder.AppendLine("# Mod Download");
            readMeBuilder.AppendLine();

            readMeBuilder.AppendLine("[How to install the mod? Click Here](https://github.com/Doreamonsky/Panzer-War-Lit-Mod)");

            foreach (var authorDir in authorDirs)
            {
                if (authorDir.Name == ".git" || authorDir.Name == "Hidden")
                {
                    continue;
                }


                readMeBuilder.AppendLine($"## {authorDir.Name}");
                readMeBuilder.AppendLine();

                foreach (var file in authorDir.GetFiles("*.modpack"))
                {
                    readMeBuilder.AppendLine($"### {Path.GetFileNameWithoutExtension(file.Name)}");
                    readMeBuilder.AppendLine();

                    var mdName = Path.GetFileNameWithoutExtension(file.Name.Replace("Android_", "").Replace("StandaloneWindows64_", ""));
                    var modRMStream = new FileStream($"{authorDir}/{mdName}.md", FileMode.OpenOrCreate);
                    var streamReader = new StreamReader(modRMStream);
                    readMeBuilder.Append(streamReader.ReadToEnd());
                    readMeBuilder.AppendLine();


                    var picPath = $"{authorDir}/{Path.GetFileNameWithoutExtension(file.Name)}.jpg";
                    var pic = new FileInfo(picPath);

                    var size = file.Length / 1024f / 1024f;
                    readMeBuilder.AppendLine("Size:" + size.ToString("f2") + "MB");

                    if (pic.Exists)
                    {
                        readMeBuilder.AppendLine($"![pic]({picPath})");
                        readMeBuilder.AppendLine();
                    }

                    var platform = "";

                    if (file.Name.Contains("Android"))
                    {
                        readMeBuilder.AppendLine($"Platform:Android");
                        platform = "Android";
                    }

                    if (file.Name.Contains("Windows"))
                    {
                        readMeBuilder.AppendLine($"Platform:Windows");
                        platform = "Windows64";
                    }

                    readMeBuilder.AppendLine($"[Click To Download](https://github.com/Doreamonsky/Panzer-War-Mod-Storage/blob/master/{authorDir.Name}/{file.Name}?raw=true)");
                    readMeBuilder.AppendLine();


                    links.downloadLinks.Add(new DownloadLink()
                    {
                        link = $"{authorDir.Name}/{file.Name}",
                        packName = Path.GetFileNameWithoutExtension(file.Name),
                        size = size.ToString("f1"),
                        description = $"第三方模组 / Game Mod {streamReader.ReadToEnd()}",
                        platform = platform,
                        editTime = $"{file.LastWriteTime.Year}/{file.LastWriteTime.Month}/{file.LastWriteTime.Day}",
                        date = file.LastWriteTime,
                        author = authorDir.Name
                    });

                    streamReader.Close();
                    modRMStream.Close();
                }
            }

            links.downloadLinks.Sort((a, b) => { return -a.date.CompareTo(b.date); });

            var fileStream = new FileStream("ReadMe.md", FileMode.Create);

            var bytes = Encoding.UTF8.GetBytes(readMeBuilder.ToString());

            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();


            var jsonStream = new FileStream("Source.json", FileMode.Create);
            bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(links));

            jsonStream.Write(bytes, 0, bytes.Length);
            jsonStream.Close();

        }
    }
}
