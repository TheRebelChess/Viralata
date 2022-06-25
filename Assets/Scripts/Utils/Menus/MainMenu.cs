using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject creditsScreen;
    public GameObject optionsScreen;
    public GameObject mainMenuScreen;
    public GameObject controlsScreen;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ShowCreditsScreen()
    {
        mainMenuScreen.SetActive(false);
        creditsScreen.SetActive(true);
    }

    public void ShowOptionsScreen()
    {

    }

    public void ShowControlsScreen()
    {
        mainMenuScreen.SetActive(false);
        controlsScreen.SetActive(true);
    }

    public void ShowMenuScreen()
    {
        creditsScreen.SetActive(false);
        controlsScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
    }
}
