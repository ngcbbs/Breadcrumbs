using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Breadcrumbs.day7 {
    public class AutoConnectClient : MonoBehaviour {
        private UdpClient udpReceiver;
        private TcpClient tcpClient;
        private const int UdpPort = 5555;
        
        private CancellationTokenSource cts;
        
        private void OnEnable() {
            cts = new CancellationTokenSource();
            StartUdpListenerAsync().Forget();
        }

        private void OnDisable() {
            StopAutoConnectClient();
        }

        private void StopAutoConnectClient() {
            cts?.Cancel();
            udpReceiver?.Close();
            tcpClient?.Close();
        }

        private async UniTaskVoid StartUdpListenerAsync() {
            udpReceiver = new UdpClient(UdpPort);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, UdpPort);

            try {
                while (!cts.Token.IsCancellationRequested) {
                    UdpReceiveResult result = await udpReceiver.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    Debug.Log($"수신된 서버 정보: {message}");

                    if (message.StartsWith("TCP_SERVER:")) {
                        string[] data = message.Split(':');
                        string serverIP = data[1];
                        int serverPort = int.Parse(data[2]);

                        await ConnectToServerAsync(serverIP, serverPort);
                    }
                }
            }
            catch (Exception e) {
                Debug.LogError("UdpListener Exception: " + e.Message);
            }

            Debug.Log("<color=red>StartUdpListenerAsync end point</color>");
        }

        private async UniTask ConnectToServerAsync(string serverIP, int port) {
            try {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIP, port);
                Debug.Log($"TCP 서버({serverIP}:{port})에 연결됨");

                await SendAndReceiveAsync();
            }
            catch (Exception e) {
                Debug.LogError("서버 연결 실패: " + e.Message);
            }
        }

        private async UniTask SendAndReceiveAsync() {
            NetworkStream stream = tcpClient.GetStream();
            byte[] message = Encoding.UTF8.GetBytes("Hello, Server!");

            await stream.WriteAsync(message, 0, message.Length);
            Debug.Log("서버에 메시지 전송");

            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Debug.Log($"서버 응답: {response}");
            tcpClient.Close();
        }

        private void OnApplicationQuit() {
            StopAutoConnectClient();
        }
    }
}
