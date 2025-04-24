using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Breadcrumbs.Utils.Gen2
{
    /// <summary>
    /// 영어 단어의 한글 발음 변환 및 분석 유틸리티
    /// </summary>
    public static class EnglishToKoreanConverter
    {
        // 발음에 따른 영어 단어 받침 여부 예외 사전
        private static readonly Dictionary<string, bool> ExceptionDictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            // 받침이 있는 발음으로 처리할 단어들
            { "apple", true },
            { "google", true },
            { "people", true },
            { "bubble", true },
            { "example", true },
            { "uncle", true },
            { "handle", true },
            { "simple", true },
            { "available", true },
            { "acceptable", true },
            { "comfortable", true },
            
            // 받침이 없는 발음으로 처리할 단어들
            { "site", false },
            { "ride", false },
            { "type", false },
            { "make", false },
            { "take", false },
            { "rice", false },
            { "face", false },
            { "case", false },
            { "place", false },
            { "price", false },
            { "race", false }
        };

        // 받침이 있는 발음으로 끝나는 단어 패턴
        private static readonly Regex HasBatchimPattern = new Regex(@"(?i)(ion|[bcdfghjklmnpqrstvwxz]|[^aeiouy]e|ome)$", RegexOptions.Compiled);

        // 받침이 없는 발음으로 끝나는 단어 패턴
        private static readonly Regex NoBatchimPattern = new Regex(@"(?i)([aeiou]|y|ed|ue|ee|ie|oe|ye|th)$", RegexOptions.Compiled);

        /// <summary>
        /// 영어 단어의 발음 기반으로 받침 여부 판별
        /// </summary>
        /// <param name="word">영어 단어</param>
        /// <returns>받침 존재 여부</returns>
        public static bool HasBatchim(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;
                
            word = word.Trim().ToLowerInvariant();
            
            // 예외 단어 먼저 확인
            if (ExceptionDictionary.TryGetValue(word, out bool hasBatchim))
                return hasBatchim;
            
            // 단어 길이가 1인 경우, 자음이면 받침 있음, 모음이면 받침 없음
            if (word.Length == 1)
            {
                char c = word[0];
                return IsConsonant(c);
            }
            
            // 유명한 접미사 패턴 확인
            if (word.EndsWith("tion", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("sion", StringComparison.OrdinalIgnoreCase) ||
                word.EndsWith("cian", StringComparison.OrdinalIgnoreCase))
                return true;
                
            if (word.EndsWith("ed", StringComparison.OrdinalIgnoreCase) &&
                word.Length > 2 && IsConsonant(word[word.Length - 3]))
                return false;
                
            // 문자열 끝이 le인 경우 (발음이 "을"로 끝남)
            if (word.EndsWith("le", StringComparison.OrdinalIgnoreCase) && 
                word.Length > 2 && IsConsonant(word[word.Length - 3]))
                return true;
                
            // 정규식 패턴 확인
            if (HasBatchimPattern.IsMatch(word))
                return true;
                
            if (NoBatchimPattern.IsMatch(word))
                return false;
            
            // 마지막 문자 확인
            return IsConsonant(word[word.Length - 1]);
        }
        
        /// <summary>
        /// 영어 자음인지 판별
        /// </summary>
        /// <param name="c">판별할 문자</param>
        /// <returns>자음 여부</returns>
        private static bool IsConsonant(char c)
        {
            c = char.ToLowerInvariant(c);
            return c != 'a' && c != 'e' && c != 'i' && c != 'o' && c != 'u';
        }
        
        /// <summary>
        /// 영어 모음인지 판별
        /// </summary>
        /// <param name="c">판별할 문자</param>
        /// <returns>모음 여부</returns>
        private static bool IsVowel(char c)
        {
            c = char.ToLowerInvariant(c);
            return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u';
        }
    }
}