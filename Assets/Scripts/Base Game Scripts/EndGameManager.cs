﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameType
{
    Moves,
    Time
}

[System.Serializable]
public class EndGameRequirements
{
    public GameType gameType;
    public int counterValue;
}

public class EndGameManager : MonoBehaviour
{
    public GameObject movesLabel;
    public GameObject timeLabel;
    public GameObject youWinPanel;
    public GameObject tryAgainPanel;
    public Text counter;
    public EndGameRequirements requirements;
    public int currentCounterValue;
    private Board board;
    public float timerSeconds;
    private GameData gameData;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        gameData = FindObjectOfType<GameData>();

        SetGameType();
        SetupGame();
    }

    void SetGameType()
    {
        if (board != null && board.world != null)
        {
            if (board.level < board.world.levels.Length)
            {
                if (board.world.levels[board.level] != null)
                {
                    requirements = board.world.levels[board.level].endGameRequirements;
                }
            }
        }
    }

    void SetupGame()
    {
        currentCounterValue = requirements.counterValue;
        if (requirements.gameType == GameType.Moves)
        {
            movesLabel.SetActive(true);
            timeLabel.SetActive(false);
        }
        else
        {
            timerSeconds = 1;
            movesLabel.SetActive(false);
            timeLabel.SetActive(true);
        }
        counter.text = "" + currentCounterValue;
    }

    // public void DecreaseCounterValue()
    // {
    //     if (board.currentState != GameState.pause)
    //     {
    //         currentCounterValue--;
    //         counter.text = "" + currentCounterValue;
    //         if (currentCounterValue <= 0)
    //         {
    //             BattleManager battleManager = FindObjectOfType<BattleManager>();
    //             if (battleManager.GetCurrentHealth() > 0)
    //             {
    //                 WinGame();
    //             }
    //             else
    //             {
    //                 //LoseGame(); achieved in DamagePlayer()
    //             }
    //         }
    //     }
    // }

    public void IncreaseCounterValue()
    {
        if (board.currentState != GameState.pause)
        {
            currentCounterValue++;
            counter.text = "" + currentCounterValue;
        }
    }

    public void WinGame()
    {
        youWinPanel.SetActive(true);
        board.currentState = GameState.win;
        // currentCounterValue = 0;
        counter.text = "" + currentCounterValue;
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();

        if (gameData != null)
        {
            int bestBefore = gameData.saveData.highScores[board.level];
            if (currentCounterValue < bestBefore)
            {
                gameData.saveData.highScores[board.level] = currentCounterValue;
            }
            gameData.Save();
        }
    }

    public void LoseGame()
    {
        tryAgainPanel.SetActive(true);
        board.currentState = GameState.lose;
        // Debug.Log("Try again!");
        // currentCounterValue = 0;
        counter.text = "" + currentCounterValue;
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();
    }

    // Update is called once per frame
    void Update()
    {
        if (requirements.gameType == GameType.Time && currentCounterValue > 0)
        {
            timerSeconds -= Time.deltaTime;
            if (timerSeconds <= 0)
            {
                // DecreaseCounterValue();
                IncreaseCounterValue();
                timerSeconds = 1;
            }
        }
    }
}
