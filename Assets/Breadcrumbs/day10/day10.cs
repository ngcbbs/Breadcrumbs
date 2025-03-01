using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class day10 : MonoBehaviour {
    [SerializeField] private Button startServer;
    [SerializeField] private Button startHost;
    [SerializeField] private Button startClient;
    
    void Start() {
        startServer.OnClickAsObservable().Subscribe((x) => {
            NetworkManager.Singleton.StartServer();
        });
        startHost.OnClickAsObservable().Subscribe((x) => {
            NetworkManager.Singleton.StartHost();
        });
        startClient.OnClickAsObservable().Subscribe((x) => {
            NetworkManager.Singleton.StartClient();
        });
    }
}
