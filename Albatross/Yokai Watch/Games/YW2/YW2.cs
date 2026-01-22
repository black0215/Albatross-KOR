using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Albatross.Tools;
using Albatross.Level5.Text;
using Albatross.Level5.Text.Logic;
using Albatross.Level5.Archive.ARC0;
using Albatross.Level5.Binary;
using Albatross.Yokai_Watch.Games.YW2.Logic;
using Albatross.Yokai_Watch.Logic;
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
        private Dictionary<string, string> _filePathCache = new Dictionary<string, string>();

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


        private void InitializeFiles()
        {
            _filePathCache.Clear();
            InitializeFilesInternal();
        }

        private void InitializeFilesInternal()
        {
            ARC0 targetArchive = Language ?? Game;
            Files = new Dictionary<string, GameFile>();

            if (targetArchive == null || targetArchive.Directory == null) return;

            string langSuffix = "_" + (string.IsNullOrEmpty(LanguageCode) ? "ko" : LanguageCode) + ".cfg.bin";
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
                }
            }
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
                    return path;
                }

                // 끝부분 매칭
                foreach (var key in dir.Files.Keys)
                {
                    if (key.EndsWith(targetFileName))
                    {
                        return key;
                    }
                }

                // 부분 매칭
                foreach (var key in dir.Files.Keys)
                {
                    if (key.Contains(targetFileName))
                    {
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



        public void Save(Action<int, int, string> progressCallback = null)
        {
            string tempPath = Path.Combine(Path.GetDirectoryName(RomfsPath), "temp");
            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

            string tempGameFile = Path.Combine(tempPath, "yw2_a.fa");
            string destGameFile = RomfsPath + @"\yw2_a.fa";

            Game.Save(tempGameFile, progressCallback);

            if (Language != null)
            {
                string tempLangFile = Path.Combine(tempPath, "yw2_lg_" + LanguageCode + ".fa");
                string destLangFile = RomfsPath + @"\yw2_lg_" + LanguageCode + ".fa";
                Language.Save(tempLangFile, progressCallback);

                Language.Close();
                if (File.Exists(destLangFile)) File.Delete(destLangFile);
                File.Move(tempLangFile, destLangFile);
                Language = new ARC0(new FileStream(destLangFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            }

            Game.Close();
            if (File.Exists(destGameFile)) File.Delete(destGameFile);
            File.Move(tempGameFile, destGameFile);

            Game = new ARC0(new FileStream(destGameFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            InitializeFiles();
        }

        private byte[] FindFileRecursive(VirtualDirectory dir, string targetName)
        {
            if (_filePathCache.TryGetValue(targetName, out string cachedPath))
            {
                try
                {
                    var file = dir.Files.ContainsKey(cachedPath) ? dir.Files[cachedPath] : null;
                    if (file != null)
                    {
                        return file.ByteContent ?? file.ReadWithoutCaching();
                    }
                }
                catch
                {
                    _filePathCache.Remove(targetName);
                }
            }

            return FindFileRecursiveUncached(dir, targetName);
        }

        private byte[] FindFileRecursiveUncached(VirtualDirectory dir, string targetName)
        {
            foreach (var fileKey in dir.Files.Keys)
            {
                if (fileKey.Equals(targetName, StringComparison.OrdinalIgnoreCase) ||
                    fileKey.EndsWith("/" + targetName, StringComparison.OrdinalIgnoreCase) ||
                    (targetName.Length > 5 && fileKey.Contains(targetName)))
                {
                    var file = dir.Files[fileKey];
                    _filePathCache[targetName] = fileKey;
                    if (file.ByteContent != null)
                        return file.ByteContent;
                    return file.ReadWithoutCaching();
                }
            }
            foreach (var sub in dir.Folders)
            {
                byte[] found = FindFileRecursiveUncached(sub, targetName);
                if (found != null) return found;
            }
            return null;
        }

        public T2bþ GetTextObject(string textFile)
        {
            T2bþ result = new T2bþ();
            result.Encoding = System.Text.Encoding.UTF8;

            byte[] fileData = null;
            string loadedFrom = "";

            if (Files.ContainsKey(textFile))
            {
                try
                {
                    var gf = Files[textFile];
                    string cleanPath = gf.Path.Replace("\\", "/").Trim('/');
                    fileData = gf.File.Directory.GetFileDataReadOnly(cleanPath);
                    loadedFrom = $"Files[{textFile}] → {cleanPath}";
                }
                catch (Exception)
                {
                    // Fallback to FindFileRecursive
                }
            }

            if (fileData == null)
            {
                string targetName = Path.GetFileName(textFile);
                if (!targetName.Contains("."))
                {
                    targetName += "_" + (string.IsNullOrEmpty(LanguageCode) ? "ko" : LanguageCode) + ".cfg.bin";
                }

                if (Language != null)
                {
                    fileData = FindFileRecursive(Language.Directory, targetName);
                    if (fileData != null) loadedFrom = $"Language Archive → FindFileRecursive({targetName})";
                }

                if (fileData == null)
                {
                    fileData = FindFileRecursive(Game.Directory, targetName);
                    if (fileData != null) loadedFrom = $"Game Archive → FindFileRecursive({targetName})";
                }

                if (fileData == null && targetName.Contains("_"))
                {
                    string originalName = targetName.Split('_')[0] + ".cfg.bin";
                    fileData = FindFileRecursive(Game.Directory, originalName);
                    if (fileData != null) loadedFrom = $"Game Archive → FindFileRecursive({originalName})";
                }
            }

            if (fileData == null) return result;

            try
            {
                result.Open(fileData);

                if (result.Texts.Count == 0 && result.Nouns.Count == 0)
                {
                    ManualParseText(fileData, result);
                }
            }
            catch (Exception)
            {
                ManualParseText(fileData, result);
            }

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

        // file path와 content를 함께 반환하는 헬퍼 메서드
        private (string Path, byte[] Content) FindFileRecursiveWithPath(VirtualDirectory dir, string targetName)
        {
            foreach (var fileKey in dir.Files.Keys)
            {
                // 경로 구분자로 나눈 마지막 부분(파일명)만 추출
                string fileName = fileKey;
                int lastSlash = fileName.LastIndexOf('/');
                if (lastSlash >= 0) fileName = fileName.Substring(lastSlash + 1);

                // 엄격한 매칭: 정확히 일치하거나, 파일명이 targetName으로 시작해야 함
                if (fileName.Equals(targetName, StringComparison.OrdinalIgnoreCase) ||
                    fileName.StartsWith(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    var file = dir.Files[fileKey];
                    // [FIX] Use ReadWithoutCaching to avoid marking file as modified
                    byte[] content = file.ByteContent ?? file.ReadWithoutCaching();
                    return (fileKey, content);
                }
            }
            foreach (var sub in dir.Folders)
            {
                var found = FindFileRecursiveWithPath(sub, targetName);
                if (found.Content != null) return found;
            }
            return (null, null);
        }

        // 로깅을 포함한 파일 검색 헬퍼
        private byte[] FindFileWithLog(string callerName, string searchKeyword)
        {
            Console.WriteLine($"\n[{callerName}] 불러올 파일 검색 중: \"{searchKeyword}\"...");
            var found = FindFileRecursiveWithPath(Game.Directory, searchKeyword);

            if (found.Content == null)
            {
                Console.WriteLine($"[{callerName}] ❌ 파일을 찾지 못했습니다.");
                return null;
            }

            Console.WriteLine($"[{callerName}] ✅ 실제 로드된 파일: \"{found.Path}\" ({found.Content.Length:N0} bytes)");
            return found.Content;
        }

        public ICharaabilityConfig[] GetAbilities()
        {
            byte[] data = FindFileWithLog("GetAbilities", "chara_ability");
            if (data == null) return new ICharaabilityConfig[0];

            CfgBin cfg = new CfgBin();
            cfg.Open(data);

            return cfg.Entries
                .Where(x => x.GetName() == "CHARA_ABILITY_CONFIG_INFO_LIST_BEG")
                .SelectMany(x => x.Children)
                .Where(x => x.GetName() == "CHARA_ABILITY_CONFIG_INFO")
                .Select(x =>
                {
                    var ab = x.ToClass<CharaabilityConfig>();
                    if (x.Variables.Count > 0) ab.CharaabilityConfigHash = Convert.ToInt32(x.Variables[0].Value);
                    return (ICharaabilityConfig)ab;
                })
                .ToArray();
        }

        public IBattleCommand[] GetBattleCommands()
        {
            Console.WriteLine("\n[GetBattleCommands] 불러올 파일 검색 중: \"battle_command\"...");

            byte[] data = null;
            string loadedPath = "";

            Action<VirtualDirectory> scanner = null;
            scanner = (d) =>
            {
                var k = d.Files.Keys
                    .Where(x => x.StartsWith("battle_command") && !x.Contains("link"))
                    .Max();
                if (k != null)
                {
                    // [FIX] Use read-only access to avoid marking file as modified
                    data = d.GetFileDataReadOnly(k);
                    loadedPath = k; // 경로 저장 (실제로는 파일명만 키로 저장됨, 필요시 수정 가능하나 여기선 키값 사용)
                    // VirtualDirectory 구조상 k가 전체 경로일 수도, 파일명일 수도 있음. 
                    // GetFileFromFullPath가 동작하므로 k는 유효한 경로.
                    return;
                }
                foreach (var s in d.Folders) if (data == null) scanner(s);
            };
            scanner(Game.Directory);

            if (data == null)
            {
                Console.WriteLine("[GetBattleCommands] ❌ 파일을 찾지 못했습니다.");
                return new IBattleCommand[0];
            }

            Console.WriteLine($"[GetBattleCommands] ✅ 실제 로드된 파일: \"{loadedPath}\" ({data.Length:N0} bytes)");

            CfgBin cfg = new CfgBin();
            cfg.Open(data);
            return cfg.Entries.Where(x => x.GetName() == "BATTLE_COMMAND_INFO_BEGIN")
                .SelectMany(x => x.Children).Select(x => (IBattleCommand)x.ToClass<Battlecommand>()).ToArray();
        }

        public ICharabase[] GetCharacterbase(bool isYokai)
        {
            byte[] data = FindFileWithLog("GetCharacterbase", "chara_base");
            if (data == null) return new ICharabase[0];
            CfgBin f = new CfgBin(); f.Open(data);
            string b = isYokai ? "CHARA_BASE_YOKAI_INFO_BEGIN" : "CHARA_BASE_INFO_BEGIN";
            var e = f.Entries.FirstOrDefault(x => x.GetName() == b) ?? f.Entries.FirstOrDefault(x => x.GetName().Contains("INFO_BEGIN"));
            if (e == null) return new ICharabase[0];
            return e.Children.Select(x =>
            {
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
            byte[] data = FindFileWithLog("GetCharaparam", "chara_param");
            if (data == null) return new ICharaparam[0];
            CfgBin f = new CfgBin(); f.Open(data);
            return f.Entries.Where(x => x.GetName() == "CHARA_PARAM_INFO_BEGIN").SelectMany(x => x.Children).Select(x => x.ToClass<Charaparam>()).ToArray();
        }

        public void SaveCharaparam(ICharaparam[] p)
        {
            // This method is kept for backward compatibility but now does nothing
            // Actual saving happens in SaveCharaevolution which saves both sections
        }

        public IItem[] GetItems(string type)
        {
            byte[] data = FindFileWithLog("GetItems", "item_config");
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
            byte[] data = FindFileWithLog("GetCharaevolution", "chara_param");
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
            sc = (d) =>
            {
                var k = d.Files.Keys.FirstOrDefault(x => x.StartsWith("chara_base"));
                if (k != null) { fn = k; td = d; return; }
                foreach (var s in d.Folders) if (td == null) sc(s);
            };
            sc(Game.Directory);
            if (td == null) return;
            CfgBin f = new CfgBin();
            // [FIX] Use read-only access for initial load
            f.Open(td.GetFileDataReadOnly(fn));
            f.ReplaceEntry("CHARA_BASE_INFO_BEGIN", "CHARA_BASE_INFO_", n);
            f.ReplaceEntry("CHARA_BASE_YOKAI_INFO_BEGIN", "CHARA_BASE_YOKAI_INFO_", y);
            td.Files[fn].ByteContent = f.Save();
        }

        public string[] GetMapWhoContainsEncounter() => new string[0];
        public (IEncountTable[], IEncountChara[]) GetMapEncounter(string m) => (null, null);
        public void SaveMapEncounter(string m, IEncountTable[] t, IEncountChara[] c) { }
        public (IShopConfig[], IShopValidCondition[]) GetShop(string s) => (null, null);
        public void SaveShop(string s, IShopConfig[] c, IShopValidCondition[] v) { }
        public void SaveCharaevolution(ICharaevolve[] charaevolutions)
        {
            // This method is kept for backward compatibility but now does nothing
            // Actual saving happens in SaveCharaparamAndEvolution
        }

        public void SaveCharaparamAndEvolution(ICharaparam[] charaparams, ICharaevolve[] charaevolutions)
        {

            if (charaparams == null && charaevolutions == null) return;

            Charaparam[] formatCharaparams = charaparams?.OfType<Charaparam>().ToArray();
            Charaevolve[] formatCharaevolutions = charaevolutions?.OfType<Charaevolve>().ToArray();

            VirtualDirectory characterFolder = Game.Directory.GetFolderFromFullPath("data/res/character");
            string lastCharaparam = characterFolder.Files.Keys
                .Where(x => x.StartsWith("chara_param"))
                .Max();

            CfgBin charaparamFile = new CfgBin();
            // [FIX] Use read-only access to avoid marking file as modified
            charaparamFile.Open(characterFolder.GetFileDataReadOnly(lastCharaparam));

            // [FIX] Save BOTH sections in single operation to prevent double-save corruption
            if (formatCharaparams != null && formatCharaparams.Length > 0)
            {
                charaparamFile.ReplaceEntry("CHARA_PARAM_INFO_BEGIN", "CHARA_PARAM_INFO_", formatCharaparams);
            }

            if (formatCharaevolutions != null && formatCharaevolutions.Length > 0)
            {
                charaparamFile.ReplaceEntry("CHARA_EVOLVE_INFO_BEGIN", "CHARA_EVOLVE_INFO_", formatCharaevolutions);
            }


            characterFolder.Files[lastCharaparam].ByteContent = charaparamFile.Save();
        }
        public void SaveCharascale(ICharascale[] charascales)
        {
            Charascale[] formatCharascales = charascales.OfType<Charascale>().ToArray();

            VirtualDirectory characterFolder = Game.Directory.GetFolderFromFullPath("data/res/character");
            string lastCharascale = characterFolder.Files.Keys
                .Where(x => x.StartsWith("chara_scale"))
                .Max();

            CfgBin charaScaleFile = new CfgBin();
            // [FIX] Use read-only access for initial load to avoid accidental marking
            charaScaleFile.Open(characterFolder.GetFileDataReadOnly(lastCharascale));

            charaScaleFile.ReplaceEntry("CHARA_SCALE_INFO_LIST_BEG", "CHARA_SCALE_INFO_", formatCharascales);

            characterFolder.Files[lastCharascale].ByteContent = charaScaleFile.Save();
        }

        public ICharascale[] GetCharascale()
        {
            VirtualDirectory characterFolder = Game.Directory.GetFolderFromFullPath("data/res/character");
            string lastCharascale = characterFolder.Files.Keys
                .Where(x => x.StartsWith("chara_scale"))
                .Max();

            CfgBin charaScaleFile = new CfgBin();
            // [FIX] Use read-only access
            charaScaleFile.Open(characterFolder.GetFileDataReadOnly(lastCharascale));

            return charaScaleFile.Entries
                .Where(x => x.GetName() == "CHARA_SCALE_INFO_LIST_BEG")
                .SelectMany(x => x.Children)
                .Select(x => x.ToClass<Charascale>())
                .ToArray();
        }
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