using System;
using System.Collections.Generic;
using System.Linq;

public static class VirtualDirectoryDebugger
{
    public static void Validate(VirtualDirectory root)
    {
        var errors = new List<string>();

        foreach (var path in root.EnumerateFilePaths())
        {
            // 1. 이중 슬래시
            if (path.Contains("//"))
                errors.Add($"[PATH] Double slash: {path}");

            // 2. 역슬래시
            if (path.Contains("\\"))
                errors.Add($"[PATH] Backslash: {path}");

            // 3. 빈 파일명
            if (path.EndsWith("/"))
                errors.Add($"[PATH] Empty filename: {path}");

            // 4. 빈 경로 요소
            var parts = path.Split('/');
            if (parts.Any(p => string.IsNullOrWhiteSpace(p)))
                errors.Add($"[PATH] Empty part: {path}");
        }

        if (errors.Count > 0)
        {
            Console.WriteLine("❌ [VirtualDirectory Validation FAILED]");
            foreach (var e in errors)
                Console.WriteLine("  " + e);

            throw new InvalidOperationException(
                "경로 구조가 깨져 있어 ARC0 저장을 중단했습니다."
            );
        }

        Console.WriteLine("✅ [VirtualDirectory Validation OK]");
    }
}
