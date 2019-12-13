using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Starting,
    Paused,
    Running,
    Won,
    Lost
}

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

    [Header("Game Score")]

    [SerializeField] int profit = 0;
    [SerializeField] int lostCustomers = 0;

    [Header("Game Settings")]
    public float despawnHeight = -10.0f;

    [SerializeField] float levelDuration = 120.0f;

    [SerializeField] float maxLostCustomers = 3;

    [SerializeField] float readyDuration = 5.0f;
    [SerializeField] int secondsCountDown = 3;

    [Header("Customer Settings")]
    [SerializeField] AnimationCurve customerFrequency = new AnimationCurve(
        new Keyframe(0.0f, 10.0f, 0.0f, 0.0f),
        new Keyframe(120.0f, 5.0f, 0.0f, 0.0f));
    [SerializeField] CustomerTypePack[] customersToSpawn = null;

    [Header("Truck Settings")]
    [SerializeField] float truckSpawnFrequency = 30.0f;

    [Tooltip("Determind if the delivery order list will be sent in order or not.")]
    [SerializeField] bool isDeliverInSequence = true;
    [SerializeField] TruckDeliveryPack[] truckDeliveryOrder = System.Array.Empty<TruckDeliveryPack>();

    [Header("Win State Settings")]
    [SerializeField] string[] randomWinMessages = new string[] { "Ayyy you did it!" };

    [Header("Lost State Settings")]
    [SerializeField] string[] randomLostMessages = new string[] { "Really?" };

    [Header("Game Over Settings")]
    [SerializeField] float endSlowDownDuration = 5.0f;

    [Header("References")]
    [SerializeField] string latestLostReason = "You're actually bad";

    [SerializeField] HUD HUDScript = null;
    [SerializeField] List<Transform> players = new List<Transform>();
    [SerializeField] List<Transform> customersInLevel = new List<Transform>();

    //************************************************************************/
    // Runtime Variables

    MapManager mapManager = null;

    GameState currrentGameState = GameState.Starting;

    int currentDeliveryPackIndex = 0;

    TruckDriver currentTruckDriver = null;

    GameState previousGameState = GameState.Starting;

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

        gameDuration = levelDuration;

        HUDScript.UpdateTimer(gameDuration);
        HUDScript.UpdateCash(profit);
    }

    // Update is called once per frame
    void Update()
    {
        // Wait until MapManager is done loading
        if (!mapManager.isDoneLoading) return;

        if (currrentGameState == GameState.Paused) return;

        // Count down
        if (currrentGameState == GameState.Starting && gameDuration >= 0.0f)
            if (startCountDownTimer >= (secondsCountDown + readyDuration))
            {
                currrentGameState = GameState.Running;
                Time.timeScale = 1.0f;
                HUDScript.UpdateCountDown("");
            }
            else
            {
                startCountDownTimer += Time.unscaledDeltaTime;

                float countDownReversed = (secondsCountDown - startCountDownTimer) + readyDuration;

                if (startCountDownTimer < readyDuration)
                {
                    HUDScript.UpdateCountDown("Ready?");
                }
                else
                {
                    int countDownText = Mathf.CeilToInt(countDownReversed);
                    if (countDownText < 0) countDownText = 0;

                    HUDScript.UpdateCountDown(countDownText.ToString());
                }

                Time.timeScale = 0.0f;
            }

        // Game over!
        else if ((currrentGameState != GameState.Running && gameDuration <= 0.0f) || currrentGameState == GameState.Lost)
        {
            
        }

        // Game on!
        else
        {
            #region Timer Management

            customerSpawnTimer += Time.deltaTime;
            gameDuration -= Time.deltaTime;

            #endregion

            #region Spawner Management

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
                //Debug.Log("Spawning customer! Current frequency:" + currentCustomerSpawnFrequency);
                customerSpawnTimer = 0.0f;
            }

            #endregion

            if (lostCustomers >= maxLostCustomers)
            {
                LostState(latestLostReason);
                return;
            }

            if (gameDuration <= 0.0f)
            {
                WinState();
                return;
            }

            HUDScript.UpdateTimer(gameDuration);
        }
    }

    void SpawnCustomer()
    {
        GameObject selected;

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

        HUDScript.UpdateCash(profit);
    }

    public int GetScore()
    {
        return profit;
    }

    public void StealScore(int amount)
    {
        profit -= amount;

        HUDScript.UpdateCash(profit);
    }

    public void LostCustomer()
    {
        lostCustomers++;

        HUDScript.UpdateUpsetCustomers(lostCustomers);
    }

    public int GetLostCustomerAmount()
    {
        return lostCustomers;
    }

    public void PauseGame()
    {
        if (currrentGameState != GameState.Running && currrentGameState != GameState.Starting) return;
        // Save previous game state
        previousGameState = currrentGameState;

        // Set up pause state
        StartCoroutine(SetSlowDown(0.5f));
        HUDScript.GetPauseMenu().SetActive(true);

        // Change state
        currrentGameState = GameState.Paused;
    }

    public void ResumeGame()
    {
        // Set up resume game
        Time.timeScale = 1.0f;
        HUDScript.GetPauseMenu().SetActive(false);

        // Restore previous game state
        currrentGameState = previousGameState;
    }

    public GameState GetGameState()
    {
        return currrentGameState;
    }

    public void ForceLoseReasonMessage(string lostReason)
    {
        latestLostReason = lostReason;
    }

    public void LostState(string lostReason)
    {
        currrentGameState = GameState.Lost;

        HUDScript.SetGameOver(lostReason);
        StartCoroutine(SetSlowDown(endSlowDownDuration));
    }

    public void LostState()
    {
        LostState(EssentialFunctions.GetRandomFromArray(randomLostMessages));
    }

    public bool IsGameOver()
    {
        return currrentGameState == GameState.Lost || currrentGameState == GameState.Won;
    }

    IEnumerator SetSlowDown(float slowDownDuration)
    {
        for (float t = slowDownDuration; t >= 0.0f; t -= Time.unscaledDeltaTime)
        {
            float p = t / slowDownDuration;

            Time.timeScale = p;

            yield return new WaitForEndOfFrame();
        }

        Time.timeScale = 0.0f;

        yield return null;
    }

    public void WinState(string winReason)
    {
        currrentGameState = GameState.Won;
        gameDuration = 0.0f;

        HUDScript.SetGameOver(winReason);

        PlayerData.currentInfo.profit = profit;
        PlayerData.currentInfo.money = profit;
        PlayerData.SaveSlotData();

        StartCoroutine(SetSlowDown(endSlowDownDuration));
    }

    public void WinState()
    {
        WinState(EssentialFunctions.GetRandomFromArray(randomWinMessages));
    }

    public void ToScoreBoard()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadSceneAsync("ScoreScene");
    }
}
