using UnityEngine;
using UnityEngine.Events;

public class BasicInteraction : MonoBehaviour
{
    public Outline outline;
    public string message;

    public UnityEvent onInteract;

    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }
    
    public void Interact()
    {
        onInteract.Invoke();
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }
    
    public void EnableOutline()
    {
        outline.enabled = true;
    }
}
