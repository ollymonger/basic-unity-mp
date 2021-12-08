using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(fileName = "New WeaponSO", menuName = "WeaponSO")]

public class WeaponSO : ScriptableObject
{
    public string WeaponName;

    public int ClipMax;

    public int WeaponDamage;
    public float WeaponRange;

    public GameObject WeaponPrefab;

    public WeaponSO(WeaponSO playerWeapon){
        playerWeapon.name = WeaponName;
        playerWeapon.ClipMax = ClipMax;
        playerWeapon.WeaponName = WeaponName;
        playerWeapon.WeaponDamage = WeaponDamage;
        playerWeapon.WeaponRange = WeaponRange;
        playerWeapon.WeaponPrefab = WeaponPrefab;
    }

    public void Fire() {
        Debug.Log("Firing");
    }
}
