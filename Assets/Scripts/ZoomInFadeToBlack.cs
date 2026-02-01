using System.Collections;
using System.Collections;
using UnityEngine;
using StarterAssets;

public class ZoomInFadeToBlack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private GameObject panToTarget;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CanvasGroup fadeCanvasGroup; // alpha 0 -> 1 (black image under it)

    [Header("Timings")]
    [SerializeField] private float panDelay = 0.1f;
    [SerializeField] private float zoomDuration = 1.0f;
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Zoom")]
    [SerializeField] private float targetFov = 20f;
    

    private void Awake()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = false;
        }
    }

    private IEnumerator CaughtSequence()
    {
        if (controller != null)
        {
            controller.lockMovement = true;

            if (panToTarget != null)
                controller.forcePan(panToTarget);
        }

        yield return new WaitForSeconds(panDelay);

        float startFov = playerCamera != null ? playerCamera.fieldOfView : 60f;

        float tZoom = 0f;
        float tFade = 0f;

        while (tZoom < zoomDuration || tFade < fadeDuration)
        {
            if (playerCamera != null && tZoom < zoomDuration)
            {
                tZoom += Time.deltaTime;
                float z = Mathf.Clamp01(tZoom / Mathf.Max(0.0001f, zoomDuration));
                playerCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, z);
            }

            if (fadeCanvasGroup != null && tFade < fadeDuration)
            {
                tFade += Time.deltaTime;
                float f = Mathf.Clamp01(tFade / Mathf.Max(0.0001f, fadeDuration));
                fadeCanvasGroup.alpha = f;
            }

            yield return null;
        }

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 1f;
    }
}