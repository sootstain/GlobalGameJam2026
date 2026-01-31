using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void GoToLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void NextLevel()
    {
        GoToLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
