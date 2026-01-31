using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Cutter : MonoBehaviour
{
    public Camera cam;
    public LineRenderer lineRenderer;
    //public Texture2D knifeIcon; //TODO: Draw this or find icon

    //just for testing for now
    [SerializeField] BodyPartSO[] bodyPartSOs;

    public float distanceThreshold = 0.005f;
    
    public SpriteRenderer target;
    public bool cutoutMode;
    public List<Vector3> cutoutShape;

    public SpriteRenderer emptySprite;
    PlayerInteraction playerInteraction;
    
    [Header("Cutout Validation (RGB reference)")]
    [SerializeField] private Texture2D referenceRGB;
    [SerializeField] private float minCoverageToPass = 0.70f;

    [SerializeField] private float minCoverageToPassRed = 0.35f;
    // Ignore black pixels
    [SerializeField, UnityEngine.Range(0f, 1f)] private float minChannelValue = 0.20f; 
    // how much a channel must exceed others to count
    [SerializeField, UnityEngine.Range(0f, 1f)] private float channelDominanceMargin = 0.15f;

    void Awake()
    {
        playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        cutoutShape = new List<Vector3>(256);
    }
    void Start()
    {
        emptySprite.enabled = false;
        referenceRGB = playerInteraction.currentInteraction.npcData.deadPhotoRGB;
        
        if (lineRenderer != null)
            lineRenderer.loop = true;

        ResetCutoutVisuals();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //using old input sys because I'm lazy
        {
            StartCutout();
        }

        if (Input.GetMouseButton(0) && cutoutMode)
        {
            var point = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            point.z = -0.1f;
            AddPointToShape(point);
            UpdateLineRenderer();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndCutout();

            bool valid = ValidateCutoutAgainstReference();
            Debug.Log($"Cutout valid? {valid}");

            if (valid)
            {
                CreateCutout();   // only apply mask if valid
                AssignToSO();
            }
            else
            {
                cutoutShape.Clear();
                ResetCutoutVisuals(); // restore original face + remove cutout
                Debug.LogWarning("Cutout invalid!");
            }
        }

    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = cutoutShape.Count;
        if (cutoutShape.Count > 0)
        {
            lineRenderer.SetPositions(cutoutShape.ToArray());
        }
    }

    public void StartCutout()
    {
        //Cursor.SetCursor(knifeIcon, Vector2.zero, CursorMode.Auto);
        cutoutMode = true;
        cutoutShape.Clear();
        
        ResetCutoutVisuals();
        
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = true;
    }

    public void EndCutout()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        cutoutMode = false;
        
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
        
    }
    
    private void ResetCutoutVisuals()
    {
        if (target != null)
        {
            target.maskInteraction = SpriteMaskInteraction.None;

            var tMask = target.GetComponent<SpriteMask>();
            if (tMask != null) tMask.sprite = null;
        }
        
        if (emptySprite != null)
        {
            emptySprite.enabled = false;
            emptySprite.maskInteraction = SpriteMaskInteraction.None;

            var eMask = emptySprite.GetComponent<SpriteMask>();
            if (eMask != null) eMask.sprite = null;
        }
    }
    
    public static Vector2 SpritePixelToWorldPosition(SpriteRenderer spriteRenderer, Vector2 pixelInSpriteRect)
    {
        var sprite = spriteRenderer.sprite;
        
        Vector2 pivot = sprite.pivot;
        float ppu = sprite.pixelsPerUnit;

        Vector2 localUnits = (pixelInSpriteRect - pivot) / ppu;
        return spriteRenderer.transform.TransformPoint(localUnits);
    }

    public bool ValidateCutoutAgainstReference()
    {
        if (referenceRGB == null)
        {
            Debug.LogWarning($"{nameof(Cutter)}: No referenceRGB assigned. Skipping validation.");
            return true;
        }

        if (cutoutShape == null || cutoutShape.Count < 3)
            return false;

        if (target == null || target.sprite == null)
            return false;

        var polygon = cutoutShape.ToArray();

        int totalRed = 0, totalGreen = 0, totalBlue = 0;
        int cutRed = 0, cutGreen = 0, cutBlue = 0;

        int w = referenceRGB.width;
        int h = referenceRGB.height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Color c = referenceRGB.GetPixel(x, y);
                if (c.a < 0.5f)
                    continue;

                bool isRed = c.r > minChannelValue && c.r > c.g + channelDominanceMargin && c.r > c.b + channelDominanceMargin;
                bool isGreen = c.g > minChannelValue && c.g > c.r + channelDominanceMargin && c.g > c.b + channelDominanceMargin;
                bool isBlue = c.b > minChannelValue && c.b > c.r + channelDominanceMargin && c.b > c.g + channelDominanceMargin;

                if (!isRed && !isGreen && !isBlue)
                    continue;

                // IMPORTANT: map reference pixels using the sprite rect/pivot, not SpriteRenderer.size
                var worldPoint = SpritePixelToWorldPosition(target, new Vector2(x, y));
                bool inside = PointInPolygon(worldPoint, polygon);

                if (isRed)
                {
                    totalRed++;
                    if (inside) cutRed++;
                }
                else if (isGreen)
                {
                    totalGreen++;
                    if (inside) cutGreen++;
                }
                else if (isBlue)
                {
                    totalBlue++;
                    if (inside) cutBlue++;
                }
            }
        }

        float redCoverage = totalRed == 0 ? 0f : (float)cutRed / totalRed;
        float greenCoverage = totalGreen == 0 ? 0f : (float)cutGreen / totalGreen;
        float blueCoverage = totalBlue == 0 ? 0f : (float)cutBlue / totalBlue;

        Debug.Log($"Coverage R:{redCoverage:P1} G:{greenCoverage:P1} B:{blueCoverage:P1} (pass >= {minCoverageToPass:P0})");

        return
            redCoverage >= minCoverageToPassRed ||
            greenCoverage >= minCoverageToPass ||
            blueCoverage >= minCoverageToPass;
    }
    
    private static Vector2 ReferencePixelToWorldPosition(SpriteRenderer spriteRenderer, Texture2D reference, int rx, int ry)
    {
        float u = (reference.width <= 1) ? 0f : (float)rx / (reference.width - 1);
        float v = (reference.height <= 1) ? 0f : (float)ry / (reference.height - 1);

        var sprite = spriteRenderer.sprite;

        Rect tr = sprite.textureRect;
        float px = tr.x + u * (tr.width - 1f);
        float py = tr.y + v * (tr.height - 1f);
        
        Vector2 pivot = sprite.pivot;
        float localPixelX = (px - tr.x) - pivot.x;
        float localPixelY = (py - tr.y) - pivot.y;

        Vector2 localUnits = new Vector2(localPixelX, localPixelY) / sprite.pixelsPerUnit;
        
        return spriteRenderer.transform.TransformPoint(localUnits);
    }
    

    public void CreateCutout()
    {
        if (cutoutShape.Count < 3) return; //need 3 vertices
        if (target == null || target.sprite == null) return;

        Color _blank = new Color(0, 0, 0, 0);
        var polygon = cutoutShape.ToArray();

        var srcSprite = target.sprite;

        // Use the sprite's actual pixel dimensions (NOT SpriteRenderer.size)
        int texW = Mathf.RoundToInt(srcSprite.rect.width);
        int texH = Mathf.RoundToInt(srcSprite.rect.height);

        var mask = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        var holeMask = new Texture2D(texW, texH, TextureFormat.RGBA32, false);

        // Match PPU + pivot so the generated sprites have identical world size/alignment
        float ppu = srcSprite.pixelsPerUnit;
        Vector2 pivotPixels = srcSprite.pivot; // pixels within sprite.rect
        Vector2 pivotNormalized = new Vector2(
            texW <= 0 ? 0.5f : pivotPixels.x / texW,
            texH <= 0 ? 0.5f : pivotPixels.y / texH
        );

        for (int x = 0; x < texW; x++)
        {
            for (int y = 0; y < texH; y++)
            {
                // default: outside polygon => "solid" on holeMask, blank on mask
                mask.SetPixel(x, y, _blank);
                holeMask.SetPixel(x, y, Color.white);
            }
        }

        for (int x = 0; x < texW; x++)
        {
            for (int y = 0; y < texH; y++)
            {
                // IMPORTANT: use the sprite-rect pixel mapping (not PixelToWorldPosition/size)
                var point = SpritePixelToWorldPosition(target, new Vector2(x, y));
                if (PointInPolygon(point, polygon))
                {
                    mask.SetPixel(x, y, Color.black);
                    holeMask.SetPixel(x, y, _blank);
                }
            }
        }

        mask.Apply(false);
        holeMask.Apply(false);

        var maskSprite = Sprite.Create(mask, new Rect(0, 0, texW, texH), pivotNormalized, ppu);
        emptySprite.GetComponent<SpriteMask>().sprite = maskSprite;
        emptySprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        emptySprite.enabled = true;

        var holeSprite = Sprite.Create(holeMask, new Rect(0, 0, texW, texH), pivotNormalized, ppu);
        target.GetComponent<SpriteMask>().sprite = holeSprite;
        target.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }

    public static Vector2 PixelToWorldPosition(SpriteRenderer spriteRenderer, Vector2 pixel)
    {
        var scale = (Vector2)spriteRenderer.transform.lossyScale;
        var position = (Vector2)spriteRenderer.transform.position;
        var size = spriteRenderer.size;
        var ppu = spriteRenderer.sprite.pixelsPerUnit;

        pixel /= ppu;
        var localPosition = (pixel - size * 0.5f) / scale;
        var rotation = spriteRenderer.transform.rotation;
        var rotatedPosition = rotation * localPosition;
        var result = (Vector2)rotatedPosition + position;
        return result;
    }

    private bool PointInPolygon(Vector2 point, Vector3[] polygon)
    {
        bool isInside = false;

        // Standard ray-cast against edges (j is previous vertex index)
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            var a = polygon[i];
            var b = polygon[j];

            bool intersects = ((a.y > point.y) != (b.y > point.y)) &&
                              (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x);

            if (intersects)
                isInside = !isInside;

            j = i;
        }

        return isInside;
    }

    void AddPointToShape(Vector3 point)
    {
        if (cutoutShape.Count == 0)
        {
            cutoutShape.Add(point); //if lr not loop, add multiple points so it actually shows up?

            return;
        }

        var lastPoint = cutoutShape[^1];
        var sqrMagnitude = Vector3.SqrMagnitude(point - lastPoint);
        if (sqrMagnitude < distanceThreshold * distanceThreshold)
        {
            return;
        }

        cutoutShape.Add(point);
    }

    public void AssignToSO()
    {
        //Assuming all masks same size and faces in roughly same position here, if not then aaaaaaaaaaa
        if(bodyPartSOs.Count(x => x.spriteMask != null) >= 3)
        {
            //No clue why checking all doesn't work but anyway we doing this pepega way now :)
            StartCoroutine(CreateTheMask());
            return;
        }
        
        foreach (var t in bodyPartSOs)
        {
            if (t.spriteMask == null)
            {
                t.sprite = target.GetComponent<SpriteRenderer>().sprite; // cropped sprite
                t.spriteMask = emptySprite.GetComponent<SpriteMask>().sprite; // same sprite
                
                StartCoroutine(BackToMasquerade());
                return;
            }
        }
    }

    IEnumerator<WaitForSeconds> BackToMasquerade()
    {
        yield return new WaitForSeconds(0.5f);
        
        SceneManager.UnloadSceneAsync("Cutout");
    }
    
    IEnumerator<WaitForSeconds> CreateTheMask()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("MaskCreation", LoadSceneMode.Single);
    }
}
