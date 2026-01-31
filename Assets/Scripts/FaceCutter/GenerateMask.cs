using UnityEngine;

public class GenerateMask : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    
    [SerializeField] EyeSO eye;
    void Start()
    {
        spriteRenderer.sprite = eye.sprite;
        spriteRenderer.GetComponent<SpriteMask>().sprite = eye.spriteMask;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }
}
