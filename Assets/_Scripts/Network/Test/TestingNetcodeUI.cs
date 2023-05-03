using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private TextMeshProUGUI ipText;

    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            Debug.Log("HOST");

            string ip = "127.0.0.1";

            if (ipText.text.Length >= 7) ip = ipText.text;

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetConnectionData(ip, 7777);

            NetworkManager.Singleton.StartHost();
            Hide();
        });

        startClientButton.onClick.AddListener(() =>
        {
            Debug.Log("CLIENT");

            string ip = "127.0.0.1";

            if (ipText.text.Length >= 7) ip = ipText.text;

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetConnectionData(ip, 7777);

            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
