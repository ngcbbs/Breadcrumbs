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

        ApiRequest CreateRequest(string userMessage) {
            if (string.IsNullOrEmpty(userMessage)) {
                throw new ArgumentNullException(nameof(userMessage));
            }

            return new ApiRequest() {
                model = "gemma-3-4b-it",
                messages = new List<Message>() {
                    new Message() {
                        role = "system",
                        content = "너의 이름은 미스터 사탄이야. 꼭 자신을 밝히고 대답을 하길 바래.", // ai 에 캐릭터를 입혀준다..
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
