using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    public GameManager gameManager { get; private set; }

    private void Awake()
    {
        Instance = this;

        gameManager = GetComponentInChildren<GameManager>();
    }
}
