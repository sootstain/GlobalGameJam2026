using UnityEngine;

[CreateAssetMenu(fileName = "NPCSO", menuName = "Scriptable Objects/NPC")]
public class NPC : ScriptableObject
{
    [Header("Attributes")]
    //Set up for original photos
    public Sprite photo;
    public Sprite deadPhoto;
    public Sprite deadPhoto2;
    public Sprite mouthPhoto;
    public Sprite nosePhoto;
    public Sprite rightEyePhoto;
    public Sprite leftEyePhoto;

    public bool isDead;
    
    [Header("Types")]
    public EyeType eyeType;
    public MouthType mouthType;
    public NoseType noseType;
    
    [Header("Loves")]
    //LOVE
    public EyeType loveEyes;
    public MouthType loveMouth;
    public NoseType loveNose;
}
