using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class WeaponsHandler : MonoBehaviour
{
    public WeaponSO[] weapons;
    public int currentWeaponIndex = 0;

    public WeaponSO currentWeaponSO;
    GameObject weaponHolder;

    void Start() {
        transform.GetComponent<Player>().localPlayerStats.currentWeapon = weapons[currentWeaponIndex];
        currentWeaponSO = Instantiate(weapons[currentWeaponIndex]);
        transform.GetComponent<Player>().localPlayerStats.currentWeapon = currentWeaponSO;
        weaponHolder = Instantiate(currentWeaponSO.WeaponPrefab, transform.Find("WeaponHolder").position, Quaternion.identity, transform.Find("WeaponHolder"));
    }
    
    public void Fire() {
        if(transform.GetComponent<Player>().localPlayerStats.isLocalPlayer){
            Debug.Log("Firing");
            HandleFire(weaponHolder.transform);
            JObject data = new JObject();
            data["type"] = "fire";
            data["id"] = transform.GetComponent<Player>().localPlayerStats.playerId;
            transform.GetComponent<Multiplayer>().SendCommand(data);
        }
    }

    public void ExternalFireHandle() {
        Debug.Log("Firing");
        HandleFire(weaponHolder.transform);
    }

    public void HandleFire(Transform firePoint){
        // Handles all incoming fire requests
        GameObject bullet = Instantiate(currentWeaponSO.bullet.prefab, firePoint.position + transform.forward, weaponHolder.transform.rotation);
        bullet.GetComponent<Renderer>().material.color = Color.red;
        bullet.transform.position = firePoint.position + transform.forward;
        bullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * 500f * 4f);
        Destroy(bullet, 2f);
    }
}
