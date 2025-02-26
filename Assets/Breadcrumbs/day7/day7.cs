using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class day7 : MonoBehaviour {
    [SerializeField] private Button serverStartButton;
    [SerializeField] private Button clientStartButton;

    [SerializeField] private GameObject server;
    [SerializeField] private GameObject client;
    
    private bool _serverStarted = false;
    private bool _clientStarted = false;

    void Start() {
        serverStartButton.OnClickAsObservable().Subscribe(x => {
                _serverStarted = !_serverStarted;
                server.SetActive(_serverStarted);
                var buttonText = serverStartButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = _serverStarted ? "Stop Server" : "Start Server";
            }
        );

        clientStartButton.OnClickAsObservable().Subscribe(x => {
            _clientStarted = !_clientStarted;
            client.SetActive(_clientStarted);
            var buttonText = clientStartButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = _clientStarted ? "Stop Client" : "Start Client";
        });
    }
}
