using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
    public GameObject UI;
    public bool paused = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = paused ? 1 : 0;
            paused = !paused;
            UI.SetActive(!UI.activeSelf);
        }
    }

    public void MainMenu()
    {
        paused = false;
        SceneManager.LoadScene("MainMenu");
    }
    public void resume()
    {
        Time.timeScale = 1;
        UI.SetActive(false);
        paused = false;
    }
}
