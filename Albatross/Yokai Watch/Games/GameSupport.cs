using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Albatross.Tools;
using Albatross.Level5.Text;
using Albatross.Level5.Binary;

namespace Albatross.Yokai_Watch.Games
{
    public static class GameSupport
    {
        public static Dictionary<int, char> PrefixLetter = new Dictionary<int, char>()
        {
            {0, 'c'},
            {1, '?'},
            {2, '?'},
            {3, '?'},
            {4, '?'},
            {5, 'x'},
            {6, 'y'},
            {7, 'z'},
            {8, '?'},
            {12, '?'},
            {17, '?'},
        };

        static string FormatVariant(int x)
        {
            if (x > -1 && x < 10)
            {
                return $"0{x}0";
            }
            else if (x > 9 && x < 100)
            {
                return $"{x}0";
            }
            else if (x > 99 && x < 1000)
            {
                char[] charArray = x.ToString().ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }
            else
            {
                throw new FormatException("Format non valide");
            }
        }

        static int ParseVariant(string str)
        {
            if (str.Length == 4 && str[0] == '0' && str[3] == '0')
            {
                return int.Parse(str[1].ToString());
            }
            else if (str.Length == 3 && str[2] == '0')
            {
                return int.Parse(str.Substring(0, 2));
            }
            else if (str.Length == 3)
            {
                char[] charArray = str.ToCharArray();
                Array.Reverse(charArray);
                return int.Parse(new string(charArray));
            }
            else
            {
                throw new FormatException("Format non valide");
            }
        }

        public static string GetFileModelText(int prefix, int number, int variant)
        {
            return PrefixLetter[prefix] + number.ToString("D3") + FormatVariant(variant);
        }

        public static (int, int, int) GetFileModelValue(string text)
        {
            int prefixIndex = PrefixLetter.FirstOrDefault(x => x.Value == text[0]).Key;
            int number = int.Parse(text.Substring(1, 3));
            int variant = ParseVariant(text.Substring(4, 3));

            number = BitConverter.ToInt32(BitConverter.GetBytes(number), 0);
            variant = BitConverter.ToInt32(BitConverter.GetBytes(variant), 0);

            return (prefixIndex, number, variant);
        }

