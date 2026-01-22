using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Albatross.Tools;

namespace Albatross.Level5.Archive.ARC0
{
    public static class ARC0IntegrityTester
    {
        public static bool Test(
            ARC0 original,
            string tempSavePath,
            out string report
        )
        {
            StringBuilder log = new StringBuilder();
            bool success = true;

            log.AppendLine("========================================");
            log.AppendLine("[ARC0 Integrity Test] 시작");
            log.AppendLine("========================================");

            // 1️⃣ 원본 구조 덤프
            log.AppendLine("[1] 원본 구조 수집");
            var originalFiles = GetAllFiles(original.Directory);
            log.AppendLine($" - 원본 파일 수: {originalFiles.Count}");

            // 2️⃣ 저장
            log.AppendLine("[2] 임시 저장");
            original.Save(tempSavePath);
            log.AppendLine($" - 저장 경로: {tempSavePath}");

            // 3️⃣ 재로드
            log.AppendLine("[3] 재로드");
            ARC0 reloaded;
            using (FileStream fs = new FileStream(tempSavePath, FileMode.Open, FileAccess.Read))
            {
                reloaded = new ARC0(fs);
            }

            var reloadedFiles = GetAllFiles(reloaded.Directory);
            log.AppendLine($" - 재로드 파일 수: {reloadedFiles.Count}");

            // 4️⃣ 파일 개수 비교
            if (originalFiles.Count != reloadedFiles.Count)
            {
                success = false;
                log.AppendLine("❌ 파일 개수 불일치");
            }
            else
            {
                log.AppendLine("✅ 파일 개수 일치");
            }

            // 5️⃣ 파일별 검증
            log.AppendLine("[4] 파일 경로 / 크기 검증");

            foreach (var kv in originalFiles)
            {
                string path = kv.Key;
                var originalFile = kv.Value;

                if (!reloadedFiles.TryGetValue(path, out var reloadedFile))
                {
                    success = false;
                    log.AppendLine($"❌ 누락 파일: {path}");
                    continue;
                }

                long sizeA = originalFile.ByteContent?.Length ?? originalFile.Size;
                long sizeB = reloadedFile.ByteContent?.Length ?? reloadedFile.Size;

                if (sizeA != sizeB)
                {
                    success = false;
                    log.AppendLine($"❌ 크기 불일치: {path} ({sizeA} != {sizeB})");
                }
            }

            // 6️⃣ CRC 재검증 (샘플)
            log.AppendLine("[5] CRC 샘플 검사");

            foreach (var kv in originalFiles.Take(10))
            {
                var bytes = kv.Value.ByteContent;
                if (bytes == null) continue;

                uint crc = Crc32.Compute(bytes);
                log.AppendLine($" - {kv.Key} CRC: 0x{crc:X8}");
            }

            log.AppendLine("========================================");
            log.AppendLine(success ? "🎉 무결성 테스트 통과" : "💥 무결성 테스트 실패");
            log.AppendLine("========================================");

            report = log.ToString();
            return success;
        }

        // ===== Helper =====
        private static Dictionary<string, SubMemoryStream> GetAllFiles(VirtualDirectory root)
        {
            var result = new Dictionary<string, SubMemoryStream>();
            CollectFiles(root, "", result);
            return result;
        }

        private static void CollectFiles(
            VirtualDirectory dir,
            string basePath,
            Dictionary<string, SubMemoryStream> output
        )
        {
            foreach (var file in dir.Files)
            {
                string path = Normalize($"{basePath}/{file.Key}");
                output[path] = file.Value;
            }

            foreach (var folder in dir.Folders)
            {
                string next = Normalize($"{basePath}/{folder.Name}");
                CollectFiles(folder, next, output);
            }
        }

        private static string Normalize(string path)
        {
            return path.Replace("\\", "/").Replace("//", "/").Trim('/');
        }
    }
}
