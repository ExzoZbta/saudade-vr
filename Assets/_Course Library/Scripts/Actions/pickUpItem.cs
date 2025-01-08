using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class pickUpItem : MonoBehaviour
{
    public GameObject obj, intText;
    public bool interactable;

    public InputActionReference pickupAction;

    private void Start()
    {
        interactable = false;

        if (pickupAction != null)
            pickupAction.action.Enable();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            intText.SetActive(true);
            interactable = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            intText.SetActive(false);
            interactable = false;
        }
    }

    private void Update()
    {
        if (interactable && pickupAction != null && pickupAction.action.triggered)
        {
            intText.SetActive(false);
            obj.SetActive(false);
            interactable = false;
        }
    }
}
