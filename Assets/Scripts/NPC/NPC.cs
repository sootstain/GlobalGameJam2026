using UnityEngine;

[CreateAssetMenu(fileName = "NPCSO", menuName = "Scriptable Objects/NPC")]
public class NPC : ScriptableObject
{
    [Header("Attributes")]
    //Set up for original photos
    public Sprite photo;
    public Sprite deadPhoto;
    public Texture2D deadPhotoRGB;
    public EyeType eyeType;
    public MouthType mouthType;
    public NoseType noseType;
    
    [Header("Loves")]
    //LOVE
    public EyeType loveEyes;
    public MouthType loveMouth;
    public NoseType loveNose;
    
    [Header("Likes")]
    //LIKE
    public EyeType likeEyes;
    public MouthType likeMouth;
    public NoseType likeNose;
}
