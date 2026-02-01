using UnityEngine;

using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Transform doorTransform;
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public float openSpeed = 2f;
    
    private Quaternion closedRotQuat;
    private Quaternion openRotQuat;
    public bool isOpen = false;
    
    void Start()
    {
        closedRotQuat = doorTransform.localRotation;
        openRotQuat = Quaternion.Euler(openRotation);
    }

    public void Update()
    {
        Quaternion targetRotation = isOpen ? openRotQuat : closedRotQuat;
        doorTransform.localRotation = Quaternion.Lerp(
            doorTransform.localRotation, 
            targetRotation, 
            Time.deltaTime * openSpeed
        );
    
    }

    
    public void ToggleDoor()
    {
        isOpen = true;
    }
}