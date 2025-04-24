using System;
using System.Collections.Generic;

namespace Breadcrumbs.Utils.Gen2 {
    /// <summary>
    /// 조사 처리 전략 인터페이스
    /// </summary>
    public interface IPostpositionStrategy {
        /// <summary>
        /// 단어에 조사 적용
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postposition">적용할 조사 (예: "은/는")</param>
        /// <returns>조사가 적용된 단어</returns>
        string ApplyPostposition(string word, string postposition);

        /// <summary>
        /// 조사 타입으로 조사 적용
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postpositionType">적용할 조사 타입</param>
        /// <returns>조사가 적용된 단어</returns>
        string ApplyPostposition(string word, PostpositionType postpositionType);

        /// <summary>
        /// 해당 전략으로 처리 가능한 언어 타입
        /// </summary>
        /// <returns>처리 가능한 언어 타입</returns>
        LanguageType SupportedLanguageType { get; }
    }

    /// <summary>
    /// 한국어 조사 처리 전략
    /// </summary>
    public class KoreanPostpositionStrategy : IPostpositionStrategy {
        // 한국어 조사 타입별 매핑
        private static readonly Dictionary<PostpositionType, PostpositionPair> PostpositionMap =
            new() {
                { PostpositionType.Subject, new PostpositionPair("은", "는") },
                { PostpositionType.SubjectAlternative, new PostpositionPair("이", "가") },
                { PostpositionType.Object, new PostpositionPair("을", "를") },
                { PostpositionType.Conjunction, new PostpositionPair("과", "와") },
                { PostpositionType.Adverbial, new PostpositionPair("으로", "로") },
                { PostpositionType.To, new PostpositionPair("에게", "에게") },
                { PostpositionType.Vocative, new PostpositionPair("아", "야") }
            };

        /// <summary>
        /// 한국어 단어에 조사 적용
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postposition">적용할 조사 (예: "은/는")</param>
        /// <returns>조사가 적용된 단어</returns>
        public string ApplyPostposition(string word, string postposition) {
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(postposition))
                return word;

            // 받침 여부 확인
            bool hasBatchim = CharacterClassifier.HasBatchim(word[word.Length - 1]);

            // 조사 쌍 분리
            string[] parts = postposition.Split('/');
            if (parts.Length != 2)
                return word + postposition; // 올바른 형식이 아닌 경우 그대로 반환

            // 받침에 따라 조사 선택
            string selectedPostposition = hasBatchim ? parts[0] : parts[1];

            return word + selectedPostposition;
        }

        /// <summary>
        /// 조사 타입으로 한국어 조사 적용
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postpositionType">적용할 조사 타입</param>
        /// <returns>조사가 적용된 단어</returns>
        public string ApplyPostposition(string word, PostpositionType postpositionType) {
            if (string.IsNullOrEmpty(word))
                return word;

            if (!PostpositionMap.TryGetValue(postpositionType, out PostpositionPair pair))
                return word;

            // 받침 여부 확인
            bool hasBatchim = CharacterClassifier.HasBatchim(word[word.Length - 1]);

            // 받침에 따라 조사 선택
            string selectedPostposition = hasBatchim ? pair.WithBatchim : pair.WithoutBatchim;

            return word + selectedPostposition;
        }

