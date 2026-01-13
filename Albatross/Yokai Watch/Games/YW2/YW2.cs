using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Albatross.Tools;
using Albatross.Level5.Text;
using Albatross.Level5.Text.Logic;
using Albatross.Level5.Archive.ARC0;
using Albatross.Level5.Archive.XPCK;
using Albatross.Level5.Binary;
using Albatross.Yokai_Watch.Games.YW2.Logic;
using Albatross.Yokai_Watch.Logic;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Albatross.Level5.Binary.Logic;
using Albatross.Level5.Archive;

namespace Albatross.Yokai_Watch.Games.YW2
{
    public class YW2 : IGame
    {
        public string Name => "Yo-Kai Watch 2";
        public Dictionary<int, string> Tribes => Common.Tribes.YW2;
        public Dictionary<int, string> FoodsType => Common.FoodsType.YW2;
        public Dictionary<int, string> ScoutablesType => Common.ScoutablesType.YW2;
        public ARC0 Game { get; set; }
        public ARC0 Language { get; set; }
        public string LanguageCode { get; set; }
        public string RomfsPath { get; private set; }
        public Dictionary<string, GameFile> Files { get; set; }

        public YW2(string romfsPath, string language)
        {
            RomfsPath = romfsPath;
            LanguageCode = language;

            // 1. 메인 게임 파일 로드
            Game = new ARC0(new FileStream(RomfsPath + @"\yw2_a.fa", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            // 2. 언어 파일 로드
            string langFilePath = RomfsPath + @"\yw2_lg_" + LanguageCode + ".fa";
            if (File.Exists(langFilePath))
            {
                Language = new ARC0(new FileStream(langFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            }

            // ✅ 파일 목록 초기화 (생성자에서 호출)
            InitializeFiles();
        }

        private void DumpDirectoryTree(VirtualDirectory dir, string indent = "")
        {
            if (dir == null) return;

            string name = string.IsNullOrEmpty(dir.Name) ? "/" : dir.Name;
            Console.WriteLine($"{indent}📁 {name}");

            // 파일들
            foreach (var file in dir.Files.Keys)
            {
                Console.WriteLine($"{indent}  📄 {file}");
            }

            // 하위 폴더들
            foreach (var sub in dir.Folders)
            {
                DumpDirectoryTree(sub, indent + "  ");
            }
        }


        // ✅ [핵심 수정] 파일 목록을 설정/갱신하는 함수 분리
        private void InitializeFiles()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("[InitializeFiles] 시작");
            Console.WriteLine("========================================");

            Console.WriteLine("\n========================================");
            Console.WriteLine("[DEBUG] Archive 전체 구조 덤프");
            Console.WriteLine("========================================");

            Console.WriteLine("\n--- Language Archive ---");
            if (Language != null)
                DumpDirectoryTree(Language.Directory);
            else
                Console.WriteLine("(Language 없음)");

            Console.WriteLine("\n--- Game Archive ---");
            DumpDirectoryTree(Game.Directory);

            Console.WriteLine("========================================\n");


            ARC0 targetArchive = Language ?? Game;
            Files = new Dictionary<string, GameFile>();

            if (targetArchive == null || targetArchive.Directory == null)
            {
                Console.WriteLine("[ERROR] Archive가 null!");
                return;
            }

            string langSuffix = "_" + (string.IsNullOrEmpty(LanguageCode) ? "ko" : LanguageCode) + ".cfg.bin";

            Console.WriteLine($"[DEBUG] 사용 Archive: {(targetArchive == Language ? "Language" : "Game")}");
            Console.WriteLine($"[DEBUG] langSuffix: \"{langSuffix}\"");

            // ===============================
            // 텍스트 파일 검색 (재귀 기반)
            // ===============================
            var textFiles = new[]
            {
                "chara_text",
                "item_text",
                "battle_text",
                "skill_text",
                "chara_ability_text",
                "system_text",
                "addmembermenu_text"
            };

            Console.WriteLine("\n[InitializeFiles] 텍스트 파일 검색");

            foreach (var key in textFiles)
            {
                string fileName = key + langSuffix;

                string foundPath = SearchFileInDirectory(Language?.Directory, fileName)
                                ?? SearchFileInDirectory(Game.Directory, fileName);

                if (foundPath != null)
                {
                    var archive = SearchFileInDirectory(Language?.Directory, fileName) != null
                        ? Language
                        : Game;

                    Files[key] = new GameFile(archive, foundPath);
                }

            }

            // ===============================
            // 리소스 폴더 검색
            // ===============================
            Console.WriteLine("\n[InitializeFiles] 리소스 폴더 검색");

            var resourceFolders = new Dictionary<string, string>
    {
        { "face_icon", "face_icon" },
        { "item_icon", "item_icon" },
        { "model", "character" },
        { "map_encounter", "map" }
    };

            foreach (var kvp in resourceFolders)
            {
                string foundPath = FindFolderPath(Game.Directory, kvp.Value);
                if (!string.IsNullOrEmpty(foundPath))
                {
                    Files[kvp.Key] = new GameFile(Game, foundPath);
                    Console.WriteLine($"  ✅ {kvp.Key}: {foundPath}");
                }
                else
                {
                    Console.WriteLine($"  ❌ {kvp.Key}: 찾지 못함");
                }
            }

            Console.WriteLine($"\n[InitializeFiles] 완료 - {Files.Count}개 파일 등록\n");
        }


        private VirtualDirectory FindFolderContainsPath(VirtualDirectory dir, string keyword)
        {
            if (dir == null) return null;

            if (!string.IsNullOrEmpty(dir.Name) && dir.Name.Contains(keyword))
                return dir;

            foreach (var sub in dir.Folders)
            {
                var found = FindFolderContainsPath(sub, keyword);
                if (found != null) return found;
            }

            return null;
        }


        // ✅ [신규] 재귀적으로 파일 검색
        private string SearchFileInDirectory(VirtualDirectory dir, string targetFileName, string currentPath = "")
        {
            if (dir == null) return null;

            // 1. 현재 폴더의 Files에서 검색
            if (dir.Files != null)
            {
                // 정확한 매칭
                if (dir.Files.ContainsKey(targetFileName))
                {
                    string path = string.IsNullOrEmpty(currentPath) ? targetFileName : currentPath + "/" + targetFileName;
                    Console.WriteLine($"    ✅ 발견 (정확 매칭): {path}");
                    return path;
                }

                // 끝부분 매칭
                foreach (var key in dir.Files.Keys)
                {
                    if (key.EndsWith(targetFileName))
                    {
                        Console.WriteLine($"    ✅ 발견 (끝부분 매칭): {key}");
                        return key;
                    }
                }

                // 부분 매칭
                foreach (var key in dir.Files.Keys)
                {
                    if (key.Contains(targetFileName))
                    {
                        Console.WriteLine($"    ✅ 발견 (부분 매칭): {key}");
                        return key;
                    }
                }
            }

            // 2. 하위 폴더 재귀 검색
            if (dir.Folders != null)
            {
                foreach (var subDir in dir.Folders)
                {
                    if (subDir == null) continue;

                    string subPath = string.IsNullOrEmpty(currentPath)
                        ? (subDir.Name ?? "")
                        : currentPath + "/" + (subDir.Name ?? "");

                    string found = SearchFileInDirectory(subDir, targetFileName, subPath);
                    if (!string.IsNullOrEmpty(found))
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        // FindFolderPath는 기존 코드 유지
        private string FindFolderPath(VirtualDirectory dir, string targetFolderName, string currentPath = "")
        {
            if (dir == null) return null;

            if (!string.IsNullOrEmpty(dir.Name) && dir.Name.Equals(targetFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return currentPath;
            }

            if (dir.Folders != null)
            {
                foreach (var subDir in dir.Folders)
                {
                    if (subDir == null) continue;

                    string subPath = string.IsNullOrEmpty(currentPath)
                        ? (subDir.Name ?? "")
                        : currentPath + "/" + (subDir.Name ?? "");

                    if (!string.IsNullOrEmpty(subDir.Name) && subDir.Name.Equals(targetFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        return subPath;
                    }

                    string found = FindFolderPath(subDir, targetFolderName, subPath);
                    if (!string.IsNullOrEmpty(found))
                    {
                        return found;
                    }
                }
            }

            return null;
        }



        public void Save()
        {
            Console.WriteLine("=== YW2 저장 시작 ===");

            // 1단계: 메모리 로드
            Console.WriteLine("1단계: 전체 파일 메모리 로드 중...");
            ReadAllFiles(Game.Directory);
            if (Language != null) ReadAllFiles(Language.Directory);

            string tempPath = Path.Combine(Path.GetDirectoryName(RomfsPath), "temp");
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

            string tempGameFile = Path.Combine(tempPath, "yw2_a.fa");
            string destGameFile = RomfsPath + @"\yw2_a.fa";

            // 2단계: 임시 파일 저장
            Console.WriteLine("2단계: Game 임시 파일 저장...");
            Game.Save(tempGameFile);

            if (Language != null)
            {
                string tempLangFile = Path.Combine(tempPath, "yw2_lg_" + LanguageCode + ".fa");
                string destLangFile = RomfsPath + @"\yw2_lg_" + LanguageCode + ".fa";

                Console.WriteLine("3단계: Language 임시 파일 저장...");
                Language.Save(tempLangFile);

                // Language 교체 및 재로드
                Language.Close();
                if (File.Exists(destLangFile)) File.Delete(destLangFile);
                File.Move(tempLangFile, destLangFile);
                Language = new ARC0(new FileStream(destLangFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            }

            // 3단계: Game 파일 교체 및 재로드
            Console.WriteLine("4단계: Game 파일 교체...");
            Game.Close();
            if (File.Exists(destGameFile)) File.Delete(destGameFile);
            File.Move(tempGameFile, destGameFile);

            Console.WriteLine("5단계: Game 재로드...");
            Game = new ARC0(new FileStream(destGameFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

            // ✅ [핵심 수정] Game 객체가 바뀌었으므로 Files 목록도 새 객체를 가리키도록 갱신
            Console.WriteLine("6단계: 파일 참조 갱신...");
            InitializeFiles();

            Console.WriteLine("=== ✅ 저장 및 재로드 완료 ===");

            string report;
            ARC0IntegrityTester.Test(Game, "arc0_test_tmp.arc", out report);
            Console.WriteLine(report);

        }

        private void ReadAllFiles(VirtualDirectory dir)
        {
            foreach (var file in dir.Files.Values)
                if (file.ByteContent == null) try { file.Read(); } catch { }

            foreach (var sub in dir.Folders)
                ReadAllFiles(sub);
        }

        private byte[] FindFileRecursive(VirtualDirectory dir, string targetName)
        {
            foreach (var fileKey in dir.Files.Keys)
            {
                if (fileKey.Equals(targetName, StringComparison.OrdinalIgnoreCase) ||
                    fileKey.EndsWith("/" + targetName, StringComparison.OrdinalIgnoreCase) ||
                    (targetName.Length > 5 && fileKey.Contains(targetName)))
                {
                    var file = dir.Files[fileKey];
                    if (file.ByteContent == null) file.Read();
                    return file.ByteContent;
                }
            }
            foreach (var sub in dir.Folders)
            {
                byte[] found = FindFileRecursive(sub, targetName);
                if (found != null) return found;
            }
            return null;
        }

        public T2bþ GetTextObject(string textFile)
        {
            // ✅ [로그 추가] 함수 시작
            Console.WriteLine("\n========================================");
            Console.WriteLine($"[GetTextObject] 로드 시작: {DateTime.Now:HH:mm:ss.fff}");
            Console.WriteLine($"[GetTextObject] 📄 요청 파일: {textFile}");
            Console.WriteLine("========================================");

            T2bþ result = new T2bþ();
            result.Encoding = System.Text.Encoding.UTF8;

            Console.WriteLine($"[GetTextObject] 🔤 기본 인코딩 설정: {result.Encoding.EncodingName} (UTF-8)");

            byte[] fileData = null;
            string loadedFrom = "";

            // ✅ [로그 추가] 1단계: Files 딕셔너리에서 찾기
            if (Files.ContainsKey(textFile))
            {
                Console.WriteLine($"[GetTextObject] ✅ Files 딕셔너리에서 발견: {textFile}");

                try
                {
                    var gf = Files[textFile];
                    string cleanPath = gf.Path.Replace("\\", "/").Trim('/');

                    Console.WriteLine($"[GetTextObject] 📁 경로: {cleanPath}");
                    Console.WriteLine($"[GetTextObject] 🗂️ Archive: {(gf.File == Language ? "Language" : "Game")}");
                    Console.WriteLine($"[GetTextObject] 🔍 GetFileFromFullPath() 호출 중...");

                    fileData = gf.File.Directory.GetFileFromFullPath(cleanPath);
                    loadedFrom = $"Files[{textFile}] → {cleanPath}";

                    Console.WriteLine($"[GetTextObject] ✅ 파일 로드 성공: {fileData.Length:N0} bytes");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetTextObject] ⚠️ Files 딕셔너리 로드 실패: {ex.Message}");
                    Console.WriteLine($"[GetTextObject] 🔄 Fallback 방식으로 재시도...");
                }
            }
            else
            {
                Console.WriteLine($"[GetTextObject] ⚠️ Files 딕셔너리에 없음: {textFile}");
            }

            // ✅ [로그 추가] 2단계: FindFileRecursive로 검색
            if (fileData == null)
            {
                Console.WriteLine($"[GetTextObject] 🔍 FindFileRecursive() 검색 시작...");

                string targetName = Path.GetFileName(textFile);
                if (!targetName.Contains("."))
                {
                    targetName += "_" + (string.IsNullOrEmpty(LanguageCode) ? "ko" : LanguageCode) + ".cfg.bin";
                    Console.WriteLine($"[GetTextObject] 📝 확장자 추가: {targetName}");
                }

                // Language Archive 검색
                if (Language != null)
                {
                    Console.WriteLine($"[GetTextObject] 🔍 Language Archive 검색 중...");
                    fileData = FindFileRecursive(Language.Directory, targetName);
                    if (fileData != null)
                    {
                        loadedFrom = $"Language Archive → FindFileRecursive({targetName})";
                        Console.WriteLine($"[GetTextObject] ✅ Language에서 발견: {fileData.Length:N0} bytes");
                    }
                }

                // Game Archive 검색
                if (fileData == null)
                {
                    Console.WriteLine($"[GetTextObject] 🔍 Game Archive 검색 중...");
                    fileData = FindFileRecursive(Game.Directory, targetName);
                    if (fileData != null)
                    {
                        loadedFrom = $"Game Archive → FindFileRecursive({targetName})";
                        Console.WriteLine($"[GetTextObject] ✅ Game에서 발견: {fileData.Length:N0} bytes");
                    }
                }

                // 원본 파일명으로 재시도
                if (fileData == null && targetName.Contains("_"))
                {
                    string originalName = targetName.Split('_')[0] + ".cfg.bin";
                    Console.WriteLine($"[GetTextObject] 🔍 원본 파일명으로 재시도: {originalName}");
                    fileData = FindFileRecursive(Game.Directory, originalName);
                    if (fileData != null)
                    {
                        loadedFrom = $"Game Archive → FindFileRecursive({originalName})";
                        Console.WriteLine($"[GetTextObject] ✅ 원본 파일명으로 발견: {fileData.Length:N0} bytes");
                    }
                }
            }

            // ✅ [로그 추가] 파일을 못 찾은 경우
            if (fileData == null)
            {
                Console.WriteLine($"[GetTextObject] ❌ 파일을 찾을 수 없음: {textFile}");
                Console.WriteLine($"[GetTextObject] 🔙 빈 T2bþ 객체 반환");
                Console.WriteLine("========================================\n");
                return result;
            }

            Console.WriteLine($"[GetTextObject] 📂 최종 로드 경로: {loadedFrom}");

            // ✅ [로그 추가] 3단계: T2bþ.Open() 호출
            try
            {
                Console.WriteLine($"[GetTextObject] 🔄 T2bþ.Open() 호출 중...");
                Console.WriteLine($"[GetTextObject] 🔤 파싱 인코딩: {result.Encoding.EncodingName}");

                result.Open(fileData);

                Console.WriteLine($"[GetTextObject] ✅ Open() 성공");
                Console.WriteLine($"[GetTextObject] 📊 Texts: {result.Texts.Count}개");
                Console.WriteLine($"[GetTextObject] 📊 Nouns: {result.Nouns.Count}개");

                // 샘플 데이터 출력
                if (result.Texts.Count > 0)
                {
                    var sample = result.Texts.First();
                    Console.WriteLine($"[GetTextObject] 📝 Texts 샘플: 0x{sample.Key:X8} = \"{sample.Value.Strings.FirstOrDefault()?.Text}\"");
                }
                if (result.Nouns.Count > 0)
                {
                    var sample = result.Nouns.First();
                    Console.WriteLine($"[GetTextObject] 📝 Nouns 샘플: 0x{sample.Key:X8} = \"{sample.Value.Strings.FirstOrDefault()?.Text}\"");
                }

                // 데이터가 비어있으면 ManualParseText 시도
                if (result.Texts.Count == 0 && result.Nouns.Count == 0)
                {
                    Console.WriteLine($"[GetTextObject] ⚠️ Texts/Nouns가 비어있음 - ManualParseText() 시도");
                    ManualParseText(fileData, result);
                    Console.WriteLine($"[GetTextObject] 📊 ManualParse 후 - Texts: {result.Texts.Count}개, Nouns: {result.Nouns.Count}개");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetTextObject] ❌ Open() 실패: {ex.Message}");
                Console.WriteLine($"[GetTextObject] 🔄 ManualParseText() 시도...");

                ManualParseText(fileData, result);

                Console.WriteLine($"[GetTextObject] 📊 ManualParse 후 - Texts: {result.Texts.Count}개, Nouns: {result.Nouns.Count}개");
            }

            Console.WriteLine($"[GetTextObject] 🎉 로드 완료");
            Console.WriteLine($"[GetTextObject] 📊 최종 결과 - Texts: {result.Texts.Count}개, Nouns: {result.Nouns.Count}개");
            Console.WriteLine($"[GetTextObject] 🔤 최종 인코딩: {result.Encoding.EncodingName}");
            Console.WriteLine("========================================\n");

            return result;
        }




        private void ManualParseText(byte[] data, T2bþ container)
        {
            try
            {
                CfgBin cfg = new CfgBin();
                cfg.Open(data);

                string[] targets = { "NOUN_INFO_BEGIN", "TEXT_INFO_BEGIN", "ITEM_TEXT_INFO_BEGIN", "BATTLE_TEXT_INFO_BEGIN", "SKILL_TEXT_INFO_BEGIN", "CHARA_ABILITY_TEXT_INFO_BEGIN", "SYSTEM_TEXT_INFO_BEGIN", "ADD_MEMBER_MENU_TEXT_INFO_BEGIN" };
                var entries = cfg.Entries.Where(x => targets.Contains(x.GetName())).ToList();
                if (entries.Count == 0 && cfg.Entries.Count > 0) entries = cfg.Entries.Where(x => x.Children.Count > 10).ToList();

                foreach (var entry in entries)
                {
                    foreach (var child in entry.Children)
                    {
                        if (child.Variables.Count > 0)
                        {
                            int crc = Convert.ToInt32(child.Variables[0].Value);
                            string txt = "";
                            foreach (var v in child.Variables)
                            {
                                if (v.Value is OffsetTextPair p && !string.IsNullOrEmpty(p.Text)) { txt = p.Text; break; }
                                else if (v.Value is string s && !string.IsNullOrEmpty(s)) { txt = s; break; }
                            }
                            if (string.IsNullOrEmpty(txt)) continue;

                            var config = new TextConfig(new List<StringLevel5> { new StringLevel5(0, txt) }, -1);
                            if (!container.Nouns.ContainsKey(crc)) container.Nouns.Add(crc, config);
                            if (!container.Texts.ContainsKey(crc)) container.Texts.Add(crc, config);
                        }
                    }
                }
            }
            catch { }
        }

        public ICharaabilityConfig[] GetAbilities()
        {
            byte[] data = FindFileRecursive(Game.Directory, "chara_ability");
            if (data == null) return new ICharaabilityConfig[0];

            CfgBin cfg = new CfgBin();
            cfg.Open(data);

            return cfg.Entries
                .Where(x => x.GetName() == "CHARA_ABILITY_CONFIG_INFO_LIST_BEG")
                .SelectMany(x => x.Children)
                .Where(x => x.GetName() == "CHARA_ABILITY_CONFIG_INFO")
                .Select(x => {
                    var ab = x.ToClass<CharaabilityConfig>();
                    if (x.Variables.Count > 0) ab.CharaabilityConfigHash = Convert.ToInt32(x.Variables[0].Value);
                    return (ICharaabilityConfig)ab;
                })
                .ToArray();
        }

        public IBattleCommand[] GetBattleCommands()
        {
            byte[] data = null;
            Action<VirtualDirectory> scanner = null;
            scanner = (d) => {
                var k = d.Files.Keys.Where(x => x.StartsWith("battle_command") && !x.Contains("link")).OrderByDescending(x => x).FirstOrDefault();
                if (k != null) { data = d.GetFileFromFullPath(k); return; }
                foreach (var s in d.Folders) if (data == null) scanner(s);
            };
            scanner(Game.Directory);

            if (data == null) return new IBattleCommand[0];
            CfgBin cfg = new CfgBin();
            cfg.Open(data);
            return cfg.Entries.Where(x => x.GetName() == "BATTLE_COMMAND_INFO_BEGIN")
                .SelectMany(x => x.Children).Select(x => (IBattleCommand)x.ToClass<Battlecommand>()).ToArray();
        }

        public ICharabase[] GetCharacterbase(bool isYokai)
        {
            byte[] data = FindFileRecursive(Game.Directory, "chara_base");
            if (data == null) return new ICharabase[0];
            CfgBin f = new CfgBin(); f.Open(data);
            string b = isYokai ? "CHARA_BASE_YOKAI_INFO_BEGIN" : "CHARA_BASE_INFO_BEGIN";
            var e = f.Entries.FirstOrDefault(x => x.GetName() == b) ?? f.Entries.FirstOrDefault(x => x.GetName().Contains("INFO_BEGIN"));
            if (e == null) return new ICharabase[0];
            return e.Children.Select(x => {
                try
                {
                    if (isYokai)
                    {
                        var y = x.ToClass<YokaiCharabase>();
                        if (x.Variables.Count > 4) { y.BaseHash = (int)x.Variables[0].Value; y.NameHash = (int)x.Variables[4].Value; }
                        return (ICharabase)y;
                    }
                    return (ICharabase)x.ToClass<NPCCharabase>();
                }
                catch { return null; }
            }).Where(x => x != null).ToArray();
        }

        public ICharaparam[] GetCharaparam()
        {
            byte[] data = FindFileRecursive(Game.Directory, "chara_param");
            if (data == null) return new ICharaparam[0];
            CfgBin f = new CfgBin(); f.Open(data);
            return f.Entries.Where(x => x.GetName() == "CHARA_PARAM_INFO_BEGIN").SelectMany(x => x.Children).Select(x => x.ToClass<Charaparam>()).ToArray();
        }

        public void SaveCharaparam(ICharaparam[] p)
        {
            if (p == null) return;
            Charaparam[] fp = p.OfType<Charaparam>().ToArray();
            VirtualDirectory td = null;
            string fn = "";
            Action<VirtualDirectory> sc = null;
            sc = (d) => {
                var k = d.Files.Keys.FirstOrDefault(x => x.StartsWith("chara_param"));
                if (k != null) { fn = k; td = d; return; }
                foreach (var s in d.Folders) if (td == null) sc(s);
            };
            sc(Game.Directory);
            if (td == null) return;
            CfgBin f = new CfgBin();
            f.Open(td.GetFileFromFullPath(fn));
            f.ReplaceEntry("CHARA_PARAM_INFO_BEGIN", "CHARA_PARAM_INFO_", fp);
            td.Files[fn].ByteContent = f.Save();
        }

        public IItem[] GetItems(string type)
        {
            byte[] data = FindFileRecursive(Game.Directory, "item_config");
            if (data == null) return new IItem[0];
            CfgBin f = new CfgBin(); f.Open(data);
            if (type == "all")
            {
                string[] t = { "ITEM_EQUIPMENT_BEGIN", "ITEM_SOUL_BEGIN", "ITEM_CONSUME_BEGIN", "ITEM_IMPORTANT_BEGIN", "ITEM_CREATURE_BEGIN" };
                return f.Entries.Where(x => t.Contains(x.GetName())).SelectMany(x => x.Children).Select(x => x.ToClass<Item>()).ToArray();
            }
            return new IItem[0];
        }

        public ICharaevolve[] GetCharaevolution()
        {
            byte[] data = FindFileRecursive(Game.Directory, "chara_param");
            if (data == null) return new ICharaevolve[0];
            CfgBin f = new CfgBin(); f.Open(data);
            return f.Entries.Where(x => x.GetName() == "CHARA_EVOLVE_INFO_BEGIN").SelectMany(x => x.Children).Select(x => x.ToClass<Charaevolve>()).ToArray();
        }

        public void SaveCharaBase(ICharabase[] c)
        {
            NPCCharabase[] n = c.OfType<NPCCharabase>().ToArray();
            YokaiCharabase[] y = c.OfType<YokaiCharabase>().ToArray();
            VirtualDirectory td = null; string fn = "";
            Action<VirtualDirectory> sc = null;
            sc = (d) => {
                var k = d.Files.Keys.FirstOrDefault(x => x.StartsWith("chara_base"));
                if (k != null) { fn = k; td = d; return; }
                foreach (var s in d.Folders) if (td == null) sc(s);
            };
            sc(Game.Directory);
            if (td == null) return;
            CfgBin f = new CfgBin();
            f.Open(td.GetFileFromFullPath(fn));
            f.ReplaceEntry("CHARA_BASE_INFO_BEGIN", "CHARA_BASE_INFO_", n);
            f.ReplaceEntry("CHARA_BASE_YOKAI_INFO_BEGIN", "CHARA_BASE_YOKAI_INFO_", y);
            td.Files[fn].ByteContent = f.Save();
        }

        public string[] GetMapWhoContainsEncounter() => new string[0];
        public (IEncountTable[], IEncountChara[]) GetMapEncounter(string m) => (null, null);
        public void SaveMapEncounter(string m, IEncountTable[] t, IEncountChara[] c) { }
        public (IShopConfig[], IShopValidCondition[]) GetShop(string s) => (null, null);
        public void SaveShop(string s, IShopConfig[] c, IShopValidCondition[] v) { }
        public void SaveCharaevolution(ICharaevolve[] c) { }
        public void SaveCharascale(ICharascale[] c) { }
        public ICharascale[] GetCharascale() => new ICharascale[0];
        public IOrgetimeTechnic[] GetOrgetimeTechnics() => new IOrgetimeTechnic[0];
        public IHackslashTechnic[] GetHackslashSkills() => null;
        public IHackslashCharaabilityConfig[] GetHackslashAbilities() => null;
        public void SaveHackslashCharaparam(IHackslashCharaparam[] h) { }
        public IHackslashCharaparam[] GetHackslashCharaparam() => null;
        public void SaveBattleCharaparam(IBattleCharaparam[] b) { }
        public IBattleCharaparam[] GetBattleCharaparam() => null;
        public ISkillconfig[] GetSkills() => null;
    }
}