using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Utils.Gen2.Example {
    /// <summary>
    /// PostpositionProcessor 모듈 사용 예제 클래스
    /// </summary>
    public class PostpositionProcessor_SimpleExample : MonoBehaviour {
        private void Start() {
            // 유니티 언어 감지기 설정 (선택사항)
            LanguageDetectorFactory.SetDetector(new UnityLanguageDetector(() => Application.systemLanguage.ToString()));

            // 예제 1: 기본 조사 처리
            var result1 = PostpositionProcessor.ApplyPostposition("사과", "은/는");
            var result2 = PostpositionProcessor.ApplyPostposition("책", "은/는");
            var result3 = PostpositionProcessor.ApplyPostposition("apple", "은/는");

            Debug.Log($"1. 기본 조사 처리: {result1}, {result2}, {result3}");
            // 출력: 1. 기본 조사 처리: 사과는, 책은, apple은

            // 예제 2: 조사 타입 사용
            var result4 = PostpositionProcessor.ApplyPostposition("사과", PostpositionType.Subject);
            var result5 = PostpositionProcessor.ApplyPostposition("책", PostpositionType.Object);

            Debug.Log($"2. 조사 타입 사용: {result4}, {result5}");
            // 출력: 2. 조사 타입 사용: 사과는, 책을

            // 예제 3: 영어 단어 처리
            var result6 = PostpositionProcessor.ApplyPostposition("apple", "이/가");
            var result7 = PostpositionProcessor.ApplyPostposition("computer", "이/가");
            var result8 = PostpositionProcessor.ApplyPostposition("google", "을/를");

            Debug.Log($"3. 영어 단어 처리: {result6}, {result7}, {result8}");
            // 출력: 3. 영어 단어 처리: apple이, computer가, google을

            // 예제 4: 숫자 처리
            var result9 = PostpositionProcessor.ApplyPostposition("1", "이/가");
            var result10 = PostpositionProcessor.ApplyPostposition("2", "이/가");
            var result11 = PostpositionProcessor.ApplyPostposition("10", "을/를");
            var result12 = PostpositionProcessor.ApplyPostposition("10", "은/는");

            Debug.Log($"4. 숫자 처리: {result9}, {result10}, {result11}, {result12}");
            // 출력: 4. 숫자 처리: 1이, 2가, 10을

            // 예제 5: 메시지 패턴 처리
            const string message = "안녕하세요, {이름:은/는} {역할:이/가} {업무:을/를} 담당하고 있습니다.";
            var keywordMap = new Dictionary<string, string> {
                { "이름", "홍길동" },
                { "역할", "개발자" },
                { "업무", "프로그래밍" }
            };

            var processedMessage = PostpositionProcessor.ProcessMessage(message, keywordMap);
            Debug.Log($"5. 메시지 패턴 처리: {processedMessage}");
            // 출력: 5. 메시지 패턴 처리: 안녕하세요, 홍길동은 개발자가 프로그래밍을 담당하고 있습니다.

            // 예제 6: 객체를 이용한 메시지 패턴 처리
            var person = new Person {
                Name = "김철수",
                Job = "디자이너",
                Department = "UI팀"
            };

            var message2 = "{Name:이/가} {Job:으로/로} {Department:에서/에서} 근무합니다.";
            var processedMessage2 = PostpositionProcessor.ProcessMessage(message2, person);

            Debug.Log($"6. 객체 기반 메시지 패턴 처리: {processedMessage2}");
            // 출력: 6. 객체 기반 메시지 패턴 처리: 김철수가 디자이너로 UI팀에서 근무합니다.
        }
    }

    /// <summary>
    /// 예제용 인적 정보 클래스
    /// </summary>
    public class Person {
        public string Name { get; set; }
        public string Job { get; set; }
        public string Department { get; set; }
    }
}