using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace Breadcrumbs.Utils {
    /// <summary>
    /// 다국어 조사 처리 모듈 (한국어/영어/일본어)
    /// </summary>
    public static class PostpositionProcessor {
        #region Constants and Fields

        // 컴파일된 정규식 패턴으로 성능 최적화
        private static readonly Regex PostpositionPattern = new Regex(@"\{([^:]+):([^}]+)\}", RegexOptions.Compiled);

        // 언어별 조사 처리 전략
        private static readonly Dictionary<LanguageType, IPostpositionStrategy> LanguageStrategies;

        // 언어 감지기
        private static ILanguageDetector _languageDetector;

        #endregion

        #region Constructor

        /// <summary>
        /// 정적 생성자에서 전략 초기화
        /// </summary>
        static PostpositionProcessor() {
            LanguageStrategies = new Dictionary<LanguageType, IPostpositionStrategy> {
                { LanguageType.Korean, new KoreanPostpositionStrategy() },
                { LanguageType.English, new EnglishPostpositionStrategy() },
                { LanguageType.Japanese, new JapanesePostpositionStrategy() }
            };

            // 기본 언어 감지기 설정
            _languageDetector = new UnityLanguageDetector();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 언어 감지기를 설정합니다.
        /// </summary>
        /// <param name="detector">사용할 언어 감지기</param>
        public static void SetLanguageDetector(ILanguageDetector detector) {
            _languageDetector = detector ?? throw new ArgumentNullException(nameof(detector));
        }

        /// <summary>
        /// 시스템 언어를 감지하여 언어 타입을 반환합니다.
        /// </summary>
        public static LanguageType DetectSystemLanguage() {
            return _languageDetector.DetectSystemLanguage();
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
                if (CharacterClassifier.IsKorean(c))
                    koreanCount++;
                else if (CharacterClassifier.IsJapanese(c))
                    japaneseCount++;
                else if (CharacterClassifier.IsEnglish(c))
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
        /// 단어와 조사 타입을 받아 적절한 조사를 적용합니다.
        /// </summary>
        /// <param name="word">조사를 적용할 단어</param>
        /// <param name="postpositionType">조사 타입</param>
        /// <returns>조사가 적용된 단어</returns>
        private static string ApplyPostposition(string word, PostpositionType postpositionType) {
            if (string.IsNullOrEmpty(word))
                return word;

            // 조사 타입에 따른 옵션 매핑
            string postpositionOptions = GetPostpositionOptions(postpositionType);
            return ApplyPostposition(word, postpositionOptions);
        }

        /// <summary>
        /// 단어와 조사 옵션을 받아 적절한 조사를 적용합니다.
        /// </summary>
        /// <param name="word">조사를 적용할 단어</param>
        /// <param name="customPostposition">사용자 정의 조사 옵션 (형식: "옵션1/옵션2")</param>
        /// <returns>조사가 적용된 단어</returns>
        public static string ApplyPostposition(string word, string customPostposition) {
            // 입력 검증
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(customPostposition))
                return word;

            if (!customPostposition.Contains("/"))
                return word + customPostposition;

            var options = customPostposition.Split('/');
            if (options.Length != 2)
                return word + customPostposition;

            try {
                var lastChar = word[^1];
                var language = DetectCharLanguage(lastChar);

                // 언어별 전략을 사용하여 조사 적용
                if (LanguageStrategies.TryGetValue(language, out var strategy) && strategy.IsApplicable(lastChar)) {
                    return strategy.Apply(word, options);
                }

                // 알 수 없는 언어 또는 전략이 없는 경우 기본값 적용
                return word + options[0];
            } catch (IndexOutOfRangeException) {
                // 빈 문자열 등 예외 상황 처리
                return word + customPostposition;
            }
        }

        /// <summary>
        /// 긴 메시지 내에서 조사 패턴을 찾아 처리합니다.
        /// 패턴: {단어:조사타입} 또는 {단어:"조사옵션1/조사옵션2"}
        /// 예: "안녕하세요, {철수:은/는} 학생입니다." 또는 "안녕하세요, {철수:Subject} 학생입니다."
        /// </summary>
        /// <param name="message">처리할 메시지</param>
        /// <param name="keywordMap">키워드 맵 (null 허용)</param>
        /// <returns>조사가 적용된 메시지</returns>
        public static string ProcessMessage(string message, Dictionary<string, string> keywordMap = null) {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            return PostpositionPattern.Replace(message, match => {
                var key = match.Groups[1].Value;
                var word = (keywordMap != null && keywordMap.TryGetValue(key, out var value) ? value : key);
                var postpositionTypeStr = match.Groups[2].Value;

                // 조사 타입 열거형으로 변환 시도
                if (Enum.TryParse<PostpositionType>(postpositionTypeStr, out var postpositionType)) {
                    return ApplyPostposition(word, postpositionType);
                }

                // 실패 시 문자열 자체를 조사 옵션으로 사용
                return ApplyPostposition(word, postpositionTypeStr);
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 문자의 언어 타입을 감지합니다.
        /// </summary>
        private static LanguageType DetectCharLanguage(char c) {
            if (CharacterClassifier.IsKorean(c))
                return LanguageType.Korean;
            else if (CharacterClassifier.IsJapanese(c))
                return LanguageType.Japanese;
            else if (CharacterClassifier.IsEnglish(c) || CharacterClassifier.IsNumeric(c))
                return LanguageType.English;
            else
                return LanguageType.Unknown;
        }

        /// <summary>
        /// 조사 타입에 따른 옵션 문자열을 반환합니다.
        /// </summary>
        private static string GetPostpositionOptions(PostpositionType type) {
            return type switch {
                PostpositionType.Subject => "은/는",
                PostpositionType.Object => "을/를",
                PostpositionType.Possessive => "의",
                PostpositionType.With => "과/와",
                PostpositionType.At => "에/에서",
                PostpositionType.To => "으로/로",
                _ => string.Empty
            };
        }

        #endregion
    }

    #region Character Classifier

    /// <summary>
    /// 문자 분류를 위한 유틸리티 클래스
    /// </summary>
    public static class CharacterClassifier {
        /// <summary>
        /// 한글 문자가 받침을 가지는지 확인합니다.
        /// </summary>
        public static bool HasKoreanFinalConsonant(char c) {
            // 한글 유니코드 범위 확인
            if (c < 0xAC00 || c > 0xD7A3)
                return false;

            // 종성 값 계산 (0이면 받침 없음)
            return (c - 0xAC00) % 28 != 0;
        }

        /// <summary>
        /// 모음 여부를 확인합니다. (영어 모음)
        /// </summary>
        public static bool IsEnglishVowel(char c) {
            return "aeiouAEIOU".Contains(c);
        }

        /// <summary>
        /// 숫자가 받침 있는 발음인지 확인합니다.
        /// </summary>
        public static bool IsNumericWithFinalConsonant(char c) {
            return "013678".Contains(c);
        }

        /// <summary>
        /// 한글 문자인지 확인합니다.
        /// </summary>
        public static bool IsKorean(char c) {
            var code = (int)c;
            // 한글 유니코드 범위: AC00-D7A3(완성형), 1100-11FF(자음, 모음)
            return code is >= 0xAC00 and <= 0xD7A3 or >= 0x1100 and <= 0x11FF;
        }

        /// <summary>
        /// 일본어 문자인지 확인합니다.
        /// </summary>
        public static bool IsJapanese(char c) {
            var code = (int)c;
            // 일본어 유니코드 범위: 3040-30FF(히라가나, 가타카나), 3400-4DBF(한자), 4E00-9FFF(한자)
            return code is >= 0x3040 and <= 0x30FF or >= 0x3400 and <= 0x4DBF or >= 0x4E00 and <= 0x9FFF;
        }

        /// <summary>
        /// 영어 문자인지 확인합니다.
        /// </summary>
        public static bool IsEnglish(char c) {
            var code = (int)c;
            // 영어 유니코드 범위: 0041-005A(대문자), 0061-007A(소문자)
            return code is >= 0x0041 and <= 0x005A or >= 0x0061 and <= 0x007A;
        }

        /// <summary>
        /// 숫자 문자인지 확인합니다.
        /// </summary>
        public static bool IsNumeric(char c) {
            return c is >= '0' and <= '9';
        }

        /// <summary>
        /// 일본어 한자 문자인지 확인합니다.
        /// </summary>
        public static bool IsJapaneseKanji(char c) {
            var code = (int)c;
            // 일본어 한자 유니코드 범위: 4E00-9FFF, 3400-4DBF
            return code is >= 0x4E00 and <= 0x9FFF or >= 0x3400 and <= 0x4DBF;
        }

        /// <summary>
        /// 일본어 히라가나 문자인지 확인합니다.
        /// </summary>
        public static bool IsJapaneseHiragana(char c) {
            var code = (int)c;
            // 일본어 히라가나 유니코드 범위: 3040-309F
            return code >= 0x3040 && code <= 0x309F;
        }

        /// <summary>
        /// 일본어 가타카나 문자인지 확인합니다.
        /// </summary>
        public static bool IsJapaneseKatakana(char c) {
            var code = (int)c;
            // 일본어 가타카나 유니코드 범위: 30A0-30FF
            return code >= 0x30A0 && code <= 0x30FF;
        }
    }

    #endregion

    #region Language Interfaces and Implementations

    /// <summary>
    /// 언어 감지 인터페이스
    /// </summary>
    public interface ILanguageDetector {
        /// <summary>
        /// 시스템 언어를 감지합니다.
        /// </summary>
        LanguageType DetectSystemLanguage();
    }

    /// <summary>
    /// 유니티 기반 언어 감지기
    /// </summary>
    public class UnityLanguageDetector : ILanguageDetector {
        /// <summary>
        /// 유니티의 시스템 언어를 감지합니다.
        /// </summary>
        public LanguageType DetectSystemLanguage() {
            return UnityEngine.Application.systemLanguage switch {
                UnityEngine.SystemLanguage.Korean => LanguageType.Korean,
                UnityEngine.SystemLanguage.Japanese => LanguageType.Japanese,
                UnityEngine.SystemLanguage.English => LanguageType.English,
                _ => LanguageType.Unknown
            };
        }
    }

    /// <summary>
    /// 기본 언어 감지기 (유니티 의존성 없음)
    /// </summary>
    public class DefaultLanguageDetector : ILanguageDetector {
        private readonly LanguageType _defaultLanguage;

        /// <summary>
        /// 기본 언어 감지기를 초기화합니다.
        /// </summary>
        /// <param name="defaultLanguage">기본 언어</param>
        public DefaultLanguageDetector(LanguageType defaultLanguage = LanguageType.English) {
            _defaultLanguage = defaultLanguage;
        }

        /// <summary>
        /// 시스템의 현재 문화권을 기반으로 언어를 감지합니다.
        /// </summary>
        public LanguageType DetectSystemLanguage() {
            try {
                // 시스템 문화권을 이용한 언어 감지
                var cultureName = System.Globalization.CultureInfo.CurrentCulture.Name.ToLower();

                if (cultureName.StartsWith("ko"))
                    return LanguageType.Korean;
                else if (cultureName.StartsWith("ja"))
                    return LanguageType.Japanese;
                else if (cultureName.StartsWith("en"))
                    return LanguageType.English;
                else
                    return _defaultLanguage;
            } catch {
                return _defaultLanguage;
            }
        }
    }

    /// <summary>
    /// 조사 처리 전략 인터페이스
    /// </summary>
    public interface IPostpositionStrategy {
        /// <summary>
        /// 단어에 조사를 적용합니다.
        /// </summary>
        /// <param name="word">적용할 단어</param>
        /// <param name="options">조사 옵션 배열</param>
        /// <returns>조사가 적용된 단어</returns>
        string Apply(string word, string[] options);

        /// <summary>
        /// 이 전략이 특정 문자에 적용 가능한지 확인합니다.
        /// </summary>
        /// <param name="c">확인할 문자</param>
        /// <returns>적용 가능 여부</returns>
        bool IsApplicable(char c);
    }

    /// <summary>
    /// 한국어 조사 처리 전략
    /// </summary>
    public class KoreanPostpositionStrategy : IPostpositionStrategy {
        /// <summary>
        /// 한국어 단어에 조사를 적용합니다.
        /// </summary>
        public string Apply(string word, string[] options) {
            if (options.Length != 2)
                return word + string.Join("/", options);

            var lastChar = word[^1];
            var hasJongseong = CharacterClassifier.HasKoreanFinalConsonant(lastChar);

            return word + (hasJongseong ? options[0] : options[1]);
        }

        /// <summary>
        /// 한글 문자인지 확인합니다.
        /// </summary>
        public bool IsApplicable(char c) {
            return CharacterClassifier.IsKorean(c);
        }
    }

    /// <summary>
    /// 영어 조사 처리 전략
    /// </summary>
    public class EnglishPostpositionStrategy : IPostpositionStrategy {
        /// <summary>
        /// 영어 단어에 조사를 적용합니다.
        /// </summary>
        public string Apply(string word, string[] options) {
            if (options.Length != 2)
                return word + string.Join("/", options);

            var info = EnglishToKoreanConverter.AnalyzePronunciation(word);
            var lastChar = word[^1];
            var hasConsonantSound = info?.HasEndingConsonant ?? !CharacterClassifier.IsEnglishVowel(lastChar);

            // 숫자인 경우 특별 처리
            if (CharacterClassifier.IsNumeric(lastChar)) {
                hasConsonantSound = CharacterClassifier.IsNumericWithFinalConsonant(lastChar);
            }

            return word + (hasConsonantSound ? options[0] : options[1]);
        }

        /// <summary>
        /// 영어 문자 또는 숫자인지 확인합니다.
        /// </summary>
        public bool IsApplicable(char c) {
            return CharacterClassifier.IsEnglish(c) || CharacterClassifier.IsNumeric(c);
        }
    }

    /// <summary>
    /// 일본어 조사 처리 전략
    /// </summary>
    public class JapanesePostpositionStrategy : IPostpositionStrategy {
        // 한자 종류별 조사 처리를 위한 매핑
        private readonly Dictionary<string, int> _kanjiEndingTypeMap = new Dictionary<string, int> {
            // 샘플 한자 종류별 조사 처리 규칙 (실제 규칙은 더 많아야 함)
            { "人", 0 }, // 사람 - 'は'
            { "山", 1 }, // 산 - 'が'
            { "国", 0 }  // 나라 - 'は'
        };

        /// <summary>
        /// 일본어 단어에 조사를 적용합니다.
        /// </summary>
        public string Apply(string word, string[] options) {
            if (options.Length != 2)
                return word + string.Join("/", options);

            var lastChar = word[^1];

            // 일본어 문자 종류에 따른 조사 처리
            if (CharacterClassifier.IsJapaneseKanji(lastChar)) {
                // 한자 종류에 따른 조사 선택
                return word + GetKanjiPostposition(lastChar, options);
            } else if (CharacterClassifier.IsJapaneseHiragana(lastChar)) {
                // 히라가나 종류에 따른 조사 선택
                // 예시로 간단하게 구현 (실제로는 더 복잡한 규칙 필요)
                var code = (int)lastChar;
                return word + (code % 2 == 0 ? options[0] : options[1]);
            } else if (CharacterClassifier.IsJapaneseKatakana(lastChar)) {
                // 가타카나 종류에 따른 조사 선택
                // 예시로 간단하게 구현 (실제로는 더 복잡한 규칙 필요)
                var code = (int)lastChar;
                return word + (code % 2 == 1 ? options[0] : options[1]);
            }

            // 알 수 없는 경우 기본값
            return word + options[0];
        }

        /// <summary>
        /// 일본어 문자인지 확인합니다.
        /// </summary>
        public bool IsApplicable(char c) {
            return CharacterClassifier.IsJapanese(c);
        }

        /// <summary>
        /// 한자에 따른 조사를 결정합니다.
        /// </summary>
        private string GetKanjiPostposition(char kanji, string[] options) {
            // 미리 정의된 한자 종류별 조사 매핑 사용
            if (_kanjiEndingTypeMap.TryGetValue(kanji.ToString(), out var type)) {
                return options[type];
            }

            // 매핑에 없는 경우 기본값
            return options[0];
        }
    }

    #endregion

    #region Enums

    /// <summary>
    /// 지원하는 언어 타입
    /// </summary>
    public enum LanguageType {
        Unknown,
        Korean,
        Japanese,
        English,
    }

    /// <summary>
    /// 조사 타입
    /// </summary>
    public enum PostpositionType {
        /// <summary>
        /// 은/는 (주어)
        /// </summary>
        Subject,

        /// <summary>
        /// 을/를 (목적어)
        /// </summary>
        Object,

        /// <summary>
        /// 의 (소유격)
        /// </summary>
        Possessive,

        /// <summary>
        /// 과/와 (접속)
        /// </summary>
        With,

        /// <summary>
        /// 에/에서 (위치)
        /// </summary>
        At,

        /// <summary>
        /// 으로/로 (방향)
        /// </summary>
        To
    }

    #endregion

    #region 영어 단어의 한글 규범 표기

    /// <summary>
    /// 영단어를 한글 규범 표기법으로 변환하는 유틸리티 클래스
    /// </summary>
    public static class EnglishToKoreanConverter {
        #region Constants and Mappings

        // 영어 자음
        private static readonly Dictionary<string, string> InitialConsonants = new Dictionary<string, string> {
            { "b", "ㅂ" }, { "c", "ㅋ" }, { "d", "ㄷ" }, { "f", "ㅍ" }, { "g", "ㄱ" },
            { "h", "ㅎ" }, { "j", "ㅈ" }, { "k", "ㅋ" }, { "l", "ㄹ" }, { "m", "ㅁ" },
            { "n", "ㄴ" }, { "p", "ㅍ" }, { "q", "ㅋ" }, { "r", "ㄹ" }, { "s", "ㅅ" },
            { "t", "ㅌ" }, { "v", "ㅂ" }, { "w", "ㅇ" }, { "x", "ㅋ" }, { "y", "ㅇ" },
            { "z", "ㅈ" }
        };

        // 영어 모음
        private static readonly Dictionary<string, string> Vowels = new Dictionary<string, string> {
            { "a", "ㅏ" }, { "e", "ㅔ" }, { "i", "ㅣ" }, { "o", "ㅗ" }, { "u", "ㅜ" },
            { "ae", "ㅐ" }, { "ai", "ㅐ" }, { "ay", "ㅔ" }, { "ea", "ㅣ" }, { "ee", "ㅣ" },
            { "ie", "ㅣ" }, { "oa", "ㅗ" }, { "oi", "ㅗ" }, { "oo", "ㅜ" }, { "ou", "ㅏ" },
            { "ue", "ㅜ" }, { "ui", "ㅟ" }, { "uy", "ㅏ" }
        };

        // 복합 자음 (digraphs & trigraphs)
        private static readonly Dictionary<string, string> ComplexConsonants = new Dictionary<string, string> {
            { "ch", "ㅊ" }, { "ck", "ㅋ" }, { "gh", "ㄱ" }, { "ng", "ㅇ" }, { "ph", "ㅍ" },
            { "sh", "ㅅ" }, { "th", "ㅅ" }, { "wh", "ㅎ" }, { "wr", "ㄹ" }, { "kn", "ㄴ" },
            { "ps", "ㅅ" }, { "qu", "ㅋㅇ" }, { "sc", "ㅅ" }, { "tch", "ㅊ" }
        };

        // 특별 패턴 (위치에 따라 달라지는 발음)
        private static readonly Dictionary<string, Func<string, int, string>> SpecialPatterns =
            new Dictionary<string, Func<string, int, string>> {
                // 위치, 전체 단어, 다음 글자 등에 따라 달라지는 발음 처리를 위한 함수
                { "c", (word, index) => IsFollowedBySoftVowel(word, index) ? "ㅅ" : "ㅋ" },
                { "g", (word, index) => IsFollowedBySoftVowel(word, index) ? "ㅈ" : "ㄱ" }
            };

        // 각 언어별 특별 사전 (예외 처리)
        private static readonly Dictionary<string, string> SpecialWords = new(StringComparer.OrdinalIgnoreCase) {
            { "apple", "애플" }, { "orange", "오렌지" }, { "computer", "컴퓨터" },
            { "coffee", "커피" }, { "juice", "주스" }, { "phone", "폰" },
            { "cinema", "시네마" }, { "taxi", "택시" }, { "bus", "버스" },
            { "game", "게임" }, { "mouse", "마우스" }, { "file", "파일" },
            { "london", "런던" }, { "paris", "파리" }, { "new york", "뉴욕" },
            { "michael", "마이클" }, { "john", "존" }, { "james", "제임스" }
        };

        // 발음이 명확하지 않은 단어의 마지막 받침 여부
        private static readonly Dictionary<string, bool> WordEndingConsonantMap = new(StringComparer.OrdinalIgnoreCase) {
            { "apple", true }, { "orange", false }, { "computer", false },
            { "coffee", false }, { "juice", true }, { "phone", true },
            { "game", true }, { "mouse", false }, { "file", true },
            { "cake", false }, { "ice", false }, { "knife", false }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// 영어 단어를 한글 규범 표기법으로 변환합니다.
        /// </summary>
        /// <param name="englishWord">변환할 영어 단어</param>
        /// <returns>한글 표기</returns>
        public static string Convert(string englishWord) {
            if (string.IsNullOrEmpty(englishWord))
                return string.Empty;

            // 소문자로 변환하고 공백 제거
            englishWord = englishWord.ToLower().Trim();

            // 예외 단어 사전 확인
            if (SpecialWords.TryGetValue(englishWord, out string specialTranslation))
                return specialTranslation;

            // 단어 분석 및 변환
            StringBuilder result = new StringBuilder();

            // 보다 정확한 변환을 위해 추가 구현이 필요합니다
            // 아래는 단순화된 구현으로 정확한 발음 변환이 아닐 수 있습니다
            for (int i = 0; i < englishWord.Length; i++) {
                // 복합 자음 처리
                bool isHandled = HandleComplexConsonants(englishWord, i, result);
                if (isHandled)
                    continue;

                // 특별 패턴 처리
                isHandled = HandleSpecialPatterns(englishWord, i, result);
                if (isHandled)
                    continue;

                // 단일 문자 처리
                string ch = englishWord[i].ToString();
                if (InitialConsonants.ContainsKey(ch))
                    result.Append(InitialConsonants[ch]);
                else if (Vowels.ContainsKey(ch))
                    result.Append(Vowels[ch]);
                else
                    result.Append(ch); // 특수문자 등 그대로 유지
            }

            // 결과가 한글 자모 형태이므로 완성형으로 변환해야 함
            // (이 부분은 실제로는 더 복잡한 구현이 필요)

            return result.ToString();
        }

        /// <summary>
        /// 영어 단어가 받침 있는 발음으로 끝나는지 확인합니다.
        /// </summary>
        /// <param name="englishWord">확인할 영어 단어</param>
        /// <returns>받침 있음 여부</returns>
        public static bool HasEndingConsonantSound(string englishWord) {
            if (string.IsNullOrEmpty(englishWord))
                return false;

            // 소문자 변환 및 공백 제거
            englishWord = englishWord.ToLower().Trim();

            // 예외 단어 사전에서 확인
            if (WordEndingConsonantMap.TryGetValue(englishWord, out bool hasConsonant))
                return hasConsonant;

            // 일반 규칙 적용
            char lastChar = englishWord[^1];

            // 묵음 e로 끝나는 경우
            if (lastChar == 'e' && englishWord.Length > 1)
                return !IsVowel(englishWord[^2]);

            // 특정 패턴 처리
            if (englishWord.EndsWith("le") || englishWord.EndsWith("el"))
                return true; // apple, angel 등

            if (englishWord.EndsWith("tion") || englishWord.EndsWith("sion"))
                return true; // action, vision 등

            // 기본 규칙: 자음으로 끝나면 받침 있음
            return !IsVowel(lastChar);
        }

        /// <summary>
        /// 영어 발음을 분석하여 한글 조사 적용에 적합한 발음 정보를 반환합니다.
        /// </summary>
        /// <param name="englishWord">분석할 영어 단어</param>
        /// <returns>발음 정보 객체</returns>
        public static PronunciationInfo AnalyzePronunciation(string englishWord) {
            if (string.IsNullOrEmpty(englishWord))
                return new PronunciationInfo { HasEndingConsonant = false, KoreanTranscription = "" };

            // 한글 변환
            string koreanTranscription = Convert(englishWord);

            // 받침 여부
            bool hasEndingConsonant = HasEndingConsonantSound(englishWord);

            return new PronunciationInfo {
                HasEndingConsonant = hasEndingConsonant,
                KoreanTranscription = koreanTranscription
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 문자가 모음인지 확인합니다.
        /// </summary>
        private static bool IsVowel(char c) {
            return "aeiou".Contains(char.ToLower(c));
        }

        /// <summary>
        /// 소프트 모음(e, i, y) 앞에 오는지 확인합니다.
        /// </summary>
        private static bool IsFollowedBySoftVowel(string word, int index) {
            if (index + 1 >= word.Length)
                return false;

            char nextChar = word[index + 1];
            return "eiy".Contains(nextChar);
        }

        /// <summary>
        /// 복합 자음을 처리합니다.
        /// </summary>
        private static bool HandleComplexConsonants(string word, int index, StringBuilder result) {
            foreach (var pair in ComplexConsonants) {
                if (index + pair.Key.Length <= word.Length &&
                    word.Substring(index, pair.Key.Length).Equals(pair.Key, StringComparison.OrdinalIgnoreCase)) {
                    result.Append(pair.Value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 특별 패턴을 처리합니다.
        /// </summary>
        private static bool HandleSpecialPatterns(string word, int index, StringBuilder result) {
            char current = word[index];
            string currentStr = current.ToString();

            if (SpecialPatterns.ContainsKey(currentStr)) {
                result.Append(SpecialPatterns[currentStr](word, index));
                return true;
            }

            return false;
        }

        // 한글 초성, 중성, 종성을 조합하여 완성형 한글로 변환하는 함수
        // (이 부분은 실제 구현이 복잡하여 여기서는 생략됨)

        #endregion
    }

    /// <summary>
    /// 영어 단어의 발음 정보
    /// </summary>
    public class PronunciationInfo {
        /// <summary>
        /// 받침 있는 발음으로 끝나는지 여부
        /// </summary>
        public bool HasEndingConsonant { get; set; }

        /// <summary>
        /// 한글 표기
        /// </summary>
        public string KoreanTranscription { get; set; }
    }


    #endregion
}