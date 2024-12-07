using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void GameStartButton()
    {
        SceneManager.LoadScene("stage_select");
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
