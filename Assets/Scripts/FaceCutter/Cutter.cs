using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cutter : MonoBehaviour
{
    public Camera cam;
    public LineRenderer lineRenderer;
    //public Texture2D knifeIcon; //TODO: Draw this or find icon

    public float distanceThreshold = 0.02f;

    public SpriteRenderer target;
    public bool cutoutMode;
    public List<Vector3> cutoutShape;


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("We cutting");
            var point = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());;
            AddPointToShape(point);
            StartCutout();
        }
        
        lineRenderer.positionCount = cutoutShape.Count;
        lineRenderer.SetPositions(cutoutShape.ToArray());

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("No more cutting, we are done");
            EndCutout();
        }
    }


    public void StartCutout()
    {
        //Cursor.SetCursor(knifeIcon, Vector2.zero, CursorMode.Auto);
        cutoutMode = true;
        //cutoutShape.Clear();
        target.maskInteraction = SpriteMaskInteraction.None;
        target.GetComponent<SpriteMask>().sprite = null;
        lineRenderer.positionCount = 0;
        
    }

    public void EndCutout()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        CreateCutout();
        cutoutMode = false;
        lineRenderer.positionCount = 0;
    }

    public void CreateCutout()
    {
        Debug.Log("Creating cutout");
        if(cutoutShape.Count < 3) return; //need 3 vertices
        
        Color _blank = new Color(0, 0, 0, 0);
        var polygon = cutoutShape.ToArray();
        var resolution = target.size * target.sprite.pixelsPerUnit;
        var mask = new Texture2D((int)resolution.x, (int)resolution.y);

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
                }
            }
        }
        mask.Apply();
        
        var sprite = Sprite.Create(mask, new Rect(0, 0, mask.width, mask.height), new Vector2(0.5f, 0.5f));
        target.GetComponent<SpriteMask>().sprite = sprite;
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
            cutoutShape.Add(point);
            return;
        }
        
        var lastPoint = cutoutShape[^1];
        var sqrMagnitude = Vector3.SqrMagnitude(point - lastPoint);
        if (sqrMagnitude < distanceThreshold * distanceThreshold) return;
        
        
        Debug.Log("Adding more than 1 point");
        cutoutShape.Add(point);
    }
}
