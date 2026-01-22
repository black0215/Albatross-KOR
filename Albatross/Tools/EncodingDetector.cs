using System;
using System.Text;

namespace Albatross.Tools
{
    /// <summary>
    /// 바이너리 데이터의 인코딩(UTF-8 vs Shift-JIS)을 휴리스틱 방식으로 감지하는 유틸리티 클래스입니다.
    /// 독자적인 't2b' 풋터 없이도 파일의 인코딩을 정확히 판별할 수 있도록 합니다.
    /// </summary>
    public static class EncodingDetector
    {
        /// <summary>
        /// 주어진 데이터가 UTF-8인지 Shift-JIS인지 판별합니다.
        /// 기본적으로 UTF-8 유효성을 검사하고, 실패할 경우 Level-5 표준인 Shift-JIS로 간주합니다.
        /// </summary>
        /// <param name="data">검사할 바이트 배열 (StringTable 샘플)</param>
        /// <returns>감지된 Encoding 객체</returns>
        public static Encoding Detect(byte[] data)
        {
            if (data == null || data.Length == 0)
                return Encoding.GetEncoding("shift_jis"); // [FIX] 원본 게임 기본 인코딩으로 변경

            // 1. BOM(Byte Order Mark) 검사
            if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            {
                return Encoding.UTF8;
            }

            // 2. UTF-8 비트 패턴 유효성 검사
            if (IsValidUtf8(data))
            {
                return Encoding.UTF8;
            }

            // 3. Fallback: Level-5 구작 게임(3DS 등)은 주로 Shift-JIS(CP932)를 사용함
            return Encoding.GetEncoding("shift_jis");
        }

        /// <summary>
        /// 바이트 배열이 유효한 UTF-8 시퀀스인지 검증합니다.
        /// </summary>
        private static bool IsValidUtf8(byte[] data)
        {
            int i = 0;
            while (i < data.Length)
            {
                byte b = data[i];

                if (b < 0x80)
                {
                    // ASCII 문자 (0xxxxxxx) - 호환됨
                    i++;
                }
                else if ((b & 0xE0) == 0xC0) // 2바이트 시퀀스 (110xxxxx)
                {
                    // 2바이트 문자의 시작 (0xC2 ~ 0xDF)
                    if (b < 0xC2) return false; // Overlong encoding 방지
                    if (i + 1 >= data.Length) return false; // 예기치 않은 EOF
                    if ((data[i + 1] & 0xC0) != 0x80) return false; // 유효하지 않은 연속 바이트
                    i += 2;
                }
                else if ((b & 0xF0) == 0xE0) // 3바이트 시퀀스 (1110xxxx)
                {
                    if (i + 2 >= data.Length) return false;
                    if ((data[i + 1] & 0xC0) != 0x80) return false;
                    if ((data[i + 2] & 0xC0) != 0x80) return false;
                    // 대리자(Surrogates) 범위 체크 등은 생략해도 실용적 감지엔 충분함
                    i += 3;
                }
                else if ((b & 0xF8) == 0xF0) // 4바이트 시퀀스 (11110xxx)
                {
                    if (i + 3 >= data.Length) return false;
                    if ((data[i + 1] & 0xC0) != 0x80) return false;
                    if ((data[i + 2] & 0xC0) != 0x80) return false;
                    if ((data[i + 3] & 0xC0) != 0x80) return false;
                    i += 4;
                }
                else
                {
                    // 유효하지 않은 UTF-8 시작 바이트 (10xxxxxx, 11111xxx 등)
                    // Shift-JIS의 2바이트 문자 첫 바이트일 확률이 높음 (0x81-0x9F, 0xE0-0xFC)
                    return false;
                }
            }
            return true;
        }
    }
}
