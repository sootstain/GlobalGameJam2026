using System;
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

    public NPC npcData;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //See if we like this or not; will need to also update during path movement I think
        transform.LookAt(Camera.main.transform);
    }

    void Start()
    {
        spriteRenderer.sprite = npcData.photo;
        outline = GetComponent<Outline>();
        outline.enabled = false;

        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
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
