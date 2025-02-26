using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Breadcrumbs.day7 {
    public class UniTaskTcpServerWithDiscovery : MonoBehaviour {
        private TcpListener server;
        private UdpClient udpBroadcaster;
        private CancellationTokenSource cts;
        private const int TcpPort = 5000;
        private const int UdpPort = 5555;

        private void OnEnable() {
            cts = new CancellationTokenSource();
            StartServerAsync(TcpPort).Forget();
            StartUdpBroadcastAsync().Forget();
        }

        private void OnDisable() {
            StopServer();
        }

        void OnApplicationQuit() {
            StopServer();
        }

        public async UniTaskVoid StartServerAsync(int port) {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Debug.Log($"서버 시작됨 (포트 {port}) / 서버 주소: ({GetLocalIPAddress()})");

            try {
                while (!cts.Token.IsCancellationRequested) {
                    TcpClient client = await server.AcceptTcpClientAsync(); //.AsUniTask(cts.Token);
                    Debug.Log("클라이언트 연결됨");

                    HandleClientAsync(client, cts.Token).Forget();
                }
            }
            catch (Exception e) {
                Debug.LogError("서버 오류: " + e.Message);
            }
            
            Debug.Log("<color=red>StartServerAsync end point</color>");
        }

        private async UniTaskVoid HandleClientAsync(TcpClient client, CancellationToken token) {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try {
                while (!token.IsCancellationRequested) {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break;

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log($"받은 메시지: {receivedMessage}");

                    byte[] response = Encoding.UTF8.GetBytes("서버 응답: " + receivedMessage);
                    await stream.WriteAsync(response, 0, response.Length, token);
                }
            }
            catch (Exception e) {
                Debug.LogError("클라이언트 오류: " + e.Message);
            }
            finally {
                client.Close();
                Debug.Log("클라이언트 연결 종료");
            }
        }

        private async UniTaskVoid StartUdpBroadcastAsync() {
            udpBroadcaster = new UdpClient();
            IPEndPoint broadcastEP = new IPEndPoint(IPAddress.Broadcast, UdpPort);
            byte[] message = Encoding.UTF8.GetBytes($"TCP_SERVER:{GetLocalIPAddress()}:{TcpPort}");

            try {
                while (!cts.Token.IsCancellationRequested) {
                    udpBroadcaster.Send(message, message.Length, broadcastEP);
                    Debug.Log("서버 정보 브로드캐스트 전송");
                    await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: cts.Token);
                }
            }
            catch (Exception e) {
                Debug.LogError("브로드캐스트 오류: " + e.Message);
            }

            Debug.Log("<color=red>StartUdpBroadcastAsync end point</color>");
        }

        private string GetLocalIPAddress() {
            string localIP = "127.0.0.1";
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName())) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    localIP = ip.ToString();
                    break;
                }
            }

            return localIP;
        }

        public void StopServer() {
            cts?.Cancel();
            server?.Stop();
            udpBroadcaster?.Close();
            Debug.Log("서버 종료됨");
        }
    }
}
