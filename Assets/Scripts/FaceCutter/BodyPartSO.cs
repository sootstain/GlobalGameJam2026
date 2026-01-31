using UnityEngine;

[CreateAssetMenu(fileName = "BodyPartSO", menuName = "Scriptable Objects/BodyPartSO")]
public class BodyPartSO : ScriptableObject
{
    public EyeType eyeType;
    public MouthType mouthType;
    public NoseType noseType;
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

public enum MouthType
{
    Open,
    Closed
}

public enum NoseType
{
    Big, //I would cry but
    Small
}