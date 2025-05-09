using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HidingPlace : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject hideText;
    public GameObject stopHideText;

    [Header("Player References")]
    public GameObject playerRig; // The single XR rig we'll use
    public Transform hidingAnchor; // Position inside the hiding place

    [Header("Enemy References")]
    public EnemyAI monsterScript;
    public Transform monsterTransform;
    public float loseDistance;

    [Header("Input & Sound")]
    public InputActionReference hideAction;
    public InputActionReference unhideAction;
    public AudioSource hideSound, stopHideSound;
    public RoomDetector detector;

    [Header("Hiding Settings")]
    public bool isBedHiding = false; // Flag to indicate if this is a bed hiding spot
    public float bedHidingHeightOffset = 0.25f; // Adjust if needed to prevent clipping with the bed

    // State tracking
    private bool interactable = false;
    private bool hiding = false;

    // Player movement reference
    private MonoBehaviour sprintMoveProvider;
    private bool originalMoveProviderState;

    // Original position tracking
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Camera reference
    private Camera playerCamera;

    // Collider reference for bed hiding
    private Collider bedCollider;
    private bool colliderDisabledForHiding = false;

    private void Start()
    {
        if (hideAction != null)
            hideAction.action.Enable();
        if (unhideAction != null)
            unhideAction.action.Enable();

        // Find the Sprint Move Provider
        sprintMoveProvider = FindSprintMoveProvider();

        // Get player camera
        playerCamera = playerRig.GetComponentInChildren<Camera>();

        if (playerCamera == null)
            Debug.LogError("Could not find camera in player rig!");

        // If this is a bed hiding spot, get the collider
        if (isBedHiding)
        {
            bedCollider = GetComponent<Collider>();
            if (bedCollider == null)
                bedCollider = GetComponentInChildren<Collider>();
        }
    }

    private MonoBehaviour FindSprintMoveProvider()
    {
        // Find and store a reference to your custom move provider
        MonoBehaviour[] components = playerRig.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component != null && component.GetType().Name == "SprintContinuousMoveProvider")
            {
                return component;
            }
        }

        Debug.LogWarning("SprintContinuousMoveProvider not found! Movement may not be properly disabled when hiding.");
        return null;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MainCamera") && detector.inTrigger)
        {
            // Only show hide text if player is not already hiding
            if (!hiding)
            {
                hideText.SetActive(true);
                interactable = true;
            }
        }
        else if (detector.inTrigger == false)
        {
            hideText.SetActive(false);
            interactable = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            hideText.SetActive(false);
            interactable = false;
        }
    }

    private void Update()
    {
        if (interactable && hideAction != null && hideAction.action.triggered && !hiding)
        {
            EnterHidingPlace();
        }

        if (hiding && unhideAction != null && unhideAction.action.triggered)
        {
            ExitHidingPlace();
        }
    }

    private void EnterHidingPlace()
    {
        hideText.SetActive(false);
        hideSound.Play();

        // Store original position and rotation to return to later
        originalPosition = playerRig.transform.position;
        originalRotation = playerRig.transform.rotation;

        // Disable movement
        if (sprintMoveProvider != null)
        {
            originalMoveProviderState = sprintMoveProvider.enabled;
            sprintMoveProvider.enabled = false;
        }

        // If this is a bed, temporarily disable its collider to avoid physics issues
        if (isBedHiding && bedCollider != null)
        {
            bedCollider.enabled = false;
            colliderDisabledForHiding = true;
        }

        // Position the player at the hiding anchor
        PositionPlayerInHidingPlace();

        // Re-enable bed collider after positioning if needed
        if (isBedHiding && colliderDisabledForHiding && bedCollider != null)
        {
            bedCollider.enabled = true;
        }

        // Check if monster should stop chasing
        float distance = Vector3.Distance(monsterTransform.position, playerRig.transform.position);
        if (distance > loseDistance && monsterScript.chasing)
        {
            monsterScript.stopChase();
        }

        stopHideText.SetActive(true);
        hiding = true;
        interactable = false;
    }

    private void ExitHidingPlace()
    {
        stopHideText.SetActive(false);
        stopHideSound.Play();

        // If this is a bed, temporarily disable its collider again to avoid physics issues when exiting
        if (isBedHiding && bedCollider != null)
        {
            bedCollider.enabled = false;
            colliderDisabledForHiding = true;
        }

        // Return player to original position
        StartCoroutine(ReturnPlayerToOriginalPosition());

        hiding = false;
        hideText.SetActive(false);
    }

    private void PositionPlayerInHidingPlace()
    {
        if (hidingAnchor == null)
        {
            Debug.LogError("Hiding anchor is not assigned! Please assign an empty GameObject inside the hiding place.");
            return;
        }

        if (playerCamera != null)
        {
            // Calculate the height offset between camera and rig origin
            float heightOffset = playerCamera.transform.position.y - playerRig.transform.position.y;

            // Position the player rig so the camera will be at the hiding anchor position
            Vector3 targetPosition = hidingAnchor.position;

            // For bed hiding, make sure we're not too close to the ground
            if (isBedHiding)
            {
                // Apply additional height offset for bed hiding to prevent falling through
                targetPosition.y = hidingAnchor.position.y - heightOffset + bedHidingHeightOffset;
            }
            else
            {
                targetPosition.y -= heightOffset;
            }

            // Move the player rig
            playerRig.transform.position = targetPosition;
            playerRig.transform.rotation = hidingAnchor.rotation;

            Debug.Log($"Positioned player at {targetPosition}, camera should be at {hidingAnchor.position}");
        }
    }

    private IEnumerator ReturnPlayerToOriginalPosition()
    {
        // Small delay to ensure things settle
        yield return new WaitForSeconds(0.1f);

        // Return player to original position
        playerRig.transform.position = originalPosition;
        playerRig.transform.rotation = originalRotation;

        // Re-enable bed collider after positioning if needed
        if (isBedHiding && colliderDisabledForHiding && bedCollider != null)
        {
            bedCollider.enabled = true;
            colliderDisabledForHiding = false;
        }

        // Restore movement
        if (sprintMoveProvider != null)
        {
            sprintMoveProvider.enabled = originalMoveProviderState;
        }
    }

    private void OnDisable()
    {
        // Disable input actions when the script is disabled
        if (hideAction != null)
            hideAction.action.Disable();
        if (unhideAction != null)
            unhideAction.action.Disable();
    }
}
