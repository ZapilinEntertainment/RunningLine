using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIController : MonoBehaviour, IPauseHandler
{
    [SerializeField] private Text scoreField;
    [SerializeField] private GameObject startWindow, victoryPanel, retryPanel;
    private bool gamePaused;
    private SessionMaster sessionMaster;

    private void Awake()
    {
        sessionMaster = SessionMaster.current;
        sessionMaster.AssignUIController(this);
    }

    private void Start()
    {
        
        gamePaused = sessionMaster.paused;
        sessionMaster.onPauseStart += this.OnPauseStart;
        sessionMaster.onPauseEnd += this.OnPauseEnd;

        startWindow.SetActive(true);
        victoryPanel.SetActive(false);
        retryPanel.SetActive(false);
    }

    public void StartButton()
    {
        sessionMaster.playerController.StartRun();
        startWindow.SetActive(false);
    }
    public void DrawRetry()
    {
        retryPanel.SetActive(true);
    }
    public void RetryButton()
    {
        retryPanel.SetActive(false);
        sessionMaster.Restart();
        startWindow.SetActive(true);
    }

    public void DrawVictory()
    {
        victoryPanel.SetActive(true);
    }

    public void OnPauseEnd()
    {
        gamePaused = false;
    }

    public void OnPauseStart()
    {
        gamePaused = true;
    }

    private void Update()
    {
        scoreField.text = sessionMaster.totalScore.ToString();
    }
}
