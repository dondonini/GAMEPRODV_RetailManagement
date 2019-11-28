using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShelfContainer : MonoBehaviour
{
    [Header("Shelf Status")]
    [SerializeField] private StockTypes shelfStockType = StockTypes.None;
    [SerializeField] private int stockAmount = 0;
    [SerializeField] private int shelfSize = 10;

    [Header("Pickup Faces")]

    [Rename("Front")]
    [SerializeField] private bool allowPickup_F = true;

    [Rename("Back")]
    [SerializeField] private bool allowPickup_B = false;

    [Rename("Left")]
    [SerializeField] private bool allowPickup_L = false;

    [Rename("Right")]
    [SerializeField] private bool allowPickup_R = false;

    const float collisionSensitivity = 2.0f;

    [Header("References")]
    [SerializeField] TextMeshProUGUI debugText = null;
    
    Vector3[] pickupPositions = null;

    const float stuckObjectMaxTime = 1.0f;
    int previousStockAmount = 0;
    Dictionary<Transform, float> stuckObjects = new Dictionary<Transform, float>();

    List<ShelfContainer> adjacentShelves = new List<ShelfContainer>();

    MapManager mapManager;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.1f);

        Bounds boundsOfSelf = EssentialFunctions.GetMaxBounds(gameObject);

        if (allowPickup_F)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.forward * 0.5f), new Vector3(boundsOfSelf.size.x, boundsOfSelf.size.y, 0.0f));
        }

        if (allowPickup_B)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.forward * -0.5f), new Vector3(boundsOfSelf.size.x, boundsOfSelf.size.y, 0.0f));
        }

        if (allowPickup_L)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.right * -0.5f), new Vector3(0.0f, boundsOfSelf.size.y, boundsOfSelf.size.z));
        }

        if (allowPickup_R)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.right * 0.5f), new Vector3(0.0f, boundsOfSelf.size.y, boundsOfSelf.size.z));
        }
    }

    private void OnValidate()
    {
        // Always have at least one face available for pickup
        if (!allowPickup_F && !allowPickup_B && !allowPickup_L && !allowPickup_R)
        {
            allowPickup_F = true;
        }

        UpdatePickupPositionsArray();

        adjacentShelves.Clear();

        Collider[] adjacentObjects = Physics.OverlapBox(transform.position + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.1f, 1.0f, 1.1f));

        foreach (Collider adjacentObject in adjacentObjects)
        {
            if (adjacentObject.CompareTag("Shelf"))
            {
                ShelfContainer shelfContainer = adjacentObject.GetComponent<ShelfContainer>();
                if (shelfContainer != this && shelfContainer.shelfStockType == shelfStockType)
                    adjacentShelves.Add(adjacentObject.GetComponent<ShelfContainer>());
            }
        }
    }

    #region Collisions

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        GameObject other = contact.otherCollider.transform.root.gameObject;

        //Debug.Log(collision.relativeVelocity.magnitude);

        // Add products to the shelf if it hits it hard enough
        if (collision.relativeVelocity.magnitude < collisionSensitivity) return;

        switch (other.tag)
        {
            case "Product":
                {
                    Debug.Log("Adding product to shelf!");

                    StockItem stockItem = other.GetComponent<StockItem>();

                    // Claim the product before any other shelf can >:D
                    if (stockItem.IsClaimed()) return;
                    else stockItem.ClaimItem(gameObject);

                    int result = AddStock(stockItem.GetStockType());

                    if (result == 0)
                        Destroy(other);

                    break;
                }
            case "StockCrate":
                {
                    StockCrate stockCrate = other.GetComponent<StockCrate>();

                    // Claim the product before any other shelf can >:D
                    if (stockCrate.IsClaimed()) return;
                    else stockCrate.ClaimItem(gameObject);

                    int result = AddStock(stockCrate.GetQuantity(), stockCrate.GetStockType());

                    if (result == 0)
                        Destroy(other);

                    break;
                }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach(ContactPoint contactPoint in collision.contacts)
        {
            Transform other = contactPoint.otherCollider.transform.root;

            if (other == stuckObjects.ContainsKey(other))
            {
                float timer = 0.0f;
                stuckObjects.TryGetValue(other, out timer);

                if (timer <= 0.0f)
                {
                    Debug.Log("LOL! " + other + " is stuck in " + transform + "! XD");
                }
                else
                {
                    //stuckObjects.
                }
            }
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        UpdatePickupPositionsArray();

        // Get MapManager
        mapManager = MapManager.GetInstance();

        previousStockAmount = stockAmount;

        UpdateDebugText();
    }

    private void Update()
    {
        if (stockAmount != previousStockAmount)
        {
            UpdateDebugText();
        }

        previousStockAmount = stockAmount;
    }

    #region Getters and Setters

    public StockTypes ShelfStockType
    {
        get
        {
            return shelfStockType;
        }
        set
        {
            shelfStockType = value;
            if (value == StockTypes.None)
            {
                EmptyShelf();
            }
        }
    }

    

    public int MaxShelfAmount()
    {
        return shelfSize;
    }

    public int StockAmount()
    {
        return stockAmount;
    }

    public int GetStock(int amount = 1)
    {
        // Check if there's stock to give
        if (IsEmpty())
        {
            // Shelf is empty - give nothing
            return 0;
        }
        else if (stockAmount < amount)
        {
            // Shelf is almost empty - give remaining amount
            int givingAmount = stockAmount;
            EmptyShelf();
            return givingAmount;
        }
        else
        {
            stockAmount -= amount;
            // Shelf has enough stock requested - give amount
            return amount;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount">The amount of stock to add onto shelf</param>
    /// <param name="stockType">The stock type that is being added to the shelf</param>
    /// <returns>The remainder</returns>
    public int AddStock(int amount, StockTypes stockType)
    {

        // If this ever happens, you're actually dumb
        if (amount <= 0)
        {
            Debug.LogWarning("Uhhhhh... why are you adding nothing to the shelf? Tf?");
        }

        // Check if shelf has stock already
        if (ShelfStockType == StockTypes.None)
        {
            // Apply new stock type
            ShelfStockType = stockType;
        }

        // Check if existing shelf stock doesn't match what you're adding
        else if (stockType != ShelfStockType)
        {
            // Stocks do not match! - do nothing
            return amount;
        }

        // Fill shelves
        int availableShelfSpace = shelfSize - stockAmount;

        stockAmount += amount;
        if (stockAmount > shelfSize) 
            stockAmount = shelfSize;

        int remainingStock = amount - availableShelfSpace;

        if (remainingStock < 0) 
            remainingStock = 0;

        mapManager.UpdateAvailableStockTypes();

        // Add to adjacent shelves
        if (remainingStock > 0)
        {
            // No shelves adjacent - just return the remaining stock
            if (adjacentShelves.Count == 0) 
                return remainingStock;

            // All adjacent shelves are full - return the remaining stock
            if (IsAllAdjacentShelvesFull()) 
                return remainingStock;

            // Add remaining stock to adjacent shelves
            else
            {
                ShelfContainer[] shelvesWithSpace = GetAdjacentShelvesWithSpace();

                // Add all remaining stock to neighbouring shelf
                if (shelvesWithSpace.Length == 1)
                    return shelvesWithSpace[0].AddStock(amount, stockType);

                // Evenly divide stock to all adjacent shelves including remaining
                else
                {
                    int shelfQuantity = shelvesWithSpace.Length;
                    int shelfStockDividedQuantity = Mathf.FloorToInt(remainingStock / shelfQuantity);
                    int shelfStockRemaining = remainingStock % shelfQuantity;

                    for (int i = 0; i < shelvesWithSpace.Length; i++)
                    {
                        int amountToAdd = shelfStockDividedQuantity;

                        // Add remaining stock to shelves
                        if (shelfStockRemaining > 0)
                        {
                            amountToAdd++;
                            shelfStockRemaining--;
                        }

                        return shelvesWithSpace[i].AddStock(amountToAdd, stockType);
                    }
                }
            }

        }

        return remainingStock;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stockType">The stock type that is being added to the shelf</param>
    /// <returns>The remainder</returns>
    public int AddStock(StockTypes stockType)
    {
        return AddStock(1, stockType);
    }

    public int AddCrate(StockCrate crate)
    {
        int remainingStock = crate.SetQuantity(AddStock(crate.GetQuantity(), crate.GetStockType()));
        if (remainingStock < 0) remainingStock = 0;
        return remainingStock;
    }

    #endregion

    #region Helpers

    void EmptyShelf()
    {
        stockAmount = 0;
        ShelfStockType = StockTypes.None;
    }

    /// <summary>
    /// Update pickup positions
    /// </summary>
    void UpdatePickupPositionsArray()
    {
        List<Vector3> foundPositions = new List<Vector3>();

        if (allowPickup_F)
        {
            foundPositions.Add(transform.position + transform.forward);
        }

        if (allowPickup_B)
        {
            foundPositions.Add(transform.position + -transform.forward);
        }

        if (allowPickup_L)
        {
            foundPositions.Add(transform.position + -transform.right);
        }

        if (allowPickup_R)
        {
            foundPositions.Add(transform.position + transform.right);
        }

        pickupPositions = null;
        pickupPositions = foundPositions.ToArray();
    }

    /// <summary>
    /// Returns pickup positions
    /// </summary>
    /// <returns></returns>
    public Vector3[] GetPickupPositions()
    {
        return pickupPositions;
    }

    public bool IsEmpty()
    {
        return stockAmount == 0;
    }

    public bool IsFull()
    {
        return stockAmount == shelfSize;
    }

    bool IsAllAdjacentShelvesFull()
    {
        bool isAllFull = true;

        for (int i = 0; i < adjacentShelves.Count; i++)
        {
            if (!adjacentShelves[i].IsFull())
                isAllFull = false;
        }

        return isAllFull;
    }

    ShelfContainer[] GetAdjacentShelvesWithSpace()
    {
        List<ShelfContainer> shelvesWithSpace = new List<ShelfContainer>();

        for (int i = 0; i < adjacentShelves.Count; i++)
        {
            if (!adjacentShelves[i].IsFull())
                shelvesWithSpace.Add(adjacentShelves[i]);
        }

        return shelvesWithSpace.ToArray();
    }

    void UpdateDebugText()
    {
        debugText.SetText(stockAmount.ToString());
    }

    #endregion
}
