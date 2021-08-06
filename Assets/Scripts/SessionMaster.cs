using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class SessionMaster : MonoBehaviour
{
    public static SessionMaster current { get; private set; }
    public static bool pause { get { return current?.paused ?? false; } }

    [SerializeField] private int pointsPerCoin = 1;
    public bool paused { get; private set; }
    public int totalScore { get; private set; }
    public PlayerController playerController { get; private set; }
    public Action onPauseStart, onPauseEnd;
    public EffectsMaster effectsMaster { get; private set; }
    public UIController uicontroller { get; private set; }

    private List<Collectible> disabledCollectiblesList;

    private void Awake()
    {
        if (current != this) Destroy(current);
        current = this;
        disabledCollectiblesList = new List<Collectible>();
        effectsMaster = EffectsMaster.current;
        if (effectsMaster == null)
        {
            effectsMaster = new GameObject("effectsMaster").AddComponent<EffectsMaster>();
        }
    }

    public void Restart()
    {
        playerController.Restart();
        if (disabledCollectiblesList.Count > 0)
        {
            foreach (var b in disabledCollectiblesList) b.gameObject.SetActive(true);
            disabledCollectiblesList.Clear();
        }        
    }
    public void Fail()
    {
        uicontroller.DrawRetry();
        totalScore = 0;
    }
    public void Victory()
    {
        effectsMaster.FinishEffect(playerController.transform.position + Vector3.up);
        uicontroller.DrawVictory();
    }

    public void AssignPlayerController(PlayerController pc)
    {
        playerController = pc;
    }
    public void AssignUIController(UIController i_uic)
    {
        uicontroller = i_uic;
    }

    public void AddScorepoint(Collectible c)
    {
        totalScore += pointsPerCoin;
        disabledCollectiblesList.Add(c);
        c.gameObject.SetActive(false);
        effectsMaster.CollectEffect(c.transform.position);
    }

    public void Reinforcement(Collectible c)
    {
        playerController.AddNewGuy(c.transform.position);
        disabledCollectiblesList.Add(c);
        c.gameObject.SetActive(false);
        effectsMaster.CollectEffect(c.transform.position);
    }
}
