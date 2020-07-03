using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    [Header ("Level Information")]
    public string levelToLoad;
    public int level;
    private GameData gameData;
    private int starsActive;
    private int highScore;

    [Header ("UI Stuff")]
    public Image[] stars;
    public Text highScoreText;
    public Text starText;

    // Start is called before the first frame update
    void OnEnable()
    {
        gameData = FindObjectOfType<GameData>();
        LoadData();
        ActivateStars();
        SetText();
    }

    void LoadData()
    {
        if (gameData != null)
        {
            starsActive = gameData.saveData.stars[level];
            highScore = gameData.saveData.highScores[level];
        }
    }

    void SetText()
    {
        highScoreText.text = "" + highScore;
        starText.text = "" + starsActive + "/3";
    }

    void ActivateStars()
    {
        // COME BACK TO THIS WHEN THE BINARY FILE IS DONE!!!
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Cancel()
    {
        this.gameObject.SetActive(false);
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level", level);
        SceneManager.LoadScene(levelToLoad);
    }
}
