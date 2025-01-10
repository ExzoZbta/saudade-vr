using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class SprintContinuousMoveProvider : ActionBasedContinuousMoveProvider
{
    [Header("Sprint Settings")]
    [SerializeField]
    [Tooltip("The speed multiplier when sprinting")]
    private float sprintSpeed = 4.0f;

    [SerializeField]
    [Tooltip("Input action for sprint (typically the joystick click/press)")]
    private InputActionProperty sprintAction;

    private float defaultMoveSpeed;

    protected override void Awake()
    {
        base.Awake();
        defaultMoveSpeed = moveSpeed; // Store the default speed set in inspector
    }

    protected new void OnEnable()
    {
        base.OnEnable();
        sprintAction.action.performed += OnSprintPressed;
        sprintAction.action.canceled += OnSprintReleased;
    }

    protected new void OnDisable()
    {
        base.OnDisable();
        sprintAction.action.performed -= OnSprintPressed;
        sprintAction.action.canceled -= OnSprintReleased;
    }

    private void OnSprintPressed(InputAction.CallbackContext context)
    {
        moveSpeed = sprintSpeed;
    }

    private void OnSprintReleased(InputAction.CallbackContext context)
    {
        moveSpeed = defaultMoveSpeed;
    }
}