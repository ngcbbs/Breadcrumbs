using System;
using System.Globalization;

namespace Breadcrumbs.Utils.Gen2 {
    /// <summary>
    /// 언어 감지 인터페이스
    /// </summary>
    public interface ILanguageDetector {
        /// <summary>
        /// 현재 시스템의 언어 코드 반환
        /// </summary>
        /// <returns>ISO 언어 코드 (ko, en, ja 등)</returns>
        string GetCurrentLanguageCode();

        /// <summary>
        /// 현재 시스템이 한국어를 사용하는지 여부
        /// </summary>
        /// <returns>한국어 사용 여부</returns>
        bool IsKoreanCulture();
    }

    /// <summary>
    /// 시스템 문화권 기반 언어 감지 기본 구현체
    /// </summary>
    public class SystemLanguageDetector : ILanguageDetector {
        /// <summary>
        /// 현재 시스템의 언어 코드 반환
        /// </summary>
        /// <returns>ISO 언어 코드 (ko, en, ja 등)</returns>
        public string GetCurrentLanguageCode() {
            return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        /// <summary>
        /// 현재 시스템이 한국어를 사용하는지 여부
        /// </summary>
        /// <returns>한국어 사용 여부</returns>
        public bool IsKoreanCulture() {
            return GetCurrentLanguageCode().Equals("ko", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// 유니티 환경에서 사용할 언어 감지 구현체 (유니티 의존성 주입 방식)
    /// </summary>
    public class UnityLanguageDetector : ILanguageDetector {
        // 유니티 언어 코드를 ISO 언어 코드로 변환하는 델리게이트
        private Func<string> _getCurrentLanguageCode;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="getCurrentLanguageCode">유니티 언어 코드 반환 함수</param>
        public UnityLanguageDetector(Func<string> getCurrentLanguageCode) {
            _getCurrentLanguageCode = getCurrentLanguageCode ?? throw new ArgumentNullException(nameof(getCurrentLanguageCode));
        }

        /// <summary>
        /// 현재 시스템의 언어 코드 반환
        /// </summary>
        /// <returns>ISO 언어 코드 (ko, en, ja 등)</returns>
        public string GetCurrentLanguageCode() {
            return _getCurrentLanguageCode();
        }

        /// <summary>
        /// 현재 시스템이 한국어를 사용하는지 여부
        /// </summary>
        /// <returns>한국어 사용 여부</returns>
        public bool IsKoreanCulture() {
            return GetCurrentLanguageCode().Equals("ko", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// 언어 감지 팩토리 클래스
    /// </summary>
    public static class LanguageDetectorFactory {
        private static ILanguageDetector _instance;

        /// <summary>
        /// 기본 언어 감지기 인스턴스 설정
        /// </summary>
        /// <param name="detector">사용할 언어 감지기</param>
        public static void SetDetector(ILanguageDetector detector) {
            _instance = detector;
        }

        /// <summary>
        /// 현재 언어 감지기 인스턴스 반환
        /// </summary>
        /// <returns>언어 감지기 인스턴스</returns>
        public static ILanguageDetector GetDetector() {
            if (_instance == null) {
                _instance = new SystemLanguageDetector();
            }

            return _instance;
        }
    }
}