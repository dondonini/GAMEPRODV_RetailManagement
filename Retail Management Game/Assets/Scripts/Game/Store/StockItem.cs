using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockItem : MonoBehaviour
{
    [SerializeField] private StockTypes stockType = StockTypes.None;

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
}