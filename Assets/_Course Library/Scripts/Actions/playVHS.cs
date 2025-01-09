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

    private void Start()
    {
        socketInteractor = GetComponent<XRSocketInteractor>();
        videoPlayer = screen.GetComponent<VideoPlayer>();
        socketInteractor.selectEntered.AddListener(OnVHSInserted);

        Debug.Log($"PlayVHS initialized with station type: {stationType}");
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
            // Notify the station manager that this tape was collected
            Debug.Log($"VHS inserted in station type: {stationType}");
            vhsAnim.SetTrigger("play");
            StartCoroutine(PlayVHSTape());
            isPlaying = true;
            VHSStationManager.Instance.CollectTape(stationType);
        }
    }

    private IEnumerator PlayVHSTape()
    {
        yield return new WaitForSeconds(1.0f);
        videoPlayer.Play();

        yield return new WaitForSeconds(videoTime);

        videoPlayer.Stop();
        isPlaying = false;
    }
}