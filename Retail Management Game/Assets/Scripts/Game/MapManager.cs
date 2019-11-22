using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Found Shelves")]
    [SerializeField] List<ShelfContainer> shelvingUnits = new List<ShelfContainer>();
    [SerializeField] List<StockTypes> stockTypesAvailable = new List<StockTypes>();

    [Header("Found Registers")]
    [SerializeField] List<CashRegister> cashRegisters = new List<CashRegister>();

    [Header("Found Exit Points")]
    [SerializeField] List<Transform> exitPoints = new List<Transform>();

    [Header("Prefabs")]
    public StockToPrefabType[] stockPrefabs;

    // Loading Stats
    [ReadOnly] public bool isDoneLoading = false;
    [HideInInspector]
    public string currentLoadingTask = "";
    [HideInInspector]
    public float finishedPercentage = 0.0f;
    int tasksToDo = 0;
    int tasksDone = 0;

    static MapManager instance = null;

    private void OnValidate()
    {
        if (tasksToDo != 0)
            finishedPercentage = tasksDone / tasksToDo;
    }

    private void Awake()
    {
        instance = this;
    }

    public static MapManager GetInstance()
    {
        return instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadMap();
    }

    void LoadMap()
    {
        //************************************************************************/
        currentLoadingTask = "Collecting shelves...";

        GameObject[] foundShelves;
        foundShelves = GameObject.FindGameObjectsWithTag("Shelf");

        tasksToDo = foundShelves.Length;

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

            tasksDone++;
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

        GameObject[] foundRegisters;
        foundRegisters = GameObject.FindGameObjectsWithTag("Register");

        tasksToDo = foundRegisters.Length;

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

            tasksDone++;
        }

        cashRegisters.AddRange(validRegisters);

        //************************************************************************/
        currentLoadingTask = "Collecting exit points...";

        GameObject[] temp_ExitPoints = GameObject.FindGameObjectsWithTag("MapExitPoint");

        for (int point = 0; point < temp_ExitPoints.Length; point++)
        {
            exitPoints.Add(temp_ExitPoints[point].transform);
        }

        isDoneLoading = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ShelfContainer GetRandomShelvingUnit(StockTypes selectedType = StockTypes.None)
    {
        // Check if there are shelves in the map
        if (shelvingUnits.Count == 0)
        {
            if (!isDoneLoading)
                Debug.LogWarning("MapManager is not done loading!");
            else
                Debug.LogWarning("There are no shelves in the map!");

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
        if (cashRegisters.Count == 0)
        {
            if (!isDoneLoading)
                Debug.LogWarning("MapManager is not done loading!");
            else
                Debug.LogWarning("There are no registers in the map!");
            return null;
        }

        return EssentialFunctions.GetRandomFromArray(cashRegisters);
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

    /// <summary>
    /// Gets the exit points in the map
    /// </summary>
    /// <returns>All exit points in the map</returns>
    public Transform[] GetMapExitPoints()
    {
        return exitPoints.ToArray();
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
            StockToPrefabType stockPrefabPack = stockPrefabs[c];

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
        StockToPrefabType stockToPrefabType = null;

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
        StockToPrefabType stockToPrefabType = null;

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
}
