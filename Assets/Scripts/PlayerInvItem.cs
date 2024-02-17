using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[CreateAssetMenu(fileName = "PlayerInvItem", menuName = "Inventory/Item", order = 1)]
public class PlayerInvItem : ScriptableObject
{
    public string itemName;
    public GameObject prefab;

}