using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Breadcrumbs.api {
    public static class WebPostRequest {
        public static async UniTask<string> PostAsync(string url, string postData) {
            using UnityWebRequest www = UnityWebRequest.Post(url, postData, "application/json");
            www.downloadHandler = new DownloadHandlerBuffer();

            try {
                await www.SendWebRequest().ToUniTask();

                if (www.result == UnityWebRequest.Result.Success) {
                    return www.downloadHandler.text;
                }
                else {
                    Debug.LogError("POST 요청 실패: " + www.error);
                    return null;
                }
            }
            catch (Exception e) {
                Debug.LogError("POST 요청 중 오류 발생: " + e.Message);
                return null;
            }
        }
    }
}