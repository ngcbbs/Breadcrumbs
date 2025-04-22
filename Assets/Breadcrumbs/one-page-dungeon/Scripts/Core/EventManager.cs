using System;
using System.Collections.Generic;

namespace Breadcrumbs.Core {
    /// <summary>
    /// 글로벌 이벤트 시스템 - 컴포넌트 간 결합도를 낮추기 위한 이벤트 기반 통신
    /// </summary>
    public static class EventManager {
        private static readonly Dictionary<string, Action<object>> _eventDictionary =
            new Dictionary<string, Action<object>>();

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        public static void Subscribe(string eventName, Action<object> listener) {
            if (_eventDictionary.TryGetValue(eventName, out var thisEvent)) {
                thisEvent += listener;
                _eventDictionary[eventName] = thisEvent;
            } else {
                _eventDictionary[eventName] = listener;
            }
        }

        /// <summary>
        /// 구독 해제
        /// </summary>
        public static void Unsubscribe(string eventName, Action<object> listener) {
            if (_eventDictionary.TryGetValue(eventName, out var thisEvent)) {
                thisEvent -= listener;
                _eventDictionary[eventName] = thisEvent;
            }
        }

        /// <summary>
        /// 이벤트 발생
        /// </summary>
        public static void Trigger(string eventName, object data = null) {
            if (_eventDictionary.TryGetValue(eventName, out var thisEvent)) {
                thisEvent?.Invoke(data);
            }
        }

        /// <summary>
        /// 모든 이벤트 초기화 (주로 테스트나 씬 전환 용도)
        /// </summary>
        public static void Reset() {
            _eventDictionary.Clear();
        }
    }
}