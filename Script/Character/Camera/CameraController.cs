using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    public float SensX;
    public float SensY;

    public Transform Orientation;

    public float xRot;
    public float yRot;

    [SerializeField] Transform Player;
    public Camera cam;
    Vector3 position;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    float pitch = 0f;
    float yaw = 0f;
    private void Update()
    {
        if (!IsOwner)
            return;

        yaw = Player.localEulerAngles.y + Input.GetAxis("Mouse X") * SensX;
        pitch -= SensY * Input.GetAxis("Mouse Y");

        // Clamp pitch between lookAngle
        pitch = Mathf.Clamp(pitch, -90, 90);

        CameraServerRpc();
        
    }
    [ServerRpc]
    private void CameraServerRpc()
    {
        CameraClientRpc();
    }
    [ClientRpc]
    private void CameraClientRpc()
    {
        Player.localEulerAngles = new Vector3(0, yaw, 0);
        transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }
}
