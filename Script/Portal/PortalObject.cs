using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PortalObject : NetworkBehaviour
{
    public List<GameObject> parkourList = new List<GameObject>();
    public GameObject cubePrefab;
    public bool hasSpawnedCube = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Dragable"))
        {
            foreach(var item in parkourList)
                item.SetActive(true);

            var ownerClientId = collision.transform.GetComponent<NetworkObject>().OwnerClientId;
            ulong tempId = 0;
            int tempIndex = -1;

            for (int i = 0; i < WeaponManager.clientIdList.Count; i++)
            {
                print(ownerClientId + " clientýdlist : " + WeaponManager.clientIdList[i]);
                if (WeaponManager.clientIdList[i] == ownerClientId)  // for 2-player co-op
                {
                    tempId = WeaponManager.clientIdList[1 - i]; // if 0 then gets 1, if 1 then gets 0. Quite opposite.
                    tempIndex = 1 - i;
                }
            }
                
            InstantiateCubeNetworkServerRpc(tempId,tempIndex); 
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("Dragable"))
        {
            foreach (var item in parkourList)
                item.SetActive(true);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void InstantiateCubeNetworkServerRpc(ulong obj,int tempIndex)
    {
        if (hasSpawnedCube)
            return;
        hasSpawnedCube = true;

        Vector3 spawnPoint = Vector3.zero;
        print(tempIndex);
        if (tempIndex == 0)
            spawnPoint = new Vector3(-10f, 1.77010012f, -0.319999993f);
        else if(tempIndex == 1)
            spawnPoint = new Vector3(2.25999999f, 1.77010012f, -0.319999993f);

        GameObject cubeInstance = Instantiate(cubePrefab, spawnPoint, Quaternion.identity);
        NetworkObject networkObject = cubeInstance.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(obj, true);
    }
}
