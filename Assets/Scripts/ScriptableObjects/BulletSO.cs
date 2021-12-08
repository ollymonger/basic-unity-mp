using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new BulletSO", menuName = "ScriptableObjects/BulletSO", order = 1)]
public class BulletSO : ScriptableObject
{
    public GameObject prefab;
    public float speed;
    public float damage;
    public float lifeTime;
    
}
