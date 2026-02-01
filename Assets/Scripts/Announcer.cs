using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class Announcer : MonoBehaviour
{

    [SerializeField] private Sprite announcer_idle;
    [SerializeField] private Sprite announcer_talk1;
    [SerializeField] private Sprite announcer_talk2;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }
    [YarnCommand("Announcer")]
    public void Announcing()
    {
        spriteRenderer.enabled = true;
        StartCoroutine(AnnouncerTalkAnimation());
    }

    private IEnumerator AnnouncerTalkAnimation()
    {
        while (true)
        {
            spriteRenderer.sprite = announcer_talk1;
            yield return new WaitForSeconds(Random.Range(1f, 4f));
            spriteRenderer.sprite = announcer_talk2;
            yield return new WaitForSeconds(Random.Range(1f, 4f));
            spriteRenderer.sprite = announcer_idle;
            yield return new WaitForSeconds(Random.Range(1f, 4f));
        }
    }

    [YarnCommand("StopAnnouncer")]
    public void StopAnnouncer()
    {
        spriteRenderer.enabled = false;
    }
}
