using Unity.Netcode;
using UnityEngine;

public class DraggObject : NetworkBehaviour
{
    private bool isBeingDragged = false;

    private void Update()
    {
        //if (Input.GetMouseButtonDown(1))
        //{
        //    StartDragging();
        //}

    }
    private void StartDragging()
    {
        if (!isBeingDragged)
        {
            isBeingDragged = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.useGravity = false;

            ChangeOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
    [ServerRpc(RequireOwnership = false)]  
    private void ChangeOwnershipServerRpc(ulong newOwnerClientId)
    {
        print("sa");
        NetworkObject.ChangeOwnership(newOwnerClientId);
        ChangeOwnershipClientRpc(newOwnerClientId);

    }

    [ClientRpc]
    private void ChangeOwnershipClientRpc(ulong newOwnerClientId)
    {
        Debug.Log($"Ownership changed to ClientId: {newOwnerClientId}");
    }
}
