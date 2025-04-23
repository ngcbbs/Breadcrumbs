// Assets/Tests/EditMode/EventHandlerTests.cs

using NUnit.Framework;
using Breadcrumbs.EventSystem; // EventHandler가 있는 네임스페이스
using System;
// using UnityEngine.TestTools; // Edit Mode에서는 보통 필요 없음

namespace Breadcrumbs.EventSystem.Tests
{
    // --- 테스트용 이벤트 정의 ---
    public struct TestEventA : IEvent { public int Value; }
    public struct TestEventB : IEvent { public string Message; }
    public class TestEventC : IEvent { public object Data; }

    public class EventHandlerTests // [TestFixture]는 선택 사항이지만 명시적으로 붙여도 좋습니다.
    {
        private EventHandler _eventHandler;
        private bool _listenerACalled;
        private IEvent _receivedEventA;
        private int _listenerACallCount;
        // ... (다른 리스너 변수들도 동일하게 선언) ...

        // --- 테스트 리스너 ---
        private void TestListenerA(IEvent @event)
        {
            _listenerACalled = true;
            _listenerACallCount++;
            _receivedEventA = @event;
        }
        // ... (다른 테스트 리스너들도 동일하게 정의) ...

        // --- Setup ---
        [SetUp] // NUnit과 동일
        public void Setup()
        {
            _eventHandler = new EventHandler();
            // ... (플래그 및 카운터 리셋 코드) ...
        }

        // --- 테스트 메서드 ---
        [Test] // NUnit과 동일
        public void Register_NewEventType_ShouldAllowDispatch()
        {
            // Arrange
            var eventToDispatch = new TestEventA { Value = 42 };
            _eventHandler.Register(typeof(TestEventA), TestListenerA);

            // Act
            _eventHandler.Dispatch(eventToDispatch);

            // Assert (NUnit.Framework.Assert 사용)
            Assert.IsTrue(_listenerACalled, "Listener A should have been called.");
            Assert.AreEqual(1, _listenerACallCount, "Listener A should have been called exactly once.");
            Assert.IsNotNull(_receivedEventA, "Listener A should have received an event.");
            Assert.IsInstanceOf<TestEventA>(_receivedEventA, "Received event should be of type TestEventA.");
            Assert.AreEqual(42, ((TestEventA)_receivedEventA).Value, "Received event value mismatch.");
        }

        // ... (이전에 제공된 다른 모든 [Test] 메서드들을 여기에 포함) ...

        [Test]
        public void Unregister_TypeNotRegistered_ShouldThrowException()
        {
            // Arrange
            EventHandler.OnEvent listenerDelegate = TestListenerA;

            // Act & Assert
            // Assert.Throws 사용법은 동일합니다.
            var ex = Assert.Throws<Exception>(() => _eventHandler.Unregister(typeof(TestEventA), listenerDelegate),
                "Unregistering from a type that was never registered should throw.");

            // Assert.That 사용법도 동일합니다.
            Assert.That(ex.Message, Does.Contain($"Type:{nameof(TestEventA)}"));
            Assert.That(ex.Message, Does.Contain("등록된 이벤트를 찾을 수 없습니다"));
        }
    }
}