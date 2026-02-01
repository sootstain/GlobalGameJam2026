using UnityEngine;

[CreateAssetMenu(fileName = "EyeSO", menuName = "Scriptable Objects/EyeSO")]
public class BodyPartSO : ScriptableObject
{
    public Vector3 position; //on mask
    public Vector3 rotation;
    public Sprite sprite;

    private void OnEnable()
    {
        position = new Vector3(Random.Range(-0.5f,1.5f), Random.Range(0,1), 0);
    }
}