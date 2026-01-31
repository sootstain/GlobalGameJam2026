using System;
using TMPro;
using UnityEngine;


public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float playerInteractionDistance = 30f;
    BasicInteraction currentInteraction;
    private CharacterController characterController;
    private bool isInDialogue;

    [SerializeField] TMP_Text interactText;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        CheckInteraction();
        TryInteractWithCurrent();
        
        if (characterController != null && !characterController.enabled && !isInDialogue)
        {
            if (currentInteraction == null || !currentInteraction.isCurrentConversation)
            {
                characterController.enabled = true;
            }
        }
    }



    private void TryInteractWithCurrent()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (currentInteraction == null)
            return;

        if (characterController != null)
            characterController.enabled = false;

        currentInteraction.Interact();
    }

    public void CheckInteraction()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, playerInteractionDistance))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                BasicInteraction npcInteraction = hit.collider.GetComponent<BasicInteraction>();
                if (npcInteraction.enabled)
                {
                    SetCurrentInteraction(npcInteraction);
                    isInDialogue = npcInteraction.isCurrentConversation;
                }
                else
                {
                    DisableCurrentInteraction();
                }
            }
            else
            {
                DisableCurrentInteraction();
            }
        }
        else
        {
            DisableCurrentInteraction();
        }
    }
    
    void SetCurrentInteraction(BasicInteraction interaction)
    {
        currentInteraction = interaction;
        currentInteraction.EnableOutline();
        interactText.gameObject.SetActive(true);
    }

    void DisableCurrentInteraction()
    {
        interactText.gameObject.SetActive(false);
        if (currentInteraction)
        {
            currentInteraction.DisableOutline();
            currentInteraction = null;
        }
    }

    public void TestInteraction()
    {
        Debug.Log("Interaction");
    }
}
