using UnityEngine;

namespace Breadcrumbs.EventSystem {
    public class EventBehaviourExample : EventBehaviour {
        protected override void RegisterEventHandlers() {
            Register(typeof(TestEvent1), OnTestEvent1);
        }

        private void TestCall() {
            Dispatch(new TestEvent1 {
                ID = 100,
                Value = 200
            });
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.A))
                TestCall();
        }

        // 여러 객체에 있는 컴포넌트에서 등록 호출 된다면?
        private void OnTestEvent1(IEvent argument) {
            if (argument is TestEvent1 @event) {
                // 이벤트 발생
                Debug.Log($"{@event}");                
            }
        }
    }
}