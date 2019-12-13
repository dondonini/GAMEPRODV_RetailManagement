using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StockCrate : MonoBehaviour
{
    [SerializeField] StockTypes stockType = StockTypes.None;
    [SerializeField] int stockQuantity = 0;
    [SerializeField] int maxQuantity = 10;
    [SerializeField] float billboardDistance = 1.0f;

    [SerializeField] float playerDetectionRadius = 1.0f;
    [SerializeField] float crossfadeSpeed = 1.0f;

    [Header("Billboard References")]
    [SerializeField] Transform billboard = null;
    [SerializeField] TextMeshProUGUI billboardStockNum = null;
    [SerializeField] Image billboardStockImage = null;
    [SerializeField] Animator billboardAnimator = null;
    [Range(0.0f, 1.0f)]
    [SerializeField] float billboardCrossfadePercentage = 0.0f;

    [Header("Stock Toaster")]
    [SerializeField] GameObject stockToaster;

    MapManager mapManager = null;

    StockTypes previousStockType = StockTypes.None;

    GameObject firstClaim = null;

    int previousStockNum = 0;

    private void OnValidate()
    {
        UpdateThumbnailNumCrossfade(billboardCrossfadePercentage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
    }

    private void Start()
    {
        mapManager = MapManager.GetInstance();

        UpdateThumbnail();

        previousStockNum = stockQuantity;

        UpdateStockNum();
    }

    private void Update()
    {
        if (stockType != previousStockType)
        {
            // Stock changed!
            UpdateThumbnail();
        }

        if (stockQuantity != previousStockNum)
        {
            UpdateStockNum();
        }

        billboardAnimator.SetBool("ShowImage", IsPlayerInDetectionRadius());

        UpdateThumbnailNumCrossfade(billboardCrossfadePercentage);

        if (stockQuantity == 0)
            Destroy(gameObject);

        previousStockType = stockType;
        previousStockNum = stockQuantity;
    }

    private void LateUpdate()
    {
        billboard.transform.position = transform.position + new Vector3(0.0f, billboardDistance, 0.0f);
    }

    public StockTypes GetStockType()
    {
        return stockType;
    }

    public void SetStockType(StockTypes newStockType)
    {
        stockType = newStockType;
    }

    public int GetQuantity()
    {
        return stockQuantity;
    }

    public int SetQuantity(int amount)
    {
        PushStockToaster(amount - stockQuantity);

        stockQuantity = amount;

        return amount;
    }

    public int AddQuantity(int amount)
    {
        int remainingStock = amount - (maxQuantity - stockQuantity);

        stockQuantity += amount;
        if (stockQuantity > maxQuantity) stockQuantity = maxQuantity;

        return (remainingStock > 0) ? remainingStock : 0;
    }

    #region Visual Updates

    private void UpdateThumbnail()
    {
        Sprite thumbnail = mapManager.GetStockTypeThumbnail(stockType);

        billboardStockImage.sprite = thumbnail;
    }

    private void UpdateStockNum()
    {
        billboardStockNum.text = stockQuantity.ToString();
    }

    private void UpdateThumbnailNumCrossfade(float fadePercentage)
    {
        // Modifying thumbnail
        Color thumbnailColour = billboardStockImage.color;
        thumbnailColour.a = billboardStockImage.sprite ? 1.0f - fadePercentage : 0.0f;
        billboardStockImage.color = thumbnailColour;


        // Modifying number
        Color numColor = billboardStockNum.color;
        numColor.a = fadePercentage;
        billboardStockNum.color = numColor;

    }

    #endregion

    #region Claiming System

    public GameObject IsClaimed()
    {
        return firstClaim;
    }

    public void ClaimItem(GameObject other)
    {
        firstClaim = other;
    }

    public void UnclaimItem(GameObject other)
    {
        if (firstClaim == other)
        {
            firstClaim = null;
        }
        else
        {
            Debug.Log(other + " is not first claim of " + gameObject);
        }
    }

    #endregion

    bool IsPlayerInDetectionRadius()
    {
        Collider[] characters = Physics.OverlapSphere(transform.position, playerDetectionRadius, LayerMask.GetMask("Character"));

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].transform.root.CompareTag("Player"))
            {
                Debug.Log("Player is near!");
                return true;
            }
        }

        return false;
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
