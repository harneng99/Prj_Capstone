using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Champ_Movement : MonoBehaviour
{
    public bool isSelected = false;
        
    private void OnMouseDown()
    {
        isSelected = true;
    }
    transform.position = targetPosition;
}
