using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CashRegister : MonoBehaviour
{
    [SerializeField] int maxQueueLength = 5;
    [Tooltip("The gap size between customers in the queue.")]
    [SerializeField] float queueGap = 0.1f;
    [SerializeField] Vector3 queueDirection = new Vector3(0.0f, 0.0f, 1.0f);

    [ReadOnly][SerializeField] List<GameObject> queue = new List<GameObject>();

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
            if (queue.Count == 0)
                Gizmos.DrawLine(queueStartPosition.position, queueStartPosition.position + (queueDirection * maxQueueLength));
            else
                Gizmos.DrawLine(queueStartPosition.position, CalculateQueuePosition(queue.Count - 1));

        Gizmos.color = Color.green;

        for (int p = 0; p < queue.Count; p++)
        {
           Gizmos.DrawCube(CalculateQueuePosition(p), new Vector3(0.1f, 0.1f, 0.1f));
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
        // Check if queue is full
        if (queue.Count >= maxQueueLength) 
            return Vector3.zero;

        // Check if customer is actually a customer (Should never happen)
        if (!customer.CompareTag("Customer"))
        {
            Debug.LogWarning("Either the customer is not tagged as Customer or you're trying to add a random object into the queue.", customer);
            return Vector3.zero;
        }

        // Check if customer is already in the queue.
        if (queue.Contains(customer))
        {
            Debug.Log("Customer is already in queue! If you're trying to get the customer's position in line, use GetCustomerQueuePosition(GameObject customer) instead.", customer);
            return GetCustomerQueuePostion(customer);
        }

        // Setup customer in queue
        if (queue.Count == 0)
            queue.Add(customer);
        else
        {
            // Add customer to queue depending on their distance
            float customerDistance = Vector3.Distance(customer.transform.position, transform.position);
            bool isCloser = false;

            for (int i = 0; i < queue.Count; i++)
            {
                if (Vector3.Distance(queue[i].transform.position, transform.position) > customerDistance)
                {
                    queue.Insert(i, customer);
                    isCloser = true;
                    break;
                }
            }

            if (!isCloser)
                queue.Add(customer);
        }

        
        

        // Invoke attached customers in queue that line has changed
        QueueChanged.Invoke();

        // Return customer position in line
        return GetCustomerQueuePostion(customer);
    }

    public void RemoveFromQueue(GameObject customer)
    {
        bool result = queue.Remove(customer);

        if (!result) 
            Debug.LogWarning("Customer " + customer + " is not part of this queue!", customer);
        else
            QueueChanged.Invoke();
    }

    public Vector3 GetFrontOfLinePosition()
    {
        return queueStartPosition.position;
    }

    public Vector3 GetCustomerQueuePostion(GameObject customer)
    {
        return CalculateQueuePosition(customer);
    }

    public int GetCustomerQueueRank(GameObject customer)
    {
        // Get customer rank in queue list
        for (int i = 0; i < queue.Count; i++)
        {
            if (customer == queue[i])
            {
                return i;
            }
        }

        return -1;
    }

    public bool IsFull()
    {
        return queue.Count >= maxQueueLength;
    }

    #endregion

    Vector3 CalculateQueuePosition(GameObject customer)
    {
        // Get customer rank in queue list
        int rank = GetCustomerQueueRank(customer);

        if (rank != -1)
            return CalculateQueuePosition(rank);
        else
            return Vector3.zero;
    }

    Vector3 CalculateQueuePosition(int rank)
    {
        // Give starter position if rank is 0
        if (rank == 0)
            return queueStartPosition.position;

        float distance = 0;

        for (int i = 0; i < rank; i++)
        {
            if (i == 1 && i == rank)
                distance += EssentialFunctions.GetMaxBounds(queue[i]).size.magnitude * 0.5f;
            else
            {
                distance += EssentialFunctions.GetMaxBounds(queue[i]).size.magnitude;
            }

            distance += queueGap;
        }

        // Calculate distance from queue start position
        return queueStartPosition.position + (queueDirection * distance);
    }
}
