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
    }
    void Start()
    {
        emptySprite.enabled = false;
        referenceRGB = playerInteraction.currentInteraction.npcData.deadPhotoRGB;
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
            
            if (valid) AssignToSO();
            else
            {
                cutoutShape.Clear();
                target.GetComponent<SpriteMask>().sprite = null;
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
        target.maskInteraction = SpriteMaskInteraction.None;
        target.GetComponent<SpriteMask>().sprite = null;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = true;
    }

    public void EndCutout()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        CreateCutout();
        cutoutMode = false;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
        
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
                
                var worldPoint = PixelToWorldPosition(target, new Vector2(x, y));
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
        
        bool pass =
            redCoverage >= minCoverageToPassRed ||
            greenCoverage >= minCoverageToPass ||
            blueCoverage >= minCoverageToPass;

        return pass;
    }
    

    public void CreateCutout()
    {
        if (cutoutShape.Count < 3) return; //need 3 vertices

        Color _blank = new Color(0, 0, 0, 0);
        var polygon = cutoutShape.ToArray();
        var resolution = target.size * target.sprite.pixelsPerUnit;

        var mask = new Texture2D((int)resolution.x, (int)resolution.y);

        var holeMask = new Texture2D((int)resolution.x, (int)resolution.y);



        for (int x = 0; x < mask.width; x++)
        {
            for (int y = 0; y < mask.height; y++)
            {
                mask.SetPixel(x, y, _blank);
            }
        }

        for (int x = 0; x < mask.width; x++)
        {
            for (int y = 0; y < mask.height; y++)
            {
                var point = PixelToWorldPosition(target, new(x, y));
                if (PointInPolygon(point, polygon))
                {
                    mask.SetPixel(x, y, Color.black);
                    holeMask.SetPixel(x, y, _blank);
                }
                else
                {
                    mask.SetPixel(x, y, _blank);
                    holeMask.SetPixel(x, y, Color.white);
                }
            }
        }

        mask.Apply();
        holeMask.Apply();

        var sprite = Sprite.Create(mask, new Rect(0, 0, mask.width, mask.height), new Vector2(0.5f, 0.5f));
        emptySprite.GetComponent<SpriteMask>().sprite = sprite;
        emptySprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        emptySprite.enabled = true;

        var holeSprite = Sprite.Create(holeMask, new Rect(0, 0, holeMask.width, holeMask.height),
            new Vector2(0.5f, 0.5f));
        
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
        int edgeEnd = polygon.Length - 1;
        for (int edgeStart = 0; edgeStart < polygon.Length; edgeEnd = edgeStart++)
        {
            var start = polygon[edgeStart];
            var end = polygon[edgeEnd];
            if ((start.y > point.y) != (end.y > point.y))
            {
                float x = (end.x - start.x) * (point.y - start.y) / (end.y - start.y) + start.x;
                if (point.x < x)
                {
                    isInside = !isInside;
                }
            }

            edgeEnd = edgeStart;
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
