using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GravityGun : NetworkBehaviour, IGunState
{
    public float StopDistance;
    public GameObject DraggedObject;
    public RaycastHit Hit;
    public Transform lookDirection;
    public Transform DraggedPosition;
    public WeaponManager m_WeaponManager;

    public void Enter()
    {
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

        Debug.DrawRay(lookDirection.position, lookDirection.forward);

        if (Input.GetMouseButton(0))
        {
            if (DraggedObject == null)
                return;

            StartCoroutine(nameof(SetTag), DraggedObject.transform);
            DraggedObject.tag = "NotDragable";
            DraggedObject.GetComponent<Rigidbody>().AddForce(lookDirection.forward * 50, ForceMode.Impulse);
            DraggedObject.GetComponent<Rigidbody>().useGravity = true;
            DraggedObject = null;
        }
        else if (Input.GetMouseButton(1))
        {
            if (DraggedObject == null)
            {
                if (Physics.Raycast(lookDirection.position, lookDirection.forward, out Hit, 100))
                {
                    if (Hit.collider.transform.CompareTag("Dragable"))
                    {
                        DraggedObject = Hit.collider.gameObject;
                        DraggedObject.GetComponent<Rigidbody>().useGravity = false;
                    }
                }
            }
            else
            {
                if (DraggedObject.transform.CompareTag("Dragable"))
                {
                    DraggedObject.transform.localPosition = Vector3.Slerp(DraggedObject.transform.position, DraggedPosition.position, Time.deltaTime * 5); // TransformServerRpc'yi çaðýr ve yeni pozisyonu geçir
                }
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (DraggedObject != null)
            {
                DraggedObject.GetComponent<Rigidbody>().useGravity = true;
                DraggedObject = null;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_WeaponManager.SetState(m_WeaponManager.TauGun);
        }
    }
    private IEnumerator SetTag(Transform setTagObject)
    {
        yield return new WaitForSeconds(1);
        if (setTagObject != null)
        {
            setTagObject.tag = "Dragable";
        }
    }
}