        /// <summary>
        /// 해당 전략으로 처리 가능한 언어 타입
        /// </summary>
        public LanguageType SupportedLanguageType => LanguageType.Korean;
    }

    /// <summary>
    /// 영어 조사 처리 전략
    /// </summary>
    public class EnglishPostpositionStrategy : IPostpositionStrategy {
        // 영어 조사 타입별 매핑
        private static readonly Dictionary<PostpositionType, PostpositionPair> PostpositionMap =
            new() {
                { PostpositionType.Subject, new PostpositionPair("은", "는") },
                { PostpositionType.SubjectAlternative, new PostpositionPair("이", "가") },
                { PostpositionType.Object, new PostpositionPair("을", "를") },
                { PostpositionType.Conjunction, new PostpositionPair("과", "와") },
                { PostpositionType.Adverbial, new PostpositionPair("으로", "로") },
                { PostpositionType.To, new PostpositionPair("에게", "에게") },
                { PostpositionType.Vocative, new PostpositionPair("아", "야") }
            };

        /// <summary>
        /// 영어 단어에 조사 적용
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postposition">적용할 조사 (예: "은/는")</param>
        /// <returns>조사가 적용된 단어</returns>
        public string ApplyPostposition(string word, string postposition) {
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(postposition))
                return word;

            // 발음 기반 받침 여부 확인
            bool hasBatchim = EnglishToKoreanConverter.HasBatchim(word);

            // 조사 쌍 분리
            string[] parts = postposition.Split('/');
            if (parts.Length != 2)
                return word + postposition; // 올바른 형식이 아닌 경우 그대로 반환

            // 받침에 따라 조사 선택
            string selectedPostposition = hasBatchim ? parts[0] : parts[1];

            return word + selectedPostposition;
        }

        /// <summary>
        /// 조사 타입으로 영어 조사 적용
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postpositionType">적용할 조사 타입</param>
        /// <returns>조사가 적용된 단어</returns>
        public string ApplyPostposition(string word, PostpositionType postpositionType) {
            if (string.IsNullOrEmpty(word))
                return word;

            if (!PostpositionMap.TryGetValue(postpositionType, out PostpositionPair pair))
                return word;

            // 발음 기반 받침 여부 확인
            bool hasBatchim = EnglishToKoreanConverter.HasBatchim(word);

            // 받침에 따라 조사 선택
            string selectedPostposition = hasBatchim ? pair.WithBatchim : pair.WithoutBatchim;

            return word + selectedPostposition;
        }

        /// <summary>
        /// 해당 전략으로 처리 가능한 언어 타입
        /// </summary>
        public LanguageType SupportedLanguageType => LanguageType.English;
    }

    /// <summary>
    /// 일본어 조사 처리 전략
    /// </summary>
    public class JapanesePostpositionStrategy : IPostpositionStrategy {
        // 일본어 조사 타입별 매핑
        private static readonly Dictionary<PostpositionType, PostpositionPair> PostpositionMap =
            new() {
                { PostpositionType.Subject, new PostpositionPair("은", "는") },
                { PostpositionType.SubjectAlternative, new PostpositionPair("이", "가") },
                { PostpositionType.Object, new PostpositionPair("을", "를") },
                { PostpositionType.Conjunction, new PostpositionPair("과", "와") },
                { PostpositionType.Adverbial, new PostpositionPair("으로", "로") },
                { PostpositionType.To, new PostpositionPair("에게", "에게") },
                { PostpositionType.Vocative, new PostpositionPair("아", "야") }
            };

        /// <summary>
        /// 일본어 단어에 조사 적용 (현재는 모든 일본어 단어를 받침 없음으로 처리)
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postposition">적용할 조사 (예: "은/는")</param>
        /// <returns>조사가 적용된 단어</returns>
        public string ApplyPostposition(string word, string postposition) {
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(postposition))
                return word;

            // 일본어는 모든 경우 받침이 없는 것으로 간주
            bool hasBatchim = false;

            // 일부 일본어 한자는 발음에 따라 받침이 있을 수 있음
            if (CharacterClassifier.IsJapanese(word[^1])) {
                // 일본어 한자 종료 패턴 (받침 있는 경우 처리)
                char lastChar = word[^1];
                // 특정 한자의 받침 여부를 처리하는 로직 (실제로는 더 복잡한 분석이 필요)
                // 여기서는 간단하게 모든 일본어를 받침 없음으로 처리
            }

            // 조사 쌍 분리
            string[] parts = postposition.Split('/');
            if (parts.Length != 2)
                return word + postposition; // 올바른 형식이 아닌 경우 그대로 반환

            // 받침에 따라 조사 선택
            string selectedPostposition = hasBatchim ? parts[0] : parts[1];

            return word + selectedPostposition;
        }

        /// <summary>
        /// 조사 타입으로 일본어 조사 적용
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postpositionType">적용할 조사 타입</param>
        /// <returns>조사가 적용된 단어</returns>
        public string ApplyPostposition(string word, PostpositionType postpositionType) {
            if (string.IsNullOrEmpty(word))
                return word;

            if (!PostpositionMap.TryGetValue(postpositionType, out PostpositionPair pair))
                return word;

            // 일본어는 모든 경우 받침이 없는 것으로 간주
            bool hasBatchim = false;

            // 받침에 따라 조사 선택
            string selectedPostposition = hasBatchim ? pair.WithBatchim : pair.WithoutBatchim;

            return word + selectedPostposition;
        }

        /// <summary>
        /// 해당 전략으로 처리 가능한 언어 타입
        /// </summary>
        public LanguageType SupportedLanguageType => LanguageType.Japanese;
    }

    /// <summary>
    /// 숫자 조사 처리 전략
    /// </summary>
    public class NumberPostpositionStrategy : IPostpositionStrategy {
        // 숫자 조사 타입별 매핑
        private static readonly Dictionary<PostpositionType, PostpositionPair> PostpositionMap =
            new() {
                { PostpositionType.Subject, new PostpositionPair("은", "는") },
                { PostpositionType.SubjectAlternative, new PostpositionPair("이", "가") },
                { PostpositionType.Object, new PostpositionPair("을", "를") },
                { PostpositionType.Conjunction, new PostpositionPair("과", "와") },
                { PostpositionType.Adverbial, new PostpositionPair("으로", "로") },
                { PostpositionType.To, new PostpositionPair("에게", "에게") },
                { PostpositionType.Vocative, new PostpositionPair("아", "야") }
            };

        /// <summary>
        /// 숫자에 조사 적용
        /// </summary>
        /// <param name="word">원본 숫자</param>
        /// <param name="postposition">적용할 조사 (예: "은/는")</param>
        /// <returns>조사가 적용된 숫자</returns>
        public string ApplyPostposition(string word, string postposition) {
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(postposition))
                return word;

            // 숫자의 마지막 자리 받침 여부 확인
            bool hasBatchim = CharacterClassifier.NumberHasBatchim(word);

            // 조사 쌍 분리
            string[] parts = postposition.Split('/');
            if (parts.Length != 2)
                return word + postposition; // 올바른 형식이 아닌 경우 그대로 반환

            // 받침에 따라 조사 선택
            string selectedPostposition = hasBatchim ? parts[0] : parts[1];

            return word + selectedPostposition;
        }

        /// <summary>
        /// 조사 타입으로 숫자 조사 적용
        /// </summary>
        /// <param name="word">원본 숫자</param>
        /// <param name="postpositionType">적용할 조사 타입</param>
        /// <returns>조사가 적용된 숫자</returns>
        public string ApplyPostposition(string word, PostpositionType postpositionType) {
            if (string.IsNullOrEmpty(word))
                return word;

            if (!PostpositionMap.TryGetValue(postpositionType, out PostpositionPair pair))
                return word;

            // 숫자의 마지막 자리 받침 여부 확인
            bool hasBatchim = CharacterClassifier.NumberHasBatchim(word);

            // 받침에 따라 조사 선택
            string selectedPostposition = hasBatchim ? pair.WithBatchim : pair.WithoutBatchim;

            return word + selectedPostposition;
        }

        /// <summary>
        /// 해당 전략으로 처리 가능한 언어 타입
        /// </summary>
        public LanguageType SupportedLanguageType => LanguageType.Number;
    }

    /// <summary>
    /// 기본 조사 처리 전략 (알 수 없는 언어 타입에 대한 대비책)
    /// </summary>
    public class DefaultPostpositionStrategy : IPostpositionStrategy {
        /// <summary>
        /// 기본 조사 적용 (항상 받침 없는 형태 사용)
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postposition">적용할 조사 (예: "은/는")</param>
        /// <returns>조사가 적용된 단어</returns>
        public string ApplyPostposition(string word, string postposition) {
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(postposition))
                return word;

            // 조사 쌍 분리
            string[] parts = postposition.Split('/');
            if (parts.Length != 2)
                return word + postposition; // 올바른 형식이 아닌 경우 그대로 반환

            // 항상 받침 없는 형태 사용
            return word + parts[1];
        }

        /// <summary>
        /// 조사 타입으로 기본 조사 적용
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postpositionType">적용할 조사 타입</param>
        /// <returns>조사가 적용된 단어</returns>
        public string ApplyPostposition(string word, PostpositionType postpositionType) {
            if (string.IsNullOrEmpty(word))
                return word;

            // 기본 조사 선택 (항상 받침 없는 형태 사용)
            string selectedPostposition;

            switch (postpositionType) {
                case PostpositionType.Subject:
                    selectedPostposition = "는";
                    break;
                case PostpositionType.SubjectAlternative:
                    selectedPostposition = "가";
                    break;
                case PostpositionType.Object:
                    selectedPostposition = "를";
                    break;
                case PostpositionType.Conjunction:
                    selectedPostposition = "와";
                    break;
                case PostpositionType.Adverbial:
                    selectedPostposition = "로";
                    break;
                case PostpositionType.To:
                    selectedPostposition = "에게";
                    break;
                case PostpositionType.Vocative:
                    selectedPostposition = "야";
                    break;
                default:
                    return word;
            }

            return word + selectedPostposition;
        }

        /// <summary>
        /// 해당 전략으로 처리 가능한 언어 타입
        /// </summary>
        public LanguageType SupportedLanguageType => LanguageType.Unknown;
    }

    /// <summary>
    /// 조사 처리 전략 팩토리
    /// </summary>
    public static class PostpositionStrategyFactory {
        // 언어별 전략 인스턴스
        private static readonly Dictionary<LanguageType, IPostpositionStrategy> Strategies
            = new() {
                { LanguageType.Korean, new KoreanPostpositionStrategy() },
                { LanguageType.English, new EnglishPostpositionStrategy() },
                { LanguageType.Japanese, new JapanesePostpositionStrategy() },
                { LanguageType.Number, new NumberPostpositionStrategy() },
                { LanguageType.Unknown, new DefaultPostpositionStrategy() }
            };

        /// <summary>
        /// 언어 타입에 따른 전략 가져오기
        /// </summary>
        /// <param name="languageType">언어 타입</param>
        /// <returns>해당 언어의 조사 처리 전략</returns>
        public static IPostpositionStrategy GetStrategy(LanguageType languageType) {
            return Strategies.TryGetValue(languageType, out var strategy) ? strategy : Strategies[LanguageType.Unknown];
        }
    }
}