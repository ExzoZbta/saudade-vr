using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum TapeType
{
    First = 0,
    Second = 1,
    Third = 2,
    Fourth = 3,
    Fifth = 4,
    Sixth = 5
}

public class VHSStationManager : MonoBehaviour
{
    public static VHSStationManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI progressText;
    public Image progressBar;

    [Header("Prefabs and Spawn Points")]
    public GameObject completeStationPrefab; // The complete VHS station setup prefab
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
        // Only spawn the first station at game start
        SpawnStation(TapeType.First);
        UpdateUI();
    }

    public void CollectTape(TapeType tapeType)
    {
        if (!collectedTapes.Contains(tapeType))
        {
            collectedTapes.Add(tapeType);
            HandleTapeCollection(tapeType);
            UpdateUI();
        }
    }

    private void HandleTapeCollection(TapeType collectedType)
    {
        switch (collectedType)
        {
            case TapeType.First:
                // Randomly spawn one of tapes 2, 3, or 4
                SpawnRandomStation(new[] { TapeType.Second, TapeType.Third, TapeType.Fourth });
                break;

            case TapeType.Second:
            case TapeType.Third:
            case TapeType.Fourth:
                if (collectedTapes.Contains(TapeType.Second) &&
                    collectedTapes.Contains(TapeType.Third) &&
                    collectedTapes.Contains(TapeType.Fourth))
                {
                    SpawnStation(TapeType.Fifth);
                }
                else
                {
                    // Spawn one of the remaining stations from 2,3,4
                    var remainingStations = new List<TapeType>();
                    if (!collectedTapes.Contains(TapeType.Second)) remainingStations.Add(TapeType.Second);
                    if (!collectedTapes.Contains(TapeType.Third)) remainingStations.Add(TapeType.Third);
                    if (!collectedTapes.Contains(TapeType.Fourth)) remainingStations.Add(TapeType.Fourth);

                    if (remainingStations.Count > 0)
                    {
                        SpawnRandomStation(remainingStations.ToArray());
                    }
                }
                break;

            case TapeType.Fifth:
                SpawnStation(TapeType.Sixth);
                break;
        }
    }

    private void SpawnRandomStation(TapeType[] possibleTypes)
    {
        if (possibleTypes.Length > 0)
        {
            int randomIndex = Random.Range(0, possibleTypes.Length);
            SpawnStation(possibleTypes[randomIndex]);
        }
    }

    private void SpawnStation(TapeType tapeType)
    {
        if (!activeStations.ContainsKey(tapeType))
        {
            // Get the spawn point for this type
            Transform spawnPoint = spawnPoints[(int)tapeType];

            // Instantiate the station
            GameObject newStation = Instantiate(completeStationPrefab,
                                             spawnPoint.position,
                                             spawnPoint.rotation);

            // Find all PlayVHS components in the station (including children)
            PlayVHS[] playVHSComponents = newStation.GetComponentsInChildren<PlayVHS>();

            // Set up each PlayVHS component found
            foreach (PlayVHS playVHS in playVHSComponents)
            {
                playVHS.stationType = tapeType;
                Debug.Log($"Set station type {tapeType} for PlayVHS component");
            }

            // Store the active station
            activeStations.Add(tapeType, newStation);

            Debug.Log($"Spawned VHS Station {tapeType} at {spawnPoint.name}");
        }
    }

    private void UpdateUI()
    {
        float progress = (float)collectedTapes.Count / 6f;
        progressBar.fillAmount = progress;
        progressText.text = $"VHS Tapes: {collectedTapes.Count}/6";
    }
}