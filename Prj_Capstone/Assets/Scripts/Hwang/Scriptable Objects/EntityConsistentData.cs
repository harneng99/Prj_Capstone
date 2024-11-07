using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newEntityConsistentData", menuName = "Data/Entity Consistent Data")]
public class EntityConsistentData : ScriptableObject
{
    [field: SerializeField] public float movementVelocity { get; private set; }

    [field: Header("Knight Animation")]
    [field: SerializeField] public float anmationDuration { get; private set; }
    [field: SerializeField] protected float destinationAlpha { get; private set; }
    [field: SerializeField] protected Vector3 destinationOffset { get; private set; }
    [field: SerializeField] protected Vector3 destinationRotation { get; private set; }
}
