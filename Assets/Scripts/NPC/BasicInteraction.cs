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

        dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueComplete.AddListener(EndConversation);
        }
        else
        {
            Debug.LogWarning($"{nameof(BasicInteraction)}: No DialogueRunner found in scene.");
        }
    }

    private void OnDestroy()
    {
        if (dialogueRunner != null)
            dialogueRunner.onDialogueComplete.RemoveListener(EndConversation);
    }

    public void Interact()
    {
        if (dialogueRunner == null)
        {
            Debug.LogWarning($"{nameof(BasicInteraction)}: Can't start dialogue; DialogueRunner is null.");
            return;
        }
        
        if (dialogueRunner.IsDialogueRunning)
            return;

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
        if (dialogueRunner == null)
            return;

        if (!dialogueRunner.isActiveAndEnabled)
            dialogueRunner.enabled = true;
        
        if (dialogueRunner.IsDialogueRunning)
            return;

        isCurrentConversation = true;
        dialogueRunner.StartDialogue(conversationStartNode);
    }

    private void EndConversation()
    {
        isCurrentConversation = false;
    }
}
