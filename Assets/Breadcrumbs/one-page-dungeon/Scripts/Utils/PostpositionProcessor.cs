using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Breadcrumbs.Utils {
    /// <summary>
    /// 다국어 조사 처리 모듈 (한국어/영어/일본어)
    /// Multilingual Postposition Processor for Unity
    /// </summary>
    public static class PostpositionProcessor {
        #region 언어 처리기 인터페이스 및 팩토리

        /// <summary>
        /// 언어별 조사 처리 인터페이스
        /// </summary>
        public interface ILanguageProcessor {
            /// <summary>
            /// 단어에 조사를 적용합니다.
            /// </summary>
            /// <param name="word">단어</param>
            /// <param name="postpositionType">조사 타입</param>
            /// <returns>조사가 적용된 단어</returns>
            string ApplyPostposition(string word, PostpositionType postpositionType);

            /// <summary>
            /// 사용자 정의 조사 쌍을 처리합니다.
            /// </summary>
            /// <param name="word">단어</param>
            /// <param name="customPostposition">사용자 정의 조사(예: "은/는")</param>
            /// <returns>적절한 조사</returns>
            string ProcessCustomPostposition(string word, string customPostposition);
        }

        /// <summary>
        /// 언어 처리기 팩토리
        /// </summary>
        private static class LanguageProcessorFactory {
            private static readonly Dictionary<LanguageType, ILanguageProcessor> Processors =
                new Dictionary<LanguageType, ILanguageProcessor> {
                    { LanguageType.Korean, new KoreanProcessor() },
                    { LanguageType.Japanese, new JapaneseProcessor() },
                    { LanguageType.English, new EnglishProcessor() }
                };

            /// <summary>
            /// 언어 타입에 맞는 처리기를 가져옵니다.
            /// </summary>
            public static ILanguageProcessor GetProcessor(LanguageType language) {
                return Processors.TryGetValue(language, out var processor) ? processor : Processors[LanguageType.English];
            }

            /// <summary>
            /// 새로운 언어 처리기를 등록합니다.
            /// </summary>
            public static void RegisterProcessor(LanguageType language, ILanguageProcessor processor) {
                Processors[language] = processor;
            }
        }

        #endregion

        #region 언어별 처리기 구현

        /// <summary>
        /// 한국어 조사 처리기
        /// </summary>
        private class KoreanProcessor : ILanguageProcessor {
            public string ApplyPostposition(string word, PostpositionType postpositionType) {
                if (string.IsNullOrEmpty(word))
                    return string.Empty;

                char lastChar = word[^1];
                bool hasJongseong = HasKoreanFinalConsonant(lastChar);

                switch (postpositionType) {
                    case PostpositionType.Subject:
                        return word + (hasJongseong ? "이" : "가");
                    case PostpositionType.Topic:
                        return word + (hasJongseong ? "은" : "는");
                    case PostpositionType.Object:
                        return word + (hasJongseong ? "을" : "를");
                    case PostpositionType.With:
                        return word + (hasJongseong ? "과" : "와");
                    case PostpositionType.To:
                        return word + (hasJongseong ? "으로" : "로");
                    case PostpositionType.At:
                        return word + "에서";
                    case PostpositionType.From:
                        return word + (hasJongseong ? "으로부터" : "로부터");
                    case PostpositionType.Possessive:
                        return word + "의";
                    default:
                        return word;
                }
            }

            public string ProcessCustomPostposition(string word, string customPostposition) {
                if (!customPostposition.Contains("/"))
                    return customPostposition;

                string[] options = customPostposition.Split('/');
                if (options.Length != 2)
                    return customPostposition;

                char lastChar = word[^1];
                bool hasJongseong = HasKoreanFinalConsonant(lastChar);
                return hasJongseong ? options[0] : options[1];
            }

            /// <summary>
            /// 한글 문자가 받침을 가지는지 확인합니다.
            /// </summary>
            private bool HasKoreanFinalConsonant(char c) {
                // 한글 유니코드 범위 확인
                if (c < 0xAC00 || c > 0xD7A3)
                    return false;

                // 종성 값 계산 (0이면 받침 없음)
                return (c - 0xAC00) % 28 != 0;
            }
        }

        /// <summary>
        /// 일본어 조사 처리기
        /// </summary>
        private class JapaneseProcessor : ILanguageProcessor {
            public string ApplyPostposition(string word, PostpositionType postpositionType) {
                if (string.IsNullOrEmpty(word))
                    return string.Empty;

                switch (postpositionType) {
                    case PostpositionType.Subject:
                        return word + "が"; // ga
                    case PostpositionType.Topic:
                        return word + "は"; // wa
                    case PostpositionType.Object:
                        return word + "を"; // wo
                    case PostpositionType.With:
                        return word + "と"; // to
                    case PostpositionType.To:
                        return word + "へ"; // e
                    case PostpositionType.At:
                        return word + "で"; // de
                    case PostpositionType.From:
                        return word + "から"; // kara
                    case PostpositionType.Possessive:
                        return word + "の"; // no
                    default:
                        return word;
                }
            }

            public string ProcessCustomPostposition(string word, string customPostposition) {
                if (!customPostposition.Contains("/"))
                    return customPostposition;

                string[] options = customPostposition.Split('/');
                if (options.Length != 2)
                    return customPostposition;

                // 일본어는 기본적으로 두 번째 옵션 사용
                return options[1];
            }
        }

        /// <summary>
        /// 영어 조사 처리기
        /// </summary>
        private class EnglishProcessor : ILanguageProcessor {
            public string ApplyPostposition(string word, PostpositionType postpositionType) {
                if (string.IsNullOrEmpty(word))
                    return string.Empty;

                char lastChar = word[^1];

                switch (postpositionType) {
                    case PostpositionType.Subject:
                        return word; // 영어에는 주격 조사가 없음
                    case PostpositionType.Topic:
                        return word + " as for"; // 주제 표시
                    case PostpositionType.Object:
                        return word; // 영어에는 목적격 표시가 없음
                    case PostpositionType.With:
                        return word + " with";
                    case PostpositionType.To:
                        return word + " to";
                    case PostpositionType.At:
                        return word + " at";
                    case PostpositionType.From:
                        return word + " from";
                    case PostpositionType.Possessive:
                        // 단어가 s, x, z로 끝나면 's를, 그렇지 않으면 '를 추가
                        char lowerLastChar = char.ToLower(lastChar);
                        if (lowerLastChar == 's' || lowerLastChar == 'x' || lowerLastChar == 'z')
                            return word + "'";
                        else
                            return word + "'s";
                    default:
                        return word;
                }
            }

            public string ProcessCustomPostposition(string word, string customPostposition) {
                if (!customPostposition.Contains("/"))
                    return customPostposition;

                string[] options = customPostposition.Split('/');
                if (options.Length != 2)
                    return customPostposition;

                char lastChar = word[^1];
                char lowerLastChar = char.ToLower(lastChar);
                bool endsWithConsonant = !IsVowel(lowerLastChar);
                return endsWithConsonant ? options[0] : options[1];
            }

            /// <summary>
            /// 영어 모음 여부를 확인합니다.
            /// </summary>
            private bool IsVowel(char c) {
                return "aeiou".Contains(char.ToLower(c));
            }
        }

        #endregion

        #region 언어 감지 유틸리티

        /// <summary>
        /// 문자열의 주요 언어를 감지합니다.
        /// </summary>
        /// <param name="text">감지할 문자열</param>
        /// <returns>감지된 언어 타입</returns>
        public static LanguageType DetectLanguage(string text) {
            if (string.IsNullOrEmpty(text))
                return LanguageType.Unknown;

            int koreanCount = 0;
            int japaneseCount = 0;
            int englishCount = 0;

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
            int code = (int)c;
            // 한글 유니코드 범위: AC00-D7A3(완성형), 1100-11FF(자음, 모음)
            return code is >= 0xAC00 and <= 0xD7A3 or >= 0x1100 and <= 0x11FF;
        }

        /// <summary>
        /// 일본어 문자인지 확인합니다.
        /// </summary>
        private static bool IsJapanese(char c) {
            int code = (int)c;
            // 일본어 유니코드 범위: 3040-30FF(히라가나, 가타카나), 3400-4DBF(한자), 4E00-9FFF(한자)
            return code is >= 0x3040 and <= 0x30FF or >= 0x3400 and <= 0x4DBF or >= 0x4E00 and <= 0x9FFF;
        }

        /// <summary>
        /// 영어 문자인지 확인합니다.
        /// </summary>
        private static bool IsEnglish(char c) {
            int code = (int)c;
            // 영어 유니코드 범위: 0041-005A(대문자), 0061-007A(소문자)
            return code is >= 0x0041 and <= 0x005A or >= 0x0061 and <= 0x007A;
        }

        #endregion

        #region 단어별 조사 처리

        /// <summary>
        /// 단어에 적절한 조사를 적용합니다.
        /// </summary>
        /// <param name="word">조사가 붙을 단어</param>
        /// <param name="postpositionType">조사 타입</param>
        /// <returns>조사가 적용된 문자열</returns>
        public static string ApplyPostposition(string word, PostpositionType postpositionType) {
            if (string.IsNullOrEmpty(word))
                return string.Empty;

            LanguageType language = DetectLanguage(word);
            ILanguageProcessor processor = LanguageProcessorFactory.GetProcessor(language);

            return processor.ApplyPostposition(word, postpositionType);
        }

        #endregion

        #region 긴 메시지 처리

        /// <summary>
        /// 긴 메시지 내에서 조사 패턴을 찾아 처리합니다.
        /// 패턴: {단어:조사타입}
        /// 예: "안녕하세요, {철수:은} 학생입니다."
        /// </summary>
        /// <param name="message">처리할 메시지</param>
        /// <param name="keywordMap">키워드 맵 (null 허용)</param>
        /// <returns>조사가 적용된 메시지</returns>
        public static string ProcessMessage(string message, Dictionary<string, string> keywordMap = null) {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            // 1. 키워드 맵으로 치환 처리
            string processedMessage = message;
            if (keywordMap is { Count: > 0 }) {
                foreach (var entry in keywordMap) {
                    // {키:값} 패턴의 키워드 치환
                    processedMessage = processedMessage.Replace("{" + entry.Key + "}", entry.Value);
                }
            }

            // 2. 조사 패턴 처리
            string pattern = @"\{([^:]+):([^}]+)\}";
            return Regex.Replace(processedMessage, pattern, match => {
                string word = match.Groups[1].Value;
                string postpositionTypeStr = match.Groups[2].Value.ToLower();

                // 언어 감지
                LanguageType language = DetectLanguage(word);
                ILanguageProcessor processor = LanguageProcessorFactory.GetProcessor(language);

                // 조사 타입 파싱
                if (Enum.TryParse(postpositionTypeStr, true, out PostpositionType postpositionType)) {
                    return processor.ApplyPostposition(word, postpositionType);
                } else {
                    // 사용자 정의 조사 쌍 처리 (예: "은/는")
                    return word + processor.ProcessCustomPostposition(word, postpositionTypeStr);
                }
            });
        }

        #endregion

        #region 언어 처리기 확장 메서드

        /// <summary>
        /// 새로운 언어 처리기를 등록합니다.
        /// </summary>
        /// <param name="language">언어 타입</param>
        /// <param name="processor">언어 처리기</param>
        public static void RegisterLanguageProcessor(LanguageType language, ILanguageProcessor processor) {
            LanguageProcessorFactory.RegisterProcessor(language, processor);
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
        // 추가 언어를 여기에 정의
    }

    /// <summary>
    /// 지원하는 조사 타입
    /// </summary>
    public enum PostpositionType {
        Subject,   // 이/가 (한), が (일), "" (영)
        Topic,     // 은/는 (한), は (일), as for (영)
        Object,    // 을/를 (한), を (일), "" (영)
        With,      // 과/와 (한), と (일), with (영)
        To,        // 으로/로 (한), へ (일), to (영)
        At,        // 에서 (한), で (일), at (영)
        From,      // 으로부터/로부터 (한), から (일), from (영)
        Possessive // 의 (한), の (일), 's (영)
        // 추가 조사 타입을 여기에 정의
    }
}