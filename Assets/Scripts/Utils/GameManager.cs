using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject hudCanvas;
    public GameObject gameOverCanvas;
    public CinemachineInputProvider cinemachineInputProvider;

    public void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        hudCanvas.SetActive(false);
        cinemachineInputProvider.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        gameOverCanvas.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
    }
}
