using UnityEngine;

public class GenerateMask : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    
    [SerializeField] BodyPartSO facePart;
    void Start()
    {
            spriteRenderer.sprite = facePart.sprite;
    }
}
