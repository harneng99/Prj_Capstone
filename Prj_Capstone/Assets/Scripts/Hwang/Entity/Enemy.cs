using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    public EnemyCombat enemyCombat { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        enemyCombat = entityCombat as EnemyCombat;
    }
}
