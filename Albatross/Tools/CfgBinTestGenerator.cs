using System;
using System.IO;
using System.Linq;
using Albatross.Level5.Binary;
using Albatross.Level5.Archive.ARC0;
using Albatross.Yokai_Watch.Logic;

namespace Albatross.Tools
{
    /// <summary>
    /// Test program to generate 3 variants of chara_param for comparison:
    /// A) Original (extracted from ROM)
    /// B) Round-trip (Open → Save without modification)
    /// C) Modified (Open → ReplaceEntry → Save)
    /// </summary>
    class CfgBinTestGenerator
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: CfgBinTestGenerator <input_arc0> <output_directory>");
                Console.WriteLine("Example: CfgBinTestGenerator yw2_a.fa C:\\test_output");
                return;
            }

            string arcPath = args[0];
            string outputDir = args[1];

            if (!File.Exists(arcPath))
            {
                Console.WriteLine($"Error: Input file not found: {arcPath}");
                return;
            }

            Directory.CreateDirectory(outputDir);

            Console.WriteLine("=== CfgBin Test File Generator ===");
            Console.WriteLine($"Input: {arcPath}");
            Console.WriteLine($"Output: {outputDir}");
            Console.WriteLine();

            try
            {
                // Load ARC0
                Console.WriteLine("[1/4] Loading ARC0...");
                ARC0 arc = new ARC0(File.OpenRead(arcPath));
                
                // Find chara_param
                Console.WriteLine("[2/4] Finding chara_param...");
                var characterFolder = arc.Directory.GetFolderFromFullPath("data/res/character");
                string charaparamName = characterFolder.Files.Keys
                    .Where(x => x.StartsWith("chara_param"))
                    .OrderByDescending(x => x)
                    .First();
                
                Console.WriteLine($"  Found: {charaparamName}");
                
                byte[] originalBytes = characterFolder.GetFileDataReadOnly(charaparamName);
                
                // File A: Original
                string fileA = Path.Combine(outputDir, "A_original.cfg.bin");
                File.WriteAllBytes(fileA, originalBytes);
                Console.WriteLine($"  [A] Original → {fileA} ({originalBytes.Length:N0} bytes)");
                
                // File B: Round-trip (Open → Save)
                Console.WriteLine("[3/4] Generating round-trip file...");
                CfgBin cfgRoundtrip = new CfgBin();
                cfgRoundtrip.Open(originalBytes);
                byte[] roundtripBytes = cfgRoundtrip.Save();
                
                string fileB = Path.Combine(outputDir, "B_roundtrip.cfg.bin");
                File.WriteAllBytes(fileB, roundtripBytes);
                Console.WriteLine($"  [B] Round-trip → {fileB} ({roundtripBytes.Length:N0} bytes)");
                
                // File C: Modified (Open → ReplaceEntry → Save)
                Console.WriteLine("[4/4] Generating modified file...");
                CfgBin cfgModified = new CfgBin();
                cfgModified.Open(originalBytes);
                
                // Get current data
                var charaparams = cfgModified.GetEntries<Charaparam>("CHARA_PARAM_INFO_BEGIN", "CHARA_PARAM_INFO_");
                var charaevolutions = cfgModified.GetEntries<Charaevolve>("CHARA_EVOLVE_INFO_BEGIN", "CHARA_EVOLVE_INFO_");
                
                // Modify first yokai's ability to "No Guard" (0xD3AEAB7D)
                if (charaparams.Length > 0)
                {
                    charaparams[0].AbilityFlat = 0xD3AEAB7D;
                    Console.WriteLine($"  Modified: {charaparams[0].Name} → Ability = 0xD3AEAB7D (No Guard)");
                }
                
                // Replace entries
                cfgModified.ReplaceEntry("CHARA_PARAM_INFO_BEGIN", "CHARA_PARAM_INFO_", charaparams);
                cfgModified.ReplaceEntry("CHARA_EVOLVE_INFO_BEGIN", "CHARA_EVOLVE_INFO_", charaevolutions);
                
                byte[] modifiedBytes = cfgModified.Save();
                
                string fileC = Path.Combine(outputDir, "C_modified.cfg.bin");
                File.WriteAllBytes(fileC, modifiedBytes);
                Console.WriteLine($"  [C] Modified → {fileC} ({modifiedBytes.Length:N0} bytes)");
                
                // Summary
                Console.WriteLine();
                Console.WriteLine("=== Generation Complete ===");
                Console.WriteLine($"A vs B: {CompareSize(originalBytes.Length, roundtripBytes.Length)}");
                Console.WriteLine($"B vs C: {CompareSize(roundtripBytes.Length, modifiedBytes.Length)}");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine($"  python compare_cfg_binary.py \"{fileA}\" \"{fileB}\"");
                Console.WriteLine($"  python compare_cfg_binary.py \"{fileB}\" \"{fileC}\"");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        static string CompareSize(int size1, int size2)
        {
            int diff = size2 - size1;
            if (diff == 0)
                return "Same size";
            else if (diff > 0)
                return $"+{diff:N0} bytes";
            else
                return $"{diff:N0} bytes";
        }
    }
}
