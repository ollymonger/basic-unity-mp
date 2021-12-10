using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
   public Texture2D crosshairTexture;
    public float crosshairScale = 1;
    public float maxAngle;
    public float minAngle;

    public float lookHeight;

    public void LookHeight(float height){
        lookHeight += height;
        if(lookHeight > maxAngle || lookHeight < minAngle){
            lookHeight -= height;
        }
    }

    void OnGUI() {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        screenPos.y = Screen.height - screenPos.y;
        GUI.DrawTexture(new Rect(screenPos.x, screenPos.y - lookHeight, crosshairScale, crosshairScale), crosshairTexture);
    }
}
