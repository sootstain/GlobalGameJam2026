using TMPro;
using UnityEngine;


public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float playerInteractionDistance = 30f;
    BasicInteraction currentInteraction;
    private CharacterController characterController;

    [SerializeField] TMP_Text interactText;

    void Update()
    {
        CheckInteraction();
        TryInteractWithCurrent();
    }



    private void TryInteractWithCurrent()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (currentInteraction == null)
            return;

        characterController = GetComponent<CharacterController>();
        characterController.enabled = false;
        currentInteraction.Interact();

        if (!currentInteraction.isCurrentConversation)
            characterController.enabled = true;
    }

    public void CheckInteraction()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, playerInteractionDistance))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                BasicInteraction npcInteraction = hit.collider.GetComponent<BasicInteraction>();
                if(npcInteraction.enabled) SetCurrentInteraction(npcInteraction);
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
