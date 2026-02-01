using System;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
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
    
    [Header("Camera Focus")]
    [SerializeField] private string npcFaceAnchorName = "FaceAnchor";
    [SerializeField] private bool lockMovementDuringDialogue = true;

    private FirstPersonController fpController;
    private bool cameraLockedToNpc;

    private bool eyesFilled;
    [SerializeField] private BodyPartSO[] collectedParts;
    
    [SerializeField] private GameObject closetPosition;

    [SerializeField] private List<GameObject> deathlocations;
    
    [SerializeField] GameObject interactTextHUD;

    [SerializeField] private GameObject[] fadeObjects;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        fpController = GetComponent<FirstPersonController>();
        
        if (playerCamera == null)
            playerCamera = Camera.main;

        foreach (var x in collectedParts)
        {
            //reset
            x.sprite = null;
        }
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
            
            currentInteraction.npcData.isDead = true;
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
        
        if (cameraLockedToNpc && (currentInteraction == null || !currentInteraction.isCurrentConversation))
        {
            cameraLockedToNpc = false;

            if (fpController != null)
                fpController.UnlockCamera();

            if (characterController != null)
                characterController.enabled = true;

            isInDialogue = false;
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
                if (x is MouthSO mouthSO)
                {
                    mouthSO.sprite = currentInteraction.npcData.mouthPhoto;
                    mouthSO.mouthType = currentInteraction.npcData.mouthType;
                    ComeOutOfTheCloset();
                    return;
                }
                if (x is EyeSO eyeSO && !eyesFilled)
                {
                    eyeSO.sprite = currentInteraction.npcData.rightEyePhoto;
                    eyeSO.eyeType = currentInteraction.npcData.eyeType;
                    eyesFilled = true;
                    ComeOutOfTheCloset();
                    return;
                }
                if (x is EyeSO eyeSO2 && eyesFilled)
                {
                    eyeSO2.sprite = currentInteraction.npcData.leftEyePhoto;
                    eyeSO2.eyeType = currentInteraction.npcData.eyeType;
                    ComeOutOfTheCloset();
                    return;
                }
                if (x is NoseSO noseSO)
                {
                    noseSO.sprite = currentInteraction.npcData.nosePhoto;
                    noseSO.noseType = currentInteraction.npcData.noseType;
                    ComeOutOfTheCloset();
                    return;
                }
            }
        }
    }

    private Transform GetNpcFaceTarget(BasicInteraction npc)
    {
        if (npc == null)
            return null;

        Transform anchor = npc.transform.Find(npcFaceAnchorName);
        return anchor != null ? anchor : npc.transform;
    }

    private void TryInteractWithCurrent()
    {

        if (currentInteraction == null)
            return;
        
        if (fpController != null)
        {
            Transform faceTarget = GetNpcFaceTarget(currentInteraction);
            if (faceTarget != null)
            {
                fpController.LockCameraTo(faceTarget);
                cameraLockedToNpc = true;
            }
        }
        
        if (lockMovementDuringDialogue && characterController != null)
            characterController.enabled = false;

        currentInteraction.Interact();
        isInDialogue = true;
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

        
        foreach (var x in fadeObjects)
        {
            x.SetActive(true);
            x.GetComponent<Animation>().Play();
        }
        
        
        StartCoroutine(TeleportCoroutine());
        
        Debug.Log("Coming out of the closet");
        
    }

    public IEnumerator<WaitForSeconds> TeleportCoroutine()
    {
        yield return new WaitForSeconds(1f);
        characterController.enabled = false;
        transform.position = closetPosition.transform.position;
        characterController.enabled = true;
    }
}
