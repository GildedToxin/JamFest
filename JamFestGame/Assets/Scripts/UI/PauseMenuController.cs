using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(transform.GetChild(0).gameObject.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; 
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        print(1);
        Time.timeScale = 1f;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void OpenSettings()
    {

    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
