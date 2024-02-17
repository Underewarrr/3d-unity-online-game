using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;

public class PlayerStats : NetworkBehaviour
{
    [Header("Stats")]
    public List<StatsObject> StatsObjects = new List<StatsObject>(); // Lista de objetos de estat�sticas

    GameObject statsPanel; // Refer�ncia ao painel de estat�sticas
    [SerializeField] private GameObject statsCanvasObject; // Objeto de canvas para exibi��o de estat�sticas
    [SerializeField] private GameObject contextMenuPrefab; // Prefab do menu de contexto
    Transform invObjectHolder; // Objeto pai dos objetos de estat�sticas
    [SerializeField] private KeyCode StatsButton = KeyCode.Tab; // Bot�o para abrir/fechar o painel de estat�sticas

    [Header("Equip")]
    Camera cam;
    Transform worldObjectHolder;

    PlayerInventory playerInventory; // Refer�ncia ao script de invent�rio do jogador

    // Vari�vel para manter uma refer�ncia ao menu de contexto atualmente ativo
    private GameObject activeContextMenu;

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

        // Encontra e define o painel de estat�sticas e o objeto pai dos objetos de estat�sticas
        statsPanel = GameObject.FindGameObjectWithTag("StatsPanel");
        invObjectHolder = GameObject.FindGameObjectWithTag("StatsObjHolder").transform;

        // Esconde o painel de estat�sticas se estiver ativo
        if (statsPanel.activeSelf)
            ToggleStats();

        // Obt�m refer�ncia ao script de invent�rio do jogador
        playerInventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        // Verifica se o bot�o de estat�sticas foi pressionado
        if (Input.GetKeyDown(StatsButton))
            ToggleStats();
    }

    // M�todo para alternar a exibi��o do painel de estat�sticas
    void ToggleStats()
    {
        if (statsPanel.activeSelf)
        {
            // Esconde o painel de estat�sticas
            statsPanel.SetActive(false);

            // Bloqueia e esconde o cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!statsPanel.activeSelf)
        {
            // Atualiza a interface de usu�rio das estat�sticas
            UpdateInvUI();

            // Exibe o painel de estat�sticas
            statsPanel.SetActive(true);

            // Libera o cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // M�todo para atualizar a interface de usu�rio das estat�sticas
    void UpdateInvUI()
    {
        // Limpa os objetos de estat�sticas na interface
        foreach (Transform child in invObjectHolder)
        {
            Destroy(child.gameObject);
        }

        // Adiciona os objetos de estat�sticas � interface
        foreach (PlayerInventory.InventoryObject invObj in playerInventory.inventoryObjects)
        {
            GameObject obj = Instantiate(statsCanvasObject, invObjectHolder);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = invObj.PlayerInvItem.itemName + " - " + invObj.amount;

            // Adiciona um listener de clique para mostrar o menu de contexto
            obj.GetComponent<Button>().onClick.AddListener(delegate { ShowContextMenu(invObj); });
        }
    }

    // M�todo para mostrar o menu de contexto para um item do invent�rio
    void ShowContextMenu(PlayerInventory.InventoryObject invObj)
    {
        // Verifica se j� h� um menu de contexto ativo e o destroi
        if (activeContextMenu != null)
        {
            Destroy(activeContextMenu);
        }

        // Instancia o menu de contexto
        activeContextMenu = Instantiate(contextMenuPrefab, invObjectHolder);

        // Define a posi��o do menu de contexto
        activeContextMenu.transform.position = Input.mousePosition;

        // Obt�m o bot�o de drop de item do menu de contexto e adiciona um listener de clique
        Button dropItemButton = activeContextMenu.transform.Find("DropItemButton").GetComponent<Button>();
        dropItemButton.onClick.AddListener(delegate { DropItem(invObj); });
    }

    // M�todo para largar um item do invent�rio
    void DropItem(PlayerInventory.InventoryObject invObj)
    {
        playerInventory.DropItem(invObj.PlayerInvItem);
        // Fecha o menu de contexto ap�s dropar o item
        Destroy(activeContextMenu);
    }

    // Estrutura para representar um objeto de estat�sticas
    [System.Serializable]
    public class StatsObject
    {
        // Implemente conforme necess�rio
    }
}
