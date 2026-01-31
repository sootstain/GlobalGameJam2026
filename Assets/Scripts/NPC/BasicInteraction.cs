using UnityEngine;
using UnityEngine.Events;
using Yarn;
using Yarn.Unity;

public class BasicInteraction : MonoBehaviour
{
    public Outline outline;
    public string message;
    private DialogueRunner dialogueRunner;
    public string conversationStartNode = "Start";
    public bool isCurrentConversation;
    BasicInteraction currentInteraction;
    public UnityEvent onInteract;

    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
        dialogueRunner = FindObjectOfType<Yarn.Unity.DialogueRunner>();
        
    }
    
    public void Interact()
    {
        onInteract.Invoke();
        StartConversation();
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }
    
    public void EnableOutline()
    {
        outline.enabled = true;
    }
    
    private void StartConversation()
    {
        isCurrentConversation = true;
        dialogueRunner.StartDialogue(conversationStartNode);
    }
    
    private void EndConversation() {
        if (isCurrentConversation) { 
            // TODO *stop animation or turn off indicator or whatever* HERE
            isCurrentConversation = false;
        }
    }
    

}
