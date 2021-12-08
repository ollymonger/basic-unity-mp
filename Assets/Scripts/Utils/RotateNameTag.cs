using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateNameTag : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // rotate this gameobject to point away from camera
        transform.LookAt(Camera.main.transform);
        transform.Rotate(new Vector3(0, 180, 0));
    }
}
