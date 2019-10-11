using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public List<MonoBehaviour> shelvingUnits = new List<MonoBehaviour>();

    // Loading Stats
    [HideInInspector]
    public bool isDoneLoading = false;
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

    public static MapManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentLoadingTask = "Finding shelves...";

        GameObject[] foundShelves;
        foundShelves = GameObject.FindGameObjectsWithTag("Shelf");

        tasksToDo = foundShelves.Length;

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
        }

        shelvingUnits.AddRange(validShelves);

        currentLoadingTask = "Map building done!";
        isDoneLoading = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public MonoBehaviour GetRandomShelvingUnit()
    {
        return shelvingUnits[Random.Range(0, shelvingUnits.Count - 1)];
    }
}
