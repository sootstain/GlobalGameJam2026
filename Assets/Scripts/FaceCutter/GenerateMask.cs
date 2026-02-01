using UnityEngine;

public class GenerateMask : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    
    [SerializeField] BodyPartSO facePart;
    void Start()
    {
            spriteRenderer.sprite = facePart.sprite;
            this.transform.localPosition = facePart.position;
    }

    public void SavePosition()
    {
        facePart.position = transform.localPosition;
    }
}
