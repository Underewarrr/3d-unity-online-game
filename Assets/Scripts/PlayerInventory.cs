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

        // Verifica se é o dono do objeto
        if (!base.IsOwner)
        {
            // Se não for o dono, desativa o script
            enabled = false;
            return;
        }

        // Obtém a câmera principal e o objeto pai dos objetos do mundo
        cam = Camera.main;
        worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;

        // Encontra e define o painel de inventário e o objeto pai dos objetos do inventário
        invPanel = GameObject.FindGameObjectWithTag("InventoryPanel");
        invObjectHolder = GameObject.FindGameObjectWithTag("InventoryObjectHolder").transform;

        // Esconde o inventário se estiver ativo
        if (invPanel.activeSelf)
            ToggleInventory();
    }

    private void Update()
    {
        // Verifica se o botão de pegar foi pressionado
        if (Input.GetKeyDown(pickupButton))
            Pickup();

        // Verifica se o botão de inventário foi pressionado
        if (Input.GetKeyDown(inventoryButton))
            ToggleInventory();
    }

    // Método para pegar objetos
    void Pickup()
    {
        // Lança um raio para frente a partir da posição da câmera
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, pickupDistance, pickupLayer))
        {
            // Verifica se o objeto atingido é um item no chão
            GroundItem groundItem = hit.transform.GetComponent<GroundItem>();
            if (groundItem == null)
                return;

            // Verifica se o item atende aos critérios de filtro
            if (IsFilteredItem(groundItem.itemScriptable))
            {
                // Adiciona o item ao inventário
                AddToInventory(groundItem.itemScriptable);
            }
            else
            {
                // Se o item não atender aos critérios de filtro, exibe uma mensagem de aviso
                Debug.Log("Item não compatível com o filtro.");
            }
        }
    }

    // Método para verificar se o item atende aos critérios de filtro
    bool IsFilteredItem(PlayerInvItem item)
    {
        // Aqui você pode adicionar lógica para definir quais itens são aceitáveis no inventário
        // Por exemplo, você pode verificar o nome do item, a tag do objeto, etc.

        // Por exemplo, vamos supor que queremos aceitar apenas itens com os nomes "Stone" ou "Wood"
        if (item.itemName == "Stone" || item.itemName == "Wood")
        {
            return true;
        }

        return false;
    }


    // Método para adicionar um item ao inventário
    void AddToInventory(PlayerInvItem newItem)
    {
        foreach (InventoryObject invObj in inventoryObjects)
        {
            // Verifica se o item já está no inventário
            if (invObj.PlayerInvItem == newItem)
            {
                // Aumenta a quantidade do item se já estiver no inventário
                invObj.amount++;
                return;
            }
        }

        // Adiciona o item ao inventário se não estiver presente
        inventoryObjects.Add(new InventoryObject() { PlayerInvItem = newItem, amount = 1 });
    }

    // Método para alternar a exibição do inventário
    void ToggleInventory()
    {
        if (invPanel.activeSelf)
        {
            // Esconde o inventário
            invPanel.SetActive(false);

            // Bloqueia e esconde o cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!invPanel.activeSelf)
        {
            // Atualiza a interface do inventário
            UpdateInvUI();

            // Exibe o inventário
            invPanel.SetActive(true);

            // Libera o cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Método para atualizar a interface do inventário
    void UpdateInvUI()
    {
        // Limpa os objetos do inventário na interface
        foreach (Transform child in invObjectHolder)
            Destroy(child.gameObject);

        // Adiciona os objetos do inventário à interface
        foreach (InventoryObject invObj in inventoryObjects)
        {
            GameObject obj = Instantiate(invCanvasObject, invObjectHolder);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = invObj.PlayerInvItem.itemName + " - " + invObj.amount;
            obj.GetComponent<Button>().onClick.AddListener(delegate { DropItem(invObj.PlayerInvItem); });
        }
    }

    // Método para largar um item do inventário
    public void DropItem(PlayerInvItem PlayerInvItem) // Definido como public para ser acessível de outros scripts
    {
        foreach (InventoryObject invObj in inventoryObjects)
        {
            if (invObj.PlayerInvItem != PlayerInvItem)
                continue;

            // Reduz a quantidade do item e o remove do inventário se necessário
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

    // Método RPC para largar um item no servidor
    [ServerRpc(RequireOwnership = false)]
    void DropItemRPC(GameObject prefab, Vector3 position)
    {
        DropItemObserver(prefab, position);
    }

    // Método RPC para largar um item nos clientes
    [ObserversRpc]
    void DropItemObserver(GameObject prefab, Vector3 position)
    {
        // Instancia o objeto no mundo
        GameObject drop = Instantiate(prefab, position, Quaternion.identity, worldObjectHolder);
        Spawn(drop);
    }

    // Estrutura para representar um objeto no inventário
    [System.Serializable]
    public class InventoryObject
    {
        public PlayerInvItem PlayerInvItem;
        public int amount;
    }
}
