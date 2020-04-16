using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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

    private static GameManager _instance;

    private void Awake()
    {
        if (_instance)
        {
            Debug.LogError("Two game instances exists in the game at once! This shouldn't be possible! WTF?!?! \n" +
                "Instance 1: " + this + "\n" +
                "Instance 2: " + _instance
            );

            Debug.LogAssertion("Quitting game so you can fix the mess you've made...");

            Application.Quit();
        }
        else
            _instance = this;
    }

    public static GameManager GetInstance()
    {
        return _instance;
    }

    //************************************************************************/
    // Variables

    [Header("Game Score")]

    [SerializeField]
    private int profit = 0;
    [SerializeField] private int lostCustomers = 0;

    [Header("Game Settings")]
    public float despawnHeight = -10.0f;

    [SerializeField] private float levelDuration = 120.0f;

    [SerializeField] private float maxLostCustomers = 3;

    [SerializeField] private float readyDuration = 5.0f;
    [SerializeField] private int secondsCountDown = 3;

    [Header("Customer Settings")]
    [SerializeField]
    private AnimationCurve customerFrequency = new AnimationCurve(
        new Keyframe(0.0f, 10.0f, 0.0f, 0.0f),
        new Keyframe(120.0f, 5.0f, 0.0f, 0.0f));
    [SerializeField] private CustomerTypePack[] customersToSpawn = null;

    [Header("Truck Settings")]
    [SerializeField] private float truckSpawnFrequency = 30.0f;

    [Tooltip("Determine if the delivery order list will be sent in order or not.")]
    [SerializeField] private bool isDeliverInSequence = true;
    [SerializeField] private TruckDeliveryPack[] truckDeliveryOrder = System.Array.Empty<TruckDeliveryPack>();

    [Header("Win State Settings")]
    [SerializeField] private string[] randomWinMessages = new string[] { "Ayyy you did it!" };

    [Header("Lost State Settings")]
    [SerializeField] private string[] randomLostMessages = new string[] { "Really?" };

    [Header("Game Over Settings")]
    [SerializeField] private float endSlowDownDuration = 5.0f;

    [Header("References")]
    [SerializeField] private string latestLostReason = "You're actually bad";

    [SerializeField] private HUD hudScript = null;
    [SerializeField] private List<Transform> players = new List<Transform>();
    [SerializeField] private List<Transform> customersInLevel = new List<Transform>();

    //************************************************************************/
    // Runtime Variables

    private MapManager _mapManager = null;

    private GameState _currentGameState = GameState.Starting;

    private int _currentDeliveryPackIndex = 0;

    private TruckDriver _currentTruckDriver = null;

    private GameState _previousGameState = GameState.Starting;

    // Timers
    private float _startCountDownTimer = 0.0f;
    private float _truckTimer = 0.0f;
    private float _customerSpawnTimer = 0.0f;
    private float _gameDuration = 0.0f;
    private static readonly int enter = Animator.StringToHash("Enter");

    private void OnValidate()
    {
        if (levelDuration <= 0.0f) return;

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
    private void Start()
    {
        _mapManager = MapManager.GetInstance();

        _currentDeliveryPackIndex = 0;

        _gameDuration = levelDuration;

        hudScript.UpdateTimer(_gameDuration);
        hudScript.UpdateCash(profit);
    }

    // Update is called once per frame
    private void Update()
    {
        // Wait until MapManager is done loading
        if (!_mapManager.isDoneLoading) return;

        if (_currentGameState == GameState.Paused) return;

        // Count down
        if (_currentGameState == GameState.Starting && _gameDuration >= 0.0f)
            if (_startCountDownTimer >= (secondsCountDown + readyDuration))
            {
                _currentGameState = GameState.Running;
                Time.timeScale = 1.0f;
                hudScript.UpdateCountDown("");
            }
            else
            {
                _startCountDownTimer += Time.unscaledDeltaTime;

                float countDownReversed = (secondsCountDown - _startCountDownTimer) + readyDuration;

                if (_startCountDownTimer < readyDuration)
                {
                    hudScript.UpdateCountDown("Ready?");
                }
                else
                {
                    int countDownText = Mathf.CeilToInt(countDownReversed);
                    if (countDownText < 0) countDownText = 0;

                    hudScript.UpdateCountDown(countDownText.ToString());
                }

                Time.timeScale = 0.0f;
            }

        // Game over!
        else if ((_currentGameState != GameState.Running && _gameDuration <= 0.0f) || _currentGameState == GameState.Lost)
        {
            
        }

        // Game on!
        else
        {
            #region Timer Management

            _customerSpawnTimer += Time.deltaTime;
            _gameDuration -= Time.deltaTime;

            #endregion

            #region Spawner Management

            if (!_currentTruckDriver || !_currentTruckDriver.isSpawning)
            {
                _truckTimer += Time.deltaTime;

                if (_truckTimer >= truckSpawnFrequency)
                {
                    SpawnTruck();
                    Debug.Log("Spawning truck!");
                    _truckTimer = 0.0f;
                }
            }

            float currentCustomerSpawnFrequency = customerFrequency.Evaluate(_gameDuration);
            if (_customerSpawnTimer >= currentCustomerSpawnFrequency)
            {
                SpawnCustomer();
                //Debug.Log("Spawning customer! Current frequency:" + currentCustomerSpawnFrequency);
                _customerSpawnTimer = 0.0f;
            }

            #endregion

            if (lostCustomers >= maxLostCustomers)
            {
                LostState(latestLostReason);
                return;
            }

            if (_gameDuration <= 0.0f)
            {
                WinState();
                return;
            }

            hudScript.UpdateTimer(_gameDuration);
        }
    }

    private void SpawnCustomer()
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
        _mapManager.GetRandomCustomerSpawner().SpawnCustomer(selected);
    }

    private void SpawnTruck()
    {
        _currentTruckDriver = _mapManager.GetRandomTruck();

        _currentTruckDriver.ClearTruck();

        if (!isDeliverInSequence)
        {
            _currentDeliveryPackIndex = Random.Range(0, truckDeliveryOrder.Length);
        }

        LoadUpTruck(_currentTruckDriver, truckDeliveryOrder[_currentDeliveryPackIndex]);

        if (isDeliverInSequence)
        {
            _currentDeliveryPackIndex = (_currentDeliveryPackIndex + 1) % truckDeliveryOrder.Length;
        }

        // Spawn truck
        _currentTruckDriver.GetAnimator().SetTrigger(enter);
    }

    private void LoadUpTruck(TruckDriver truck, TruckDeliveryPack deliveryPack)
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
                default:
                    throw new ArgumentOutOfRangeException();
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
            if (players[s] != player) continue;
            
            players.RemoveAt(s);
            return;
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
            if (customersInLevel[c] != customer) continue;
            
            customersInLevel.RemoveAt(c);
            return;
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

        hudScript.UpdateCash(profit);
    }

    public int GetScore()
    {
        return profit;
    }

    public void StealScore(int amount)
    {
        profit -= amount;

        hudScript.UpdateCash(profit);
    }

    public void LostCustomer()
    {
        lostCustomers++;

        hudScript.UpdateUpsetCustomers(lostCustomers);
    }

    public int GetLostCustomerAmount()
    {
        return lostCustomers;
    }

    public void PauseGame()
    {
        if (_currentGameState != GameState.Running && _currentGameState != GameState.Starting) return;
        // Save previous game state
        _previousGameState = _currentGameState;

        // Set up pause state
        StartCoroutine(SetSlowDown(0.5f));
        hudScript.GetPauseMenu().SetActive(true);

        // Change state
        _currentGameState = GameState.Paused;
    }

    public void ResumeGame()
    {
        // Set up resume game
        Time.timeScale = 1.0f;
        hudScript.GetPauseMenu().SetActive(false);

        // Restore previous game state
        _currentGameState = _previousGameState;
    }

    public GameState GetGameState()
    {
        return _currentGameState;
    }

    public void ForceLoseReasonMessage(string lostReason)
    {
        latestLostReason = lostReason;
    }

    public void LostState(string lostReason)
    {
        _currentGameState = GameState.Lost;

        hudScript.SetGameOver(lostReason);
        StartCoroutine(SetSlowDown(endSlowDownDuration));
    }

    public void LostState()
    {
        LostState(EssentialFunctions.GetRandomFromArray(randomLostMessages));
    }

    public bool IsGameOver()
    {
        return _currentGameState == GameState.Lost || _currentGameState == GameState.Won;
    }

    private IEnumerator SetSlowDown(float slowDownDuration)
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

    public void WinState()
    {
        WinState(EssentialFunctions.GetRandomFromArray(randomWinMessages));
    }
    
    public void WinState(string winReason)
    {
        _currentGameState = GameState.Won;
        _gameDuration = 0.0f;

        hudScript.SetGameOver(winReason);

        PlayerData.currentInfo.profit = profit;
        PlayerData.currentInfo.money = profit;
        PlayerData.SaveSlotData();

        StartCoroutine(SetSlowDown(endSlowDownDuration));
    }

    public void ToScoreBoard()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadSceneAsync("ScoreScene");
    }
}
