using UnityEngine;
using TMPro;
using System.Collections.Generic;

public enum TapeType
{
    First = 0,
    Second = 1,
    Third = 2,
}

public class VHSStationManager : MonoBehaviour
{
    public static VHSStationManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI progressText;

    [Header("Prefabs and Spawn Points")]
    public GameObject[] stationPrefabs;     // Array of different station prefabs (TapeSpawn, TapeSpawn 2, TapeSpawn 3)
    public Transform[] spawnPoints;          // Array of spawn locations

    private HashSet<TapeType> collectedTapes = new HashSet<TapeType>();
    private Dictionary<TapeType, GameObject> activeStations = new Dictionary<TapeType, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        collectedTapes.Clear();
        activeStations.Clear();

        // Only spawn the first station at game start
        SpawnStation(TapeType.First);
        UpdateUI();
    }

    public void CollectTape(TapeType tapeType)
    {
        Debug.Log($"Collecting tape: {tapeType}");

        if (!collectedTapes.Contains(tapeType))
        {
            collectedTapes.Add(tapeType);
            Debug.Log($"Total tapes collected: {collectedTapes.Count}");

            // Handle spawning the next station based on which tape was collected
            HandleTapeCollection(tapeType);
            UpdateUI();
        }
        else
        {
            Debug.Log($"Tape {tapeType} already collected, not counting it again");
        }
    }

    private void HandleTapeCollection(TapeType collectedType)
    {
        switch (collectedType)
        {
            case TapeType.First:
                // Spawn second station after collecting first tape
                SpawnStation(TapeType.Second);
                break;

            case TapeType.Second:
                // Spawn third station after collecting second tape
                SpawnStation(TapeType.Third);
                break;

            case TapeType.Third:
                // All tapes collected
                Debug.Log("All VHS tapes have been collected!");
                // You can add game completion logic here
                break;
        }
    }

    private void SpawnStation(TapeType tapeType)
    {
        int typeIndex = (int)tapeType;

        // Only spawn if we don't already have an active station of this type
        if (!activeStations.ContainsKey(tapeType))
        {
            // Validate we have enough prefabs in the array
            if (stationPrefabs == null || stationPrefabs.Length <= typeIndex)
            {
                Debug.LogError($"Missing prefab for station type {tapeType}! Make sure to assign all prefabs in the inspector.");
                return;
            }

            if (stationPrefabs[typeIndex] == null)
            {
                Debug.LogError($"Prefab for station type {tapeType} is null! Check the inspector.");
                return;
            }

            // Get the spawn point for this type
            if (spawnPoints == null || spawnPoints.Length <= typeIndex)
            {
                Debug.LogError($"Missing spawn point for station type {tapeType}! Make sure to assign all spawn points in the inspector.");
                return;
            }

            if (spawnPoints[typeIndex] == null)
            {
                Debug.LogError($"Spawn point for station type {tapeType} is null! Check the inspector.");
                return;
            }

            Transform spawnPoint = spawnPoints[typeIndex];
            GameObject prefabToSpawn = stationPrefabs[typeIndex];

            // Instantiate the station
            GameObject newStation = Instantiate(
                prefabToSpawn,
                spawnPoint.position,
                spawnPoint.rotation
            );

            // Set up PlayVHS components
            PlayVHS[] playVHSComponents = newStation.GetComponentsInChildren<PlayVHS>(true);

            if (playVHSComponents.Length == 0)
            {
                Debug.LogWarning($"No PlayVHS components found in station {tapeType}");
            }

            // Set up each PlayVHS component found
            foreach (PlayVHS playVHS in playVHSComponents)
            {
                playVHS.stationType = tapeType;
                Debug.Log($"Set station type {tapeType} for PlayVHS component");
            }

            // Store the active station
            activeStations.Add(tapeType, newStation);
            Debug.Log($"Spawned VHS Station {tapeType} at {spawnPoint.name} using prefab {prefabToSpawn.name}");
        }
        else
        {
            Debug.Log($"Station {tapeType} is already active, not spawning again");
        }
    }

    private void UpdateUI()
    {
        if (progressText != null)
        {
            progressText.SetText($"Tapes: {collectedTapes.Count}/3");
            Debug.Log($"Updated UI text to: Tapes: {collectedTapes.Count}/3");
        }
        else
        {
            Debug.LogError("Progress Text is null when trying to update UI!");
        }
    }
}