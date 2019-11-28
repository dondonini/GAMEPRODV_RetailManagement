using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //************************************************************************/
    //* Instance Management

    private static GameManager instance;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogError("Two game instances exists in the game at once! This shouldn't be possible! WTF?!?! \n" +
                "Instance 1: " + this + "\n" +
                "Instance 2: " + instance
            );

            Debug.LogAssertion("Quitting game so you can fix the mess you've made...");

            Application.Quit();
        }
        else
            instance = this;
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    //************************************************************************/
    // Variables

    public float despawnHeight = -10.0f;

    [SerializeField] int profit = 0;
    [SerializeField] int lostCustomers = 0;

    [SerializeField] float levelDuration = 120.0f;

    [SerializeField] int secondsCountDown = 3;
    [SerializeField] AnimationCurve customerFrequency = new AnimationCurve(
        new Keyframe(0.0f, 10.0f, 0.0f, 0.0f),
        new Keyframe(120.0f, 5.0f, 0.0f, 0.0f));
    [SerializeField] float truckSpawnFrequency = 30.0f;

    [SerializeField] bool isDeliverInSequence = true;
    [SerializeField] TruckDeliveryPack[] truckDeliveryOrder = new TruckDeliveryPack[] { };

    [SerializeField] CustomerTypePack[] customersToSpawn = null;

    [SerializeField] List<Transform> players = new List<Transform>();
    [SerializeField] List<Transform> customersInLevel = new List<Transform>();

    //************************************************************************/
    // Runtime Variables

    MapManager mapManager = null;

    bool isGameActive = false;

    int currentDeliveryPackIndex = 0;

    TruckDriver currentTruckDriver = null;

    // Timers
    float startCountDownTimer = 0.0f;
    float truckTimer = 0.0f;
    float customerSpawnTimer = 0.0f;
    float gameDuration = 0.0f;

    private void OnValidate()
    {
        if (levelDuration == 0.0f) return;

        List<Keyframe> newKeyframes = new List<Keyframe>();

        float previousDuration = customerFrequency.keys[customerFrequency.keys.Length - 1].time;

        // Snap first keyframe to beginning
        Keyframe firstKeyframe = customerFrequency.keys[0];
        firstKeyframe.time = 0.0f;
        newKeyframes.Add(firstKeyframe);

        // Rescale customerSpawnChanges to levelDuration
        for (int k = 1; k < customerFrequency.keys.Length - 1; k++)
        {
            Keyframe currentKeyframe = customerFrequency.keys[k];

            float updatedKeyTime = (currentKeyframe.time / previousDuration) * levelDuration;

            currentKeyframe.time = updatedKeyTime;

            newKeyframes.Add(currentKeyframe);
        }

        // Snap last keyframe to end
        if (customerFrequency.keys.Length > 1)
        {
            Keyframe lastKeyframe = customerFrequency.keys[customerFrequency.keys.Length - 1];
            lastKeyframe.time = levelDuration;
            newKeyframes.Add(lastKeyframe);
        }

        AnimationCurve newCurve = new AnimationCurve(newKeyframes.ToArray());

        customerFrequency = newCurve;
    }

    // Start is called before the first frame update
    void Start()
    {
        mapManager = MapManager.GetInstance();

        currentDeliveryPackIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Wait until MapManager is done loading
        if (!mapManager.isDoneLoading) return;

        // Count down
        if (isGameActive == false)
            if (startCountDownTimer >= secondsCountDown)
            {
                isGameActive = true;
            }
            else
            {
                startCountDownTimer += Time.deltaTime;
            }

        // Game on!
        else
        {
            #region Timer Management

            customerSpawnTimer += Time.deltaTime;
            gameDuration += Time.deltaTime;

            #endregion

            if (!currentTruckDriver || !currentTruckDriver.isSpawning)
            {
                truckTimer += Time.deltaTime;

                if (truckTimer >= truckSpawnFrequency)
                {
                    SpawnTruck();
                    Debug.Log("Spawning truck!");
                    truckTimer = 0.0f;
                }
            }

            float currentCustomerSpawnFrequency = customerFrequency.Evaluate(gameDuration);
            if (customerSpawnTimer >= currentCustomerSpawnFrequency)
            {
                SpawnCustomer();
                Debug.Log("Spawning customer! Current frequency:" + currentCustomerSpawnFrequency);
                customerSpawnTimer = 0.0f;
            }


        }
    }

    void SpawnCustomer()
    {
        GameObject selected = null;

        // Select a random customer is list by chance
        if (customersToSpawn.Length == 1)
        {
            selected = customersToSpawn[0].customerPrefab;
        }
        else if (customersToSpawn.Length > 1)
        {
            Dictionary<GameObject, int> weights = new Dictionary<GameObject, int>();

            for (int i = 1; i < customersToSpawn.Length; i++)
            {
                weights.Add(customersToSpawn[i].customerPrefab, customersToSpawn[i].chancesOfSpawning);
            }

            selected = WeightedRandomizer.From(weights).TakeOne();
        }
        else
        {
            Debug.LogError("There are no customers to spawn in the GameManager!", gameObject);
            return;
        }
        
        // Spawn selected customer
        mapManager.GetRandomCustomerSpawner().SpawnCustomer(selected);
    }

    void SpawnTruck()
    {
        currentTruckDriver = mapManager.GetRandomTruck();

        currentTruckDriver.ClearTruck();

        if (!isDeliverInSequence)
        {
            currentDeliveryPackIndex = Random.Range(0, truckDeliveryOrder.Length);
        }

        LoadUpTruck(currentTruckDriver, truckDeliveryOrder[currentDeliveryPackIndex]);

        if (isDeliverInSequence)
        {
            currentDeliveryPackIndex = (currentDeliveryPackIndex + 1) % truckDeliveryOrder.Length;
        }

        // Spawn truck
        currentTruckDriver.GetAnimator().SetTrigger("Enter");
    }

    void LoadUpTruck(TruckDriver truck, TruckDeliveryPack deliveryPack)
    {
        for (int p = 0; p < deliveryPack.deliveryPacks.Length; p++)
        {
            TruckReceivingPacks currentLoadIn = deliveryPack.deliveryPacks[p];

            switch (currentLoadIn.storageType)
            {
                case StorageTypes.Product:
                    {
                        for (int a = 0; a < currentLoadIn.stockAmount; a++)
                        {
                            truck.AddProduct(currentLoadIn.stockType);
                        }
                        break;
                    }
                case StorageTypes.Crate:
                    {
                        truck.AddCrate(currentLoadIn.stockType, currentLoadIn.stockAmount);
                        break;
                    }
            }
        }
    }

    public void AddPlayer(Transform newPlayer)
    {
        if (players.Contains(newPlayer))
        {
            Debug.LogWarning(newPlayer + " is already a subject... what are you doing?");
            return;
        }

        // Set new main subject
        players.Add(newPlayer);
    }

    public void RemovePlayer(Transform player)
    {
        for (int s = 0; s < players.Count; s++)
        {
            if (players[s] == player)
            {
                players.RemoveAt(s);
                return;
            }
        }

        // If it gets to here, it means that the targeted subject was not found
        Debug.LogWarning("Player " + player + " is not in the players list.");
    }

    public void AddCustomer(Transform newCustomer)
    {
        if (customersInLevel.Contains(newCustomer)) return;

        customersInLevel.Add(newCustomer);
    }

    public void RemoveCustomer(Transform customer)
    {
        for (int c = 0; c < customersInLevel.Count; c++)
        {
            if (customersInLevel[c] == customer)
            {
                customersInLevel.RemoveAt(c);
                return;
            }
        }

        // If it gets to here, it means that the targeted subject was not found
        Debug.LogWarning("Customer " + customer + " is not in the customer list.");
    }

    public Transform[] GetPlayers()
    {
        return players.ToArray();
    }

    public void AddScore(int amount)
    {
        profit += amount;
    }

    public int GetScore()
    {
        return profit;
    }

    public void StealScore(int amount)
    {
        profit -= amount;
    }

    public void LostCustomer()
    {
        lostCustomers++;
    }

    public int GetLostCustomerAmount()
    {
        return lostCustomers;
    }
}
