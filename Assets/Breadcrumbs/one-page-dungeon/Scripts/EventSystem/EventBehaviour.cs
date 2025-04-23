using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.EventSystem {
    public abstract class EventBehaviour : MonoBehaviour {
        private static EventHandler _handler;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public static EventHandler EventHandler => _handler ??= new EventHandler();

        private readonly List<(Type type, EventHandler.OnEvent handler)> _registeredHandlers = new(64);

        protected void OnEnable() {
            RegisterEventHandlers();
        }

        protected void OnDisable() {
            UnregisterEventHandlers();
        }
        
        protected abstract void RegisterEventHandlers();

        private void UnregisterEventHandlers() {
            foreach (var (type, handler) in _registeredHandlers)
                EventHandler.Unregister(type, handler);
            _registeredHandlers.Clear();
        }

        internal void Register(Type type, EventHandler.OnEvent action) {
            EventHandler.Register(type, action);
            if (!_registeredHandlers.Contains((type, action)))
                _registeredHandlers.Add((type, action));
            else
                Debug.LogWarning($"이미 등록된 이벤트 핸들러입니다. ({type})");
        }

        internal void Dispatch(IEvent @event) {
            EventHandler.Dispatch(@event);
        }
    }
}
