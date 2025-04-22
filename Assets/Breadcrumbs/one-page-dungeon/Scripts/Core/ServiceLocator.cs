using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Core {
    /// <summary>
    /// 서비스 로케이터 패턴 구현 - 싱글톤 패턴을 대체하고 의존성을 명시적으로 관리
    /// </summary>
    public class ServiceLocator {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// 서비스 등록
        /// </summary>
        public static void RegisterService<T>(T service) where T : class {
            _services[typeof(T)] = service;
            Debug.Log($"서비스 등록: {typeof(T).Name}");
        }

        /// <summary>
        /// 서비스 조회
        /// </summary>
        public static T GetService<T>() where T : class {
            if (_services.TryGetValue(typeof(T), out var service)) {
                return (T)service;
            }

            Debug.LogWarning($"서비스를 찾을 수 없음: {typeof(T).Name}");
            return null;
        }

        /// <summary>
        /// 서비스 목록 초기화 (주로 테스트 용도)
        /// </summary>
        public static void Reset() {
            _services.Clear();
        }
    }
}