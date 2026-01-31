using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void GoToLevel(int level)
    {
        SceneManager.LoadScene(level);
    }
}
