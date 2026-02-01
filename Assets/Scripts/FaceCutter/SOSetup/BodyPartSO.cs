using UnityEngine;

[CreateAssetMenu(fileName = "EyeSO", menuName = "Scriptable Objects/EyeSO")]
public class BodyPartSO : ScriptableObject
{
    public Vector3 position; //on mask
    public Vector3 rotation;
    public Sprite sprite;

    private void OnEnable()
    {
        position = new Vector3(Random.Range(-2f,2f), Random.Range(-1.5f,1.5f), 0);
    }
}