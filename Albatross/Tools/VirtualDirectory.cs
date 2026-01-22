using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace Albatross.Tools
{
    public class VirtualDirectory
    {
        public string Name;
        public List<VirtualDirectory> Folders;
        public Dictionary<string, SubMemoryStream> Files;
        public Color Color = Color.Black;

        public VirtualDirectory()
        {
            Name = "";
            Folders = new List<VirtualDirectory>();
            Files = new Dictionary<string, SubMemoryStream>();
        }

        public VirtualDirectory(string name)
        {
            Name = name;
            Folders = new List<VirtualDirectory>();
            Files = new Dictionary<string, SubMemoryStream>();
        }

        // -------------------------
        // Folder helpers
        // -------------------------
        public VirtualDirectory GetFolder(string name)
        {
            return Folders.FirstOrDefault(f => f.Name == name);
        }

        public VirtualDirectory GetOrCreateFolder(string name)
        {
            var folder = GetFolder(name);
            if (folder == null)
            {
                folder = new VirtualDirectory(name);
                Folders.Add(folder);
            }
            return folder;
        }

        public VirtualDirectory GetFolderFromFullPath(string path)
        {
            string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            VirtualDirectory current = this;

            for (int i = 0; i < parts.Length; i++)
            {
                current = current.GetFolder(parts[i]);
                if (current == null)
                    throw new DirectoryNotFoundException(path + " not exist");
            }

            return current;
        }

        public VirtualDirectory GetOrCreateFolderFromFullPath(string path)
        {
            string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            VirtualDirectory current = this;

            for (int i = 0; i < parts.Length; i++)
                current = current.GetOrCreateFolder(parts[i]);

            return current;
        }

        // -------------------------
        // File access
        // -------------------------
        public byte[] GetFileFromFullPath(string path)
        {
            string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string fileName = parts[parts.Length - 1];

            VirtualDirectory current = this;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                current = current.GetFolder(parts[i]);
                if (current == null)
                    throw new DirectoryNotFoundException(path + " not exist");
            }

            if (!current.Files.ContainsKey(fileName))
                throw new FileNotFoundException(fileName + " not exist");

            SubMemoryStream stream = current.Files[fileName];
            if (stream.ByteContent == null)
                stream.Read();

            return stream.ByteContent;
        }

        /// <summary>
        /// Gets file data without marking it as modified.
        /// Use this for read-only operations to prevent files from being unnecessarily re-saved.
        /// </summary>
        /// <param name="path">Full path to the file</param>
        /// <returns>Byte array containing the file data</returns>
        public byte[] GetFileDataReadOnly(string path)
        {
            string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string fileName = parts[parts.Length - 1];

            VirtualDirectory current = this;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                current = current.GetFolder(parts[i]);
                if (current == null)
                    throw new DirectoryNotFoundException(path + " not exist");
            }

            if (!current.Files.ContainsKey(fileName))
                throw new FileNotFoundException(fileName + " not exist");

            SubMemoryStream stream = current.Files[fileName];

            // If ByteContent is already set, return it (file was already loaded/modified)
            if (stream.ByteContent != null)
                return stream.ByteContent;

            // Otherwise, read without caching to avoid marking as modified
            return stream.ReadWithoutCaching();
        }

        // -------------------------
        // Enumeration
        // -------------------------
        public Dictionary<string, VirtualDirectory> GetAllFoldersAsDictionnary()
        {
            Dictionary<string, VirtualDirectory> result = new Dictionary<string, VirtualDirectory>();
            CollectFolders(result, "");
            return result;
        }

        private void CollectFolders(Dictionary<string, VirtualDirectory> dict, string basePath)
        {
            string currentPath;
            if (string.IsNullOrEmpty(basePath))
                currentPath = Name;
            else if (string.IsNullOrEmpty(Name))
                currentPath = basePath;
            else
                currentPath = basePath + "/" + Name;

            if (!string.IsNullOrEmpty(currentPath))
            {
                string key = currentPath + "/";
                if (!dict.ContainsKey(key))
                    dict.Add(key, this);
            }

            foreach (var folder in Folders)
                folder.CollectFolders(dict, currentPath);
        }

        // -------------------------
        // Modification
        // -------------------------
        public void AddFile(string name, SubMemoryStream data)
        {
            Files[name] = data;
        }

        public void AddFolder(VirtualDirectory folder)
        {
            if (GetFolder(folder.Name) == null)
                Folders.Add(folder);
        }

        // -------------------------
        // Utilities
        // -------------------------
        public void SortAlphabetically()
        {
            Folders = Folders.OrderBy(f => f.Name).ToList();
            foreach (var f in Folders)
                f.SortAlphabetically();

            Files = Files.OrderBy(f => f.Key)
                         .ToDictionary(f => f.Key, f => f.Value);
        }

        public void Print(int level)
        {
            string indent = new string(' ', level * 2);
            Console.WriteLine(indent + "/" + Name);

            foreach (var f in Files.Keys)
                Console.WriteLine(indent + "  - " + f);

            foreach (var folder in Folders)
                folder.Print(level + 1);
        }

        // Albatross.Tools.VirtualDirectory.cs 안에 있어야 함
        public VirtualDirectory GetFolderFromFullPathSafe(string path)
        {
            path = path.Replace('\\', '/').Trim('/');
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            VirtualDirectory current = this;

            foreach (var part in parts)
            {
                var next = current.GetFolder(part);
                if (next == null)
                {
                    next = new VirtualDirectory(part);
                    current.AddFolder(next);
                }
                current = next;
            }

            return current;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            path = path.Replace('\\', '/');

            while (path.Contains("//"))
                path = path.Replace("//", "/");

            return path.Trim('/');
        }


    }
}