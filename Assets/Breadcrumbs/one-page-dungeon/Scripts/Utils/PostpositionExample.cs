using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace Breadcrumbs.Utils {
    public class PostpositionExample : MonoBehaviour {
        [SerializeField]
        private TMP_Text outputText;

        // 테스트할 단어들
        private readonly List<string> _testWords = new() {
            "철수",
            "철수0",
            "철수1",
            "철수3",
            "철수4",
            "철수5",
            "철수6",
            "철수7",
            "철수8",
            "철수9",
            "상철",
            "사과",
            "책상",
            "컴퓨터",
            "의자",
            "リンゴ",// 사과
            "机",// 책상
            "コンピュータ", // 컴퓨터
            "椅子",     // 의자
            "Apple",
            "Desk",
            "Computer",
            "Chair",
            "사람a",
            "AnotherWord",
            "슛팜b"
        };

        void Start() {
            // 커스텀 테스트
            TestCustom();

            // 메시지 처리 테스트
            TestMessageProcessing();

            // 키워드 맵 테스트
            TestKeywordMap();
        }

        private void TestCustom() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 커스텀 테스트 (Subject Only) ===\n");
            foreach (var word in _testWords) {
                sb.AppendLine(PostpositionProcessor.ApplyPostposition(word, "은/는"));
            }
            // 결과 출력
            Debug.Log(sb.ToString());
        }

        private void TestMessageProcessing() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 메시지 처리 테스트 ===\n");

            // 한국어 메시지 테스트
            string koreanMessage = "안녕하세요, {철수:은/는} 학생입니다. {학교:으로/로} 갑니다. " +
                                   "{선생님:과/와} 함께 {공부:을/를} 합니다. {책상:이/가} 있습니다.";
            sb.AppendLine("한국어 원본: " + koreanMessage);
            sb.AppendLine("처리 결과: " + PostpositionProcessor.ProcessMessage(koreanMessage));
            sb.AppendLine();

            // 일본어 메시지 테스트
            string japaneseMessage = "こんにちは、{タロウ:は} 学生です。{学校:へ} 行きます。" +
                                     "{先生:と} 一緒に {勉強:を} します。{机:が} あります。";
            sb.AppendLine("일본어 원본: " + japaneseMessage);
            sb.AppendLine("처리 결과: " + PostpositionProcessor.ProcessMessage(japaneseMessage));
            sb.AppendLine();

            // 영어 메시지 테스트
            string englishMessage = "안녕, {John:은/는} 학생이야. 그는 {school:로} 가고 있어. " +
                                    "그는 {book:을/를} 가이고 있어. {teacher:With}. {John:Possessive} desk is here.";
            sb.AppendLine("영어 원본: " + englishMessage);
            sb.AppendLine("처리 결과: " + PostpositionProcessor.ProcessMessage(englishMessage));

            // 결과 출력
            Debug.Log(sb.ToString());

            if (outputText != null)
                outputText.text += "\n\n" + sb.ToString();
        }

        private void TestKeywordMap() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 키워드 맵 테스트 ===\n");

            // 키워드 맵 설정
            Dictionary<string, string> keywordMap = new Dictionary<string, string> {
                { "userName", "홍길동" },
                { "itemName", "책" },
                { "location", "서울" },
                { "userJP", "タナカ" },
                { "itemJP", "本" },
                { "userEN", "John" },
                { "itemEN", "book" }
            };

            // 키워드 맵 적용 테스트
            string template = "{userName}님이 {itemName:을/를} 구매했습니다. {location:에서} 배송됩니다.";
            sb.AppendLine("한국어 템플릿: " + template);
            sb.AppendLine("처리 결과: " + PostpositionProcessor.ProcessMessage(template, keywordMap));
            sb.AppendLine();

            // 일본어 템플릿
            string templateJP = "{userJP}さんが {itemJP:を} 購入しました。";
            sb.AppendLine("일본어 템플릿: " + templateJP);
            sb.AppendLine("처리 결과: " + PostpositionProcessor.ProcessMessage(templateJP, keywordMap));
            sb.AppendLine();

            // 영어 템플릿
            string templateEN = "{userEN} purchased {itemEN}. {userEN:Possessive} delivery location is set.";
            sb.AppendLine("영어 템플릿: " + templateEN);
            sb.AppendLine("처리 결과: " + PostpositionProcessor.ProcessMessage(templateEN, keywordMap));

            // 결과 출력
            Debug.Log(sb.ToString());

            if (outputText != null)
                outputText.text += "\n\n" + sb.ToString();
        }
    }
}