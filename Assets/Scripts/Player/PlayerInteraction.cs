using System;
using System.Collections.Generic;
using System.Linq;
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

    private bool eyesFilled;
    [SerializeField] private BodyPartSO[] collectedParts;
    
    [SerializeField] private GameObject closetPosition;

    [SerializeField] private List<GameObject> deathlocations;
    
    [SerializeField] GameObject interactTextHUD;

    [SerializeField] private GameObject[] fadeObjects;
    
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
            //will add a fade to black and scream here later
            
            
            currentInteraction.gameObject.transform.position = deathlocations[0].transform.position;
            currentInteraction.gameObject.GetComponent<SpriteRenderer>().sprite = currentInteraction.npcData.deadPhoto2;
            
            var x = currentInteraction.GetComponent<BasicPatrol>();
            if(x != null) x.enabled = false;
            currentInteraction.enabled = false;
            
            deathlocations.RemoveAt(0);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            HandleCutting();
        }
        
        if (characterController != null && !characterController.enabled && !isInDialogue)
        {
            if (currentInteraction == null || !currentInteraction.isCurrentConversation)
            {
                characterController.enabled = true;
                
            }
        }
    }

    private void HandleCutting()
    {
        foreach (var x in collectedParts)
        {
            if (x.sprite == null)
            {
                if (x is MouthSO)
                {
                    x.sprite = currentInteraction.npcData.mouthPhoto;
                    ComeOutOfTheCloset();
                    return;
                }
                if (x is EyeSO && !eyesFilled)
                {
                    x.sprite = currentInteraction.npcData.rightEyePhoto;
                    eyesFilled = true;
                    ComeOutOfTheCloset();
                    return;
                }
                if (x is EyeSO && eyesFilled)
                {
                    x.sprite = currentInteraction.npcData.leftEyePhoto;
                    ComeOutOfTheCloset();
                    return;
                }
                if (x is NoseSO)
                {
                    x.sprite = currentInteraction.npcData.nosePhoto;
                    ComeOutOfTheCloset();
                    return;
                }
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
        if (collectedParts.All(_ => _.sprite != null))
        {
            
            Debug.Log("All parts collected, loading next scene");
            SceneManager.LoadScene("MaskCreationManualCut", LoadSceneMode.Single);
            return;
        };

        
        transform.position = closetPosition.transform.position;
        
        foreach (var x in fadeObjects)
        {
            x.SetActive(true);
            x.GetComponent<Animation>().Play();
        }
        
        Debug.Log("Coming out of the closet");
        
    }
}
