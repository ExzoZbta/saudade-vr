using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HidingPlace : MonoBehaviour
{
    public GameObject hideText, stopHideText;
    public GameObject normalPlayer, hidingPlayer;
    public EnemyAI monsterScript;
    public Transform monsterTransform;
    bool interactable, hiding;
    public float loseDistance;
    public RoomDetector detector;

    public InputActionReference hideAction;
    public InputActionReference unhideAction;

    public AudioSource hideSound, stopHideSound;


    private void Start()
    {
        interactable = false;
        hiding = false;

        if (hideAction != null)
            hideAction.action.Enable();
        if (unhideAction != null)
            unhideAction.action.Enable();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MainCamera") && detector.inTrigger)
        {
            hideText.SetActive(true);
            interactable = true;
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
        if (interactable && hideAction != null && hideAction.action.triggered)
        {
            hideText.SetActive(false);
            hideSound.Play();
            hidingPlayer.SetActive(true);
            float distance = Vector3.Distance(monsterTransform.position, normalPlayer.transform.position);

            if (distance > loseDistance)
            {
                if (monsterScript.chasing == true)
                {
                    monsterScript.stopChase();
                }
            }

            stopHideText.SetActive(true);
            hiding = true;
            normalPlayer.SetActive(false);
            interactable = false;
        }
        if (hiding && unhideAction != null && unhideAction.action.triggered)
        {
            stopHideText.SetActive(false);
            stopHideSound.Play();
            normalPlayer.SetActive(true);
            hidingPlayer.SetActive(false);
            hiding = false;
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
