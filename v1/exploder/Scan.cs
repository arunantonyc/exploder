using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Exploder
{
    public enum ItemType
    {
        Folder,
        File
    }
    public class FolderInfo
    {
        public string FolderName;
        public long Size;
        public string ParentFolder;
        public ItemType Type;

        public FolderInfo()
        {
            Size = 0;
            Type = ItemType.Folder;
        }
    }

    public class Scan
    {
        public event EventHandler OnCompleted;
        public static string SearchString;
        public static bool FoundFolderOnly;

        public void GetFiles(string folderPath, string parentPath = "")
        {
            List<FolderInfo> foundFiles = new List<FolderInfo>();
            long totalSize = 0;
            string[] files = new string[0];
            try
            {
                files = Directory.GetFiles(folderPath);
            }
            catch { }
            
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo fi = new FileInfo(files[i]);
                if (!string.IsNullOrEmpty(SearchString))
                    if (fi.Name.ToLower().Contains(SearchString.ToLower()))
                    {
                        FolderInfo ff = new FolderInfo() { FolderName = fi.Name, Size = fi.Length, ParentFolder = folderPath, Type = ItemType.File };
                        foundFiles.Add(ff);
                    }
                totalSize += fi.Length;
            }
            if (OnCompleted != null)
            {
                FolderInfo fi = new FolderInfo() { FolderName = folderPath, Size = totalSize, ParentFolder = parentPath };
                if (string.IsNullOrEmpty(SearchString) || !FoundFolderOnly)
                {
                    OnCompleted(fi, EventArgs.Empty);
                }

                if (foundFiles.Count > 0)
                {
                    if (FoundFolderOnly)
                        OnCompleted(fi, EventArgs.Empty);

                    foreach (FolderInfo ffi in foundFiles)
                    {
                        OnCompleted(ffi, EventArgs.Empty);
                    }
                }
            }
            GetSubfolders(folderPath);
        }

        public void GetSubfolders(string folderPath)
        {
            string[] folders = Directory.GetDirectories(folderPath);
            for (int i = 0; i < folders.Length; i++)
            {
                string folderName = folders[i].Replace("\\", "/");
                GetFiles(folderName, folderPath);
            }
        }
    }
}
