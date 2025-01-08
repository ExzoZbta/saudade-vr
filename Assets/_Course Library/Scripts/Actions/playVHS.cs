using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject vhs, screen;
    public Animator vhsAnim;

    public float videoTime;

    private VideoPlayer videoPlayer;
    private XRSocketInteractor socketInteractor;
    private bool isPlaying = false;

    private void Start()
    {
        // Get the XR Socket Interactor component
        socketInteractor = GetComponent<XRSocketInteractor>();
        videoPlayer = screen.GetComponent<VideoPlayer>();

        // Subscribe to the socket's select entered event
        socketInteractor.selectEntered.AddListener(OnVHSInserted);
        Debug.Log("PlayVHS initialized on socket: " + gameObject.name);
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (socketInteractor != null)
        {
            socketInteractor.selectEntered.RemoveListener(OnVHSInserted);
        }
    }

    private void OnVHSInserted(SelectEnterEventArgs args)
    {
        Debug.Log("Socket received VHS tape");
        if (!isPlaying && args.interactableObject.transform.CompareTag("VHS"))
        {
            StartVHSPlayback();
        }
    }

    private void StartVHSPlayback()
    {
        if (!isPlaying)
        {
            Debug.Log("Starting VHS animation");
            isPlaying = true;
            vhsAnim.SetTrigger("play");
            StartCoroutine(PlayVHSTape());
        }
    }

    private IEnumerator PlayVHSTape()
    {
        // Wait for VHS insertion animation
        yield return new WaitForSeconds(1.0f);

        Debug.Log("Starting video playback");
        videoPlayer.Play();

        // Wait for specified video time
        yield return new WaitForSeconds(videoTime);

        // Stop video
        videoPlayer.Stop();
        isPlaying = false;
        Debug.Log("Video playback complete");
    }

}
