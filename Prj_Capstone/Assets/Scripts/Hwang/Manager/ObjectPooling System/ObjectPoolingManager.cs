using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolingManager : MonoBehaviour
{
    [System.Serializable]
    private class ObjectInfo
    {
        public string objectName;
        public GameObject prefab;
        public int defaultCapacity;
    }

    [SerializeField] private List<ObjectInfo> objectInfos;

    private string objectName;

    private Dictionary<string, IObjectPool<GameObject>> objectPoolDictionary = new Dictionary<string, IObjectPool<GameObject>>();

    private void Awake()
    {
        Initialize();
    }


    private void Initialize()
    {
        for (int objectIndex = 0; objectIndex < objectInfos.Count; objectIndex++)
        {
            IObjectPool<GameObject> objectPool = new ObjectPool<GameObject>(CreatePooledObject, OnTakeFromPool, OnReturnToPool, OnDestroyPooledObject, true, objectInfos[objectIndex].defaultCapacity);

            if (objectPoolDictionary.ContainsKey(objectInfos[objectIndex].objectName))
            {
                Debug.LogWarning($"{objectInfos[objectIndex].objectName} is already pooled.");
                continue;
            }

            objectPoolDictionary.Add(objectInfos[objectIndex].objectName, objectPool);

            for (int objectCount = 0; objectCount < objectInfos[objectIndex].defaultCapacity; objectCount++)
            {
                objectName = objectInfos[objectIndex].objectName;
                CreatePooledObject().GetComponent<PooledObject>().ReleaseObject();
            }
        }
    }

    private GameObject CreatePooledObject()
    {
        GameObject pooledObject = Instantiate(objectInfos.FirstOrDefault(objectInfo => objectInfo.objectName.Equals(objectName)).prefab);
        pooledObject.GetComponent<PooledObject>().objectPool = objectPoolDictionary[objectName];
        pooledObject.transform.SetParent(transform);
        return pooledObject;
    }

    private void OnTakeFromPool(GameObject pooledObject)
    {
        pooledObject.SetActive(true);
    }

    private void OnReturnToPool(GameObject pooledObject)
    {
        pooledObject.SetActive(false);
    }

    private void OnDestroyPooledObject(GameObject pooledObject)
    {
        Destroy(pooledObject);
    }

    public GameObject GetGameObject(string objectName)
    {
        this.objectName = objectName;

        if (objectPoolDictionary.ContainsKey(objectName) == false)
        {
            Debug.LogError($"{objectName} is not assigned to pool.");
            return null;
        }

        return objectPoolDictionary[objectName].Get();
    }

    public void ReleaseGameObject(GameObject pooledObject)
    {
        pooledObject.GetComponent<PooledObject>().ReleaseObject();
    }
}
