using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

public class DialogueUiInputYarn : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private PlayerInput playerInput;

    [Header("Action Maps")]
    [SerializeField] private string gameplayMap = "Player";
    [SerializeField] private string uiMap = "UI";

    private void Awake()
    {
        if (dialogueRunner == null) dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInput>();
    }

    private void OnEnable()
    {
        if (dialogueRunner == null) return;
        dialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
    }

    private void OnDisable()
    {
        if (dialogueRunner == null) return;
        dialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
    }

    private void OnDialogueStart()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerInput != null)
            playerInput.enabled = false;
    }

    private void OnDialogueComplete()
    {
        if (playerInput != null)
            playerInput.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}