using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    public GameObject levelSelect;
    public GameObject mainMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("JoyLevel");
    }

    public void OpenSettings() {}

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevel(string level)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(level);
    }
    public void OpenLevelSelect()
    {
        levelSelect.SetActive(true);
        mainMenu.SetActive(false);
    }
    public void OpenMainMenu()
    {
        levelSelect.SetActive(false);
        mainMenu.SetActive(true);
    }
}
