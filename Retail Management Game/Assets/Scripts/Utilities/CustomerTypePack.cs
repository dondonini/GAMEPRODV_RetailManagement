using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomerTypePack
{
    [HideInInspector] public string name = "Customer";
    public GameObject customerPrefab;
    public int chancesOfSpawning = 50;
}
