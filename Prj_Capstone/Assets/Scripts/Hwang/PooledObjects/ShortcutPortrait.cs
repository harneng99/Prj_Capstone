using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutPortrait : PooledObject
{
    public Player mercenary { get; set; }

    public void OnClick()
    {
        Manager.Instance.gameManager.SetVirtualCameraFollowTransformTo(mercenary.transform);
        Manager.Instance.gameManager.Select(mercenary);
    }
}
