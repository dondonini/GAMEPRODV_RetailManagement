using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockItem : MonoBehaviour
{
    [SerializeField] protected StockTypes stockType = StockTypes.None;

    GameObject firstClaim = null;

    public StockTypes GetStockType()
    {
        return stockType;
    }

    public GameObject IsClaimed()
    {
        return firstClaim;
    }

    public void ClaimItem(GameObject other)
    {
        firstClaim = other;
    }

    public void UnclaimItem(GameObject claimer)
    {
        if (firstClaim == claimer)
            firstClaim = null;
        else
        {
            Debug.Log(claimer + " is not first claim of " + gameObject);
        }
    }
}