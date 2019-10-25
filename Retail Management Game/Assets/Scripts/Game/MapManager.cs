using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Found Shelves")]
    [ReadOnly][SerializeField] List<ShelfContainer> shelvingUnits = new List<ShelfContainer>();
    [ReadOnly][SerializeField] List<StockTypes> stockTypesAvailable = new List<StockTypes>();

    [Header("Found Registers")]
    [ReadOnly][SerializeField] List<CashRegister> cashRegisters = new List<CashRegister>();

    // Loading Stats
    [HideInInspector]
    public bool isDoneLoading = false;
    [HideInInspector]
    public string currentLoadingTask = "";
    [HideInInspector]
    public float finishedPercentage = 0.0f;
    int tasksToDo = 0;
    int tasksDone = 0;

    [Header("Prefabs")]
    public StockToPrefabType[] stockPrefabs;

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
        StartCoroutine(LoadMap());
    }

    IEnumerator LoadMap()
    {
        //************************************************************************/
        currentLoadingTask = "Collecting shelves...";

        GameObject[] foundShelves;
        foundShelves = GameObject.FindGameObjectsWithTag("Shelf");

        tasksToDo = foundShelves.Length;

        //************************************************************************/
        currentLoadingTask = "Validating shelves...";

        List<ShelfContainer> validShelves = new List<ShelfContainer>();

        foreach (GameObject shelf in foundShelves)
        {
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

            yield return new WaitForEndOfFrame();
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

                yield return new WaitForEndOfFrame();
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

        foreach (GameObject register in foundRegisters)
        {
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
            yield return new WaitForEndOfFrame();
        }

        cashRegisters.AddRange(validRegisters);

        isDoneLoading = true;

        yield return new WaitForEndOfFrame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ShelfContainer GetRandomShelvingUnit(StockTypes selectedType = StockTypes.None)
    {

        if (shelvingUnits.Count == 0)
        {
            if (!isDoneLoading)
                Debug.LogWarning("MapManager is not done loading!");
            else
                Debug.LogWarning("There are no shelves in the map!");

            return null;
        }

        List<ShelfContainer> sortedShelfList = shelvingUnits;

        if (selectedType != StockTypes.None)
        {
            for (int i = 0; i < sortedShelfList.Count; i++)
            {
                ShelfContainer currentShelf = sortedShelfList[i];

                if (currentShelf.ShelfStockType != selectedType)
                {
                    sortedShelfList.RemoveAt(i);
                }
            }
        }

        if (sortedShelfList.Count == 0)
        {
            Debug.LogWarning("There are no shelves that have this type of stock: " + selectedType.ToString());
            return null;
        }

        Random.InitState((int)(Random.value * Time.realtimeSinceStartup * 1000));

        return sortedShelfList[Random.Range(0, sortedShelfList.Count - 1)];
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

        return cashRegisters[Random.Range(0, cashRegisters.Count)];
    }

    public void UpdateAvailableStockTypes()
    {
        List<StockTypes> updatedList = new List<StockTypes>();

        foreach (ShelfContainer s in shelvingUnits)
        {
            if (stockTypesAvailable.Count == 0 || s.ShelfStockType != StockTypes.None)
            {
                updatedList.Add(s.ShelfStockType);
            }
            else
            {
                // Compare each shelf to available stock type list
                for (int i = 0; i < stockTypesAvailable.Count; i++)
                {
                    if (s.ShelfStockType == stockTypesAvailable[i] || s.ShelfStockType == StockTypes.None)
                        // Ignore stock type of shelf cause it is either empty or already is in the list
                        continue;
                    else
                        // Add new type to list
                        updatedList.Add(s.ShelfStockType);
                }
            }
        }

        // Replace old list with updated one
        stockTypesAvailable = updatedList;
    }

    #region Getters and Setters

    public ShelfContainer[] GetShelvingUnits()
    {
        return shelvingUnits.ToArray();
    }

    public StockTypes[] GetStockTypesAvailable()
    {
        return stockTypesAvailable.ToArray();
    }

    public GameObject GetStockTypePrefab(StockTypes stockType)
    {
        foreach(StockToPrefabType c in stockPrefabs)
        {
            if (c.GetStockType() == stockType)
            {
                if (c.prefabs.Length == 1)
                {
                    return c.prefabs[0];
                }
                else
                {
                    return c.prefabs[Random.Range(0, c.prefabs.Length)];
                }
                
            }
        }

        return null;
    }

    #endregion
}
