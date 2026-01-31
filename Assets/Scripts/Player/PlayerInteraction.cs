using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float playerInteractionDistance = 30f;
    public BasicInteraction currentInteraction;
    private CharacterController characterController;
    private bool isInDialogue;

    private Vector3 closetPosition = new(13.4200001f,-12.9350004f,1.92499995f);
    
    [SerializeField] GameObject interactTextHUD;
    
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        CheckInteraction();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithCurrent();
        }
        else if (Input.GetKeyDown(KeyCode.K) && currentInteraction != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Additive);
        }
        
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
        interactTextHUD.gameObject.SetActive(true);
    }

    void DisableCurrentInteraction()
    {
        interactTextHUD.gameObject.SetActive(false);
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

    public void ComeOutOfTheCloset()
    {
        transform.position = closetPosition;
    }
}
