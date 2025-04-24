using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Breadcrumbs.Utils {
    /// <summary>
    /// 다국어 조사 처리 모듈 (한국어/영어/일본어)
    /// </summary>
    public static class PostpositionProcessor {
        #region 모음 여부 확인
        
        /// <summary>
        /// 한글 문자가 받침을 가지는지 확인합니다.
        /// </summary>
        private static bool HasKoreanFinalConsonant(char c) {
            // 한글 유니코드 범위 확인
            if (c < 0xAC00 || c > 0xD7A3)
                return false;

            // 종성 값 계산 (0이면 받침 없음)
            return (c - 0xAC00) % 28 != 0;
        }
            
        /// <summary>
        /// 영어 모음 여부를 확인합니다.
        /// </summary>
        private static bool IsVowel(char c) {
            return "aeiou013678".Contains(char.ToLower(c));
        }
        
        #endregion

        #region 언어 감지 유틸리티

        /// <summary>
        /// 유니티의 시스템 언어를 감지하여 언어 타입을 반환합니다.
        /// </summary>
        public static LanguageType DetectSystemLanguage() {
            return UnityEngine.Application.systemLanguage switch {
                UnityEngine.SystemLanguage.Korean => LanguageType.Korean,
                UnityEngine.SystemLanguage.Japanese => LanguageType.Japanese,
                UnityEngine.SystemLanguage.English => LanguageType.English,
                _ => LanguageType.Unknown
            };
        }

        /// <summary>
        /// 문자열의 주요 언어를 감지합니다.
        /// </summary>
        /// <param name="text">감지할 문자열</param>
        /// <returns>감지된 언어 타입</returns>
        public static LanguageType DetectLanguage(string text) {
            if (string.IsNullOrEmpty(text))
                return LanguageType.Unknown;

            var koreanCount = 0;
            var japaneseCount = 0;
            var englishCount = 0;

            foreach (char c in text) {
                if (IsKorean(c))
                    koreanCount++;
                else if (IsJapanese(c))
                    japaneseCount++;
                else if (IsEnglish(c))
                    englishCount++;
            }

            if (koreanCount > japaneseCount && koreanCount > englishCount)
                return LanguageType.Korean;
            else if (japaneseCount > koreanCount && japaneseCount > englishCount)
                return LanguageType.Japanese;
            else if (englishCount > koreanCount && englishCount > japaneseCount)
                return LanguageType.English;
            else
                return LanguageType.Unknown;
        }

        /// <summary>
        /// 한글 문자인지 확인합니다.
        /// </summary>
        private static bool IsKorean(char c) {
            var code = (int)c;
            // 한글 유니코드 범위: AC00-D7A3(완성형), 1100-11FF(자음, 모음)
            return code is >= 0xAC00 and <= 0xD7A3 or >= 0x1100 and <= 0x11FF;
        }

        /// <summary>
        /// 일본어 문자인지 확인합니다.
        /// </summary>
        private static bool IsJapanese(char c) {
            var code = (int)c;
            // 일본어 유니코드 범위: 3040-30FF(히라가나, 가타카나), 3400-4DBF(한자), 4E00-9FFF(한자)
            return code is >= 0x3040 and <= 0x30FF or >= 0x3400 and <= 0x4DBF or >= 0x4E00 and <= 0x9FFF;
        }

        /// <summary>
        /// 영어 문자인지 확인합니다.
        /// </summary>
        private static bool IsEnglish(char c) {
            var code = (int)c;
            // 영어 유니코드 범위: 0041-005A(대문자), 0061-007A(소문자)
            return code is >= 0x0041 and <= 0x005A or >= 0x0061 and <= 0x007A;
        }
        
        /// <summary>
        /// 숫자 문자인지 확인합니다.
        /// </summary>
        private static bool IsNumeric(char c) {
            return c is >= '0' and <= '9';
        }

        #endregion

        /// <summary>
        /// 조사 타입을 적용하여 단어를 변환합니다.
        /// </summary>
        public static string ApplyPostposition(string word, string customPostposition) {
            if (!customPostposition.Contains("/"))
                return customPostposition;

            var options = customPostposition.Split('/');
            if (options.Length != 2)
                return customPostposition;

            var lastChar = word[^1];
            var hasJongseong = false;
            if (IsKorean(lastChar))
                hasJongseong = HasKoreanFinalConsonant(lastChar);
            else if (IsEnglish(lastChar))
                hasJongseong = !IsVowel(lastChar);
            else if (IsNumeric(lastChar))
                hasJongseong = "013678".Contains(lastChar);
            return word + (hasJongseong ? options[0] : options[1]);
        }

        #region 긴 메시지 처리

        /// <summary>
        /// 긴 메시지 내에서 조사 패턴을 찾아 처리합니다.
        /// 패턴: {단어:조사타입}
        /// 예: "안녕하세요, {철수:Subject} 학생입니다."
        /// </summary>
        /// <param name="message">처리할 메시지</param>
        /// <param name="keywordMap">키워드 맵 (null 허용)</param>
        /// <returns>조사가 적용된 메시지</returns>
        public static string ProcessMessage(string message, Dictionary<string, string> keywordMap = null) {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            const string pattern = @"\{([^:]+):([^}]+)\}";
            return Regex.Replace(message, pattern, match => {
                var key = match.Groups[1].Value;
                var word = (keywordMap != null && keywordMap.TryGetValue(key, out var value) ? value : key);
                var postpositionTypeStr = match.Groups[2].Value;
                return ApplyPostposition(word, postpositionTypeStr);
            });
        }

        #endregion
    }

    /// <summary>
    /// 지원하는 언어 타입
    /// </summary>
    public enum LanguageType {
        Unknown,
        Korean,
        Japanese,
        English,
    }
}