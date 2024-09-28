using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // TODO: Add Game Data
    public string lastPlayTime;

    public GameData()
    {
        // TODO: Initialize Data
        this.lastPlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
