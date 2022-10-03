using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;
using UnityEngine.UI;
 
public class PlayerInventory : NetworkBehaviour
{
    [Header("Inventory")]
    public List<InventoryObject> inventoryObjects = new List<InventoryObject>();
    GameObject invPanel;
    [SerializeField] GameObject invCanvasObject;
    Transform invObjectHolder;
    [SerializeField] KeyCode inventoryButton = KeyCode.Tab;
 
    [Header("Pickup")]
    [SerializeField] LayerMask pickupLayer;
    [SerializeField] float pickupDistance;
    [SerializeField] KeyCode pickupButton = KeyCode.E;
 
    Camera cam;
    Transform worldObjectHolder;
 
    public override void OnStartClient()
    {
        base.OnStartClient();
 
        if (!base.IsOwner)
        {
            enabled = false;
            return;
        }
 
        cam = Camera.main;
        worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;
        invPanel = GameObject.FindGameObjectWithTag("InventoryPanel");
        invObjectHolder = GameObject.FindGameObjectWithTag("InventoryObjectHolder").transform;
 
        if (invPanel.activeSelf)
            ToggleInventory();
    }
 
    private void Update()
    {
        if (Input.GetKeyDown(pickupButton))
            Pickup();
 
        if (Input.GetKeyDown(inventoryButton))
            ToggleInventory();
    }
 
    void Pickup()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, pickupDistance, pickupLayer))
        {
            if (hit.transform.GetComponent<GroundItem>() == null)
                return;
 
            AddToInventory(hit.transform.GetComponent<GroundItem>().itemScriptable);
        }
    }
 
 
    void AddToInventory(PlayerInvItem newItem)
    {
        foreach(InventoryObject invObj in inventoryObjects)
        {
            if(invObj.PlayerInvItem == newItem)
            {
                invObj.amount++;
                return;
            }
        }
 
        inventoryObjects.Add(new InventoryObject() { PlayerInvItem = newItem, amount = 1 });
    }
 
    void ToggleInventory()
    {
        if (invPanel.activeSelf)
        {
            invPanel.SetActive(false);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!invPanel.activeSelf)
        {
            UpdateInvUI();
 
            invPanel.SetActive(true);
 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
 
    void UpdateInvUI()
    {
        foreach (Transform child in invObjectHolder)
            Destroy(child.gameObject);
 
        foreach (InventoryObject invObj in inventoryObjects)
        {
            GameObject obj = Instantiate(invCanvasObject, invObjectHolder);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = invObj.PlayerInvItem.itemName + " - " + invObj.amount;
            obj.GetComponent<Button>().onClick.AddListener(delegate { DropItem(invObj.PlayerInvItem); });
        }
    }
 
    void DropItem(PlayerInvItem PlayerInvItem)
    {
        foreach(InventoryObject invObj in inventoryObjects)
        {
            if (invObj.PlayerInvItem != PlayerInvItem)
                continue;
 
            if (invObj.amount > 1)
            {
                invObj.amount--;
                DropItemRPC(invObj.PlayerInvItem.prefab, cam.transform.position + cam.transform.forward);
                UpdateInvUI();
                return;
            }
            if(invObj.amount <= 1)
            {
                inventoryObjects.Remove(invObj);
                DropItemRPC(invObj.PlayerInvItem.prefab, cam.transform.position + cam.transform.forward);
                UpdateInvUI();
                return;
            }
        }
    }
 
    [ServerRpc(RequireOwnership = false)]
    void DropItemRPC(GameObject prefab, Vector3 position)
    {
        DropItemObserver(prefab, position);
    }
 
    [ObserversRpc]
    void DropItemObserver(GameObject prefab, Vector3 position)
    {
        GameObject drop = Instantiate(prefab, position, Quaternion.identity, worldObjectHolder);
        Spawn(drop);
    }
 
 
    [System.Serializable]
    public class InventoryObject
    {
        public PlayerInvItem PlayerInvItem;
        public int amount;
    }
}