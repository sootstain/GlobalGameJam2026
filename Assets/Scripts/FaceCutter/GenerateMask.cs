using UnityEngine;

public class GenerateMask : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] BodyPartSO facePart;
    void Start()
    {
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
