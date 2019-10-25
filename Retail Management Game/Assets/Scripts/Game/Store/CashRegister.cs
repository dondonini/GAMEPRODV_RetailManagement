using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CashRegister : MonoBehaviour
{
    [SerializeField] int maxQueueLength = 5;
    [SerializeField] Vector3 queueDirection = new Vector3(0.0f, 0.0f, 1.0f);

    [ReadOnly][SerializeField] Dictionary<GameObject, Vector3> queue = new Dictionary<GameObject, Vector3>();

    //*************************************************************************
    [Header("References")]

    [SerializeField] Transform queueStartPosition;

    //*************************************************************************
    [Header("Events")]

    public UnityEvent QueueChanged;

    //*************************************************************************
    // Managers

    GameManager gameManager;

    private void OnValidate()
    {
        queueDirection.Normalize();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        if (queueStartPosition)
            Gizmos.DrawLine(queueStartPosition.position, queueStartPosition.position + (queueDirection * maxQueueLength));

        Gizmos.color = Color.green;

        for (int p = 0; p < queue.Count; p++)
        {
            Gizmos.DrawCube(queue.Values.ElementAt(p), new Vector3(0.1f, 0.1f, 0.1f));
        }
    }

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

                // Delete sold product
                product.parent = null;
                Destroy(product.gameObject);
            }
            else
                Debug.LogError("Product is missing the StockItem component!", stockItem);
        }
    }

    #region Getters and Setters

    public Vector3 AddToQueue(GameObject customer)
    {
        if (queue.Count >= maxQueueLength) 
            return Vector3.zero;

        if (!customer.CompareTag("Customer")) 
            return Vector3.zero;

        if (queue.ContainsKey(customer))
        {
            Debug.Log("Customer is already in queue!", customer);
            return Vector3.zero;
        }

        Vector3 customerPosition = GetNextQueuePostion();

        queue.Add(customer, customerPosition);

        QueueChanged.Invoke();

        return customerPosition;
    }

    public void RemoveToQueue(GameObject customer)
    {
        bool result = queue.Remove(customer);

        if (!result) 
            Debug.LogWarning("Customer " + customer + " is not part of this queue!", customer);

        QueueChanged.Invoke();
    }

    public Vector3 GetNextQueuePostion()
    {
        float newPositionDistance = 0.0f;

        for (int i = 0; i < queue.Count; i++)
        {
            Bounds customerBound = EssentialFunctions.GetMaxBounds(queue.Keys.First());

            if (i == 0)
            {
                newPositionDistance += customerBound.size.magnitude * 0.5f;
                continue;
            }

            newPositionDistance += customerBound.size.magnitude;
        }

        return queueStartPosition.position + (queueDirection * newPositionDistance);
    }

    public Vector3 GetCustomerQueuePostion(GameObject customer)
    {
        Vector3 customerPosition;
        bool result = queue.TryGetValue(customer, out customerPosition);

        if (result)
            return customerPosition;
        else
            return Vector3.zero;
    }

    public int GetCustomerQueueRank(GameObject customer)
    {
        for (int i = 0; i < queue.Count; i++)
        {
            if (queue.Keys.ElementAt(i) == customer)
            {
                return ++i;
            }
        }

        return 0;
    }

    public bool IsFull()
    {
        return queue.Count >= maxQueueLength;
    }

    #endregion
}
