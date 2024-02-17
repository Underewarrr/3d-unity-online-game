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

        // Verifica se � o dono do objeto
        if (!base.IsOwner)
        {
            // Se n�o for o dono, desativa o script
            enabled = false;
            return;
        }

        // Obt�m a c�mera principal e o objeto pai dos objetos do mundo
        cam = Camera.main;
        worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;

        // Encontra e define o painel de invent�rio e o objeto pai dos objetos do invent�rio
        invPanel = GameObject.FindGameObjectWithTag("InventoryPanel");
        invObjectHolder = GameObject.FindGameObjectWithTag("InventoryObjectHolder").transform;

        // Esconde o invent�rio se estiver ativo
        if (invPanel.activeSelf)
            ToggleInventory();
    }

    private void Update()
    {
        // Verifica se o bot�o de pegar foi pressionado
        if (Input.GetKeyDown(pickupButton))
            Pickup();

        // Verifica se o bot�o de invent�rio foi pressionado
        if (Input.GetKeyDown(inventoryButton))
            ToggleInventory();
    }

    // M�todo para pegar objetos
    void Pickup()
    {
        // Lan�a um raio para frente a partir da posi��o da c�mera
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, pickupDistance, pickupLayer))
        {
            // Verifica se o objeto atingido � um item no ch�o
            GroundItem groundItem = hit.transform.GetComponent<GroundItem>();
            if (groundItem == null)
                return;

            // Verifica se o item atende aos crit�rios de filtro
            if (IsFilteredItem(groundItem.itemScriptable))
            {
                // Adiciona o item ao invent�rio
                AddToInventory(groundItem.itemScriptable);
            }
            else
            {
                // Se o item n�o atender aos crit�rios de filtro, exibe uma mensagem de aviso
                Debug.Log("Item n�o compat�vel com o filtro.");
            }
        }
    }

    // M�todo para verificar se o item atende aos crit�rios de filtro
    bool IsFilteredItem(PlayerInvItem item)
    {
        // Aqui voc� pode adicionar l�gica para definir quais itens s�o aceit�veis no invent�rio
        // Por exemplo, voc� pode verificar o nome do item, a tag do objeto, etc.

        // Por exemplo, vamos supor que queremos aceitar apenas itens com os nomes "Stone" ou "Wood"
        if (item.itemName == "Stone" || item.itemName == "Wood")
        {
            return true;
        }

        return false;
    }


    // M�todo para adicionar um item ao invent�rio
    void AddToInventory(PlayerInvItem newItem)
    {
        foreach (InventoryObject invObj in inventoryObjects)
        {
            // Verifica se o item j� est� no invent�rio
            if (invObj.PlayerInvItem == newItem)
            {
                // Aumenta a quantidade do item se j� estiver no invent�rio
                invObj.amount++;
                return;
            }
        }

        // Adiciona o item ao invent�rio se n�o estiver presente
        inventoryObjects.Add(new InventoryObject() { PlayerInvItem = newItem, amount = 1 });
    }

    // M�todo para alternar a exibi��o do invent�rio
    void ToggleInventory()
    {
        if (invPanel.activeSelf)
        {
            // Esconde o invent�rio
            invPanel.SetActive(false);

            // Bloqueia e esconde o cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!invPanel.activeSelf)
        {
            // Atualiza a interface do invent�rio
            UpdateInvUI();

            // Exibe o invent�rio
            invPanel.SetActive(true);

            // Libera o cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // M�todo para atualizar a interface do invent�rio
    void UpdateInvUI()
    {
        // Limpa os objetos do invent�rio na interface
        foreach (Transform child in invObjectHolder)
            Destroy(child.gameObject);

        // Adiciona os objetos do invent�rio � interface
        foreach (InventoryObject invObj in inventoryObjects)
        {
            GameObject obj = Instantiate(invCanvasObject, invObjectHolder);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = invObj.PlayerInvItem.itemName + " - " + invObj.amount;
            obj.GetComponent<Button>().onClick.AddListener(delegate { DropItem(invObj.PlayerInvItem); });
        }
    }

    // M�todo para largar um item do invent�rio
    public void DropItem(PlayerInvItem PlayerInvItem) // Definido como public para ser acess�vel de outros scripts
    {
        foreach (InventoryObject invObj in inventoryObjects)
        {
            if (invObj.PlayerInvItem != PlayerInvItem)
                continue;

            // Reduz a quantidade do item e o remove do invent�rio se necess�rio
            if (invObj.amount > 1)
            {
                invObj.amount--;
                DropItemRPC(invObj.PlayerInvItem.prefab, cam.transform.position + cam.transform.forward);
                UpdateInvUI();
                return;
            }
            if (invObj.amount <= 1)
            {
                inventoryObjects.Remove(invObj);
                DropItemRPC(invObj.PlayerInvItem.prefab, cam.transform.position + cam.transform.forward);
                UpdateInvUI();
                return;
            }
        }
    }

    // M�todo RPC para largar um item no servidor
    [ServerRpc(RequireOwnership = false)]
    void DropItemRPC(GameObject prefab, Vector3 position)
    {
        DropItemObserver(prefab, position);
    }

    // M�todo RPC para largar um item nos clientes
    [ObserversRpc]
    void DropItemObserver(GameObject prefab, Vector3 position)
    {
        // Instancia o objeto no mundo
        GameObject drop = Instantiate(prefab, position, Quaternion.identity, worldObjectHolder);
        Spawn(drop);
    }

    // Estrutura para representar um objeto no invent�rio
    [System.Serializable]
    public class InventoryObject
    {
        public PlayerInvItem PlayerInvItem;
        public int amount;
    }
}
