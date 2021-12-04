using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalVariables : MonoBehaviour
{
    public static GlobalVariables Instance;

    public bool connectToServer = false;

    public string IPAddress = "";

    public string selectedName = "";

    public void LoadAsMultiplayer()
    {
        // Load Game Scene using SceneManager
        SceneManager.LoadScene("GAME");
        connectToServer = true;
    }

    public void LoadAsSingleplayer()
    {   
        // Load Game Scene using SceneManager
        SceneManager.LoadScene("GAME");
        connectToServer = false;
    }

    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
}
