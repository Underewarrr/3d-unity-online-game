using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectInfo : MonoBehaviour
{
    [SerializeField] private float raycastDistance;
    [SerializeField] private LayerMask pickupLayer;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        CheckObjectInfo();
    }

    private void CheckObjectInfo()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, raycastDistance, pickupLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            string objectName = hitObject.name;
            Debug.Log("Player is pointing at: " + objectName);
        }
    }
}
