using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Video;
using System.Collections;

public class PlayVHS : MonoBehaviour
{
    [Header("References")]
    public GameObject screen;
    public Animator vhsAnim;
    public TapeType stationType;
    public float videoTime = 30f;

    private VideoPlayer videoPlayer;
    private XRSocketInteractor socketInteractor;
    private bool isPlaying = false;
    private bool hasBeenCollected = false;  // Track if this tape has been collected

    private void Start()
    {
        socketInteractor = GetComponent<XRSocketInteractor>();

        // Get the VideoPlayer component
        if (screen != null)
        {
            videoPlayer = screen.GetComponent<VideoPlayer>();
            // Make sure the video doesn't auto-play when the object spawns
            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.Stop();
                // Log video clip info for debugging
                if (videoPlayer.clip != null)
                {
                    Debug.Log($"Station {stationType} has video clip: {videoPlayer.clip.name}");
                }
                else
                {
                    Debug.LogWarning($"Station {stationType} has no video clip assigned!");
                }
            }
            else
            {
                Debug.LogError($"No VideoPlayer component found on screen object for station {stationType}");
            }
        }
        else
        {
            Debug.LogError($"Screen reference is missing for station {stationType}");
        }

        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.AddListener(OnVHSInserted);
            Debug.Log($"PlayVHS initialized with station type: {stationType}");
        }
        else
        {
            Debug.LogError($"No XRSocketInteractor found on station {stationType}");
        }
    }

    private void OnDestroy()
    {
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnVHSInserted);
        }
    }

    private void OnVHSInserted(SelectEnterEventArgs args)
    {
        if (!isPlaying)
        {
            // Check if we have a valid video player and clip
            if (videoPlayer == null)
            {
                Debug.LogError($"Cannot play video for station {stationType}: No VideoPlayer component");
                return;
            }

            if (videoPlayer.clip == null)
            {
                Debug.LogError($"Cannot play video for station {stationType}: No video clip assigned");
                return;
            }

            Debug.Log($"VHS inserted in station type: {stationType}");

            // Trigger animation if animator exists
            if (vhsAnim != null)
            {
                vhsAnim.SetTrigger("play");
            }
            else
            {
                Debug.LogWarning($"No animator found for station {stationType}");
            }

            StartCoroutine(PlayVHSTape());
            isPlaying = true;

            // Only notify the station manager once per tape
            if (!hasBeenCollected && VHSStationManager.Instance != null)
            {
                VHSStationManager.Instance.CollectTape(stationType);
                hasBeenCollected = true;
            }
            else if (VHSStationManager.Instance == null)
            {
                Debug.LogError("VHSStationManager.Instance is null!");
            }
        }
    }

    private IEnumerator PlayVHSTape()
    {
        yield return new WaitForSeconds(1.0f);

        if (videoPlayer != null && videoPlayer.clip != null)
        {
            videoPlayer.Play();
            Debug.Log($"Video started playing for station: {stationType}, clip: {videoPlayer.clip.name}");

            yield return new WaitForSeconds(videoTime);

            videoPlayer.Stop();
            Debug.Log($"Video stopped for station: {stationType}");
        }
        else
        {
            Debug.LogError($"Failed to play video for station {stationType}: VideoPlayer or clip is null");
        }

        isPlaying = false;
    }
}