using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalVariables : MonoBehaviour
{
    public static GlobalVariables Instance;

    public GameObject mainMenu;

    public GameObject multiplayerMenu;

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

    public void OpenMultiplayerMenu(){
        mainMenu.SetActive(false);
        multiplayerMenu.SetActive(true);
    }

    public void UpdateName(string name){
        selectedName = name;
    }

    public void UpdateIP(string IP){
        IPAddress = IP;
    }

    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
}
