using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;

public class PlayerStats : NetworkBehaviour
{
    [Header("Stats")]
    public List<StatsObject> StatsObjects = new List<StatsObject>(); // Lista de objetos de estatísticas

    GameObject statsPanel; // Referência ao painel de estatísticas
    [SerializeField] private GameObject statsCanvasObject; // Objeto de canvas para exibição de estatísticas
    [SerializeField] private GameObject contextMenuPrefab; // Prefab do menu de contexto
    Transform invObjectHolder; // Objeto pai dos objetos de estatísticas
    [SerializeField] private KeyCode StatsButton = KeyCode.Tab; // Botão para abrir/fechar o painel de estatísticas

    [Header("Equip")]
    Camera cam;
    Transform worldObjectHolder;

    PlayerInventory playerInventory; // Referência ao script de inventário do jogador

    // Variável para manter uma referência ao menu de contexto atualmente ativo
    private GameObject activeContextMenu;

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

        // Encontra e define o painel de estatísticas e o objeto pai dos objetos de estatísticas
        statsPanel = GameObject.FindGameObjectWithTag("StatsPanel");
        invObjectHolder = GameObject.FindGameObjectWithTag("StatsObjHolder").transform;

        // Esconde o painel de estatísticas se estiver ativo
        if (statsPanel.activeSelf)
            ToggleStats();

        // Obtém referência ao script de inventário do jogador
        playerInventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        // Verifica se o botão de estatísticas foi pressionado
        if (Input.GetKeyDown(StatsButton))
            ToggleStats();
    }

    // Método para alternar a exibição do painel de estatísticas
    void ToggleStats()
    {
        if (statsPanel.activeSelf)
        {
            // Esconde o painel de estatísticas
            statsPanel.SetActive(false);

            // Bloqueia e esconde o cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!statsPanel.activeSelf)
        {
            // Atualiza a interface de usuário das estatísticas
            UpdateInvUI();

            // Exibe o painel de estatísticas
            statsPanel.SetActive(true);

            // Libera o cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Método para atualizar a interface de usuário das estatísticas
    void UpdateInvUI()
    {
        // Limpa os objetos de estatísticas na interface
        foreach (Transform child in invObjectHolder)
        {
            Destroy(child.gameObject);
        }

        // Adiciona os objetos de estatísticas à interface
        foreach (PlayerInventory.InventoryObject invObj in playerInventory.inventoryObjects)
        {
            GameObject obj = Instantiate(statsCanvasObject, invObjectHolder);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = invObj.PlayerInvItem.itemName + " - " + invObj.amount;

            // Adiciona um listener de clique para mostrar o menu de contexto
            obj.GetComponent<Button>().onClick.AddListener(delegate { ShowContextMenu(invObj); });
        }
    }

    // Método para mostrar o menu de contexto para um item do inventário
    void ShowContextMenu(PlayerInventory.InventoryObject invObj)
    {
        // Verifica se já há um menu de contexto ativo e o destroi
        if (activeContextMenu != null)
        {
            Destroy(activeContextMenu);
        }

        // Instancia o menu de contexto
        activeContextMenu = Instantiate(contextMenuPrefab, invObjectHolder);

        // Define a posição do menu de contexto
        activeContextMenu.transform.position = Input.mousePosition;

        // Obtém o botão de drop de item do menu de contexto e adiciona um listener de clique
        Button dropItemButton = activeContextMenu.transform.Find("DropItemButton").GetComponent<Button>();
        dropItemButton.onClick.AddListener(delegate { DropItem(invObj); });
    }

    // Método para largar um item do inventário
    void DropItem(PlayerInventory.InventoryObject invObj)
    {
        playerInventory.DropItem(invObj.PlayerInvItem);
        // Fecha o menu de contexto após dropar o item
        Destroy(activeContextMenu);
    }

    // Estrutura para representar um objeto de estatísticas
    [System.Serializable]
    public class StatsObject
    {
        // Implemente conforme necessário
    }
}
