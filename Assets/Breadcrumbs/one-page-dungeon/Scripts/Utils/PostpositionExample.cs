using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Breadcrumbs.Utils {
    public class PostpositionExample : MonoBehaviour {
        [SerializeField]
        private TMP_Text outputText;

        // 테스트할 단어들
        private readonly Dictionary<string, LanguageType> _testWords = new Dictionary<string, LanguageType> {
            // 한국어 단어들
            { "사과", LanguageType.Korean },
            { "책상", LanguageType.Korean },
            { "컴퓨터", LanguageType.Korean },
            { "의자", LanguageType.Korean },

            // 일본어 단어들
            { "リンゴ", LanguageType.Japanese },    // 사과
            { "机", LanguageType.Japanese },      // 책상
            { "コンピュータ", LanguageType.Japanese }, // 컴퓨터
            { "椅子", LanguageType.Japanese },     // 의자

            // 영어 단어들
            { "Apple", LanguageType.English },
            { "Desk", LanguageType.English },
            { "Computer", LanguageType.English },
            { "Chair", LanguageType.English }
        };

        void Start() {
            // 단어별 조사 적용 테스트
            TestSingleWordPostpositions();

            // 메시지 처리 테스트
            TestMessageProcessing();

            // 키워드 맵 테스트
            TestKeywordMap();

            // 커스텀 언어 처리기 등록 테스트
            TestCustomLanguageProcessor();
        }

        private void TestSingleWordPostpositions() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 단어별 조사 적용 테스트 ===\n");

            foreach (var wordPair in _testWords) {
                string word = wordPair.Key;
                sb.AppendLine($"[{word}] 단어 테스트:");

                // 주격 조사 (이/가, が, "")
                sb.AppendLine($"  주격: {PostpositionProcessor.ApplyPostposition(word, PostpositionType.Subject)}");

                // 주제 조사 (은/는, は, as for)
                sb.AppendLine($"  주제: {PostpositionProcessor.ApplyPostposition(word, PostpositionType.Topic)}");

                // 목적격 조사 (을/를, を, "")
                sb.AppendLine($"  목적격: {PostpositionProcessor.ApplyPostposition(word, PostpositionType.Object)}");

                // 동반 조사 (과/와, と, with)
                sb.AppendLine($"  동반: {PostpositionProcessor.ApplyPostposition(word, PostpositionType.With)}");

                // 방향 조사 (으로/로, へ, to)
                sb.AppendLine($"  방향: {PostpositionProcessor.ApplyPostposition(word, PostpositionType.To)}");

                // 소유격 조사 (의, の, 's)
                sb.AppendLine($"  소유격: {PostpositionProcessor.ApplyPostposition(word, PostpositionType.Possessive)}\n");
            }

            // 결과 출력
            Debug.Log(sb.ToString());

            if (outputText != null)
                outputText.text = sb.ToString();
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
            string englishMessage = "Hello, {John:TopicMarker} is a student. He goes {school:To}. " +
                                    "He studies {book:ObjectMarker} with {teacher:With}. {John:Possessive} desk is here.";
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

        private void TestCustomLanguageProcessor() {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 커스텀 언어 처리기 테스트 ===\n");

            // 커스텀 언어 처리기 구현
            SpanishProcessor spanishProcessor = new SpanishProcessor();

            // 커스텀 언어 테스트 전 메시지
            string spanishMessage = "Hola, {Juan:SubjectMarker} es un estudiante.";
            sb.AppendLine("스페인어 처리기 등록 전: " + PostpositionProcessor.ProcessMessage(spanishMessage));

            // 새 언어 처리기 등록
            PostpositionProcessor.RegisterLanguageProcessor(LanguageType.Unknown, spanishProcessor);

            // 등록 후 테스트
            sb.AppendLine("스페인어 처리기 등록 후: " + PostpositionProcessor.ProcessMessage(spanishMessage));

            // 결과 출력
            Debug.Log(sb.ToString());

            if (outputText != null)
                outputText.text += "\n\n" + sb.ToString();
        }

        /// <summary>
        /// 스페인어 처리기 예시 - 새 언어 추가 방법 데모용
        /// </summary>
        private class SpanishProcessor : PostpositionProcessor.ILanguageProcessor {
            public string ApplyPostposition(string word, PostpositionType postpositionType) {
                switch (postpositionType) {
                    case PostpositionType.Subject:
                        return word; // 스페인어에는 주격 조사가 없음
                    case PostpositionType.Topic:
                        return "en cuanto a " + word; // 주제 표시
                    case PostpositionType.Object:
                        return word; // 목적격 표시 없음
                    case PostpositionType.With:
                        return "con " + word; // ~와 함께
                    case PostpositionType.To:
                        return "a " + word; // ~로
                    case PostpositionType.At:
                        return "en " + word; // ~에서
                    case PostpositionType.From:
                        return "desde " + word; // ~로부터
                    case PostpositionType.Possessive:
                        return "de " + word; // ~의
                    default:
                        return word;
                }
            }

            public string ProcessCustomPostposition(string word, string customPostposition) {
                if (!customPostposition.Contains("/"))
                    return customPostposition;

                string[] options = customPostposition.Split('/');
                if (options.Length != 2)
                    return customPostposition;

                // 스페인어는 마지막 글자에 따라 첫 번째 또는 두 번째 옵션 사용
                char lastChar = word[^1];
                return "aeiou".Contains(char.ToLower(lastChar)) ? options[1] : options[0];
            }
        }
    }
}