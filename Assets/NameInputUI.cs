using UnityEngine;
using Yarn.Unity;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class NameInputUI : MonoBehaviour
{
    public TMP_InputField nameInput;
    public GameObject panel;
    public DialogueRunner dialogueRunner;

    [Header("Player Lock")]
    [SerializeField] private StarterAssets.FirstPersonController playerController;
    [SerializeField] private bool unlockCursorWhileTyping = true;

    [Header("Behavior")]
    [SerializeField] private string defaultNameIfEmpty = "";
    [SerializeField] private bool allowCancel = true;

    private bool submitted = false;
    private bool cancelled = false;

    void Awake()
    {
        panel.SetActive(false);

        if (playerController == null)
            playerController = FindFirstObjectByType<StarterAssets.FirstPersonController>();

        if (dialogueRunner != null)
        {
            dialogueRunner.AddCommandHandler("show_name_input", ShowNameInput);
            Debug.Log("Command registered successfully");
        }
        else
        {
            Debug.LogError("DialogueRunner not assigned!");
        }

        if (nameInput != null)
        {
            nameInput.lineType = TMP_InputField.LineType.SingleLine;
        }
    }

    void Update()
    {
        if (!panel.activeSelf)
            return;

        if (allowCancel && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelNameInput();
            return;
        }

        bool submitPressed =
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter);

        if (submitPressed)
        {
            ConfirmName();
        }
    }

    public IEnumerator ShowNameInput()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (playerController != null)
            playerController.lockCamera = true;

        if (playerController != null)
            playerController.lockMovement = true;
        
        submitted = false;
        cancelled = false;

        panel.SetActive(true);

        nameInput.text = "";

        nameInput.Select();
        nameInput.ActivateInputField();

        while (!submitted && !cancelled)
            yield return null;

        panel.SetActive(false);

        if (unlockCursorWhileTyping)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerController != null)
            playerController.lockCamera = false;

        if (playerController != null)
            playerController.lockMovement = false;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ConfirmName()
    {
        if (submitted || cancelled)
            return;

        string playerName = nameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            if (!string.IsNullOrEmpty(defaultNameIfEmpty))
                playerName = defaultNameIfEmpty;
            else
                return;
        }

        dialogueRunner.VariableStorage.SetValue("$playerName", playerName);
        submitted = true;
    }

    public void CancelNameInput()
    {
        if (submitted || cancelled)
            return;

        cancelled = true;
    }
}