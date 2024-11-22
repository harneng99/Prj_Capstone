using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PooledObject : MonoBehaviour
{
    public IObjectPool<GameObject> objectPool;

    public void ReleaseObject()
    {
        transform.SetParent(Manager.Instance.objectPoolingManager.transform);
        objectPool.Release(gameObject);
    }
}
