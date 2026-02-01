using NUnit.Framework.Constraints;
using UnityEngine;

public class GenerateMask : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] BodyPartSO facePart;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        spriteRenderer.sprite = facePart.sprite;
        this.transform.localPosition = facePart.position;
        transform.localEulerAngles = facePart.rotation;
    }

    public void SavePosition()
    {
        facePart.position = transform.localPosition;
        facePart.rotation = transform.localEulerAngles;
    }
}
