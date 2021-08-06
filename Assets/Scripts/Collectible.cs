using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectibleType : byte { Coin, Booster, NewGuy}

public class Collectible : MonoBehaviour
{
    [SerializeField] private CollectibleType type;

    public void Collect()
    {
        switch (type)
        {
            case CollectibleType.Coin: SessionMaster.current.AddScorepoint(this); break;
            case CollectibleType.NewGuy: SessionMaster.current.Reinforcement(this); break;
        }
    }
}
