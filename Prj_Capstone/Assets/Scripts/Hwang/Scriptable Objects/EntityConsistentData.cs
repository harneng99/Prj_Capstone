using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newEntityConsistentData", menuName = "Data/Entity Consistent Data")]
public class EntityConsistentData : ScriptableObject
{
    [field: SerializeField] public float movementVelocity { get; private set; }
}
