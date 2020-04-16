using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockItem : MonoBehaviour
{
    [SerializeField] protected StockTypes stockType = StockTypes.None;

    private GameObject _firstClaim = null;

    public StockTypes GetStockType()
    {
        return stockType;
    }

    public GameObject IsClaimed()
    {
        return _firstClaim;
    }

    public void ClaimItem(GameObject other)
    {
        _firstClaim = other;
    }

    public void UnclaimItem(GameObject claimer)
    {
        if (_firstClaim == claimer)
            _firstClaim = null;
        else
        {
            Debug.Log(claimer + " is not first claim of " + gameObject);
        }
    }

    public virtual StockItem TakeProduct()
    {
        return this;
    }
}