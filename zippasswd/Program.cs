using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace zippasswd
{
    public class Program
    {
        public static ZipEntryFactory zipEntryFactory=new ZipEntryFactory();
        static void Main(string[] args)
        {
            const string suffix = ".aria2";
            string pureName = "",prefixOfPath="";
            //Delete original destination file
            DeleteFileAndDirectory(args[0]);
            using (var fs = File.Create(args[0]))
            {
                using (var outStream = new ZipOutputStream(fs))
                {
                    outStream.SetLevel(5);
                    outStream.Password = (args[args.Length - 1]);
                    for (int i = 1; i < args.Length - 1; i++)
                    {
                        /*var cleanName = args[i].Trim();
                        if (cleanName.Last() == '/')
                        {
                            //Clean the last slash(/)
                            pureName = cleanName.Substring(0, cleanName.Length - 1);
                        }
                        else
                        {
                            //Don't need to clean
                            pureName = cleanName;
                        }*/
                        pureName=GetPureName(args[i]);

                        prefixOfPath = pureName.Substring(0, pureName.Length -
                            (pureName.Split('/').Last()).Length);

                        //Console.WriteLine($"prefix of path: {prefixOfPath.Length}");
                        //Console.WriteLine($"pure name: {pureName}");

                        if (prefixOfPath.Length > 0)
                        {
                            Compress(pureName, outStream, prefixOfPath);
                        }
                        else
                        {
                            //No prefix
                            Compress(pureName, outStream);
                        }
                        //string aria2File = pureName + suffix;
                        /*if(File.Exists(aria2File))
                        {
                            File.Delete(aria2File);
                        }
                        else { }*/
                        //DeleteFileAndDirectory(aria2File);
                        //DeleteFileAndDirectory(pureName);
                        /*var attr = File.GetAttributes(pureName);
                        if (!attr.HasFlag(FileAttributes.Directory))
                        {
                            File.Delete(pureName);
                        }
                        else
                        {
                            Directory.Delete(pureName,true);
                        }*/
                    }
                    outStream.Close();
                }
            }

            for (int i = 1; i < args.Length - 1; i++)
            {
                /*var cleanName = args[i].Trim();
                if (cleanName.Last() == '/')
                {
                    //Clean the last slash(/)
                    pureName = cleanName.Substring(0, cleanName.Length - 1);
                }
                else
                {
                    //Don't need to clean
                    pureName = cleanName;
                }*/
                pureName=GetPureName(args[i]);
                string aria2File = pureName + suffix;
                /*
                 * if(File.Exists(aria2File))
                {
                    File.Delete(aria2File);
                }
                else { }*/
                DeleteFileAndDirectory(aria2File);
                DeleteFileAndDirectory(pureName);

            }
        }

        public static string GetPureName(string inName)
        {
            var cleanName=inName.Trim();
            string? tmpName = null;
                if (cleanName.Last() == '/')
                {
                    //Clean the last slash(/)
                    tmpName = cleanName.Substring(0, cleanName.Length - 1);
                }
                else
                {
                    //Don't need to clean
                    tmpName = cleanName;
                }
            return tmpName;
        }
        public static void DeleteFileAndDirectory(string fileDirectoryName)
        {
            //Console.WriteLine($"Now start delete {fileDirectoryName}");
            if (File.Exists(fileDirectoryName) || Directory.Exists(fileDirectoryName))
            {

                //Console.WriteLine($"This {fileDirectoryName} exist");
                var attr = File.GetAttributes(fileDirectoryName);
                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    File.Delete(fileDirectoryName);
                }
                else
                {
                    Directory.Delete(fileDirectoryName, true);
                }
            }
            else
            {
            }
        }
        static void Compress(string path,ZipOutputStream inputStream,string prefixGet="")
        {
            var attr = File.GetAttributes(path);
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                CompressFiles(path, inputStream,prefixGet);
                //Console.WriteLine("This is a file");
            }
            else
            {
                //Get all file paths below this directories.
                var files = Directory.GetFiles(path);
                //Console.WriteLine("Folder");
                //Console.WriteLine($"prefixGet is {prefixGet}");

                foreach (var singleFile in files)
                {
                    CompressFiles(singleFile, inputStream,prefixGet);
                }

                //Recursively
                var subFolders = Directory.GetDirectories(path);
                foreach (var dire in subFolders)
                {
                    Compress(dire, inputStream,prefixGet);
                }
            }
        }
        public static void CompressFiles(string path, ZipOutputStream inputStream,string prefixAdjustment)
        {
            var EntryName = ZipEntry.CleanName(path);
            //var newEntry = new ZipEntry(EntryName);
            ZipEntry newEntry=CreateEntryMore(EntryName,prefixAdjustment);
            
            /*
            if (prefixAdjustment.Length > 0)
            {
                newEntry = zipEntryFactory.MakeFileEntry(EntryName, EntryName.Substring(prefixAdjustment.Length - 1), true);
            }
            else
            {
                newEntry = zipEntryFactory.MakeFileEntry(EntryName, true);
            }*/

            newEntry.IsUnicodeText = true;
            inputStream.PutNextEntry(newEntry);

            var buffer = new byte[4096];
            using FileStream fileStream = File.OpenRead(path);
            StreamUtils.Copy(fileStream, inputStream, buffer);
        }

        public static ZipEntry CreateEntryMore(string entryNameString,string prefixName)
        {
            ZipEntry? newEntry = null;
            if(prefixName.Length>0)
            {
                newEntry = zipEntryFactory.MakeFileEntry(entryNameString, entryNameString.Substring(prefixName.Length - 1), true);
            }
            else
            {
                newEntry=zipEntryFactory.MakeFileEntry(entryNameString, true);
            }
            return newEntry;
        }
    }
}
