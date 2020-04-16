using System;
using UnityEngine;

public class StoreFloor : MonoBehaviour
{
    private static StoreFloor _storeFloor;
    private GameObject _floor;

    private void Awake()
    {
        if (_storeFloor)
        {
            Debug.LogWarning("There are multiple store floors. Why?");
            
            // There is suppose to be only one store floor instances
            DestroyImmediate(gameObject);
        }
        else
        {
            _storeFloor = this;
        }
    }

    private void Start()
    {
        _floor = gameObject;
    }

    public static StoreFloor GetInstance()
    {
        return _storeFloor;
    }

    public GameObject ExistingFloor => _floor;
    
    public Vector3 FloorSize => _floor.transform.localScale;

    public Vector3 FloorPosition => _floor.transform.position;

    public bool DoesFloorExist()
    {
        return _floor;
    }
}
