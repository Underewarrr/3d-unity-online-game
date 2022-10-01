    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
 
public class PlayerColorNetwork : NetworkBehaviour
{
    public GameObject body;
    public Color endColor;
 
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            
        }
        else
        {
            GetComponent<PlayerColorNetwork>().enabled = false;
        }
    }
 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ChangeColorServer(gameObject, endColor);
        }
    }
 
    [ServerRpc]
    public void ChangeColorServer(GameObject player, Color color)
    {
        // that not easy, to call result  we need to pass for the server RPC
        ChangeColor(player, color);
    }
 
    [ObserversRpc]
    public void ChangeColor(GameObject player, Color color)
    {
        // get script from player refence over to the network 
        player.GetComponent<PlayerColorNetwork>().body.GetComponent<Renderer>().material.color = color;
    }
}