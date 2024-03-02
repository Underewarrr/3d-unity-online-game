using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

// Script para permitir que o jogador pegue e solte objetos
public class PlayerPickup : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float raycastDistance;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private Transform pickupPosition;
    [SerializeField] private Vector3 pickupOffset; // Offset para ajustar a posição de pegar em relação ao transformador pickupPosition
    [SerializeField] private KeyCode pickupButton = KeyCode.E;
    [SerializeField] private KeyCode dropButton = KeyCode.Q;

    private Camera cam;
    private bool hasObjectInHand;
    private GameObject objInHand;
    private Transform worldObjectHolder;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Se não for o dono deste objeto, desativa este script
        if (!base.IsOwner)
            enabled = false;

        // Encontra a câmera principal e o objeto para segurar os objetos do mundo
        cam = Camera.main;
        worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;
    }

    private void Update()
    {
        // Verifica se o jogador pressionou o botão para pegar ou soltar
        if (Input.GetKeyDown(pickupButton))
            Pickup();

        if (Input.GetKeyDown(dropButton))
            Drop();
    }

    // Função para pegar objetos
    private void Pickup()
    {
        // Lança um raio para frente da câmera para detectar objetos que podem ser pegos
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, raycastDistance, pickupLayer))
        {
            // Se o jogador não tiver um objeto na mão, pega o objeto atingido
            if (!hasObjectInHand)
            {
                // Calcula a posição final do objeto com base no offset
                Vector3 finalPosition = pickupPosition.position + pickupOffset;
                SetObjectInHandServer(hit.transform.gameObject, finalPosition, pickupPosition.rotation, gameObject);
                objInHand = hit.transform.gameObject;
                hasObjectInHand = true;
            }
            // Se o jogador já tiver um objeto na mão, solta o objeto atual e pega o novo objeto
            else if (hasObjectInHand)
            {
                Drop();

                // Calcula a posição final do objeto com base no offset
                Vector3 finalPosition = pickupPosition.position + pickupOffset;
                SetObjectInHandServer(hit.transform.gameObject, finalPosition, pickupPosition.rotation, gameObject);
                objInHand = hit.transform.gameObject;
                hasObjectInHand = true;
            }
        }
    }

    // Função para definir o objeto na mão do jogador no servidor
    [ServerRpc(RequireOwnership = false)]
    private void SetObjectInHandServer(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        SetObjectInHandObserver(obj, position, rotation, player);
    }

    // Função para definir o objeto na mão do jogador nos observadores
    [ObserversRpc]
    private void SetObjectInHandObserver(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.parent = player.transform;

        // Desativa a física do objeto enquanto está na mão do jogador
        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = true;
    }

    // Função para soltar objetos
    private void Drop()
    {
        // Verifica se o jogador tem um objeto na mão para soltar
        if (!hasObjectInHand)
            return;

        // Solta o objeto atual
        DropObjectServer(objInHand, worldObjectHolder);
        hasObjectInHand = false;
        objInHand = null;
    }

    // Função para soltar o objeto no servidor
    [ServerRpc(RequireOwnership = false)]
    private void DropObjectServer(GameObject obj, Transform worldHolder)
    {
        DropObjectObserver(obj, worldHolder);
    }

    // Função para soltar o objeto nos observadores
    [ObserversRpc]
    private void DropObjectObserver(GameObject obj, Transform worldHolder)
    {
        obj.transform.parent = worldHolder;

        // Reativa a física do objeto quando é solto
        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = false;
    }
}
