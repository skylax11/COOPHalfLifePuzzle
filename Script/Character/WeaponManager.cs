using System;
using System.Collections;
using System.Collections.Generic;
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
    }
    private void OnStartPointChanged(Vector3 old_startPoint, Vector3 new_startPoint)
    {
        transform.position = _startPoint.Value;
    }
    private void OnColorChanged(Color oldPlayerColor, Color newPlayerColor)
    {
        GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
    }
    public static NetworkList<ulong> clientIdList = new NetworkList<ulong>();
    private void Start()
    {
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
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>();
    private void Singleton_OnClientConnectedCallback(ulong obj)
    {
        if (IsHost)
        {
            clientIdList.Add(obj);
            InstantiateCubeNetwork(obj);
        }
    }
    private void InstantiateCubeNetwork(ulong obj)
    {
        Vector3 spawnPoint = Vector3.zero;
        if (clientIdList.Count == 1)
            spawnPoint = new Vector3(-10f, 1.77010012f, -0.319999993f);
        else if (clientIdList.Count == 2)
            spawnPoint = new Vector3(2.25999999f, 1.77010012f, -0.319999993f);

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
        }
    }
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        if (TauGun == null && GravityGun == null)
        {
            Initialize();
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
