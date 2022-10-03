using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
 
// Reference Bobi https://forum.unity.com/threads/3d-character-picking-up-item.900098/
public class PlayerPickup : NetworkBehaviour
{
    [SerializeField] float raycastDistance;
    [SerializeField] LayerMask pickupLayer;
    [SerializeField] Transform pickupPosition;
    [SerializeField] KeyCode pickupButton = KeyCode.E;
    [SerializeField] KeyCode dropButton = KeyCode.Q;
 
    Camera cam;
    bool hasObjectInHand;
    GameObject objInHand;
    Transform worldObjectHolder;
 
    public override void OnStartClient()
    {
        base.OnStartClient();
 
        if (!base.IsOwner)
            enabled = false;
 
        cam = Camera.main;
        worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;
    }
 
    private void Update()
    {
        if (Input.GetKeyDown(pickupButton))
            Pickup();
 
        if (Input.GetKeyDown(dropButton))
            Drop();
    }
 
    void Pickup()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, raycastDistance, pickupLayer))
        {
            if (!hasObjectInHand)
            {
                SetObjectInHandServer(hit.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hit.transform.gameObject;
                hasObjectInHand = true;
            } 
            else if(hasObjectInHand)
            {
                Drop();
 
                SetObjectInHandServer(hit.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hit.transform.gameObject;
                hasObjectInHand = true;
            }
        }
    }
 
    [ServerRpc(RequireOwnership = false)]
    void SetObjectInHandServer(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        SetObjectInHandObserver(obj, position, rotation, player);
    }
 
    [ObserversRpc]
    void SetObjectInHandObserver(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.parent = player.transform;
 
        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = true;
    }
 
    void Drop()
    {
        if(!hasObjectInHand)
            return;
 
        DropObjectServer(objInHand, worldObjectHolder);
        hasObjectInHand = false;
        objInHand = null;
    }
 
    [ServerRpc(RequireOwnership = false)]
    void DropObjectServer(GameObject obj, Transform worldHolder)
    {
        DropObjectObserver(obj, worldHolder);
    }
 
    [ObserversRpc]
    void DropObjectObserver(GameObject obj, Transform worldHolder)
    {
        obj.transform.parent = worldHolder;
 
        if(obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = false;
    }
}