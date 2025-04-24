using System.Text.RegularExpressions;

namespace Breadcrumbs.Utils.Gen2 {
    /// <summary>
    /// 문자 유형 분류 및 특성 판별 유틸리티
    /// </summary>
    public static class CharacterClassifier {
        // 한글 문자 범위 (유니코드)
        private const int kHangulStart = 0xAC00;
        private const int kHangulEnd = 0xD7A3;

        // 히라가나 문자 범위 (유니코드)
        private const int kHiraganaStart = 0x3040;
        private const int kHiraganaEnd = 0x309F;

        // 가타카나 문자 범위 (유니코드)
        private const int kKatakanaStart = 0x30A0;
        private const int kKatakanaEnd = 0x30FF;

        // 일본어 한자 범위 (유니코드) 
        private const int kKanjiStart = 0x4E00;
        private const int kKanjiEnd = 0x9FAF;

        // 영문자 정규식 패턴
        private static readonly Regex EnglishPattern = new Regex(@"^[a-zA-Z]+$", RegexOptions.Compiled);

        // 숫자 정규식 패턴
        private static readonly Regex NumberPattern = new Regex(@"^[0-9]+$", RegexOptions.Compiled);

        /// <summary>
        /// 마지막 문자의 언어 유형을 판별
        /// </summary>
        /// <param name="word">판별할 단어</param>
        /// <returns>언어 유형</returns>
        public static LanguageType GetLanguageType(string word) {
            if (string.IsNullOrEmpty(word))
                return LanguageType.Unknown;

            char lastChar = word[^1];

            // 한글인 경우
            if (IsKorean(lastChar))
                return LanguageType.Korean;

            // 일본어인 경우
            if (IsJapanese(lastChar))
                return LanguageType.Japanese;

            // 영어인 경우
            if (IsEnglish(lastChar.ToString()))
                return LanguageType.English;

            // 숫자인 경우
            if (IsNumber(lastChar.ToString()))
                return LanguageType.Number;

            return LanguageType.Unknown;
        }

        /// <summary>
        /// 한글 문자인지 판별
        /// </summary>
        /// <param name="c">판별할 문자</param>
        /// <returns>한글 여부</returns>
        public static bool IsKorean(char c) {
            return c >= kHangulStart && c <= kHangulEnd;
        }

        /// <summary>
        /// 영어 문자인지 판별
        /// </summary>
        /// <param name="text">판별할 텍스트</param>
        /// <returns>영어 여부</returns>
        public static bool IsEnglish(string text) {
            return EnglishPattern.IsMatch(text);
        }

        /// <summary>
        /// 일본어 문자인지 판별 (히라가나, 가타카나, 한자)
        /// </summary>
        /// <param name="c">판별할 문자</param>
        /// <returns>일본어 여부</returns>
        public static bool IsJapanese(char c) {
            return (c >= kHiraganaStart && c <= kHiraganaEnd) ||
                   (c >= kKatakanaStart && c <= kKatakanaEnd) ||
                   (c >= kKanjiStart && c <= kKanjiEnd);
        }

        /// <summary>
        /// 숫자인지 판별
        /// </summary>
        /// <param name="text">판별할 텍스트</param>
        /// <returns>숫자 여부</returns>
        public static bool IsNumber(string text) {
            return NumberPattern.IsMatch(text);
        }

        /// <summary>
        /// 한글 문자가 받침을 가지고 있는지 판별
        /// </summary>
        /// <param name="c">판별할 한글 문자</param>
        /// <returns>받침 존재 여부</returns>
        public static bool HasBatchim(char c) {
            if (!IsKorean(c))
                return false;

            // 한글 유니코드 계산식으로 종성(받침) 존재 여부 확인
            int code = c - kHangulStart;
            int finalConsonant = code % 28;

            return finalConsonant != 0;
        }

        /// <summary>
        /// 숫자의 받침 여부 판별
        /// </summary>
        /// <param name="number">판별할 숫자</param>
        /// <returns>받침 존재 여부</returns>
        public static bool NumberHasBatchim(string number) {
            if (string.IsNullOrEmpty(number))
                return false;

            char lastChar = number[^1];

            // 숫자별 받침 여부
            switch (lastChar) {
                case '0': // 영, 공
                case '1': // 일
                case '3': // 삼
                case '6': // 육
                case '7': // 칠
                case '8': // 팔
                    return true;

                case '2': // 이
                case '4': // 사
                case '5': // 오
                case '9': // 구

                default:
                    return false;
            }
        }
    }
}