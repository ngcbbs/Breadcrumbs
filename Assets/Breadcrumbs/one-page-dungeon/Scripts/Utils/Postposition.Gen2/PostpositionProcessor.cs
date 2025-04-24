using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#endif

namespace Breadcrumbs.Utils.Gen2
{
    /// <summary>
    /// 한국어 조사 처리 유틸리티 클래스
    /// </summary>
    public static class PostpositionProcessor
    {
        // 정규식 패턴 (문자열 패턴 {키워드:조사} 형식)
        private static readonly Regex PatternRegex = new Regex(@"\{([^:{}]+):([^:{}]+)\}", RegexOptions.Compiled);
        
        // 언어별 조사 처리 전략 캐시
        private static readonly Dictionary<LanguageType, IPostpositionStrategy> StrategyCache = 
            new Dictionary<LanguageType, IPostpositionStrategy>();
        
        // 조사 타입별 기본 조사 문자열 매핑
        private static readonly Dictionary<PostpositionType, string> PostpositionStringMap = 
            new Dictionary<PostpositionType, string>
        {
            { PostpositionType.Subject, "은/는" },
            { PostpositionType.SubjectAlternative, "이/가" },
            { PostpositionType.Object, "을/를" },
            { PostpositionType.Conjunction, "과/와" },
            { PostpositionType.Adverbial, "으로/로" },
            { PostpositionType.To, "이에게/에게" },
            { PostpositionType.Vocative, "아/야" }
        };
        
        /// <summary>
        /// 단어에 조사를 적용하여 반환
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postposition">적용할 조사 (예: "은/는")</param>
        /// <returns>조사가 적용된 단어</returns>
        public static string ApplyPostposition(string word, string postposition)
        {
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(postposition))
                return word;
                
            try
            {
                // 단어의 마지막 문자 언어 유형 확인
                LanguageType languageType = CharacterClassifier.GetLanguageType(word);
                
                // 해당 언어에 맞는 전략 가져오기
                IPostpositionStrategy strategy = GetStrategy(languageType);
                
                // 해당 전략을 사용하여 조사 적용
                return strategy.ApplyPostposition(word, postposition);
            }
            catch (Exception ex)
            {
                // 예외 발생 시 원본 단어를 그대로 반환
#if UNITY_2017_1_OR_NEWER
                Debug.LogError($"PostpositionProcessor Error: {ex.Message}");
#else
                System.Diagnostics.Debug.WriteLine($"PostpositionProcessor Error: {ex.Message}");
#endif
                return word;
            }
        }
        
        /// <summary>
        /// 단어에 특정 조사 타입을 적용하여 반환
        /// </summary>
        /// <param name="word">원본 단어</param>
        /// <param name="postpositionType">적용할 조사 타입</param>
        /// <returns>조사가 적용된 단어</returns>
        public static string ApplyPostposition(string word, PostpositionType postpositionType)
        {
            if (string.IsNullOrEmpty(word))
                return word;
                
            try
            {
                // 단어의 마지막 문자 언어 유형 확인
                LanguageType languageType = CharacterClassifier.GetLanguageType(word);
                
                // 해당 언어에 맞는 전략 가져오기
                IPostpositionStrategy strategy = GetStrategy(languageType);
                
                // 해당 전략을 사용하여 조사 타입 적용
                return strategy.ApplyPostposition(word, postpositionType);
            }
            catch (Exception ex)
            {
                // 예외 발생 시 원본 단어와 기본 조사 조합 시도
                try
                {
                    if (PostpositionStringMap.TryGetValue(postpositionType, out string defaultPostposition))
                    {
                        return word + defaultPostposition.Split('/')[1]; // 항상 받침 없는 형태 적용
                    }
                }
                catch { }
                
#if UNITY_2017_1_OR_NEWER
                Debug.LogError($"PostpositionProcessor Error: {ex.Message}");
#else
                System.Diagnostics.Debug.WriteLine($"PostpositionProcessor Error: {ex.Message}");
#endif
                return word;
            }
        }
        
