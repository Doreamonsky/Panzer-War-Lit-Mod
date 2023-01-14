using Newtonsoft.Json;
using ShanghaiWindy.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
                    string author = authorDir.Name;
                    string packageDes = string.Empty;

                    readMeBuilder.AppendLine($"### {Path.GetFileNameWithoutExtension(file.Name)}");
                    readMeBuilder.AppendLine();

                    using (var fileSteam = new FileStream(file.FullName, FileMode.Open))
                    {
                        using (var archive = new ZipArchive(fileSteam, ZipArchiveMode.Read))
                        {
                            foreach(var entry in archive.Entries)
                            {
                                if(entry.Name == "package.json")
                                {
                                    var reader = new StreamReader(entry.Open());
                                    var text = reader.ReadToEnd();
                                    var package = JsonConvert.DeserializeObject<ModPackageInfo>(text);

                                    if(package != null)
                                    {
                                        if (package.description != "The Description of the mod")
                                        {
                                            packageDes = package.description;
                                        }

                                        if (package.dependencies.Length > 0)
                                        {
                                            foreach (var dependency in package.dependencies)
                                            {
                                                packageDes += $"Dependecy:{dependency.packageName} {dependency.packageGuid}";
                                            }
                                        }

                                        if (package.author != "Your Name")
                                        {
                                            author = package.author;
                                        }
                                    }
                                    break;
                                }
                            }
                        };
                    }
                   

                    readMeBuilder.Append(packageDes);
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

                    if (file.Name.Contains("iOS"))
                    {
                        readMeBuilder.AppendLine($"Platform:iOS");
                        platform = "iOS";
                    }


                    readMeBuilder.AppendLine($"[Click To Download](https://github.com/Doreamonsky/Panzer-War-Mod-Storage/blob/master/{authorDir.Name}/{file.Name}?raw=true)");
                    readMeBuilder.AppendLine();


                    links.downloadLinks.Add(new DownloadLink()
                    {
                        link = $"{authorDir.Name}/{file.Name}",
                        packName = Path.GetFileNameWithoutExtension(file.Name),
                        size = size.ToString("f1"),
                        description = $"第三方模组 / Game Mod. {packageDes}",
                        platform = platform,
                        editTime = $"{file.LastWriteTime.Year}/{file.LastWriteTime.Month}/{file.LastWriteTime.Day}",
                        date = file.LastWriteTime,
                        author = author
                    });
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
