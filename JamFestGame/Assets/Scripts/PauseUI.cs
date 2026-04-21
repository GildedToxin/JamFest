using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
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
            pause();
        }
    }

    public void MainMenu()
    {
        paused = false;
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
        paused = false;
    }
    public void resume()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        UI.SetActive(false);
        paused = false;
    }
    public void pause()
    {
        Cursor.visible = !Cursor.visible;
        Cursor.lockState = Cursor.lockState == CursorLockMode.Confined ? CursorLockMode.Locked : CursorLockMode.Confined;;
        Time.timeScale = paused ? 1 : 0;
        paused = !paused;
        UI.SetActive(!UI.activeSelf);
    }
}
