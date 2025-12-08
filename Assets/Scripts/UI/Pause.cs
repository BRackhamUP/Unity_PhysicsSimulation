using UnityEngine;

/// <summary>
///  implementation of simple pause menu, adapted from this tutorial : https://www.youtube.com/watch?v=9dYDBomQpBQ
/// </summary>
public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject controlsMenu;
    public static bool isPaused;

    void Start()
    {
        ResumeGame();

        if (controlsMenu != null)
            controlsMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) // need to hook up to my input actions
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);

        controlsMenu.SetActive(false);

        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(false);

        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Controls()
    {
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(true);

        Time.timeScale = 0;
        isPaused = false;
    }

    public void Back()
    {
        controlsMenu.SetActive(false);
        pauseMenu.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