        /// <summary>
        /// 메시지 내의 패턴을 처리하여 조사가 적용된 메시지 반환
        /// </summary>
        /// <param name="message">원본 메시지</param>
        /// <param name="keywordMap">키워드 매핑 사전</param>
        /// <returns>조사가 적용된 메시지</returns>
        public static string ProcessMessage(string message, Dictionary<string, string> keywordMap)
        {
            if (string.IsNullOrEmpty(message) || keywordMap == null || keywordMap.Count == 0)
                return message;
                
            try
            {
                // 정규식 패턴 매칭 및 조사 적용
                return PatternRegex.Replace(message, match =>
                {
                    string keyword = match.Groups[1].Value;
                    string postposition = match.Groups[2].Value;
                    
                    // 키워드 매핑이 존재하면 조사 적용
                    if (keywordMap.TryGetValue(keyword, out string replacement))
                    {
                        return ApplyPostposition(replacement, postposition);
                    }
                    
                    // 매핑이 없으면 원본 패턴 유지
                    return match.Value;
                });
            }
            catch (Exception ex)
            {
#if UNITY_2017_1_OR_NEWER
                Debug.LogError($"PostpositionProcessor Error: {ex.Message}");
#else
                System.Diagnostics.Debug.WriteLine($"PostpositionProcessor Error: {ex.Message}");
#endif
                return message;
            }
        }
        
        /// <summary>
        /// 메시지 처리 (객체 키워드 매핑 버전)
        /// </summary>
        /// <param name="message">원본 메시지</param>
        /// <param name="keywordObject">키워드 객체</param>
        /// <returns>조사가 적용된 메시지</returns>
        public static string ProcessMessage(string message, object keywordObject)
        {
            if (string.IsNullOrEmpty(message) || keywordObject == null)
                return message;
                
            try
            {
                // 객체의 속성들을 Dictionary로 변환
                Dictionary<string, string> keywordMap = new Dictionary<string, string>();
                
                var properties = keywordObject.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var value = property.GetValue(keywordObject);
                    if (value != null)
                    {
                        keywordMap[property.Name] = value.ToString();
                    }
                }
                
                return ProcessMessage(message, keywordMap);
            }
            catch (Exception ex)
            {
#if UNITY_2017_1_OR_NEWER
                Debug.LogError($"PostpositionProcessor Error: {ex.Message}");
#else
                System.Diagnostics.Debug.WriteLine($"PostpositionProcessor Error: {ex.Message}");
#endif
                return message;
            }
        }
        
        /// <summary>
        /// 조사 타입에 해당하는 문자열 조사 가져오기
        /// </summary>
        /// <param name="postpositionType">조사 타입</param>
        /// <returns>문자열 형태의 조사 (예: "은/는")</returns>
        public static string GetPostpositionString(PostpositionType postpositionType)
        {
            if (PostpositionStringMap.TryGetValue(postpositionType, out string postposition))
                return postposition;
                
            return string.Empty;
        }
        
        /// <summary>
        /// 언어 타입에 해당하는 전략 가져오기 (캐싱 적용)
        /// </summary>
        /// <param name="languageType">언어 타입</param>
        /// <returns>조사 처리 전략</returns>
        private static IPostpositionStrategy GetStrategy(LanguageType languageType)
        {
            // 캐시에서 전략 확인
            if (!StrategyCache.TryGetValue(languageType, out IPostpositionStrategy strategy))
            {
                // 캐시에 없으면 팩토리에서 생성
                strategy = PostpositionStrategyFactory.GetStrategy(languageType);
                StrategyCache[languageType] = strategy;
            }
            
            return strategy;
        }
        
        /// <summary>
        /// 조사 처리 캐시 초기화
        /// </summary>
        public static void ClearCache()
        {
            StrategyCache.Clear();
        }
    }
}