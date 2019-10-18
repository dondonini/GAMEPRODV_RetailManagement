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

    public float despawnHeight = -100.0f;
    public float defaultViewableRadius = 10.0f;

    [SerializeField]
    List<Transform> players;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPlayer(Transform newPlayer)
    {
        foreach(Transform p in players)
            if (p == newPlayer)
            {
                Debug.LogWarning(newPlayer + " is already the main subject... what are you doing?");
                return;
            }

        // Set new main subject
        players.Add(newPlayer);
    }

    public void RemovePlayer(Transform _targetSubject)
    {
        for (int s = 0; s < players.Count; s++)
        {
            if (players[s] == _targetSubject)
            {
                players.RemoveAt(s);
                return;
            }
        }

        // If it gets to hear, it means that the targeted subject was not found
        Debug.LogWarning("Subject " + _targetSubject + " is not in the subject list.");
    }

    public Transform[] Players()
    {
        return players.ToArray();
    }
}
