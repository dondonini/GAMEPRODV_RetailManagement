using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TruckProduct
{
    public StockTypes stockType;
    public int stockAmount;

    public TruckProduct(StockTypes _stockType, int _stockAmount)
    {
        stockType = _stockType;
        stockAmount = _stockAmount;
    }
}
