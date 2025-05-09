using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BodySocket
{
    public GameObject gameObject;
    [Range(0.01f, 1f)]
    public float heightRatio;
    public Vector3 initialLocalPosition;
    public string socketID; // Unique identifier for each socket
}

// Singleton to manage inventory across player states
public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("InventoryManager");
                _instance = go.AddComponent<InventoryManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // List of all inventory items
    public List<GameObject> itemsList = new List<GameObject>();

    // Current mapping of items to sockets
    public Dictionary<string, GameObject> socketToItem = new Dictionary<string, GameObject>();

    // Active body socket inventory
    public BodySocketInventory activeInventory;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Register a new item with the inventory system
    public void RegisterItem(GameObject item)
    {
        if (!itemsList.Contains(item))
        {
            itemsList.Add(item);
            Debug.Log($"Item {item.name} registered with inventory system");
        }
    }

    // Set active inventory (call when switching player states)
    public void SetActiveInventory(BodySocketInventory inventory)
    {
        if (inventory == activeInventory) return;

        activeInventory = inventory;
        Debug.Log($"Active inventory set to: {inventory.gameObject.name}");

        // Transfer all items to the new active inventory
        TransferItemsToActiveInventory();
    }

    // Transfer all tracked items to the active inventory's sockets
    public void TransferItemsToActiveInventory()
    {
        if (activeInventory == null) return;

        // Create map of socket IDs to socket GameObjects
        Dictionary<string, GameObject> idToSocket = new Dictionary<string, GameObject>();
        foreach (var socket in activeInventory.bodySockets)
        {
            idToSocket[socket.socketID] = socket.gameObject;
        }

        // Move each item to its appropriate socket in active inventory
        foreach (var entry in socketToItem)
        {
            string socketID = entry.Key;
            GameObject item = entry.Value;

            if (item == null) continue;

            if (idToSocket.TryGetValue(socketID, out GameObject socketObject))
            {
                // Parent item to new socket
                item.transform.SetParent(socketObject.transform);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
                item.SetActive(true);

                Debug.Log($"Transferred {item.name} to socket {socketID} on active inventory");
            }
            else
            {
                // If socket doesn't exist in active inventory, unparent the item
                item.transform.SetParent(null);
                Debug.LogWarning($"No socket {socketID} found in active inventory for item {item.name}");
            }
        }
    }

    // Update item-socket mapping when item is placed in a socket
    public void UpdateItemSocketMapping(string socketID, GameObject item)
    {
        socketToItem[socketID] = item;
    }

    // Remove item from socket mapping
    public void RemoveItemFromSocket(string socketID)
    {
        if (socketToItem.ContainsKey(socketID))
        {
            socketToItem.Remove(socketID);
        }
    }
}

public class BodySocketInventory : MonoBehaviour
{
    public GameObject HMD;
    public BodySocket[] bodySockets;
    private Vector3 _currentHMDlocalPosition;
    private Quaternion _currentHMDRotation;

    // Used for initialization
    private bool _initialized = false;

    private void OnEnable()
    {
        // Set this as the active inventory whenever this object is enabled
        // (but wait until Start has been called to ensure initialization)
        if (_initialized && gameObject.activeInHierarchy)
        {
            Debug.Log($"Setting {gameObject.name} as active inventory (OnEnable)");
            InventoryManager.Instance.SetActiveInventory(this);
        }
    }

    void Start()
    {
        // Initialize socket items
        if (HMD == null)
        {
            HMD = Camera.main.gameObject;
            Debug.Log("HMD auto-assigned to main camera");
        }

        foreach (var bodySocket in bodySockets)
        {
            // Ensure each socket has a unique ID
            if (string.IsNullOrEmpty(bodySocket.socketID))
            {
                bodySocket.socketID = gameObject.name + "_" + bodySocket.gameObject.name;
                Debug.Log($"Auto-assigned socket ID: {bodySocket.socketID}");
            }

            bodySocket.initialLocalPosition = bodySocket.gameObject.transform.localPosition;

            // Check for items in this socket and register them
            if (bodySocket.gameObject.transform.childCount > 0)
            {
                GameObject item = bodySocket.gameObject.transform.GetChild(0).gameObject;
                InventoryManager.Instance.RegisterItem(item);
                InventoryManager.Instance.UpdateItemSocketMapping(bodySocket.socketID, item);
            }
        }

        _initialized = true;

        // Set this as the active inventory if this object is active
        if (gameObject.activeInHierarchy)
        {
            Debug.Log($"Setting {gameObject.name} as active inventory (Start)");
            InventoryManager.Instance.SetActiveInventory(this);
        }
    }

    void Update()
    {
        if (HMD == null) return;

        _currentHMDlocalPosition = HMD.transform.localPosition;
        _currentHMDRotation = HMD.transform.rotation;

        // First update the inventory's position and rotation
        UpdateSocketInventory();

        // Then update individual socket positions
        foreach (var bodySocket in bodySockets)
        {
            UpdateBodySocketHeight(bodySocket);

            // Check for new items in sockets and update mappings
            CheckForNewItemsInSocket(bodySocket);
        }
    }

    private void CheckForNewItemsInSocket(BodySocket bodySocket)
    {
        if (bodySocket.gameObject.transform.childCount > 0)
        {
            GameObject item = bodySocket.gameObject.transform.GetChild(0).gameObject;

            // Register new items with inventory manager
            InventoryManager.Instance.RegisterItem(item);

            // Update socket mapping
            InventoryManager.Instance.UpdateItemSocketMapping(bodySocket.socketID, item);
        }
        else if (InventoryManager.Instance.socketToItem.TryGetValue(bodySocket.socketID, out GameObject mappedItem))
        {
            // Socket is empty but has mapped item - check if it was removed
            if (mappedItem.transform.parent != bodySocket.gameObject.transform)
            {
                InventoryManager.Instance.RemoveItemFromSocket(bodySocket.socketID);
            }
        }
    }

    private void UpdateBodySocketHeight(BodySocket bodySocket)
    {
        // Only modify the Y position based on height ratio, keep X and Z from initial position
        bodySocket.gameObject.transform.localPosition = new Vector3(
            bodySocket.initialLocalPosition.x,
            (_currentHMDlocalPosition.y * bodySocket.heightRatio),
            bodySocket.initialLocalPosition.z
        );
    }

    private void UpdateSocketInventory()
    {
        // Update inventory position - follow HMD on X and Z axes only
        transform.localPosition = new Vector3(_currentHMDlocalPosition.x, 0, _currentHMDlocalPosition.z);

        // Only use Y rotation from HMD to prevent tilting
        transform.rotation = Quaternion.Euler(0, HMD.transform.rotation.eulerAngles.y, 0);
    }

    private void OnDisable()
    {
        // When this inventory is disabled, notify the manager
        Debug.Log($"Inventory {gameObject.name} disabled");
    }
}