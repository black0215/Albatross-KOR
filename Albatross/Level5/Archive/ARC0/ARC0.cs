using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Albatross.Tools;
using Albatross.Level5.Compression;
using Albatross.Level5.Compression.NoCompression;

namespace Albatross.Level5.Archive.ARC0
{
    public class ARC0 : IArchive
    {
        public string Name => "ARC0";

        public VirtualDirectory Directory { get; set; }
        public Stream BaseStream;
        public ARC0Support.Header Header;

        public ARC0()
        {
            Directory = new VirtualDirectory("");
        }

        public ARC0(Stream stream)
        {
            BaseStream = stream;
            Directory = Open();
        }

        // ============================================================
        // OPEN
        // ============================================================

        public VirtualDirectory Open()
        {
            VirtualDirectory root = new VirtualDirectory("");

            BinaryDataReader data = new BinaryDataReader(BaseStream);
            Header = data.ReadStruct<ARC0Support.Header>();

            // Directory Entries
            data.Seek((uint)Header.DirectoryEntriesOffset);
            var dirEntries = DecompressBlockTo<ARC0Support.DirectoryEntry>(
                data.GetSection(Header.DirectoryHashOffset - Header.DirectoryEntriesOffset),
                Header.DirectoryEntriesCount
            );

            // File Entries
            data.Seek((uint)Header.FileEntriesOffset);
            var fileEntries = DecompressBlockTo<ARC0Support.FileEntry>(
                data.GetSection(Header.NameOffset - Header.FileEntriesOffset),
                Header.FileEntriesCount
            );

            // Name Table
            data.Seek((uint)Header.NameOffset);
            var nameTable = Compressor.Decompress(
                data.GetSection(Header.DataOffset - Header.NameOffset)
            );
            BinaryDataReader names = new BinaryDataReader(nameTable);

            foreach (var dir in dirEntries)
            {
                names.Seek((uint)dir.DirectoryNameStartOffset);
                string dirName = NormalizePath(names.ReadString(Encoding.UTF8));

                VirtualDirectory folder = string.IsNullOrEmpty(dirName)
                    ? root
                    : root.GetFolderFromFullPathSafe(dirName);

                var files = fileEntries
                    .Skip(dir.FirstFileIndex)
                    .Take(dir.FileCount);

                foreach (var file in files)
                {
                    names.Seek((uint)(dir.FileNameStartOffset + file.NameOffsetInFolder));
                    string fileName = names.ReadString(Encoding.UTF8);

                    folder.AddFile(
                        fileName,
                        new SubMemoryStream(
                            BaseStream,
                            Header.DataOffset + file.FileOffset,
                            file.FileSize
                        )
                    );
                }
            }

            root.SortAlphabetically();
            return root;
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


        // ============================================================
        // SAVE
        // ============================================================
        public void Save(string fileName, ProgressBar progressBar = null)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinaryDataWriter writer = new BinaryDataWriter(stream);

                var folders = Directory.GetAllFoldersAsDictionnary()
                    .Where(f => !string.IsNullOrEmpty(f.Key))
                    .OrderBy(f => f.Key)
                    .ToList();

                var dirEntries = new List<ARC0Support.DirectoryEntry>();
                var fileEntries = new List<ARC0Support.FileEntry>();
                var fileMap = new Dictionary<ARC0Support.FileEntry, SubMemoryStream>();
                var nameTable = new List<byte>();

                int fileIndex = 0;
                uint dataOffsetCursor = 0;

                foreach (var kv in folders)
                {
                    string dirPath = NormalizePath(kv.Key);
                    var dir = kv.Value;

                    int dirNameOffset = nameTable.Count;
                    nameTable.AddRange(Encoding.UTF8.GetBytes(dirPath + '\0'));

                    var dirEntry = new ARC0Support.DirectoryEntry
                    {
                        Crc32 = Crc32.Compute(Encoding.UTF8.GetBytes(dirPath)),
                        FirstFileIndex = (ushort)fileIndex,
                        FileCount = (short)dir.Files.Count,
                        DirectoryCount = (short)dir.Folders.Count,
                        DirectoryNameStartOffset = dirNameOffset,
                        FileNameStartOffset = nameTable.Count
                    };

                    foreach (var file in dir.Files.OrderBy(f => f.Key))
                    {
                        int fileNameOffset = nameTable.Count - dirEntry.FileNameStartOffset;
                        nameTable.AddRange(Encoding.UTF8.GetBytes(file.Key + '\0'));

                        var entry = new ARC0Support.FileEntry
                        {
                            Crc32 = Crc32.Compute(Encoding.UTF8.GetBytes(file.Key)),
                            NameOffsetInFolder = (uint)fileNameOffset,
                            FileOffset = dataOffsetCursor,
                            FileSize = (uint)(file.Value.ByteContent?.Length ?? file.Value.Size)
                        };

                        fileEntries.Add(entry);
                        fileMap.Add(entry, file.Value);

                        dataOffsetCursor = (uint)((dataOffsetCursor + entry.FileSize + 3) & ~3);
                        fileIndex++;
                    }

                    dirEntries.Add(dirEntry);
                }


                // ===== Write =====
                writer.Seek(0x48);
                long dirOffset = writer.BaseStream.Position;
                writer.Write(CompressBlockTo(dirEntries.ToArray(), new NoCompression()));
                writer.WriteAlignment(4);

                long dirHashOffset = writer.BaseStream.Position;
                writer.Write(CompressBlockTo(dirEntries.Select(d => d.Crc32).ToArray(), new NoCompression()));
                writer.WriteAlignment(4);

                long fileOffset = writer.BaseStream.Position;
                writer.Write(CompressBlockTo(fileEntries.ToArray(), new NoCompression()));
                writer.WriteAlignment(4);

                long nameOffset = writer.BaseStream.Position;
                writer.Write(CompressBlockTo(nameTable.ToArray(), new NoCompression()));
                writer.WriteAlignment(4);

                long dataOffset = writer.BaseStream.Position;
                foreach (var f in fileEntries)
                {
                    var sms = fileMap[f];

                    // ⭐⭐⭐ 핵심: 실제 데이터를 메모리로 로드 ⭐⭐⭐
                    if (sms.ByteContent == null)
                        sms.Read();
                    writer.BaseStream.Position = dataOffset + f.FileOffset;
                    sms.CopyTo(stream);
                }

                // Header
                Header.Magic = 0x30435241;
                Header.DirectoryEntriesOffset = (int)dirOffset;
                Header.DirectoryHashOffset = (int)dirHashOffset;
                Header.FileEntriesOffset = (int)fileOffset;
                Header.NameOffset = (int)nameOffset;
                Header.DataOffset = (int)dataOffset;
                Header.DirectoryEntriesCount = (short)dirEntries.Count;
                Header.DirectoryHashCount = (short)dirEntries.Count;
                Header.FileEntriesCount = fileEntries.Count;
                Header.DirectoryCount = dirEntries.Count;
                Header.FileCount = fileEntries.Count;

                writer.Seek(0);
                writer.WriteStruct(Header);
            }
        }



        // ============================================================
        // HELPERS
        // ============================================================
        private T[] DecompressBlockTo<T>(byte[] data, int count)
        {
            BinaryDataReader reader = new BinaryDataReader(Compressor.Decompress(data));
            return reader.ReadMultipleStruct<T>(count);
        }

        private byte[] CompressBlockTo<T>(T[] data, ICompression compression)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryDataWriter w = new BinaryDataWriter(ms);
                w.WriteMultipleStruct(data);
                return compression.Compress(ms.ToArray());
            }
        }

        public void Close()
        {
            BaseStream?.Dispose();
            BaseStream = null;
            Directory = null;
        }
    }

    // ============================================================
    // SAFE EXTENSION
    // ============================================================
    static class VirtualDirectoryExt
    {
        public static VirtualDirectory GetFolderFromFullPathSafe(
            this VirtualDirectory root, string path)
        {
            var parts = path.Split('/');
            var current = root;

            foreach (var p in parts)
            {
                var next = current.GetFolder(p);
                if (next == null)
                {
                    next = new VirtualDirectory(p);
                    current.AddFolder(next);
                }
                current = next;
            }

            return current;
        }
    }
}
