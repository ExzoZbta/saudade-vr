using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Optional helper script to attach to all inventory items
public class InventoryItem : MonoBehaviour
{
    // Keep track of which socket this item belongs to
    private string currentSocketID;

    private void OnEnable()
    {
        // Register with inventory manager when enabled
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RegisterItem(gameObject);
        }
    }

    public void SetCurrentSocket(string socketID)
    {
        currentSocketID = socketID;
    }

    public string GetCurrentSocket()
    {
        return currentSocketID;
    }

    // This can be used to handle item pickup/placement logic
    public void PlaceInSocket(GameObject socket, string socketID)
    {
        transform.SetParent(socket.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        currentSocketID = socketID;

        // Update mapping in inventory manager
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UpdateItemSocketMapping(socketID, gameObject);
        }
    }
}