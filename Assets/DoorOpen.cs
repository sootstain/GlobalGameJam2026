using UnityEngine;

using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Transform doorTransform;
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public float openSpeed = 2f;
    
    private Vector3 closedRotation;
    private Vector3 originalPosition;
    public bool isOpen = false;
    
    void Start()
    {
        closedRotation = doorTransform.localEulerAngles;
        originalPosition = doorTransform.localPosition;
    }

    
    public void ToggleDoor()
    {
        Vector3 targetRotation = isOpen ? openRotation : closedRotation;
        doorTransform.localEulerAngles = Vector3.Lerp(
            doorTransform.localEulerAngles, 
            targetRotation, 
            Time.deltaTime * openSpeed
        );
        
        isOpen = true;
    }
}