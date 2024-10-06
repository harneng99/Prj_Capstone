using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : Combat
{
    public PlayerCharacter currentTarget;

    public void SetTarget()
    {
        PlayerCharacter[] players = FindObjectsOfType<PlayerCharacter>();
        FindAnyObjectByType(typeof(EnemyCombat));
    }
}
