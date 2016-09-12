using System.Collections.Generic;
using System.IO;
using System.Linq;
using Uco.Infrastructure.Livecycle;

namespace Uco.Models
{
    public class Folder
    {
        //private
        private string FolderPath = string.Empty;

        //public
        public List<string> AllowedFileExtentions
        {
            get
            {
                List<string> Extensions = new List<string>();
                try
                {
                    string RootPath = LS.CurrentHttpContext.Server.MapPath(FileExtensionsPath);
                    string[] AllFiles = Directory.GetFiles(RootPath);

                    foreach (string FileName in AllFiles)
                    {
                        if (string.IsNullOrEmpty(FileName)) continue;
                        string FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FileName);
                        Extensions.Add(FileNameWithoutExtension);
                    }
                }
                catch
                {

                }
                return Extensions;
            }
        }

        public string FileExtensionsPath = "~/Content/FileExtensions/";

        public string GetFileExtentionNoDot(string FilePath)
        {
            string Extension = Path.GetExtension(FilePath);
            if (string.IsNullOrEmpty(Extension)) return string.Empty;
            else return Extension.Replace(".", "");
        }

        public List<FileItem> Folder2List()
        {
            List<FileItem> allFiles = new List<FileItem>();

            //create directory if not exist
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

            //get all files in directory
            string RootPath = LS.CurrentHttpContext.Server.MapPath("~");
            string[] AllFiles = Directory.GetFiles(LS.CurrentHttpContext.Server.UrlDecode(FolderPath));

            //add files from directory to allFiles
            foreach (string item in AllFiles)
            {
                string Extension = this.GetFileExtentionNoDot(item);
                if (!AllowedFileExtentions.Contains(Extension)) continue;

                string FileName = item.Replace(RootPath, "");
                FileName = "~/" + FileName.Replace("\\", "/").ToLower();

                FileItem newFile = new FileItem();
                newFile.FileName = FileName;
                newFile.FileType = Extension;
                newFile.Title = Path.GetFileNameWithoutExtension(FileName);
                allFiles.Add(newFile);
            }

            //create info.data in directory if not exist
            string FolderFilePath = FolderPath + "\\info.data";
            if (!File.Exists(FolderFilePath)) File.CreateText(FolderFilePath).Close();

            //read info.data to allFiles
            string FolderFile = File.ReadAllText(FolderFilePath);
            if (!string.IsNullOrEmpty(FolderFile))
            {
                foreach (string item in FolderFile.Split('\n'))
                {
                    if (string.IsNullOrEmpty(item)) { continue; }
                    string[] t = item.Split('^');
                    if (t.Length == 4)
                    {
                        FileItem n = allFiles.FirstOrDefault(r => r.FileName == t[1]);
                        if (n != null)
                        {
                            n.Title = t[3];
                            int tempInt = 0;
                            int.TryParse(t[0], out tempInt);
                            if (tempInt == 0) { tempInt = 100; }
                            n.Order = tempInt;
                        }
                    }
                }
            }

            return allFiles.OrderBy(r => r.Order).ToList();
        }

        public bool List2Folder(List<FileItem> Items)
        {
            //create directory if not exist
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

            //create string
            string allFilesString = "";
            foreach (FileItem item in Items)
            {
                allFilesString = allFilesString + item.Order + "^" + item.FileName + "^^" + item.Title + "\n";
            }

            //create file
            string FolderFilePath = FolderPath + "\\info.data";
            try
            {
                File.WriteAllText(FolderFilePath, allFilesString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //constructor
        public Folder(string FolderPath)
        {
            this.FolderPath = FolderPath;
        }

        //nested class
        public class FileItem
        {
            public int Order { get; set; }
            public string Title { get; set; }
            public string FileName { get; set; }
            public string FileType { get; set; }

            public string FileNameNoFolder
            {
                get
                {
                    if (string.IsNullOrEmpty(FileName)) return string.Empty;
                    else return Path.GetFileName(FileName);
                }
            }

            public FileItem()
            {
                this.Order = 100;
            }
        }
    }
}