using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInteraction : MonoBehaviour
{
    [SerializeField] private float raycastDistance;
    [SerializeField] private LayerMask treeLayer;
    [SerializeField] private KeyCode interactionKey = KeyCode.X;
    [SerializeField] private GameObject fruitPrefab; // Prefab da fruta a ser gerada
    [SerializeField] private Transform fruitSpawnPoint; // Ponto de spawn da fruta

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        CheckTreeInteraction();
    }

    private void CheckTreeInteraction()
    {
        // Verifica se o jogador pressionou a tecla de interação e está olhando para uma árvore
        if (Input.GetKeyDown(interactionKey) && Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, raycastDistance, treeLayer))
        {
            // Instancia uma fruta no ponto de spawn da fruta
            Instantiate(fruitPrefab, fruitSpawnPoint.position, Quaternion.identity);

            // Você também pode adicionar lógica adicional aqui, como reduzir a quantidade de frutas disponíveis na árvore, etc.
        }
    }
}
