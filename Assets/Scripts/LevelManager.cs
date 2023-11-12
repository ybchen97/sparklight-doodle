using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager manager;

    // public GameObject gameOverScreen;
    public TextMeshProUGUI scoreText;
    // public TextMeshProUGUI gameOverScoreText;
    // public TextMeshProUGUI infoText;
    public int score;
    // public int missPenalty = 5;

    void Awake()
    {
        manager = this;
    }

    void Start()
    {
        Time.timeScale = 1;
    }

    // public void GameOver()
    // {
    //     Time.timeScale = 0;
    //     gameOverScoreText.text = scoreText.text;
    //     gameOverScreen.SetActive(true);
    // }

    public void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene(0);
    }

    // public void CatchFish(string name, int amount)
    // {
    //     infoText.text = "You caught a " + name + "! +" + amount.ToString() + " points.";
    //     infoText.gameObject.SetActive(true);
    //     manager.IncreaseScore(amount);
    // }

    public void IncreaseScore(int amount)
    {
        score += amount;
        scoreText.text = score.ToString();
    }

    // public void DeductScoreFromMiss()
    // {
    //     score -= missPenalty;
    //     scoreText.text = "Score: " + score.ToString();
    //     infoText.text = "Miss! -" + missPenalty.ToString() + " points.";
    //     infoText.gameObject.SetActive(true);
    // }
}
