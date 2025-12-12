using UnityEngine;

/// <summary>
///  implementation of simple pause menu, adapted from this tutorial : https://www.youtube.com/watch?v=9dYDBomQpBQ
/// </summary>
public class Pause : MonoBehaviour
{
    // ui object references
    public GameObject pauseMenu;
    public GameObject controlsMenu;

    // ispaused state variable
    public static bool isPaused { get; private set; }

    void Start()
    {
        // start game states
        isPaused = false;
        ResumeGame();
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(false);
        LockCursor();
    }

    void Update()
    {    
        // need to hook up to my input actions
        if (Input.GetKeyUp(KeyCode.Escape) || (Input.GetKeyUp(KeyCode.P) || (Input.GetKeyUp(KeyCode.Joystick1Button7)))) 
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // pausing the game sets the ui and freezes time, also unlocks cursor to click on screen
    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        controlsMenu.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
        UnlockCursor();
    }

    // resume game removes ui resets time and locks cursor
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        LockCursor();
    }

    // controls menu sets relevant ui and keeps time frozen
    public void Controls()
    {
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
        UnlockCursor();
    }

    // returns from controls menu to pause screen
    public void Back()
    {
        controlsMenu.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        UnlockCursor();
    }

    // simple quit game function
    public void QuitGame()
    {
        Application.Quit();
    }

    // unlocking the cursor form screen and giving visibility 
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //locking cursor to screen and removing visibility
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
