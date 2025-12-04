using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject controlsMenu;
    public static bool isPaused;

    void Start()
    {
        ResumeGame(); 

        if (controlsMenu != null )
            controlsMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pauseMenu != null) pauseMenu.SetActive(true);
        if (controlsMenu != null) controlsMenu.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (controlsMenu != null) controlsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Controls()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (controlsMenu != null) controlsMenu.SetActive(true);

        Time.timeScale = 0;
        isPaused = false;
    }

    public void Back()
    {
        if (controlsMenu != null) controlsMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
