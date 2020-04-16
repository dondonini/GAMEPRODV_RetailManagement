using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] 
    private float dataUpdateFreq = 2.5f;
    
    [Header("Found Shelves")] [SerializeField]
    private List<ShelfContainer> shelvingUnits = new List<ShelfContainer>();

    [SerializeField] private List<StockTypes> stockTypesAvailable = new List<StockTypes>();

    [Header("Found Registers")] [SerializeField]
    private List<CashRegister> cashRegisters = new List<CashRegister>();

    [Header("Found Customer Spawners")] [SerializeField]
    private List<CustomerSpawner> customerSpawners = new List<CustomerSpawner>();

    [Header("Found Exit Points")] [SerializeField]
    private List<Transform> exitPoints = new List<Transform>();

    [Header("All Product Instances")] [SerializeField]
    private List<GameObject> itemInstances = new List<GameObject>();

    [Header("Trucks")] [SerializeField] 
    private List<TruckDriver> trucks = new List<TruckDriver>();

    [Header("Prefabs")]
    public StockData[] stockPrefabs;

    [Header("Level Unlocks")]
    public UnlockSegment[] unlockableSegments;

    // Loading Stats
    [ReadOnly] public bool isDoneLoading;
    [HideInInspector]
    public string currentLoadingTask = "";
    [HideInInspector]
    public float finishedPercentage;

    private int _tasksToDo;
    private int _tasksDone;

    private static MapManager _instance;

    private float _updateWaitTime = 0.0f;

    private void OnValidate()
    {
        if (_tasksToDo != 0)
            finishedPercentage = (float)_tasksDone / _tasksToDo;

        UpdateSegments();
    }

    private void Awake()
    {
        _instance = this;
    }

    public static MapManager GetInstance()
    {
        return _instance;
    }

    // Start is called before the first frame update
    private void Start()
    {
        LoadMap();
    }

    private void LoadMap()
    {
        UpdateSegments();

        //************************************************************************/
        currentLoadingTask = "Collecting shelves...";

        GameObject[] foundShelves = GameObject.FindGameObjectsWithTag("Shelf");

        _tasksToDo = foundShelves.Length;

        //************************************************************************/
        currentLoadingTask = "Validating shelves...";

        List<ShelfContainer> validShelves = new List<ShelfContainer>();

        for (int s = 0; s < foundShelves.Length; s++)
        {
            GameObject shelf = foundShelves[s];

            ShelfContainer result = shelf.GetComponent<ShelfContainer>();

            if (result)
            {
                validShelves.Add(result);
            }
            else
            {
                Debug.LogWarning("Shelf \"" + shelf + "\" is tagged as a shelf, but doesn't have a ShelfContainer compnent!");
            }

            _tasksDone++;
        }

        shelvingUnits.AddRange(validShelves);

        //************************************************************************/
        currentLoadingTask = "Detecting available types of stock...";

        UpdateAvailableStockTypes();

        //************************************************************************/
        currentLoadingTask = "Map building done!";

        //************************************************************************/
        currentLoadingTask = "Checking stock prefabs...";

        if (stockPrefabs.Length != EnumConverter.ToNameArray<StockTypes>().Length)
        {
            StockTypes[] allStocks = EnumConverter.ToListOfValues<StockTypes>().ToArray();

            List<string> missingStockTypes = new List<string>();

            for (int stock = 0; stock < allStocks.Length; stock++)
            {
                // Skip None enum
                if (allStocks[stock] == StockTypes.None) continue;

                bool isMissing = true;

                for (int p = 0; p < stockPrefabs.Length; p++)
                {

                    if (stockPrefabs[p].GetStockType() == allStocks[stock])
                    {
                        isMissing = false;
                    }
                }

                if (isMissing)
                {
                    missingStockTypes.Add(allStocks[stock].ToString());
                }
            }

            Debug.LogWarning("The following prefabs are missing for stock types: " + string.Join(", ", missingStockTypes));
        }

        //************************************************************************/
        currentLoadingTask = "Collecting registers...";

        GameObject[] foundRegisters = GameObject.FindGameObjectsWithTag("Register");

        _tasksToDo = foundRegisters.Length;

        //************************************************************************/
        currentLoadingTask = "Validating registers...";

        List<CashRegister> validRegisters = new List<CashRegister>();

        for (int r = 0; r < foundRegisters.Length; r++)
        {
            GameObject register = foundRegisters[r];

            CashRegister result = register.GetComponent<CashRegister>();

            if (result)
            {
                validRegisters.Add(result);
            }
            else
            {
                Debug.LogWarning("Register \"" + register + "\" is tagged as a Register, but doesn't have a CashRegister compnent!");
            }

            _tasksDone++;
        }

        cashRegisters.AddRange(validRegisters);

        //************************************************************************/
        currentLoadingTask = "Collecting customer spawn locations...";

        GameObject[] tempCustomerSpawner = GameObject.FindGameObjectsWithTag("CustomerSpawn");

        for (int customerSpawner = 0; customerSpawner < tempCustomerSpawner.Length; customerSpawner++)
        {
            if (customerSpawners.Contains(tempCustomerSpawner[customerSpawner].GetComponent<CustomerSpawner>())) continue;
            customerSpawners.Add(tempCustomerSpawner[customerSpawner].GetComponent<CustomerSpawner>());
        }

        //************************************************************************/
        currentLoadingTask = "Collecting exit points...";

        GameObject[] tempExitPoints = GameObject.FindGameObjectsWithTag("MapExitPoint");

        for (int point = 0; point < tempExitPoints.Length; point++)
        {
            if (exitPoints.Contains(tempExitPoints[point].transform)) continue;
            exitPoints.Add(tempExitPoints[point].transform);
        }

        LoadPlayerData();

        isDoneLoading = true;
    }

    private bool LoadPlayerData()
    {
        if (unlockableSegments.Length == 0) return false;

        bool noUnlocks = true;
        for(int i = 0; i < unlockableSegments.Length; i++)
        {
            unlockableSegments[i].enableSegment = PlayerData.LoadSlotSegmentData(unlockableSegments[i].m_segmentKey);

            if (unlockableSegments[i].enableSegment)
                noUnlocks = false;
        }

        if (noUnlocks)
            unlockableSegments[0].enableSegment = true;

        return true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_updateWaitTime >= dataUpdateFreq)
        {
            UpdateData();
            
            // Reset timer
            _updateWaitTime = 0.0f;
        }

        _updateWaitTime += Time.deltaTime;
    }

    private void UpdateData()
    {
        // Get all items
        List<GameObject> instances = new List<GameObject>();
        instances.AddRange(GameObject.FindGameObjectsWithTag("Product"));
        instances.AddRange(GameObject.FindGameObjectsWithTag("StockCrate"));
        
        // Replace lists
        itemInstances.Clear();
        itemInstances = instances;

    }

    public ShelfContainer GetRandomShelvingUnit(StockTypes selectedType = StockTypes.None)
    {
        // Check if there are shelves in the map
        if (shelvingUnits.Count == 0)
        {
            Debug.LogWarning(!isDoneLoading ? "MapManager is not done loading!" : "There are no shelves in the map!");

            return null;
        }

        // Filter out shelves that are needed
        List<ShelfContainer> sortedShelfList = new List<ShelfContainer>();
        if (selectedType != StockTypes.None)
        {
            for (int i = 0; i < shelvingUnits.Count; i++)
            {
                ShelfContainer currentShelf = shelvingUnits[i];

                if (currentShelf.ShelfStockType == selectedType)
                {
                    sortedShelfList.Add(shelvingUnits[i]);
                }
            }
        }
        else
        {
            CopyData.CopyObjectData(shelvingUnits, sortedShelfList);
        }

        if (sortedShelfList.Count == 0)
        {
            Debug.LogWarning("There are no shelves that have this type of stock: " + selectedType.ToString());
            return null;
        }

        // Filter out empty shelves
        List<ShelfContainer> stockedShelves = new List<ShelfContainer>();
        for (int s = 0; s < sortedShelfList.Count; s++)
        {
            ShelfContainer shelf = sortedShelfList[s];
            if (!shelf.IsEmpty())
                stockedShelves.Add(shelf);
        }

        if (stockedShelves.Count == 0)
        {
            Debug.Log("All shelves of " + selectedType.ToString() + " is empty!");
            return EssentialFunctions.GetRandomFromArray(sortedShelfList); ;
        }
        else
            return EssentialFunctions.GetRandomFromArray(stockedShelves);
    }

    public CashRegister GetRandomCashRegister()
    {
        if (cashRegisters.Count != 0) return EssentialFunctions.GetRandomFromArray(cashRegisters);

        Debug.LogWarning(!isDoneLoading ? "MapManager is not done loading!" : "There are no registers in the map!");
        return null;

    }

    public TruckDriver GetRandomTruck()
    {
        return EssentialFunctions.GetRandomFromArray(trucks);
    }

    public void UpdateAvailableStockTypes()
    {
        List<StockTypes> updatedList = new List<StockTypes>();

        for (int s = 0; s < shelvingUnits.Count; s++)
        {
            ShelfContainer shelves = shelvingUnits[s];

            if (stockTypesAvailable.Count == 0 || shelves.ShelfStockType != StockTypes.None)
                updatedList.Add(shelves.ShelfStockType);
            else
                if (!stockTypesAvailable.Contains(shelves.ShelfStockType) && shelves.ShelfStockType != StockTypes.None)
                    updatedList.Add(shelves.ShelfStockType);
        }

        // Replace old list with updated one
        stockTypesAvailable = updatedList;
    }

    #region Getters and Setters

    /// <summary>
    /// Gets all of the shelving units in the map
    /// </summary>
    /// <returns>All shelving units in the map</returns>
    public ShelfContainer[] GetShelvingUnits()
    {
        return shelvingUnits.ToArray();
    }

    public GameObject[] GetAllItemInstances()
    {
        // Remove missing items
        for (int i = 0; i < itemInstances.Count; i++)
        {
            if (itemInstances[i]) continue;
            
            itemInstances.RemoveAt(i);
        }
        
        return itemInstances.ToArray();
    }

    /// <summary>
    /// Gets the exit points in the map
    /// </summary>
    /// <returns>All exit points in the map</returns>
    public Transform[] GetMapExitPoints()
    {
        return exitPoints.ToArray();
    }

    public CustomerSpawner GetRandomCustomerSpawner()
    {
        return EssentialFunctions.GetRandomFromArray(customerSpawners);
    }

    /// <summary>
    /// Gets the available stock types in the current map
    /// </summary>
    /// <returns>All available stock types</returns>
    public StockTypes[] GetStockTypesAvailable()
    {
        return stockTypesAvailable.ToArray();
    }

    /// <summary>
    /// Converts stockType to an actual prefab model
    /// </summary>
    /// <param name="stockType">StockType you're looking for</param>
    /// <returns>Random prefab of said stockType</returns>
    public GameObject GetStockTypePrefab(StockTypes stockType)
    {
        for (int c = 0; c < stockPrefabs.Length; c++)
        {
            // Collect stock pack
            StockData stockPrefabPack = stockPrefabs[c];

            // Check if stockType matches what you're looking for
            if (stockPrefabPack.GetStockType() == stockType)
            {
                // Check if there's only one prefab model in the pack
                if (stockPrefabPack.GetPrefabs().Length == 1)
                {
                    // Return the one
                    return stockPrefabPack.GetPrefabs()[0];
                }
                else
                {
                    // Return a random prefab in the pack
                    return EssentialFunctions.GetRandomFromArray(stockPrefabPack.GetPrefabs());
                    //return stockPrefabPack.prefabs[Random.Range(0, stockPrefabPack.prefabs.Length)];
                }

            }
        }

        // Return nothing because it doesn't exist
        return null;
    }

    public int GetStockTypePrice(StockTypes stockType)
    {
        StockData stockToPrefabType = null;

        for (int i = 0; i < stockPrefabs.Length; i++)
        {
            if (stockPrefabs[i].GetStockType() == stockType)
            {
                stockToPrefabType = stockPrefabs[i];
            }
        }

        if (stockToPrefabType != null)
            return stockToPrefabType.GetStockTypePrice();
        else
            return 0;
    }

    public Sprite GetStockTypeThumbnail(StockTypes stockType)
    {
        StockData stockToPrefabType = null;

        for (int i = 0; i < stockPrefabs.Length; i++)
        {
            if (stockPrefabs[i].GetStockType() == stockType)
            {
                stockToPrefabType = stockPrefabs[i];
            }
        }

        if (stockToPrefabType != null)
            return stockToPrefabType.GetThumbnail();
        else
            return null;
    }

    #endregion

    public void EnableSegment(string segmentKey)
    {
        for (int i = 0; i < unlockableSegments.Length; i++)
        {
            if (unlockableSegments[i].CompareKey(segmentKey))
                unlockableSegments[i].enableSegment = true;
        }
        UpdateSegments(GetSegment(segmentKey));
    }

    private void UpdateSegments()
    {
        // Check unlockable list for enabled segments
        for (int i = 0; i < unlockableSegments.Length; i++)
        {
            UpdateSegments(unlockableSegments[i]);
        }
    }

    private UnlockSegment GetSegment(string segmentKey)
    {
        for (int i = 0; i < unlockableSegments.Length; i++)
        {
            if (unlockableSegments[i].CompareKey(segmentKey))
                return unlockableSegments[i];
        }
        return null;
    }

    private void UpdateSegments(UnlockSegment selectedSegment)
    {
        if (selectedSegment.IsEnabled())
        {
            // Disable segments that are not compatible with this segment
            for (int segKey = 0; segKey < selectedSegment.m_incompatibleKeys.Length; segKey++)
            {
                for (int incomp = 0; incomp < unlockableSegments.Length; incomp++)
                {
                    if (unlockableSegments[incomp].CompareKey(selectedSegment.m_incompatibleKeys[segKey]))
                        unlockableSegments[incomp].enableSegment = false;
                }
            }
        }

        // Enable/Disable segment
        selectedSegment.UpdateSegment();
    }
}