        public static void SaveTextFile(GameFile fileName, T2bþ fileData)
        {
            // ✅ [로그 추가] 함수 시작
            Console.WriteLine("\n========================================");
            Console.WriteLine($"[SaveTextFile] 저장 시작: {DateTime.Now:HH:mm:ss.fff}");
            Console.WriteLine("========================================");

            // ✅ [핵심 안전 장치] 입력값 유효성 검사
            if (fileName == null || fileData == null)
            {
                Console.WriteLine("[SaveTextFile] ❌ fileName 또는 fileData가 null - 저장 건너뜀");
                return;
            }

            // ✅ [로그 추가] 입력 정보
            Console.WriteLine($"[SaveTextFile] 📄 파일 경로: {fileName.Path}");
            Console.WriteLine($"[SaveTextFile] 📊 Texts 개수: {fileData.Texts.Count}");
            Console.WriteLine($"[SaveTextFile] 📊 Nouns 개수: {fileData.Nouns.Count}");
            Console.WriteLine($"[SaveTextFile] 🔤 인코딩: {fileData.Encoding.EncodingName} ({fileData.Encoding.CodePage})");

            // ✅ [추가] 스트림이 닫혔거나 접근 불가능한 경우 조용히 반환
            try
            {
                if (fileName.File == null || fileName.File.Directory == null)
                {
                    Console.WriteLine($"[SaveTextFile] ⚠️ 스트림이 이미 닫힘 - 저장 건너뜀: {fileName.Path}");
                    return;
                }

                // Directory 접근 가능 여부 테스트
                var testAccess = fileName.File.Directory.Files;
                if (testAccess == null)
                {
                    Console.WriteLine($"[SaveTextFile] ⚠️ Directory.Files 접근 불가 - 저장 건너뜀: {fileName.Path}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveTextFile] ❌ 스트림 접근 실패 - 저장 건너뜀: {ex.GetType().Name} - {ex.Message}");
                return;
            }

            // 1. 경로 및 파일명 계산
            string rawPath = fileName.Path ?? "";
            string normalizedPath = rawPath.Replace("\\", "/").Trim('/');

            Console.WriteLine($"[SaveTextFile] 🔍 정규화된 경로: {normalizedPath}");

            // 폴더 경로와 파일명 분리
            string folderPath = "";
            string targetFileName = normalizedPath;

            int lastSlashIndex = normalizedPath.LastIndexOf('/');
            if (lastSlashIndex >= 0)
            {
                folderPath = normalizedPath.Substring(0, lastSlashIndex);
                targetFileName = normalizedPath.Substring(lastSlashIndex + 1);
            }

            Console.WriteLine($"[SaveTextFile] 📁 폴더 경로: {(string.IsNullOrEmpty(folderPath) ? "(루트)" : folderPath)}");
            Console.WriteLine($"[SaveTextFile] 📄 파일명: {targetFileName}");

            // 2. 폴더 가져오기
            VirtualDirectory directory = null;

            try
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    directory = fileName.File.Directory;
                    Console.WriteLine($"[SaveTextFile] 🗂️ 루트 디렉토리 사용");
                }
                else
                {
                    directory = fileName.File.Directory.GetFolderFromFullPath(folderPath);
                    Console.WriteLine($"[SaveTextFile] 🗂️ 폴더 접근 성공: {folderPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveTextFile] ❌ 폴더 접근 실패: {ex.GetType().Name} - {ex.Message}");
                return;
            }

            // [방어 코드] 폴더가 null이면 저장 중단
            if (directory == null)
            {
                Console.WriteLine($"[SaveTextFile] ❌ 경로를 찾을 수 없음 - 저장 건너뜀: {folderPath}");
                return;
            }

            // 3. 파일 존재 여부 확인 후 저장
            try
            {
                if (directory.Files.ContainsKey(targetFileName))
                {
                    Console.WriteLine($"[SaveTextFile] 💾 파일 찾음: {targetFileName}");
                    Console.WriteLine($"[SaveTextFile] 🔄 T2bþ.Save() 호출 중...");
                    Console.WriteLine($"[SaveTextFile] 🔤 저장 인코딩: {fileData.Encoding.EncodingName}");

                    // ✅ [로그 추가] 저장 전 데이터 정보
                    var sampleTexts = fileData.Texts.Take(3).Select(t => $"0x{t.Key:X8}");
                    var sampleNouns = fileData.Nouns.Take(3).Select(n => $"0x{n.Key:X8}");
                    Console.WriteLine($"[SaveTextFile] 📝 Texts 샘플 (처음 3개): {string.Join(", ", sampleTexts)}");
                    Console.WriteLine($"[SaveTextFile] 📝 Nouns 샘플 (처음 3개): {string.Join(", ", sampleNouns)}");

                    byte[] savedData = fileData.Save(false);

                    Console.WriteLine($"[SaveTextFile] ✅ Save() 완료: {savedData.Length:N0} bytes");

                    directory.Files[targetFileName].ByteContent = savedData;

                    Console.WriteLine($"[SaveTextFile] ✅ ByteContent 설정 완료");
                    Console.WriteLine($"[SaveTextFile] 🎉 저장 성공: {targetFileName} ({savedData.Length:N0} bytes)");
                    Console.WriteLine("========================================\n");
                }
                else
                {
                    Console.WriteLine($"[SaveTextFile] ⚠️ 파일이 존재하지 않음 - 저장 건너뜀: {targetFileName}");
                    Console.WriteLine($"[SaveTextFile] 📋 사용 가능한 파일들:");
                    foreach (var key in directory.Files.Keys.Take(10))
                    {
                        Console.WriteLine($"   - {key}");
                    }
                    if (directory.Files.Count > 10)
                    {
                        Console.WriteLine($"   ... 외 {directory.Files.Count - 10}개");
                    }
                    Console.WriteLine("========================================\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveTextFile] ❌ 파일 저장 실패: {ex.GetType().Name}");
                Console.WriteLine($"[SaveTextFile] 오류 메시지: {ex.Message}");
                Console.WriteLine($"[SaveTextFile] 스택 트레이스:\n{ex.StackTrace}");
                Console.WriteLine("========================================\n");
            }
        }

        public static T GetLogic<T>() where T : class, new()
        {
            return new T();
        }
    }
}