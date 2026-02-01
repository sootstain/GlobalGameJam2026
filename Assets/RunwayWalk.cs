using System.Collections;
using System.Linq;
using TMPro;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunwayWalk : MonoBehaviour
{
    [SerializeField] GameObject targetLocation;
    [SerializeField] private Sprite yay;
    [SerializeField] private GameObject fadeObject;

    public NPC[] npcs;
    public BodyPartSO[] bodyParts;
    public float score = 0;
    public float finalScore;

    bool eyesFilled;
    
    public TMP_Text scoreText;
    
    private void Start()
    {
        foreach (NPC npc in npcs.Where(_ => _.isDead == false))
        {
            foreach (var bodyPart in bodyParts)
            {
                if (bodyPart is MouthSO mouth)
                {
                    if (mouth.mouthType == npc.loveMouth) score++;
                    Debug.Log(">" + npc.name + " " + npc.loveMouth + " " + mouth.mouthType + "");
                    break;
                }
                if (bodyPart is EyeSO eyeSO1 && !eyesFilled)
                {
                    if (eyeSO1.eyeType == npc.loveEyes) score++;
                    Debug.Log(">" + npc.name + " " + npc.loveEyes + " " + eyeSO1.eyeType + "");
                    eyesFilled = true;
                    break;
                }
                if (bodyPart is EyeSO eyeSO2)
                {
                    if (eyeSO2.eyeType == npc.loveEyes) score++;
                    Debug.Log(">" + npc.name + " " + npc.loveEyes + " " + eyeSO2.eyeType + "");
                    break;
                }
                if (bodyPart is NoseSO nose)
                {
                    if (nose.noseType == npc.loveNose) score++;
                    Debug.Log(">" + npc.name + " " + npc.loveNose + " " + nose.noseType + "");
                    break;
                }
            }
            eyesFilled = false;
        }
        
        Debug.Log("Score:" + score);
        
        var aliveNPCS = npcs.Count(_ => _.isDead == false) * 4;
        
        finalScore = Mathf.CeilToInt((score/aliveNPCS) * 100);
        
        Debug.Log("Final Score: +" + finalScore);
        Debug.Log("Alive:" + aliveNPCS);
        
        scoreText.text = finalScore + "%";
        
        if (finalScore > 50)
        {
            scoreText.color = Color.green;
        }
        else
        {
            scoreText.color = Color.red;
        }
    }
    
    private void Update()
    {

        if (Vector3.Distance(transform.position, targetLocation.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position,  targetLocation.transform.position, 1f * Time.deltaTime);    
        }
        else
        {
            GetComponent<Animator>().enabled = false;
            GetComponent<SpriteRenderer>().sprite = yay;
            StartCoroutine(FadeBacktoMenu());
        }
    }
    
    IEnumerator FadeBacktoMenu()
    {
        yield return new WaitForSeconds(2f);
        fadeObject.SetActive(true);
        fadeObject.GetComponent<Animation>().Play();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
    }
}
