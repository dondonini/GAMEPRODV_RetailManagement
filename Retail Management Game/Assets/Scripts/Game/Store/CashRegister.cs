using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashRegister : MonoBehaviour
{
    [SerializeField] int maxQueueLength = 5;

    [ReadOnly][SerializeField] List<Transform> queue = new List<Transform>();

    //*************************************************************************
    [Header("References")]

    [SerializeField] Transform queueStartPosition;
    [SerializeField] Transform queueEndDirection;

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

    #region Getters and Setters

    public bool AddToQueue(GameObject customer)
    {
        if (queue.Count >= maxQueueLength) return false;

        if (!customer.CompareTag("Customer")) return false;

        queue.Add(customer.transform);

        return true;
    }

    public void RemoveToQueue(GameManager customer)
    {
        bool result = queue.Remove(customer.transform);

        if (!result) Debug.LogWarning("Customer "
    }

    #endregion
}
