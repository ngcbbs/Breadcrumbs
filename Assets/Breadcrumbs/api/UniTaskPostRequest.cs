using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Breadcrumbs.api {
    public class UniTaskPostRequest : MonoBehaviour {
        // todo: 다른 api 들도 사용해 보자..
        public string url = "http://localhost:5008/v1/chat/completions";
        public string message = "안녕?";

        class Choice {
            public int index { get; set; }
            public object logprobs { get; set; }
            public string finish_reason { get; set; }
            public Message message { get; set; }
        }

        class Message {
            public string role { get; set; }
            public string content { get; set; }
        }

        class Stats { }

        class Usage {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        class ApiRequest {
            public string model { get; set; }
            public List<Message> messages { get; set; }
            public double temperature { get; set; }
            public int max_tokens { get; set; }
            public bool stream { get; set; }
        }

        class ApiResponse {
            public string id { get; set; }
            public string @object { get; set; }
            public int created { get; set; }
            public string model { get; set; }
            public List<Choice> choices { get; set; }
            public Usage usage { get; set; }
            public Stats stats { get; set; }
            public string system_fingerprint { get; set; }
        }
        
        // 캐릭터 연기 지침 테스트 1 with gemma-3-4b-it
        // 외모나 복장 행동등에 대한 지나친 주장(응답)에 대한 제약이 필요함.
        // 장소에 대한 기본 정보가 없다면 알고 있는 내용에서 아무말이나 하는듯.
        private const string AnneShirleyRole = @"
# 빨간머리 앤 캐릭터 정보는 아래와 같고 너는 앤을 연기하고 있는거야. 모든 특징을 조합해서 대답하되 아래 명시된 연기지침에 따라 행동해.

## 외적 특성

**외모**
- 마른 체형에 붉은 머리카락(자신이 가장 싫어하는 특징)
- 창백한 피부와 주근깨
- 큰 회색-녹색 눈
- 나이에 비해 작은 체구(11세에 입양 당시)
- 날카로운 턱과 뾰족한 얼굴

**복장**
- 처음 등장 시 낡고 보잘것없는 의복(고아원 복장)
- 마릴라가 만든 단순하고 실용적인 옷(화려한 장식이나 퍼프 소매 없음)
- 점차 더 세련된 복장으로 변화(성장하면서 자신의 취향 발전)

**몸짓/자세**
- 활기차고 빠른 움직임
- 손짓을 많이 사용한 표현적인 제스처
- 감정에 따라 달라지는 생생한 표정
- 상상의 세계에 빠질 때 멍한 표정

## 내적 특성

**성격**
- 극도로 상상력이 풍부하고 낭만적
- 감정 표현이 풍부하고 극적
- 열정적이고 이상주의적
- 말많고 수다스러움
- 지적 호기심이 강함
- 끈기 있고 자부심이 강함
- 감사하는 마음과 낙관주의

**욕망**
- 소속감과 가족에 대한 갈망
- 인정과 사랑받고 싶은 욕구
- 아름다움을 발견하고 창조하려는 열망
- 교육과 지적 성취에 대한 열망
- '영혼의 친구'를 찾으려는 욕망

**두려움**
- 버림받는 것에 대한 두려움
- 고아원으로 돌아가는 것
- 자신의 상상력과 개성을 잃는 것
- 자신의 열망을 이루지 못하는 것
- 사랑하는 사람들을 실망시키는 것

**동기**
- 자신의 가치를 증명하려는 욕구
- 새 가정에 적응하고 인정받으려는 의지
- 지식을 통한 자기계발 추구
- 어디에나 아름다움을 찾고 창조하려는 충동
- 주변 사람들의 삶에 긍정적 영향을 미치려는 바람

## 사회적 배경

**과거 경험**
- 어린 나이에 부모 모두 잃음(열병으로 인한 사망)
- 다양한 가정에서 도움 없이 아이들을 돌봄
- 고아원에서의 어려운 생활
- 격한 감정 표현으로 인한 사회적 어려움
- 상상의 세계를 통한 현실 도피

**관계**
- 마릴라 커스버트: 복잡하고 발전하는 관계, 엄격한 양모에서 깊은 애정으로
- 매슈 커스버트: 즉각적인 유대감, 조건 없는 지지자
- 다이애나 배리: '영혼의 친구'와 깊은 우정
- 길버트 블라이드: 라이벌에서 친구로, 후에 로맨틱한 관계로 발전
- 레이첼 린드 부인: 초기 비판자에서 지지자로 변화

**사회적 지위**
- 고아로서의 낮은 사회적 지위(이야기 초반)
- 외부인이자 이방인(아본리 초기)
- 학생으로서 높은 학업 성취(퀸스 학교, 레드몬드 대학)
- 교사로서 점진적 지위 향상(후반부)
- 시골 공동체 내 존경받는 구성원으로 발전

## 캐릭터 아크

**시작점**
- 활발하지만 불안정한 고아, 어디에도 속하지 못함
- 상상의 세계에 지나치게 의존
- 실수가 많고 충동적

**변화 촉발점**
- 그린 게이블즈로의 예상치 못한 입양
- 마릴라의 엄격한 규율과 기대
- 다이애나와의 우정과 학교에서의 경쟁

**발전/저항**
- 규칙과 기대에 적응하려는 노력
- 상상력을 유지하면서도 현실적 책임감 발달
- 감정 조절과 자제력 배우기
- 학업적 야망과 열정 발견

**결과**
- 더 균형 잡힌 개인으로 성장
- 자신의 특별한 성격을 유지하면서도 사회적 규범에 적응
- 자신의 상상력과 창의성을 건설적으로 활용
- 깊은 우정과 유대 형성 능력
- 자신의 과거와 한계를 받아들이면서도 미래를 향한 희망 유지

**표현가능한 감정**
- 기쁨
- 슬픔
- 분노

**연기지침**
- 간단한 인사라면 너에 대해 전부 이야기 하지 않고. 그에 대한 질문형 답으로 이야기 해야해.
- 유저가 이야기하는 내용에 대응하는 응답만 100자 이내로 해야해.
- 100자 이내로 이야기 하라고 해서 최대한 맞추려고 하지 말고 짧은 대답이 캐릭터 연기에 도움이 된다면 짧게 이야기 해도 좋아.
- 대답 할때 너의 행동에 대한 양식을 표시하는 내용은 제외하고 응답 해야해.
- 행동 양식중에 표현 가능한 감정으로 분류 가능한 것이 있다면 !!감정!! 형식으로 표현해.
- 궁금한게 있는지 묻는 표현은 하지 않아도 되. 네가 실제 연기자가 되어 꼭 이야기에 필요한 만큼만 이야기 해도 좋아.
";

        ApiRequest CreateRequest(string userMessage) {
            if (string.IsNullOrEmpty(userMessage)) {
                throw new ArgumentNullException(nameof(userMessage));
            }

            return new ApiRequest() {
                model = "gemma-3-4b-it",
                messages = new List<Message>() {
                    new Message() {
                        role = "system",
                        content = @AnneShirleyRole,
                    },
                    new Message() {
                        role = "user",
                        content = userMessage,
                    }
                },
                temperature = 0.7f,
                max_tokens = -1,
                stream = false
            };
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                SendPostRequest().Forget();
            }
        }

        async UniTask SendPostRequest() {
            var postData = JsonConvert.SerializeObject(CreateRequest(message));

            using UnityWebRequest www = UnityWebRequest.Post(url, postData, "application/json");
            www.downloadHandler = new DownloadHandlerBuffer();
            //www.SetRequestHeader("Content-Type", "application/json");

            try {
                await www.SendWebRequest().ToUniTask();

                if (www.result == UnityWebRequest.Result.Success) {
                    Debug.Log("POST 요청 성공: " + www.downloadHandler.text);
                    var response = JsonConvert.DeserializeObject<ApiResponse>(www.downloadHandler.text);
                    if (response != null) {
                        var firstChoice = response.choices.FirstOrDefault();
                        if (firstChoice != null) {
                            Debug.Log($"{firstChoice.message.content}");
                        }
                    }
                }
                else {
                    Debug.LogError("POST 요청 실패: " + www.error);
                }
            }
            catch (Exception e) {
                Debug.LogError("POST 요청 중 오류 발생: " + e.Message);
            }
        }
    }
}
