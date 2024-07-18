using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TeleportPlayer : NetworkBehaviour
{
    public bool forClient;
    public NetworkVariable<Vector3> pos;
    public GameObject thePlayer;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (forClient)
            {
                RequestOwnershipAndTeleportServerRpc(WeaponManager.clientIdList[1], pos.Value);
            }
            else
            {
                RequestOwnershipAndTeleportServerRpc(WeaponManager.clientIdList[0], pos.Value);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipAndTeleportServerRpc(ulong clientId, Vector3 newPosition)
    {
        // Change ownership of the player object to the specified client
        NetworkObject.ChangeOwnership(clientId);

        thePlayer = GetPlayerFromServer(clientId);

        if (thePlayer != null)
        {
            // Teleport the player
            thePlayer.transform.position = newPosition;
            TeleportPlayerClientRpc(newPosition);
        }
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(Vector3 newPosition)
    {
        thePlayer.transform.position = newPosition;
    }

    private GameObject GetPlayerFromServer(ulong netId)
    {
        print("WSAAAAAAA");
        if (IsServer)
        {
            print("ASDA2233");
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(netId, out NetworkClient client))
            {
                print("ASSS");
                return client.PlayerObject.gameObject;
            }
        }
        else
        {
            Debug.LogWarning("Only the server can access the ConnectedClients dictionary.");
        }

        return null;
    }
}