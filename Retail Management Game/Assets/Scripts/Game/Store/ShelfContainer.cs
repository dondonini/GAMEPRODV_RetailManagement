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
    [SerializeField] private float playerDetectionDistance = 1.0f;

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
    [SerializeField] TextMeshProUGUI stockNumIndicator = null;
    [SerializeField] Animator billboardAnimator = null;
    [SerializeField] ShelfVisual shelfVisual = null;
    [SerializeField] GameObject stockToaster = null;
    
    Vector3[] pickupPositions = null;

    const float stuckObjectMaxTime = 1.0f;
    int previousStockAmount = 0;
    Dictionary<Transform, float> stuckObjects = new Dictionary<Transform, float>();

    List<ShelfContainer> adjacentShelves = new List<ShelfContainer>();

    MapManager mapManager;

    Bounds shelfBounds;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);

        Bounds boundsOfSelf = EssentialFunctions.GetMaxBounds(gameObject);

        if (allowPickup_F)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.forward * 0.5f), new Vector3(0.1f, boundsOfSelf.size.y, 0.1f));
        }

        if (allowPickup_B)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.forward * -0.5f), new Vector3(0.1f, boundsOfSelf.size.y, 0.1f));
        }

        if (allowPickup_L)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.right * -0.5f), new Vector3(0.1f, boundsOfSelf.size.y, 0.1f));
        }

        if (allowPickup_R)
        {
            Gizmos.DrawCube(boundsOfSelf.center + (transform.right * 0.5f), new Vector3(0.1f, boundsOfSelf.size.y, 0.1f));
        }

        Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.5f);
        Gizmos.DrawWireCube(boundsOfSelf.center, boundsOfSelf.size + new Vector3(playerDetectionDistance, 0.0f, playerDetectionDistance));

        
        foreach(ShelfContainer shelf in GetAllAdjacentShelves())
        {
            if (shelf == this)
                Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
            else
                Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 0.25f);

            Gizmos.DrawCube(shelf.transform.position + new Vector3(0.0f, 2.0f, 0.0f), new Vector3(0.1f, 0.1f, 0.1f));
        }
    }

    private void OnValidate()
    {
        // Always have at least one face available for pickup
        if (!allowPickup_F && !allowPickup_B && !allowPickup_L && !allowPickup_R)
        {
            allowPickup_F = true;
        }

        stockAmount = Mathf.Clamp(stockAmount, 0, shelfSize);

        UpdatePickupPositionsArray();
        UpdateVisuals();

        adjacentShelves.Clear();
        adjacentShelves.AddRange(GetAdjacentShelves());
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

                    int result = AddCrate(stockCrate);

                    Debug.Log(result);

                    if (result <= 0)
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
                stuckObjects.TryGetValue(other, out float timer);

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

        adjacentShelves.Clear();
        adjacentShelves.AddRange(GetAdjacentShelves());

        shelfBounds = EssentialFunctions.GetMaxBounds(gameObject);

        previousStockAmount = stockAmount;

        UpdateStockBillboard();
        UpdateVisuals();

        //Debug.Log(GetAllAdjacentShelves().Length);
    }

    private void Update()
    {
        // Update text when stock changes
        if (stockAmount != previousStockAmount)
        {
            UpdateStockBillboard();
            UpdateVisuals();
        }

        // Show billboard when player is near by or when low in stock
        if (stockAmount <= 2)
        { 
            billboardAnimator.SetBool("ShowNum", true);
        }
        else
        {
            billboardAnimator.SetBool("ShowNum", IsPlayerInDetectionBox());
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
            PushStockToaster(-givingAmount);
            return givingAmount;
        }
        else
        {
            stockAmount -= amount;
            PushStockToaster(-amount);
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
            Debug.LogWarning("Uhhhhh... why are you adding nothing to the shelf? Tf?");

        // Check if shelf has stock already
        if (ShelfStockType == StockTypes.None)
            // Apply new stock type
            ShelfStockType = stockType;

        // Check if existing shelf stock doesn't match what you're adding
        else if (stockType != ShelfStockType)
        {
            // Stocks do not match! - do nothing

            Debug.Log("StockType doesn't match shelf! " + amount);
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
                {
                    remainingStock = shelvesWithSpace[0].AddStock(remainingStock, stockType);

                    Debug.Log("Current Shelf Remaining: " + remainingStock);
                }

                // Evenly divide stock to all adjacent shelves including remaining
                else
                {
                    int newRemainingStock = 0;

                    for (int i = 0; i < remainingStock; i++)
                    {
                        newRemainingStock += shelvesWithSpace[i % shelvesWithSpace.Length].AddStock(stockType);
                    }

                    remainingStock = newRemainingStock;
                    //int shelfQuantity = shelvesWithSpace.Length;
                    //int shelfStockDividedQuantity = Mathf.FloorToInt(remainingStock / shelfQuantity);
                    //int shelfStockRemaining = remainingStock % shelfQuantity;

                    //int newRemaining = 0;

                    //for (int i = 0; i < shelvesWithSpace.Length; i++)
                    //{
                    //    int amountToAdd = shelfStockDividedQuantity;

                    //    // Add remaining stock to shelves
                    //    if (i < shelfStockRemaining)
                    //    {
                    //        amountToAdd++;
                    //    }

                    //    newRemaining += shelvesWithSpace[i].AddStock(amountToAdd, stockType);
                    //}

                    //Debug.Log("Current Shelf Remaining: " + remainingStock + " Total Remaining Stock: " + newRemaining);
                }
            }
        }

        PushStockToaster(amount - remainingStock);
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

        crate.UnclaimItem(gameObject);

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
        return stockAmount >= shelfSize;
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
        List<ShelfContainer> shelvesWithSpace = new List<ShelfContainer>(GetAllAdjacentShelves());

        for (int i = 0; i < adjacentShelves.Count; i++)
        {
            if (!adjacentShelves[i].IsFull())
                shelvesWithSpace.Add(adjacentShelves[i]);
        }

        return shelvesWithSpace.ToArray();
    }

    void UpdateStockBillboard()
    {
        if (stockAmount <= 2)
        {
            stockNumIndicator.CrossFadeColor(Color.red, 0.5f, false, false);
        }
        else
        {
            stockNumIndicator.CrossFadeColor(Color.white, 0.5f, false, false);
        }

        stockNumIndicator.SetText(stockAmount.ToString());
    }

    ShelfContainer[] GetAdjacentShelves()
    {
        //adjacentShelves.Clear();
        List<ShelfContainer> foundShelves = new List<ShelfContainer>();

        Collider[] adjacentObjects = Physics.OverlapBox(transform.position + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.1f, 1.0f, 1.1f));

        foreach (Collider adjacentObject in adjacentObjects)
        {
            if (adjacentObject.CompareTag("Shelf"))
            {
                ShelfContainer shelfContainer = adjacentObject.GetComponent<ShelfContainer>();
                if (shelfContainer != this && shelfContainer.shelfStockType == shelfStockType)
                    foundShelves.Add(adjacentObject.GetComponent<ShelfContainer>());
            }
        }

        return foundShelves.ToArray();
    }

    #endregion

    bool IsPlayerInDetectionBox()
    {
        Collider[] characters = Physics.OverlapBox(
                shelfBounds.center,
                shelfBounds.size + new Vector3(playerDetectionDistance, 0.0f, playerDetectionDistance) * 0.5f, 
                transform.rotation, 
                LayerMask.GetMask("Character")
            );

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].transform.root.CompareTag("Player"))
            {
                //Debug.Log("Player is near!");
                return true;
            }
        }

        return false;
    }

    ShelfContainer[] GetAllAdjacentShelves()
    {
        List<ShelfContainer> allAdjShelves = new List<ShelfContainer>();

        CollectShelves(this, ref allAdjShelves);

        void CollectShelves(ShelfContainer shelfContainer, ref List<ShelfContainer> collectedShelves)
        {
            shelfContainer.adjacentShelves.Clear();
            shelfContainer.adjacentShelves.AddRange(shelfContainer.GetAdjacentShelves());

            if (shelfContainer.adjacentShelves.Count > 0)
            {
                for (int i = 0; i < shelfContainer.adjacentShelves.Count; i++)
                {
                    if (collectedShelves.Contains(shelfContainer.adjacentShelves[i])) continue;

                    collectedShelves.Add(shelfContainer.adjacentShelves[i]);

                    CollectShelves(shelfContainer.adjacentShelves[i], ref collectedShelves);
                }
            }
        }

        return allAdjShelves.ToArray();
    }

    void UpdateVisuals()
    {
        if (shelfVisual)
            shelfVisual.StockAmountPercentage = (float)stockAmount / shelfSize;
        else
        {
            ShelfVisual sv = GetComponentInChildren<ShelfVisual>();
            if (sv)
            {
                shelfVisual = sv;
                UpdateVisuals();
            }
        }
    }

    void PushStockToaster(int amount)
    {
        GameObject newToaster = Instantiate(stockToaster) as GameObject;

        newToaster.transform.position = transform.position;

        Color fontColour = amount > 0 ? Color.green : Color.red;

        UIToaster uiToaster = newToaster.GetComponent<UIToaster>();
        uiToaster.SetupToaster(
            amount.ToString(),
            fontColour,
            1.0f,
            0.8f,
            EasingFunction.Ease.OutExpo,
            4.0f);
    }
}
