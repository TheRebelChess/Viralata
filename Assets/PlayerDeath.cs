using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public GameManager gameManager;
    public float gameOverTimer = 3f;

    private void Start()
    {
        StartCoroutine(GameOverTimer(gameOverTimer));
    }
    IEnumerator GameOverTimer(float t)
    {
        yield return new WaitForSeconds(t);
        gameManager.GameOver();
    }
}
