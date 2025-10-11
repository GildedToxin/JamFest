using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
 

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false); 
    }

    public void OpenSettings()
    {

    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
