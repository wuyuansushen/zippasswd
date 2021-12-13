using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace zippasswd
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var fs = File.Create(args[0]))
            {
                string suffix = ".aria2";
                using (var outStream = new ZipOutputStream(fs))
                {
                    outStream.SetLevel(5);
                    outStream.Password = (args[args.Length - 1]);
                    for (int i = 1; i < args.Length - 1; i++)
                    {
                        var cleanName = args[i].Trim();
                        var pureName = "";
                        if(cleanName.Last()=='/')
                        { pureName = cleanName.Substring(0, cleanName.Length - 1); }
                        else { pureName = cleanName; }
                        Compress(pureName, outStream);
                        File.Delete(pureName);
                        string aria2File = pureName + suffix;
                        if(File.Exists(aria2File))
                        {
                            File.Delete(aria2File);
                        }
                        else { }
                        if(File.Exists(pureName))
                        {
                            File.Delete(pureName);
                        }
                        else { }
                    }
                    outStream.Close();
                }
            }
        }

        static void Compress(string path,ZipOutputStream inputStream)
        {
            var attr = File.GetAttributes(path);
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                CompressFiles(path, inputStream);
            }
            else
            {
                //Get all file paths below this directories.
                var files = Directory.GetFiles(path);

                foreach (var singleFile in files)
                {
                    CompressFiles(singleFile, inputStream);
                }

                //Recursively
                var subFolders = Directory.GetDirectories(path);
                foreach (var dire in subFolders)
                {
                    Compress(dire, inputStream);
                }
            }
        }
        static void CompressFiles(string path, ZipOutputStream inputStream)
        {
            var EntryName = ZipEntry.CleanName(path);
            var newEntry = new ZipEntry(EntryName);
            newEntry.IsUnicodeText = true;
            inputStream.PutNextEntry(newEntry);

            var buffer = new byte[4096];
            using FileStream fileStream = File.OpenRead(path);
            StreamUtils.Copy(fileStream, inputStream, buffer);
        }
    }
}
