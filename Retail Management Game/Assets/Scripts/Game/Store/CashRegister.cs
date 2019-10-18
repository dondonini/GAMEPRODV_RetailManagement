using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashRegister : MonoBehaviour
{
    //*************************************************************************
    // Managers

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PurchaseProduct(Transform product)
    {
        if (product.CompareTag("Product"))
        {
            StockItem stockItem = product.GetComponent<StockItem>();

            if (stockItem)
            {
                gameManager.AddScore(stockItem.GetPrice());
            }
            else
            {
                Debug.LogError("Product is missing the StockItem component!", stockItem);
            }
        }
    }
}
