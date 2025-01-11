using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// 
/// Custom move provider that allows players to sprint in a VR environment.
/// Players can sprint by pressing a button or performing an arm-swing gesture.
/// 
public class SprintContinuousMoveProvider : ActionBasedContinuousMoveProvider
{
    [Header("Sprint Settings")]
    [SerializeField]
    private float sprintSpeed = 4.0f;
    [SerializeField]
    private InputActionProperty sprintAction;

    [Header("Arm Swing Settings")]
    [SerializeField]
    private ActionBasedController leftController;
    [SerializeField]
    private ActionBasedController rightController;
    [SerializeField]
    [Tooltip("Minimum vertical velocity needed to trigger sprint")]
    private float verticalSwingThreshold = 0.0f;
    [SerializeField]
    [Tooltip("How long to maintain sprint after arm swing stops")]
    private float sprintCooldown = 0.5f;
    [SerializeField]
    [Tooltip("Number of samples to use for velocity smoothing")]
    private int velocitySmoothingSamples = 3;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs = true;

    private float defaultMoveSpeed;
    private float lastSprintTime;
    private bool isSprintingFromButton;

    private Vector3 previousLeftPosition;
    private Vector3 previousRightPosition;
    private Queue<Vector3> leftVelocitySamples = new Queue<Vector3>();
    private Queue<Vector3> rightVelocitySamples = new Queue<Vector3>();

    // Initialize the move provider and log default settings
    protected override void Awake()
    {
        base.Awake();
        defaultMoveSpeed = moveSpeed;
        Debug.Log($"[Sprint Provider] Initialized with defaultMoveSpeed: {defaultMoveSpeed}, sprintSpeed: {sprintSpeed}");
    }

    // Enable the input actions and reset tracking when the object becomes active
    protected new void OnEnable()
    {
        base.OnEnable();
        sprintAction.action.performed += OnSprintPressed;
        sprintAction.action.canceled += OnSprintReleased;
        ResetArmTracking();

        if (leftController == null || rightController == null)
        {
            Debug.LogError("[Sprint Provider] One or both controllers are not assigned!");
        }
    }

    // Disable the input actions when the object is deactivated
    protected new void OnDisable()
    {
        base.OnDisable();
        sprintAction.action.performed -= OnSprintPressed;
        sprintAction.action.canceled -= OnSprintReleased;
    }

    // Resets tracking positions and clears velocity sample queues
    private void ResetArmTracking()
    {
        if (leftController != null) previousLeftPosition = leftController.transform.position;
        if (rightController != null) previousRightPosition = rightController.transform.position;

        leftVelocitySamples.Clear();
        rightVelocitySamples.Clear();

        for (int i = 0; i < velocitySmoothingSamples; i++)
        {
            leftVelocitySamples.Enqueue(Vector3.zero);
            rightVelocitySamples.Enqueue(Vector3.zero);
        }
    }

    // Check for arm-swing patterns to control sprint activation
    protected void LateUpdate()
    {
        if (!isSprintingFromButton)
        {
            CheckArmSwingPattern();
        }
    }

    // Smooths velocity over a sliding window of samples
    private Vector3 GetSmoothedVelocity(Vector3 currentVelocity, Queue<Vector3> samples)
    {
        samples.Dequeue();
        samples.Enqueue(currentVelocity);

        Vector3 smoothedVelocity = Vector3.zero;
        foreach (Vector3 sample in samples)
        {
            smoothedVelocity += sample;
        }
        return smoothedVelocity / samples.Count;
    }

    // Determines if the arm-swing pattern is sufficient to trigger sprint
    private void CheckArmSwingPattern()
    {
        if (leftController == null || rightController == null) return;

        // Calculate instantaneous velocities
        Vector3 leftVelocity = (leftController.transform.position - previousLeftPosition) / Time.deltaTime;
        Vector3 rightVelocity = (rightController.transform.position - previousRightPosition) / Time.deltaTime;

        // Update previous positions
        previousLeftPosition = leftController.transform.position;
        previousRightPosition = rightController.transform.position;

        // Get smoothed velocities
        Vector3 smoothedLeftVelocity = GetSmoothedVelocity(leftVelocity, leftVelocitySamples);
        Vector3 smoothedRightVelocity = GetSmoothedVelocity(rightVelocity, rightVelocitySamples);

        // Check if both controllers exceed the threshold
        bool leftSwinging = Mathf.Abs(smoothedLeftVelocity.y) > verticalSwingThreshold;
        bool rightSwinging = Mathf.Abs(smoothedRightVelocity.y) > verticalSwingThreshold;

        // Only trigger sprint if both controllers are swinging
        if (leftSwinging && rightSwinging)
        {
            lastSprintTime = Time.time;
            if (moveSpeed != sprintSpeed)
            {
                moveSpeed = sprintSpeed;
                if (showDebugLogs)
                {
                    Debug.Log($"[Sprint Provider] Sprint activated! Left vel: {smoothedLeftVelocity.y}, Right vel: {smoothedRightVelocity.y}");
                }
            }
        }
        else if (Time.time - lastSprintTime > sprintCooldown)
        {
            if (moveSpeed != defaultMoveSpeed)
            {
                moveSpeed = defaultMoveSpeed;
                if (showDebugLogs)
                {
                    Debug.Log("[Sprint Provider] Sprint deactivated");
                }
            }
        }

        // Debug output for velocity and sprinting status (logged every 60 frames)
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Sprint Provider] Velocities - Left: {smoothedLeftVelocity.y:F2}, Right: {smoothedRightVelocity.y:F2}, Is Sprinting: {moveSpeed == sprintSpeed}");
        }
    }

    // Triggered when sprint button is pressed
    private void OnSprintPressed(InputAction.CallbackContext context)
    {
        isSprintingFromButton = true;
        moveSpeed = sprintSpeed;
    }

    // Triggered when sprint button is released
    private void OnSprintReleased(InputAction.CallbackContext context)
    {
        isSprintingFromButton = false;
        moveSpeed = defaultMoveSpeed;
    }
}