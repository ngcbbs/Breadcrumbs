using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Breadcrumbs.Utils.Gen2.Example {
    /// <summary>
    /// PostpositionProcessor 모듈 사용 예제 클래스
    /// </summary>
    public class PostpositionProcessor_Example : MonoBehaviour {
        [SerializeField]
        private TMP_Text outputText;

        // 예제에서 사용할 데이터 클래스
        [System.Serializable]
        public class PlayerData {
            public string name;
            public string weapon;
            public int level;

            public PlayerData(string name, string weapon, int level) {
                this.name = name;
                this.weapon = weapon;
                this.level = level;
            }
        }

        private void Start() {
            // 초기화
            InitializePostpositionProcessor();

            // 테스트 실행
            RunAllExamples();
        }

        private void InitializePostpositionProcessor() {
            // 유니티용 언어 감지기 설정 (선택 사항)
            LanguageDetectorFactory.SetDetector(new UnityLanguageDetector(() => Application.systemLanguage.ToString()));
        }

        private void RunAllExamples() {
            string result = "===== PostpositionProcessor 예제 =====\n\n";

            // 기본 조사 처리 예제
            result += RunBasicExamples();

            // 영어 단어 조사 처리 예제
            result += RunEnglishExamples();

            // 숫자 조사 처리 예제
            result += RunNumberExamples();

            // 메시지 패턴 처리 예제
            result += RunMessagePatternExamples();

            // 출력
            if (outputText != null) {
                outputText.text = result;
            } else {
                Debug.Log(result);
            }
        }

        private string RunBasicExamples() {
            string result = "1. 기본 조사 처리 예제\n";

            // 은/는 조사 처리
            result += $"• 사과 + 은/는 = {PostpositionProcessor.ApplyPostposition("사과", "은/는")}\n";
            result += $"• 바나나 + 은/는 = {PostpositionProcessor.ApplyPostposition("바나나", "은/는")}\n";

            // 이/가 조사 처리
            result += $"• 컴퓨터 + 이/가 = {PostpositionProcessor.ApplyPostposition("컴퓨터", "이/가")}\n";
            result += $"• 의자 + 이/가 = {PostpositionProcessor.ApplyPostposition("의자", "이/가")}\n";

            // 을/를 조사 처리
            result += $"• 책 + 을/를 = {PostpositionProcessor.ApplyPostposition("책", "을/를")}\n";
            result += $"• 연필 + 을/를 = {PostpositionProcessor.ApplyPostposition("연필", "을/를")}\n";

            // 조사 타입 사용
            result += $"• 사과 + Subject = {PostpositionProcessor.ApplyPostposition("사과", PostpositionType.Subject)}\n";
            result += $"• 컴퓨터 + Object = {PostpositionProcessor.ApplyPostposition("컴퓨터", PostpositionType.Object)}\n";

            result += "\n";
            return result;
        }

        private string RunEnglishExamples() {
            string result = "2. 영어 단어 조사 처리 예제\n";

            // 일반 영어 단어
            result += $"• Apple + 은/는 = {PostpositionProcessor.ApplyPostposition("Apple", "은/는")}\n";
            result += $"• Banana + 은/는 = {PostpositionProcessor.ApplyPostposition("Banana", "은/는")}\n";

            // 발음 기반 처리 (받침 있는 발음)
            result += $"• Game + 이/가 = {PostpositionProcessor.ApplyPostposition("Game", "이/가")}\n";
            result += $"• Pen + 이/가 = {PostpositionProcessor.ApplyPostposition("Pen", "이/가")}\n";

            // 발음 기반 처리 (받침 없는 발음)
            result += $"• Photo + 을/를 = {PostpositionProcessor.ApplyPostposition("Photo", "을/를")}\n";
            result += $"• Coffee + 을/를 = {PostpositionProcessor.ApplyPostposition("Coffee", "을/를")}\n";

            // 예외 단어 처리
            result += $"• Apple + 이/가 = {PostpositionProcessor.ApplyPostposition("Apple", "이/가")}\n";
            result += $"• Google + 이/가 = {PostpositionProcessor.ApplyPostposition("Google", "이/가")}\n";

            result += "\n";
            return result;
        }

        private string RunNumberExamples() {
            string result = "3. 숫자 조사 처리 예제\n";

            // 숫자별 조사 처리
            result += $"• 1 + 이/가 = {PostpositionProcessor.ApplyPostposition("1", "이/가")}\n";
            result += $"• 2 + 이/가 = {PostpositionProcessor.ApplyPostposition("2", "이/가")}\n";
            result += $"• 3 + 이/가 = {PostpositionProcessor.ApplyPostposition("3", "이/가")}\n";
            result += $"• 10 + 이/가 = {PostpositionProcessor.ApplyPostposition("10", "이/가")}\n";

            // 숫자 조합 조사 처리
            result += $"• 234 + 을/를 = {PostpositionProcessor.ApplyPostposition("234", "을/를")}\n";
            result += $"• 567 + 을/를 = {PostpositionProcessor.ApplyPostposition("567", "을/를")}\n";

            result += "\n";
            return result;
        }

        private string RunMessagePatternExamples() {
            string result = "4. 메시지 패턴 처리 예제\n";

            // 기본 패턴 처리
            string pattern1 = "안녕하세요, {이름:은/는} 오늘 {활동:을/를} 했습니다.";
            Dictionary<string, string> keywordMap1 = new Dictionary<string, string> {
                { "이름", "홍길동" },
                { "활동", "공부" }
            };
            result += $"• 패턴: \"{pattern1}\"\n";
            result += $"• 결과: \"{PostpositionProcessor.ProcessMessage(pattern1, keywordMap1)}\"\n\n";

            // 영어 단어 포함 패턴 처리
            string pattern2 = "{아이템:을/를} 획득했습니다. {플레이어:이/가} {포인트:을/를} 얻었습니다.";
            Dictionary<string, string> keywordMap2 = new Dictionary<string, string> {
                { "아이템", "Magic Sword" },
                { "플레이어", "Mike" },
                { "포인트", "10" }
            };
            result += $"• 패턴: \"{pattern2}\"\n";
            result += $"• 결과: \"{PostpositionProcessor.ProcessMessage(pattern2, keywordMap2)}\"\n\n";

            // 객체 사용 패턴 처리
            string pattern3 = "{name:은/는} {weapon:을/를} 사용하여 레벨 {level:이/가} 되었습니다.";
            PlayerData playerData = new PlayerData("김용사", "레이저건", 5);
            result += $"• 패턴: \"{pattern3}\"\n";
            result += $"• 결과: \"{PostpositionProcessor.ProcessMessage(pattern3, playerData)}\"\n";

            result += "\n";
            return result;
        }
    }
}