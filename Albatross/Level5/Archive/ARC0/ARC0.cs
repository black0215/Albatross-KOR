using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Albatross.Tools;
using Albatross.Level5.Compression;
using Albatross.Level5.Compression.LZ10;
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
            Console.WriteLine("============== ARC0.Open() 시작 ==============");
            VirtualDirectory root = new VirtualDirectory("");

            BinaryDataReader data = new BinaryDataReader(BaseStream);
            Header = data.ReadStruct<ARC0Support.Header>();

            Console.WriteLine($"[Header] Magic: 0x{Header.Magic:X8}");
            Console.WriteLine($"[Header] DirectoryEntriesOffset: 0x{Header.DirectoryEntriesOffset:X8} ({Header.DirectoryEntriesOffset})");
            Console.WriteLine($"[Header] DirectoryHashOffset: 0x{Header.DirectoryHashOffset:X8} ({Header.DirectoryHashOffset})");
            Console.WriteLine($"[Header] FileEntriesOffset: 0x{Header.FileEntriesOffset:X8} ({Header.FileEntriesOffset})");
            Console.WriteLine($"[Header] NameOffset: 0x{Header.NameOffset:X8} ({Header.NameOffset})");
            Console.WriteLine($"[Header] DataOffset: 0x{Header.DataOffset:X8} ({Header.DataOffset})");
            Console.WriteLine($"[Header] DirectoryEntriesCount: {Header.DirectoryEntriesCount}");
            Console.WriteLine($"[Header] FileEntriesCount: {Header.FileEntriesCount}");

            Console.WriteLine($"[Open Deep Log] DirectoryEntriesOffset: 0x{Header.DirectoryEntriesOffset:X8}");
            Console.WriteLine($"[Open Deep Log] FileEntriesOffset: 0x{Header.FileEntriesOffset:X8}");
            Console.WriteLine($"[Open Deep Log] NameTableOffset: 0x{Header.NameOffset:X8}");

            // Directory Entries
            Console.WriteLine($"\n[Step 1] Directory Entries 읽기 시작...");
            data.Seek((uint)Header.DirectoryEntriesOffset);
            int dirSectionSize = Header.DirectoryHashOffset - Header.DirectoryEntriesOffset;
            Console.WriteLine($"  - Offset: 0x{Header.DirectoryEntriesOffset:X8}");
            Console.WriteLine($"  - Section Size: {dirSectionSize} bytes");
            var dirEntries = DecompressBlockTo<ARC0Support.DirectoryEntry>(
                data.GetSection(dirSectionSize),
                Header.DirectoryEntriesCount
            );
            Console.WriteLine($"  ✓ {dirEntries.Length}개의 디렉토리 엔트리 로드 완료");
            LogDirectoryEntries(dirEntries.ToList()); // Added deep logging

            // File Entries
            Console.WriteLine($"\n[Step 2] File Entries 읽기 시작...");
            data.Seek((uint)Header.FileEntriesOffset);
            int fileSectionSize = Header.NameOffset - Header.FileEntriesOffset;
            Console.WriteLine($"  - Offset: 0x{Header.FileEntriesOffset:X8}");
            Console.WriteLine($"  - Section Size: {fileSectionSize} bytes");
            var fileEntries = DecompressBlockTo<ARC0Support.FileEntry>(
                data.GetSection(fileSectionSize),
                Header.FileEntriesCount
            );
            Console.WriteLine($"  ✓ {fileEntries.Length}개의 파일 엔트리 로드 완료");
            LogFileEntries(fileEntries.ToList()); // Added deep logging

            // Name Table
            Console.WriteLine($"\n[Step 3] Name Table 읽기 시작...");
            data.Seek((uint)Header.NameOffset);
            int nameTableSize = Header.DataOffset - Header.NameOffset;
            Console.WriteLine($"  - Offset: 0x{Header.NameOffset:X8}");
            Console.WriteLine($"  - Section Size: {nameTableSize} bytes");
            var nameTable = Compressor.Decompress(
                data.GetSection(nameTableSize)
            );
            Console.WriteLine($"  ✓ Name Table 압축 해제 완료 (해제 후: {nameTable.Length} bytes)");
            BinaryDataReader names = new BinaryDataReader(nameTable);

            Console.WriteLine($"\n[Step 4] 디렉토리 및 파일 처리 시작...");
            int dirIndex = 0;
            int totalFilesProcessed = 0;
            int totalDirCount = dirEntries.Length;

            foreach (var dir in dirEntries)
            {
                names.Seek((uint)dir.DirectoryNameStartOffset);
                string dirName = NormalizePath(names.ReadString(Encoding.UTF8));

                // 상위 5개만 로그 출력
                if (dirIndex < 5)
                {
                    Console.WriteLine($"  [Dir {dirIndex}] \"{dirName}\" (CRC32: 0x{dir.Crc32:X8}, FirstFile: {dir.FirstFileIndex}, Count: {dir.FileCount})");
                    Console.WriteLine($"    [DEBUG] DirNameOffset: 0x{dir.DirectoryNameStartOffset:X}, FileNameOffset: 0x{dir.FileNameStartOffset:X}");
                }
                else if (dirIndex == 5)
                {
                    Console.WriteLine($"  ... ({totalDirCount - 5}개 디렉토리 생략) ...");
                }

                VirtualDirectory folder = string.IsNullOrEmpty(dirName)
                    ? root
                    : root.GetFolderFromFullPathSafe(dirName);
                var files = fileEntries
                    .Skip(dir.FirstFileIndex)
                    .Take(dir.FileCount);

                int fileInDirIndex = 0;

                foreach (var file in files)
                {
                    names.Seek((uint)(dir.FileNameStartOffset + file.NameOffsetInFolder));
                    string fileName = names.ReadString(Encoding.UTF8);

                    // 상위 5개 디렉토리의 파일 중 처음 2개만 로그 출력
                    if (dirIndex < 5 && fileInDirIndex < 2)
                    {
                        Console.WriteLine($"    [{fileInDirIndex}] {fileName} (CRC32: 0x{file.Crc32:X8})");
                    }
                    else if (dirIndex < 5 && fileInDirIndex == 2)
                    {
                        Console.WriteLine($"    ... ({dir.FileCount - 2}개 파일 생략)");
                    }

                    folder.AddFile(
                        fileName,
                        new SubMemoryStream(
                            BaseStream,
                            Header.DataOffset + file.FileOffset,
                            file.FileSize
                        )
                    );

                    fileInDirIndex++;
                    totalFilesProcessed++;
                }

                dirIndex++;
            }


            Console.WriteLine($"\n[Step 5] 완료...");
            // 주의: 정렬하지 않음! 원본 ARC0는 CRC32 해시값 순서로 정렬되어 있음
            Console.WriteLine($"✓ 총 {dirIndex}개 디렉토리, {totalFilesProcessed}개 파일 처리 완료 (CRC32 순서 유지)");



            Console.WriteLine("=== ARC0.Open() 완료 ===\n");

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
        public void Save(string fileName, Action<int, int, string> progressCallback = null)
        {
            Console.WriteLine("\n============== ARC0.Save() 시작 ==============");
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinaryDataWriter writer = new BinaryDataWriter(stream);

                Console.WriteLine("\n============== ARC0.Save() 시작 ==============");
                Console.WriteLine($"[Save] 저장 파일: {fileName}");

                // VirtualDirectory 기반으로 폴더 수집
                var folders = Directory.GetAllFoldersAsDictionnary()
                    .OrderBy(f => Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(NormalizePath(f.Key) + "/")))
                    .ToList();

                var dirEntries = new List<ARC0Support.DirectoryEntry>();
                var fileEntries = new List<ARC0Support.FileEntry>();
                var fileMap = new Dictionary<ARC0Support.FileEntry, SubMemoryStream>();
                var fileNameMap = new Dictionary<ARC0Support.FileEntry, string>();
                var nameTable = new List<byte>();

                // [FIX] 중간 경로의 빈 폴더 제거 로직 (항상 실행)
                // 원본 구조 유지를 위해 필요한 경우 조정 가능
                // [FIX] 보존: 원본 구조 유지를 위해 빈 폴더 제거 로직 비활성화
                folders.RemoveAll(k => k.Value.Files.Count == 0 && k.Value.Folders.Count > 0);

                Console.WriteLine($"[Save] 디렉토리 개수: {folders.Count}");
                Console.WriteLine($"[Save] 디렉토리 개수: {folders.Count}");
                Console.WriteLine($"[Save] NameTable 인코딩: Shift-JIS");

                int fileIndex = 0;
                uint dataOffsetCursor = 0;

                // NameTable 0번지는 항상 빈 문자열(\0)로 초기화
                nameTable.Add(0);

                // 1. Root Directory 파일 선행 처리
                var rootDir = this.Directory;

                // folders에서 Root Directory 제거
                folders.RemoveAll(k => string.IsNullOrEmpty(k.Key) || k.Value == rootDir);

                int rootFileCount = 0;
                int rootFileNameStartOffset = nameTable.Count;

                // Root 파일 직접 처리
                if (rootDir != null && rootDir.Files.Count > 0)
                {
                    // [FIX] Match Reference: Root File Sorting using Shift-JIS
                    var sortedRootFiles = rootDir.Files.OrderBy(f => Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(f.Key)));
                    rootFileCount = sortedRootFiles.Count();

                    foreach (var file in sortedRootFiles)
                    {
                        int fileNameOffset = nameTable.Count - rootFileNameStartOffset;
                        nameTable.AddRange(Encoding.GetEncoding("Shift-JIS").GetBytes(file.Key + '\0'));

                        var entry = new ARC0Support.FileEntry
                        {
                            Crc32 = Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(file.Key)),
                            NameOffsetInFolder = (uint)fileNameOffset,
                            FileOffset = dataOffsetCursor,
                            FileSize = (uint)(file.Value.ByteContent?.Length ?? file.Value.Size)
                        };

                        fileEntries.Add(entry);
                        fileMap.Add(entry, file.Value);
                        fileNameMap.Add(entry, file.Key);
                        dataOffsetCursor = (uint)((dataOffsetCursor + entry.FileSize + 3) & ~3);
                        fileIndex++;
                    }
                }

                // 2. 일반 디렉토리 처리
                foreach (var kv in folders)
                {
                    string dirPath = NormalizePath(kv.Key);

                    // [FIX - Trailing Slash]
                    // 원본 ARC0는 디렉토리 이름 끝에 '/'를 포함하여 CRC를 계산하고 NameTable에 저장함
                    string saveDirPath = dirPath;
                    if (!string.IsNullOrEmpty(saveDirPath) && !saveDirPath.EndsWith("/"))
                    {
                        saveDirPath += "/";
                    }

                    var dir = kv.Value;

                    // Directory Name 처리
                    // Directory Name 처리 (Shift-JIS)
                    int dirNameOffset = nameTable.Count;
                    nameTable.AddRange(Encoding.GetEncoding("Shift-JIS").GetBytes(saveDirPath + '\0'));

                    // File Name Start Offset 결정
                    int currentFileNameStartOffset = nameTable.Count;

                    // Directory Entry 생성
                    var dirEntry = new ARC0Support.DirectoryEntry
                    {
                        // [FIX] Match Kuriimu: CRC uses Shift-JIS even for directories
                        Crc32 = Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(saveDirPath)),
                        FirstFileIndex = (ushort)fileIndex,
                        FileCount = (short)dir.Files.Count,
                        DirectoryCount = (short)dir.Folders.Count,
                        DirectoryNameStartOffset = dirNameOffset,
                        FileNameStartOffset = currentFileNameStartOffset
                    };

                    // 파일 처리
                    var sortedFiles = dir.Files.OrderBy(f => Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(f.Key)));

                    foreach (var file in sortedFiles)
                    {
                        int fileNameOffset = nameTable.Count - currentFileNameStartOffset;
                        nameTable.AddRange(Encoding.GetEncoding("Shift-JIS").GetBytes(file.Key + '\0'));

                        var entry = new ARC0Support.FileEntry
                        {
                            // [FIX] Match Reference: File CRC uses Shift-JIS
                            Crc32 = Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(file.Key)),
                            NameOffsetInFolder = (uint)fileNameOffset,
                            FileOffset = dataOffsetCursor,
                            FileSize = (uint)(file.Value.ByteContent?.Length ?? file.Value.Size)
                        };

                        fileEntries.Add(entry);
                        fileMap.Add(entry, file.Value);
                        fileNameMap.Add(entry, saveDirPath + file.Key);
                        dataOffsetCursor = (uint)((dataOffsetCursor + entry.FileSize + 3) & ~3);
                        fileIndex++;
                    }

                    dirEntries.Add(dirEntry);
                }

                // 3. Root Directory Entry 추가 (맨 마지막)
                if (rootFileCount > 0 || rootDir != null)
                {
                    var rootDirEntry = new ARC0Support.DirectoryEntry
                    {
                        Crc32 = 0xFFFFFFFF,
                        FirstFileIndex = 0,
                        FileCount = (short)rootFileCount,
                        DirectoryCount = 0,
                        DirectoryNameStartOffset = 0,
                        FileNameStartOffset = rootFileNameStartOffset
                    };

                    dirEntries.Add(rootDirEntry);
                }
                else
                {
                    // Root 파일 없어도 엔트리는 추가
                    var rootDirEntry = new ARC0Support.DirectoryEntry
                    {
                        Crc32 = 0xFFFFFFFF,
                        FirstFileIndex = 0,
                        FileCount = 0,
                        DirectoryCount = 0,
                        DirectoryNameStartOffset = 0,
                        FileNameStartOffset = 0
                    };
                    dirEntries.Add(rootDirEntry);
                }

                Console.WriteLine($"[File-Centric] ========== 완료 ==========\n");


                Console.WriteLine($"\n[Save] 메타데이터 구축 완료");
                Console.WriteLine($"  - 디렉토리: {dirEntries.Count}개");
                Console.WriteLine($"  - 파일: {fileEntries.Count}개");
                Console.WriteLine($"  - NameTable 크기: {nameTable.Count} bytes");

                // [FIX] Match Kuriimu Logic: Sort Directory Entries by CRC and Re-assign FirstDirectoryIndex
                // This logic is ported directly from Kuriimu2's Arc0.cs
                int directoryIndex = 0;
                dirEntries = dirEntries.OrderBy(x => x.Crc32).Select(x =>
                {
                    x.FirstDirectoryIndex = (ushort)directoryIndex;
                    directoryIndex += x.DirectoryCount;
                    return x;
                }).ToList();

                // ===== Write =====
                Console.WriteLine($"\n[Save] 파일 쓰기 및 구조 로깅...");

                // [Fix] 헤더 공간(0x48) 확보를 위해 Seek 필수
                writer.Seek(0x48);

                // 1. Directory Entries
                long dirOffset = writer.BaseStream.Position;
                Console.WriteLine($"[Save Deep Log] DirectoryEntriesOffset: 0x{dirOffset:X8}");
                writer.Write(CompressBlockTo(dirEntries.ToArray(), new LZ10()));
                writer.WriteAlignment(4);

                // 로그: 처음 5개 및 마지막 5개 디렉토리
                LogDirectoryEntries(dirEntries);

                // 2. Directory Hash
                long dirHashOffset = writer.BaseStream.Position;
                Console.WriteLine($"[Save Deep Log] DirectoryHashOffset: 0x{dirHashOffset:X8}");
                writer.Write(CompressBlockTo(dirEntries.Select(d => d.Crc32).ToArray(), new LZ10()));
                writer.WriteAlignment(4);

                // 3. File Entries
                long fileOffset = writer.BaseStream.Position;
                Console.WriteLine($"[Save Deep Log] FileEntriesOffset: 0x{fileOffset:X8}");
                writer.Write(CompressBlockTo(fileEntries.ToArray(), new LZ10()));
                writer.WriteAlignment(4);

                // 로그: 처음 5개 및 마지막 5개 파일
                LogFileEntries(fileEntries);

                // 4. Name Table
                long nameOffset = writer.BaseStream.Position;
                Console.WriteLine($"[Save Deep Log] NameTableOffset: 0x{nameOffset:X8}");
                writer.Write(CompressBlockTo(nameTable.ToArray(), new LZ10()));
                writer.WriteAlignment(4);
                Console.WriteLine($"[Save Deep Log] NameTable Size: {nameTable.Count} bytes");

                // [FIX] Revert Data Section Alignment to 4-byte (Match Kuriimu)
                writer.WriteAlignment(4);

                // 5. Data Section
                // 5. Data Section
                // long dataOffset = writer.BaseStream.Position; (Moved below)
                Console.WriteLine($"\n========================================");
                Console.WriteLine($"[Modified Files] 수정된 파일 추적");
                Console.WriteLine($"========================================\n");
                int modifiedCount = 0;
                foreach (var f in fileEntries)
                {
                    var sms = fileMap[f];
                    if (sms.ByteContent != null)
                    {
                        modifiedCount++;
                        // 파일 이름 찾기
                        string fName = fileNameMap.ContainsKey(f) ? fileNameMap[f] : "Unknown";
                        Console.WriteLine($"  [Modified {modifiedCount}] Name: {fName}, CRC: 0x{f.Crc32:X8}, Size: {f.FileSize} bytes");
                    }
                }
                Console.WriteLine($"총 {modifiedCount}개 파일이 수정되었습니다.");
                Console.WriteLine($"========================================\n");

                // [FIX] Prepare parallel lists to avoid Dictionary Key issues (KeyNotFoundException)
                // When we modify FileEntry struct (which is a key in fileMap/fileNameMap), the hash code changes
                // and we can no longer look up items. So we cache them by index first.
                List<SubMemoryStream> orderedStreams = new List<SubMemoryStream>(fileEntries.Count);
                List<string> orderedNames = new List<string>(fileEntries.Count);

                for (int i = 0; i < fileEntries.Count; i++)
                {
                    var f = fileEntries[i]; // Original unchanged struct
                    orderedStreams.Add(fileMap[f]);
                    orderedNames.Add(fileNameMap.ContainsKey(f) ? fileNameMap[f] : "Unknown");
                }

                // [EMERGENCY TEST] LZ11 압축 완전 비활성화 - 테스트용
                /*
                // [FIX] Apply Raw LZ11 Compression for specific files
                for (int i = 0; i < fileEntries.Count; i++)
                {
                    var f = fileEntries[i];
                    var sms = orderedStreams[i]; // Access by index
                    if (sms.ByteContent != null)
                    {
                        string fName = orderedNames[i]; // Access by index
                        // [FIX] chara_param은 LZ11로 감싸면 CfgBin 구조가 깨짐 (KeyTable 손상)
                        // List of files known to require LZ11 compression by the game engine
                        if (fName.Contains("item_config") ||
                            fName.Contains("battle_command") ||
                            fName.Contains("chara_base") ||
                            fName.Contains("chara_ability"))
                        {
                            Console.WriteLine($"[LZ11 Protection] Compressing {fName} with Raw LZ11...");
                            byte[] compressed = Albatross.Level5.Compression.LZ11.Lz11Compression.CompressRawLZ11(sms.ByteContent);

                            // Update content and size in SubMemoryStream
                            sms.ByteContent = compressed;
                            sms.Size = compressed.Length;

                            // Update FileEntry Size
                            f.FileSize = (uint)compressed.Length;

                            // [FIX] DO NOT update CRC32! It is the Filename Hash, not Content Hash.
                            // f.Crc32 = Crc32.Compute(compressed); 
                            Console.WriteLine($"  -> Compressed Size: {f.FileSize} bytes, CRC retained: 0x{f.Crc32:X8}");

                            // Re-assign struct to list to persist changes
                            fileEntries[i] = f;
                        }
                    }
                }
                */

                // [FIX] Recalculate File Offsets!
                // Since file sizes changed (compression), we must update FileOffset for ALL files
                // to ensure they are packed contiguously in the new Data Section.
                uint currentFileOffset = 0;
                for (int i = 0; i < fileEntries.Count; i++)
                {
                    var f = fileEntries[i];

                    // Align current offset to 4 bytes
                    currentFileOffset = (uint)((currentFileOffset + 3) & ~3);
                    f.FileOffset = currentFileOffset;

                    // Add file size
                    currentFileOffset += f.FileSize;

                    // Re-assign struct to list
                    fileEntries[i] = f;
                }
                Console.WriteLine($"[Save] Total Data Size: {currentFileOffset} bytes");

                // 5. Data
                long dataOffset = writer.BaseStream.Position;
                Console.WriteLine($"[Save Deep Log] DataOffset: 0x{dataOffset:X8}");

                // 데이터 쓰기 (수정되지 않은 파일은 Read() 호출 안 함)
                int dataLogCount = 0;
                for (int i = 0; i < fileEntries.Count; i++)
                {
                    var f = fileEntries[i]; // This is the MODIFIED struct
                    var sms = orderedStreams[i]; // This is the stable stream reference

                    // [FIX] ByteContent가 없는 파일은 원본 그대로 복사 (Read() 호출 안 함)
                    // CopyTo()가 내부적으로 BaseStream에서 직접 읽음

                    writer.BaseStream.Position = dataOffset + f.FileOffset;
                    sms.CopyTo(stream);

                    // 앞쪽 3개만 데이터 오프셋 로그
                    if (dataLogCount < 3)
                    {
                        Console.WriteLine($"[Save Deep Log] Writing Data - File[{dataLogCount}] Offset: 0x{f.FileOffset:X8} Size: {f.FileSize}");
                        dataLogCount++;
                    }

                    // [Progress] Update Progress (User Request)
                    // "변경된 파일 (n/n) name" - name should be the NEXT file to be saved, or current.
                    // Usually "Saving..." implies current.
                    if (progressCallback != null)
                    {
                        string currentName = orderedNames[i];
                        // Report (i+1) out of Count
                        progressCallback(i + 1, fileEntries.Count, currentName);
                    }
                }

                // Header
                Header.Magic = 0x30435241;
                Header.DirectoryEntriesOffset = (int)dirOffset;
                Header.DirectoryHashOffset = (int)dirHashOffset;
                Header.FileEntriesOffset = (int)fileOffset;
                Header.NameOffset = (int)nameOffset;
                Header.DataOffset = (int)dataOffset;

                // ... 기존 Header 설정 ...
                Header.DirectoryEntriesCount = (short)dirEntries.Count;
                Header.DirectoryHashCount = (short)dirEntries.Count;
                Header.FileEntriesCount = fileEntries.Count;
                Header.DirectoryCount = dirEntries.Count;
                Header.FileCount = fileEntries.Count;

                // [FIX] Calculate TableChunkSize based on UNCOMPRESSED size of table contents (Match Kuriimu)
                // This tells the game how much memory to allocate for metadata tables.
                // 20 = DirEntrySize, 4 = DirHashSize, 16 = FileEntrySize
                // 32 = Header Reserve or Padding? Kuriimu adds 0x20 + 3 and aligns to 4.
                Header.TableChunkSize = (int)(dirEntries.Count * 20 +
                                              dirEntries.Count * 4 +
                                              fileEntries.Count * 16 +
                                              nameTable.Count + 0x20 + 3) & ~3;

                writer.Seek(0);
                writer.WriteStruct(Header);

                Console.WriteLine($"\n[Save] 헤더 정보:");
                Console.WriteLine($"  - DirectoryEntriesOffset: 0x{Header.DirectoryEntriesOffset:X8}");
                Console.WriteLine($"  - DirectoryHashOffset: 0x{Header.DirectoryHashOffset:X8}");
                Console.WriteLine($"  - FileEntriesOffset: 0x{Header.FileEntriesOffset:X8}");
                Console.WriteLine($"  - NameOffset: 0x{Header.NameOffset:X8}");
                Console.WriteLine($"  - DataOffset: 0x{Header.DataOffset:X8}");
                Console.WriteLine($"  - 최종 파일 크기: {stream.Length} bytes");
                Console.WriteLine("=== ARC0.Save() 완료 ===\n");
            }
        }



        private void LogDirectoryEntries(List<ARC0Support.DirectoryEntry> entries)
        {
            Console.WriteLine("[Save Deep Log] --- Directory Entries Sample ---");
            for (int i = 0; i < entries.Count; i++)
            {
                if (i < 11 || i >= entries.Count - 5)
                {
                    var d = entries[i];
                    Console.WriteLine($"  Dir[{i}]: CRC:{d.Crc32:X8} NameOff:{d.DirectoryNameStartOffset:X} FileOff:{d.FileNameStartOffset:X} Files:{d.FileCount}");
                }
                else if (i == 11) Console.WriteLine("  ...");
            }
        }

        private void LogFileEntries(List<ARC0Support.FileEntry> entries)
        {
            Console.WriteLine("[Save Deep Log] --- File Entries Sample ---");
            for (int i = 0; i < entries.Count; i++)
            {
                if (i < 11 || i >= entries.Count - 5)
                {
                    var f = entries[i];
                    Console.WriteLine($"  File[{i}]: CRC:{f.Crc32:X8} NameInFolder:{f.NameOffsetInFolder:X} Offset:{f.FileOffset:X8} Size:{f.FileSize}");
                }
                else if (i == 11) Console.WriteLine("  ...");
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
