using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public static GlobalVariables Instance;

    public bool connectToServer = false;

    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
}
