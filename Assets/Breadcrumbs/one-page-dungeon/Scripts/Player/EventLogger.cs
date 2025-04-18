using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Player {
    public class EventLogger : MonoBehaviour {
        public List<string> loggedEvents = new List<string>();
        public int maxLogCount = 100;
        public bool logInputEvents = true;
        // 필요에 따라 다른 이벤트 로깅 옵션 추가

        void OnEnable() {
            if (logInputEvents) {
                InputManager.OnInput += LogInputEvent;
            }

            ContextData.OnHealthChanged += LogHealthChangeEvent;
            // 다른 Behavior에서 발행하는 이벤트 구독 (예시)
            // MovementBehavior mb = GetComponent<MovementBehavior>();
            // if (mb != null) mb.OnMoveStarted += LogMoveStartEvent;
        }

        void OnDisable() {
            if (logInputEvents) {
                InputManager.OnInput -= LogInputEvent;
            }

            ContextData.OnHealthChanged -= LogHealthChangeEvent;
            // MovementBehavior mb = GetComponent<MovementBehavior>();
            // if (mb != null) mb.OnMoveStarted -= LogMoveStartEvent;
        }

        void LogInputEvent(object sender, InputData input) {
            string log =
                $"[Input Event] Move: {input.IsMoving} / {input.MoveInput}, Attack: {input.attackPressed}, Dash: {input.dashPressed}, Time: {Time.time}";
            LogEvent(log);
        }

        void LogHealthChangeEvent(object sender, ContextData.HealthChangedEventArgs e) {
            string log = $"[Context Event] Health Changed: {e.OldHealth} -> {e.NewHealth}, Time: {Time.time}";
            LogEvent(log);
        }

        // 예시: 이동 시작 이벤트 로깅 (MovementBehavior에 해당 이벤트가 있다고 가정)
        // void LogMoveStartEvent(object sender, EventArgs e)
        // {
        //     string log = $"[Move Event] Move Started, Time: {Time.time}";
        //     LogEvent(log);
        // }

        void LogEvent(string log) {
            loggedEvents.Add(log);
            if (loggedEvents.Count > maxLogCount) {
                loggedEvents.RemoveAt(0);
            }

            Debug.Log(log);
        }

        // 재생 기능 (기본적인 구조 - 실제 구현은 훨씬 복잡할 수 있음)
        public void StartReplay() {
            Debug.Log("Event Replay Started (Not Fully Implemented)");
            // loggedEvents를 순회하며 특정 액션 재현
        }

        public void StopReplay() {
            Debug.Log("Event Replay Stopped (Not Fully Implemented)");
        }
    }
}