using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex_Tile : MonoBehaviour
{
    public GameObject champ;
    private Champ_Movement champ_movement;
    // Start is called before the first frame update
    private void Start()
    {
        champ_movement = FindObjectOfType<Champ_Movement>();
    }

    // Update is called once per frame
    private void OnMouseDown()
    {
        if(champ_movement != null)
        {
            Vector3 targetPosition = transform.position;
            champ_movement.MoveToPosition(targetPosition);
        }
    }
}
