using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class TauGun : NetworkBehaviour, IGunState
{
    public bool BurstMode = false;

    private RaycastHit hit;
    public NetworkVariable<float> Energy = new NetworkVariable<float>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    private int layer;

    private bool doOnce = true;

    Vector3 reflectedVector;
    RaycastHit tempHit;

    private bool instantiatedByBullet = false;

    public LineRenderer m_LineRenderer;

    // WEAPON MANAGERDEN GELENLER

    private float holdingForSec = 0;

    public Transform lookDirection;

    public Rigidbody playerRigidbody;

    public static Action OnHoldStarted;
    public static Action OnHoldUnLeashed;

    public GameObject InstantiateProjectile;

    public WeaponManager m_WeaponManager;

    public void Enter()
    {
        layer = LayerMask.NameToLayer("Enemy");
        EnterServerRpc();
    }
    public void Exit()
    {
        ExitServerRpc();
    }
    #region Switch Weapon Server-Client
    [ServerRpc]
    public void EnterServerRpc()
    {
        gameObject.SetActive(true);
        EnterClientRpc();
    }
    [ClientRpc]
    public void EnterClientRpc()
    {
        gameObject.SetActive(true);

    }
    [ServerRpc]
    public void ExitServerRpc()
    {
        gameObject.SetActive(false);
        ExitClientRpc();
    }
    [ClientRpc]
    public void ExitClientRpc()
    {
        gameObject.SetActive(false);
    }
    #endregion
    public void HandleInput()
    {
        if (!IsOwner)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            SingleShotServerRpc();
        }
        else if (Input.GetMouseButton(1))
        {
            m_LineRenderer.positionCount = 0;
            holdingForSec += Time.deltaTime * 8f;
            holdingForSec = Mathf.Clamp(holdingForSec, 0, 20);
            OnHoldStarted?.Invoke();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            HoldShotServerRpc();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            m_WeaponManager.SetState(m_WeaponManager.GravityGun);
        }
        else
        {
            holdingForSec -= Time.deltaTime * 16f;
            holdingForSec = Mathf.Clamp(holdingForSec, 0, 20);
            OnHoldUnLeashed?.Invoke();

        }
    }
    [ServerRpc]
    private void SingleShotServerRpc()
    {
        SingleShotClientRpc();
    }
    [ClientRpc]
    private void SingleShotClientRpc()
    {
        m_LineRenderer.positionCount = 0;
        Fire(5, lookDirection.position, lookDirection.forward, burstMode: false);
    }
    [ServerRpc]
    private void HoldShotServerRpc()
    {
        HoldShotClientRpc();
    }
    [ClientRpc] 
    private void HoldShotClientRpc()
    {
        m_LineRenderer.positionCount = 0;
        playerRigidbody.AddForce(-lookDirection.forward * 50f * holdingForSec / 1.1f, ForceMode.Force);
        Fire(holdingForSec, lookDirection.position, lookDirection.forward, burstMode: true);

    }
    private IEnumerator ResetLineRenderer()
    {
        yield return new WaitForSeconds(0.6f);
        m_LineRenderer.positionCount = 0;
    }
    [ServerRpc(RequireOwnership = false)]
    private void UpdateEnergyServerRpc(float energy)
    {
        print(Energy.Value + "Enerji degeri:  " + energy);
        Energy.Value = energy;
        print(Energy.Value + "AA");
    }
    private void Awake()
    {
        Energy.OnValueChanged += OnEnergyChanged;
    }
    private void OnEnergyChanged(float oldEnergy, float newEnergy)
    {
        // Energy deðiþtiðinde yapýlacak iþlemler
        Debug.Log($"Energy deðiþti: {oldEnergy} -> {newEnergy}");
        ReflectClientRpc();
    }
    private Vector3 forward;
    private Vector3 position;
    public void Fire(float energy,Vector3 pos,Vector3 direction,bool burstMode)
    {

        BurstMode = burstMode;

        forward = direction;
        position = pos;

        UpdateEnergyServerRpc(energy);
    }
    [ClientRpc]
    private void RaycastShotClientRpc()
    {
        if (Physics.Raycast(position, forward, out tempHit, 5000))
        {
            if (tempHit.collider.transform.TryGetComponent(out IDamagable damage))
            {
                print(tempHit.transform.name);
            }
        }
    }
    [ClientRpc]
    private void ReflectClientRpc()
    {
        bool isCastingRay = false;
        float angle = 0;
        do
        {
            if (Physics.Raycast(position, forward, out tempHit, 5000))
            {

                if (m_LineRenderer.positionCount == 0)
                {
                    m_LineRenderer.positionCount++;
                    m_LineRenderer.SetPosition(m_LineRenderer.positionCount - 1, position);
                }

                //StartCoroutine("ResetLineRenderer");
                isCastingRay = true;

                if (tempHit.transform.CompareTag("Wall"))
                {
                    angle = 90 - Vector3.Angle(-tempHit.normal, forward);
                    float rotY = 90 - angle;
                    Debug.Log("Wall Angle : " + angle);
                    reflectedVector = -Vector3.Reflect((position - tempHit.point).normalized, -tempHit.normal);

                    CheckBehindWall(position, forward, BurstMode);

                }
                else if (tempHit.transform.CompareTag("Ground"))
                {
                    angle = Vector3.Angle(tempHit.normal, forward);

                    if (angle > 90)
                        angle = angle - 90;

                    Debug.Log("Ground Angle : " + angle);

                    reflectedVector = -Vector3.Reflect((position - tempHit.point).normalized, -tempHit.normal);
                }
                else if (tempHit.transform.TryGetComponent(out IDamagable d))
                    d.TakeDamage(10);

                Debug.DrawLine(position, tempHit.point, UnityEngine.Color.magenta, 2f);
                Debug.DrawRay(tempHit.point, reflectedVector, UnityEngine.Color.gray, 2f);

                position = tempHit.point;
                forward = reflectedVector;

                m_LineRenderer.positionCount++;
                m_LineRenderer.SetPosition(m_LineRenderer.positionCount - 1, position);
            }
            else
                isCastingRay = false;
        }
        while (isCastingRay && Energy.Value > 0 && angle < 60);
    }
    private void CheckBehindWall(Vector3 pos,Vector3 dir, bool BurstMode)
    {
        if (BurstMode)
        {
            var raycasts = Physics.RaycastAll(pos, dir, 2000);  // WALLBANG
            for (int i = 0; i < raycasts.Length; i++)
            {
                if (raycasts[i].collider.gameObject.layer == 6)
                {
                    if (raycasts[i].transform.TryGetComponent(out IDamagable d)) // polymorphism
                        d.TakeDamage(10);
                }
            }
        }
    }
}
