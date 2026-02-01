using UnityEngine;

[CreateAssetMenu(fileName = "EyeSO", menuName = "Scriptable Objects/EyeSO")]
public class BodyPartSO : ScriptableObject
{
    public Vector3 position = new (Random.Range(-0.5f,1.5f), Random.Range(0,1), 0); //on mask
    public Sprite sprite;
}