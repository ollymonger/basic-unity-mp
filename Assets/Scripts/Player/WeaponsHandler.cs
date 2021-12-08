using System.Collections;
using System.Collections.Generic;
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
        GameObject.FindObjectOfType<Player>().localPlayerStats.currentWeapon = currentWeaponSO;
        weaponHolder = Instantiate(currentWeaponSO.WeaponPrefab, transform.Find("WeaponHolder").position, Quaternion.identity, transform.Find("WeaponHolder"));
    }
    
    public void Fire() {
        Debug.Log("Firing");
        HandleFire(weaponHolder.transform);
    }

    public void HandleFire(Transform firePoint){
        // Handles all incoming fire requests
        GameObject bullet = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), firePoint.position + transform.forward, weaponHolder.transform.rotation);
        bullet.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        bullet.GetComponent<Renderer>().material.color = Color.red;
        bullet.transform.position = firePoint.position + transform.forward;
        bullet.AddComponent<Rigidbody>();
        bullet.GetComponent<Rigidbody>().AddForce(firePoint.forward * 1000f * 4f);
    }
}
