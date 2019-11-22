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

    [SerializeField] float customerSpawnFrequency = 5.0f;
    [SerializeField] float truckSpawnFrequency = 30.0f;

    [SerializeField] List<Transform> players = new List<Transform>();
    [SerializeField] List<Transform> customers = new List<Transform>();

    //************************************************************************/
    // Runtime Variables

    MapManager mapManager = null;

    bool isGameActive = false;

    float truckTimer = 0.0f;
    float customerSpawnTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        mapManager = MapManager.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (customers.Contains(newCustomer)) return;

        customers.Add(newCustomer);
    }

    public void RemoveCustomer(Transform customer)
    {
        for (int c = 0; c < customers.Count; c++)
        {
            if (customers[c] == customer)
            {
                customers.RemoveAt(c);
                return;
            }
        }

        // If it gets to here, it means that the targeted subject was not found
        Debug.LogWarning("Customer " + customer + " is not in the customer list.");
    }

    public Transform[] Players()
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
