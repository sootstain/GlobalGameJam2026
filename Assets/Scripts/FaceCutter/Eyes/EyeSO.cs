using UnityEngine;

[CreateAssetMenu(fileName = "EyeSO", menuName = "Scriptable Objects/EyeSO")]
public class EyeSO : ScriptableObject
{
    public EyeType eyeType;
    public Sprite sprite;
    public Sprite spriteMask;
}


public enum EyeType
{
    Blue,
    Brown,
    Green,
    Black
}