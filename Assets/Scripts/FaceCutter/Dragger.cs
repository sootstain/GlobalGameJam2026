using System;
using UnityEngine;


public class Dragger : MonoBehaviour
{
    private Vector3 dragOffset;
    private Camera cam;

    private bool isDragging;
    private bool isRotating;

    private SpriteRenderer sprite;
    
    private float sensitivity = 0.4f;
    private Vector3 mouseReference;
    private Vector3 mouseOffset;
    private Vector3 rotation;
    
    void Awake()
    {
        cam = Camera.main;
        sprite = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //STILL TESTING: Only allow dragging if over non-transparent pixel as the full sprite is the size of the face but with only the mask textured
            //Otherwise can drag when hovering over nothing
            
            if (IsPixelAtPosition(GetMousePosition()))
            {
                isDragging = true;
                sprite.sortingOrder = 10;
                dragOffset = transform.position - GetMousePosition();
            }
            else
            {
                sprite.sortingOrder = 1;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (IsPixelAtPosition(GetMousePosition()))
            {
                isRotating = true;
                mouseReference = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(1) && isRotating)
        {
            mouseOffset = (Input.mousePosition - mouseReference);
            
            transform.Rotate(0, 0, -mouseOffset.x * sensitivity);

            mouseReference = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            transform.position = GetMousePosition() + dragOffset;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }
    }

    Vector3 GetMousePosition()
    {
        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
    
    private bool IsPixelAtPosition(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        
        Sprite maskSprite = sprite.sprite;
        if (maskSprite == null) return false;
        
        Texture2D texture = maskSprite.texture;
        
        Rect spriteRect = maskSprite.rect;
        float pixelX = spriteRect.x + (localPos.x * maskSprite.pixelsPerUnit + maskSprite.pivot.x);
        float pixelY = spriteRect.y + (localPos.y * maskSprite.pixelsPerUnit + maskSprite.pivot.y);
        
        if (pixelX < spriteRect.x || pixelX >= spriteRect.x + spriteRect.width ||
            pixelY < spriteRect.y || pixelY >= spriteRect.y + spriteRect.height)
        {
            return false;
        }
        
        Color pixel = texture.GetPixel((int)pixelX, (int)pixelY);
        return pixel.a > 0.1f;
    }
}