using System;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using Yarn;
using Yarn.Unity;
using Random = UnityEngine.Random;

public class BasicInteraction : MonoBehaviour
{
    public Outline outline;
    public string message;
    private DialogueRunner dialogueRunner;
    public string conversationStartNode = "Start";
    public bool isCurrentConversation;
    BasicInteraction currentInteraction;
    public UnityEvent onInteract;
    private bool isInCoroutine;

    public NPC npcData;
    private SpriteRenderer spriteRenderer;
    
    [SerializeField] private GameObject panToTarget;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private int waitforsecondslow = 1;
    [SerializeField] private int waitforsecondshigh = 3;
    
    [Header("Facing")]
    [SerializeField] private float facePlayerPanSeconds = 0.25f;
    [SerializeField] private bool billboardToCameraWhenIdle = true;
    
    private Transform _playerFaceTarget;
    private Coroutine _faceRoutine;
    private bool _lockFacingDuringConversation;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //See if we like this or not; will need to also update during path movement I think
        transform.LookAt(Camera.main.transform);

        if (isCurrentConversation && !isInCoroutine)
        {
            StartCoroutine(AnimateTalkingSprites());
            isInCoroutine = true;
        }
        
    }
    
    private System.Collections.IEnumerator AnimateTalkingSprites()
    {
        switch (Random.Range(1, 4))
        {
            case 1:
                spriteRenderer.sprite = npcData.talking1;
                break;
            case 2: 
                spriteRenderer.sprite = npcData.talking2; 
                break;
            case 3:
                spriteRenderer.sprite = npcData.talking3;
                break;
        }
        yield return new WaitForSeconds(Random.Range(waitforsecondslow, waitforsecondshigh));
        isInCoroutine = false;
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
        
        if (playerCamera != null) _playerFaceTarget = playerCamera.transform;
        else if (controller != null) _playerFaceTarget = controller.transform;
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
        
        if (_playerFaceTarget != null)
        {
            if (_faceRoutine != null) StopCoroutine(_faceRoutine);
            _faceRoutine = StartCoroutine(PanFaceTargetThenLock(_playerFaceTarget, facePlayerPanSeconds));
        }

        onInteract.Invoke();

        if (spriteRenderer)
        {
            int randomTalk = UnityEngine.Random.Range(1, 4);
            switch (randomTalk)
            {
                case 1:
                    spriteRenderer.sprite = npcData.talking1;
                    break;
                case 2: 
                    spriteRenderer.sprite = npcData.talking2;
                    break;
                case 3:
                    spriteRenderer.sprite = npcData.talking3;
                    break;
            }
        }
        
        StartConversation();
        
    }
    
    private System.Collections.IEnumerator PanFaceTargetThenLock(Transform target, float seconds)
    {
        _lockFacingDuringConversation = true;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f; 

        if (toTarget.sqrMagnitude < 0.0001f)
            yield break;

        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.LookRotation(toTarget, Vector3.up);

        if (seconds <= 0f)
        {
            transform.rotation = to;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / seconds;
            transform.rotation = Quaternion.Slerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }

        transform.rotation = to;
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
        spriteRenderer.sprite = npcData.photo;
    }
}
