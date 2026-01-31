using UnityEngine;

public class GenerateMask : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] BodyPartSO facePart;

    void Start()
    {
        spriteRenderer.sprite = facePart.sprite;
        spriteRenderer.GetComponent<SpriteMask>().sprite = facePart.spriteMask;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        if (facePart.sprite != null)
        {
            var b = facePart.sprite.bounds.size;
            Debug.Log($"[GenerateMask] FACE sprite='{facePart.sprite.name}' PPU={facePart.sprite.pixelsPerUnit} rect={facePart.sprite.rect.size} bounds={b}");
        }

        if (facePart.spriteMask != null)
        {
            var b = facePart.spriteMask.bounds.size;
            Debug.Log($"[GenerateMask] MASK sprite='{facePart.spriteMask.name}' PPU={facePart.spriteMask.pixelsPerUnit} rect={facePart.spriteMask.rect.size} bounds={b}");
        }
    }
}