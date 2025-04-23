using System;
using System.Collections.Generic;
using System.Linq;

namespace Breadcrumbs.EventSystem {
    public sealed class EventHandler {
        public delegate void OnEvent(IEvent @event);
        private readonly Dictionary<Type, OnEvent> _events = new(2048);

        public void Register(Type type, OnEvent action) {
            if (_events.TryAdd(type, action))
                return;

            if (_events[type].GetInvocationList().Contains(action))
                return;

            _events[type] += action;
        }

        /*
        public void Dispatch<T>(T @event) where T : IEvent {
            if (_events.TryGetValue(typeof(T), out var action)) {
                action.Invoke(@event);
            }
        }
        // */

        public void Dispatch(IEvent @event) {
            var type = @event.GetType();
            if (_events.TryGetValue(type, out var action)) {
                action.Invoke(@event);
            }
        }

        public void Unregister(Type type, OnEvent action) {
            if (!_events.ContainsKey(type))
                throw new Exception($"Type:{type.Name}, 으로 등록된 이벤트를 찾을 수 없습니다.");

            _events[type] -= action;
            if (_events[type] is null || _events[type].GetInvocationList().Length == 0) {
                _events.Remove(type);
            }
        }

        /*
        public void Destroy() {
            foreach (var kvp in _events) {
                var type = kvp.Key;
                var action = kvp.Value;
                foreach (var it in action.GetInvocationList()) {
                    action -= (OnEvent)it;
                }

                _events.Remove(type);
            }

            _events.Clear();
        }
        // */
    }
}