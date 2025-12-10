using UnityEngine;

/// <summary>
///  implementation of simple pause menu, adapted from this tutorial : https://www.youtube.com/watch?v=9dYDBomQpBQ
/// </summary>
public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject controlsMenu;
    public static bool isPaused { get; private set; }

    void Start()
    {
        isPaused = false;

        ResumeGame();

        pauseMenu.SetActive(false);
        controlsMenu.SetActive(false);

        LockCursor();
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

        Time.timeScale = 0f;
        isPaused = true;

        UnlockCursor();
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        LockCursor();
    }

    public void Controls()
    {
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(true);

        Time.timeScale = 0;
        isPaused = true;

        UnlockCursor();
    }

    public void Back()
    {
        controlsMenu.SetActive(false);
        pauseMenu.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        UnlockCursor();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
