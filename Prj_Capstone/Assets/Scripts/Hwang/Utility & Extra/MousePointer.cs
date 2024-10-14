using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePointer : MonoBehaviour
{
    [SerializeField] private Texture2D defaultCursorSprite;
    [SerializeField] private Vector2 pointerOffset;

    private void Awake()
    {
        Cursor.SetCursor(defaultCursorSprite, pointerOffset, CursorMode.ForceSoftware);
    }
}
