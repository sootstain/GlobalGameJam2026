using UnityEngine;
using Yarn.Unity;
using TMPro;
using System.Collections;

public class NameInputUI : MonoBehaviour
{
    public TMP_InputField nameInput;
    public GameObject panel;
    public DialogueRunner dialogueRunner;

    private bool submitted = false;
    
    void Awake()
    {
        panel.SetActive(false);
        
        if (dialogueRunner != null)
        {
            dialogueRunner.AddCommandHandler("show_name_input", ShowNameInput);
            Debug.Log("Command registered successfully");
        }
        else
        {
            Debug.LogError("DialogueRunner not assigned!");
        }
    }
    
    void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            ConfirmName();
        }
    }
    
    public IEnumerator ShowNameInput()
    {
        submitted = false;

        panel.SetActive(true);
        nameInput.text = "";
        nameInput.ActivateInputField();
        
        while (!submitted)
            yield return null;

        panel.SetActive(false);

        yield break;
    }

    public void ConfirmName()
    {
        string playerName = nameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
            return;

        dialogueRunner.VariableStorage.SetValue("$playerName", playerName);

        submitted = true;
    }
}