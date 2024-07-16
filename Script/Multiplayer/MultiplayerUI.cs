using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] Button HostButton;
    [SerializeField] Button ClientButton;
    void Awake()
    {
        HostButton.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); });
        ClientButton.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); });
    }
    void Update()
    {
        
    }
}
