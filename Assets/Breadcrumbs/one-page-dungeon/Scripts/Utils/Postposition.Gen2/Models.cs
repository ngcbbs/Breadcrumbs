using System;

namespace Breadcrumbs.Utils.Gen2 {
    /// <summary>
    /// 지원하는 언어 유형
    /// </summary>
    public enum LanguageType {
        Korean,
        English,
        Japanese,
        Number,
        Unknown
    }

    /// <summary>
    /// 한국어 조사 유형
    /// </summary>
    public enum PostpositionType {
        // 은/는 (주격 조사)
        Subject,

        // 이/가 (주격 조사)
        SubjectAlternative,

        // 을/를 (목적격 조사)
        Object,

        // 와/과 (접속 조사)
        Conjunction,

        // 이/히/리/기/으로/로 (부사격 조사)
        Adverbial,

        // 이에게/에게 (부사격 조사)
        To,

        // 이/야 (호격 조사)
        Vocative
    }

    /// <summary>
    /// 조사 쌍을 정의하는 구조체
    /// </summary>
    public readonly struct PostpositionPair {
        // 받침이 있을 때 사용하는 조사
        public readonly string WithBatchim;

        // 받침이 없을 때 사용하는 조사
        public readonly string WithoutBatchim;

        public PostpositionPair(string withBatchim, string withoutBatchim) {
            WithBatchim = withBatchim;
            WithoutBatchim = withoutBatchim;
        }

        public override string ToString() {
            return $"{WithBatchim}/{WithoutBatchim}";
        }
    }
}