using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    private IGunState currentState;
    
    public IGunState GravityGun;
    public IGunState TauGun;

    public GameObject cubePrefab;

    [SerializeField] NetworkVariable<Vector3> _startPoint;
    public static NetworkList<ulong> clientIdList = new NetworkList<ulong>();
    public static List<GameObject> playerPrefabs = new List<GameObject>();

    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>();

    // timer variables
    private TextMeshProUGUI _timer;
    public float timer = 0f;
    public NetworkVariable<bool> startTimer = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void OnApplicationQuit()
    {
        clientIdList.Dispose();
    }
    private void Initialize()
    {
        TauGun = GetComponentInChildren<TauGun>();
        GravityGun = GetComponentInChildren<GravityGun>();
        TauGun.Exit();
        currentState = GravityGun;
        currentState.Enter();
    }
    private void Awake()
    {
        PlayerColor.OnValueChanged += OnColorChanged;
        _startPoint.OnValueChanged += OnStartPointChanged;
        _timer = GameObject.FindWithTag("Timer").GetComponent<TextMeshProUGUI>();
    }
    private void OnStartPointChanged(Vector3 old_startPoint, Vector3 new_startPoint)
    {
        transform.position = _startPoint.Value;
    }
    private void OnColorChanged(Color oldPlayerColor, Color newPlayerColor)
    {
        GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner)
            return;

        //if (collision.transform.CompareTag("FinishCollide"))
        //{
        //    startTimer.Value = false;
        //}
    }
    private void Start()
    {
        Unity.Collections.NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;

        if (IsServer)
        {
            PlayerColor.Value = UnityEngine.Random.ColorHSV();
        }
        if (IsHost && IsOwner)
        {
            _startPoint.Value = new Vector3(-18.9000011f, 1f, 3.99000001f);

            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            if (NetworkManager.Singleton.ConnectedClients.Count == 1)
                Singleton_OnClientConnectedCallback(NetworkManager.Singleton.LocalClientId);
        }

        transform.position = _startPoint.Value;

    }
    private void Singleton_OnClientConnectedCallback(ulong obj)
    {
        if (IsHost)
        {
            clientIdList.Add(obj);
            playerPrefabs.Add(NetworkManager.ConnectedClients[obj].PlayerObject.gameObject);
            InstantiateCubeNetwork(obj);
        }
    }
    private void InstantiateCubeNetwork(ulong obj)
    {
        Vector3 spawnPoint = Vector3.zero;
        if (clientIdList.Count == 1)
            spawnPoint = new Vector3(-10f, 1.77010012f, -0.319999993f);
        else if (clientIdList.Count == 2)
        {
            spawnPoint = new Vector3(2.25999999f, 1.77010012f, -0.319999993f);
        }

        GameObject cubeInstance = Instantiate(cubePrefab, spawnPoint, Quaternion.identity);
        NetworkObject networkObject = cubeInstance.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(obj, true);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient)
        {
            GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
            if(!IsHost)
                startTimer.Value = true;
        }
        if (IsHost && NetworkManager.ConnectedClients.Count == 2)
                startTimer.Value = true;
    }
    
    
    private void Update()
    {
        if (!IsOwner)
            return;
        if (IsHost && NetworkManager.ConnectedClients.Count == 2)
                startTimer.Value = true;

        if (TauGun == null && GravityGun == null)
            Initialize();

        if (startTimer.Value)
        {
            timer += Time.deltaTime;
            _timer.text = timer.ToString("F2");
        }
        currentState.HandleInput();
       
    }
    public void SetState(IGunState nextState)
    {
        currentState.Exit();
        currentState = nextState;
        currentState.Enter();
    }

}
