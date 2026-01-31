
using UnityEngine;

public class LoadImage : MonoBehaviour
{
    PlayerInteraction playerInteraction;
    [SerializeField] private SpriteRenderer[] images;
    private void Awake()
    {
        playerInteraction = FindFirstObjectByType<PlayerInteraction>();
    }

    private void OnEnable()
    {
        foreach (var image in images)
        {
            image.sprite = playerInteraction.currentInteraction.npcData.deadPhoto;
        }
    }

    private void OnDisable()
    {
        if(playerInteraction != null) playerInteraction.currentInteraction = null;
    }
}
