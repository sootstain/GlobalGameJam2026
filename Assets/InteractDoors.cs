
using UnityEngine;

public class InteractDoors : MonoBehaviour
{
    public DoorOpen[] doors;
    public GameObject interactUI;
    public KeyCode interactKey = KeyCode.E;
    private bool isPlayerNearby = false;

    private bool interacted;
    
    void Start()
    {
        
        if (interactUI != null)
            interactUI.SetActive(false);
    }
    
    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            foreach(var x in doors)
            {
                x.ToggleDoor();
            }
            interacted = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !interacted)
        {
            isPlayerNearby = true;
            if (interactUI != null)
                interactUI.SetActive(true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }    
}

