using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ZipCreate
{
    class Program
    {
        static void Main(string[] args)
        {
            using var fs = File.Create(args[0]);
            using var outStream = new ZipOutputStream(fs);
            outStream.SetLevel(5);
            outStream.Password = (args[args.Length-1]);
            for(int i=1;i<args.Length-1;i++)
            {
                CompressFiles(args[i], outStream);
            }
            outStream.Close();
        }

        static void CompressFiles(string path,ZipOutputStream inputStream)
        {
            var attr = File.GetAttributes(path);
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                var EntryName = ZipEntry.CleanName(path);
                var newEntry = new ZipEntry(EntryName);
                inputStream.PutNextEntry(newEntry);

                var buffer = new byte[4096];
                using FileStream fileStream = File.OpenRead(path);
                StreamUtils.Copy(fileStream, inputStream, buffer);
            }
            else
            {
                //Get all file paths below this directories.
                var files = Directory.GetFiles(path);

                foreach (var singleFile in files)
                {
                    //Set Entry information
                    var EntryName = ZipEntry.CleanName(singleFile);
                    var newEntry = new ZipEntry(EntryName);
                    inputStream.PutNextEntry(newEntry);

                    //stream input
                    var buffer = new byte[4096];
                    using FileStream fileStream = File.OpenRead(singleFile);
                    StreamUtils.Copy(fileStream, inputStream, buffer);

                }

                //Recursively
                var subFolders = Directory.GetDirectories(path);
                foreach (var dire in subFolders)
                {
                    CompressFiles(dire, inputStream);
                }
            }
        }
    }
}