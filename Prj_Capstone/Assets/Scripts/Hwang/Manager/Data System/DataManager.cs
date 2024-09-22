using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

public class DataManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool disableAutoSaving = false;
    [SerializeField] private bool initializeDataIfNull = false;
    [SerializeField] private bool overrideSelectedProfileId;
    [SerializeField] private string testSelectedProfileId = "test";

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    public string selectedProfileId { get; private set; } // Profile Id of current save file. Initial value is null when nothing is selected. Value changes when the player selects a save slot.

    public GameData gameData { get; private set; } // current selected Profile Id's game data
    private List<IDataPersistance> dataPersistanceObjects;
    private FileDataHandler dataHandler;

    private void Awake()
    {
        if (disableAutoSaving)
        {
            Debug.LogWarning("Auto saving is currently disabled. No auto save supported when you leave the game.");
        }

        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

        InitializeSelectedProfileId();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // sceneLoaded�� OnEnable�� Start ���̿� ȣ��ȴ�. Unity ���� �������� ��õ�ϴ� subscribe ��ġ�� OnEnable�̴�.
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �Ź� scene�� load�ɶ����� Monobehaviour�� ��ӹް� �ִ� ������Ʈ �߿��� IDataPersistance�� ���� �ִ� ������Ʈ�� ã�� List ���·� ����
        // �Ź� �� �ʿ�� �����.
        // TODO: ���Ŀ� ����
        dataPersistanceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void ChangeSelectedProfileIdAndLoadGameWithData(string newProfileId)
    {
        selectedProfileId = newProfileId;
        LoadGame();
    }

    public void DeleteProfileData(string profileId)
    {
        dataHandler.Delete(profileId);
        InitializeSelectedProfileId();
        LoadGame();
    }

    private void InitializeSelectedProfileId()
    {
        if (overrideSelectedProfileId)
        {
            selectedProfileId = testSelectedProfileId;
            Debug.LogWarning("Overrode selected profile id with test id: " + testSelectedProfileId);
        }
    }

    public void NewGame()
    {
        gameData = new GameData();
        Debug.Log("New data created");
    }

    public void SaveGame()
    {
        if (disableAutoSaving) return;

        if (gameData == null)
        {
            Debug.LogWarning("No data was found. A new game needs to be started before data can be saved.");
            return;
        }

        foreach (IDataPersistance dataPersistanceObject in dataPersistanceObjects)
        {
            dataPersistanceObject.SaveData(gameData);
        }

        gameData.lastPlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        dataHandler.Save(gameData, selectedProfileId);
    }

    public void LoadGame()
    {
        if (disableAutoSaving) return;

        gameData = dataHandler.Load(selectedProfileId);

        if (gameData == null && initializeDataIfNull)
        {
            NewGame();
        }

        if (gameData == null)
        {
            Debug.Log("No Data found. A new game has to be started.");
            return;
        }

        foreach (IDataPersistance dataPersistanceObject in dataPersistanceObjects)
        {
            dataPersistanceObject.LoadData(gameData);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistance> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistance> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistance>();

        return new List<IDataPersistance>(dataPersistenceObjects);
    }

    public bool HasGameData()
    {
        return dataHandler.LoadAllProfiles().Count > 0;
    }

    public bool HasGameData(string profileId)
    {
        return dataHandler.Load(profileId) != null;
    }

    public Dictionary<string, GameData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }
}
