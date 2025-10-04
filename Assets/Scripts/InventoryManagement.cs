using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManagement : MonoBehaviour
{
    [SerializeField] private Vector3[] itemsPositions;
    [SerializeField] private GameObject inventory;
    
    public List<GameObject> _powerUps;

    public bool AddItem(GameObject powerUp)
    {
        if (!powerUp) Debug.Log("Power-up is null.");
        if (itemsPositions.Length < _powerUps.Count + 1) return false;
        
        _powerUps.Add(powerUp);
        powerUp.transform.position = itemsPositions[_powerUps.Count - 1];
        powerUp.transform.SetParent(inventory.transform, true);
        
        return true;
    }

    public void RemoveItem(GameObject powerUp)
    {
        // TODO
    }

    public void ArrangeItems()
    {
        // TODO: rearrange items after removing one from the inventory
    }
}
