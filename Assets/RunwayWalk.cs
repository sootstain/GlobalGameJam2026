using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunwayWalk : MonoBehaviour
{
    [SerializeField] GameObject targetLocation;
    [SerializeField] private Sprite yay;
    [SerializeField] private GameObject fadeObject;
    
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
