using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject hudCanvas;
    public GameObject gameOverCanvas;
    public GameObject pauseScreen;
    public CinemachineInputProvider cinemachineInputProvider;
    public Inventory inventoryScript;
    public bool isPaused;
    public bool inPlayerInteraction;

    public void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        inventoryScript.Initialize();
    }

    public void GameOver()
    {
        SceneManager.LoadScene(2);
        //Time.timeScale = 0f;
        //hudCanvas.SetActive(false);
        //cinemachineInputProvider.enabled = false;
        //Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
        //gameOverCanvas.SetActive(true);
    }

    public void PauseGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f;
            hudCanvas.SetActive(true);
            cinemachineInputProvider.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            pauseScreen.SetActive(false);
        }
        else
        {
            isPaused = true;
            Time.timeScale = 0f;
            hudCanvas.SetActive(false);
            cinemachineInputProvider.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pauseScreen.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !inPlayerInteraction)
        {
            PauseGame();
        }

        print(isPaused);
    }
}
