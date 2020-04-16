using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private bool autoMode;
    [SerializeField] private float autoSpawnRate = 5.0f;
    public GameObject customer;

    private GameManager _gameManager;

    private float _autoTimer = 0.0f;

    // Update is called once per frame
    private void Update()
    {
        if (!autoMode) return;
        
        if (_autoTimer >= autoSpawnRate)
        {
            GameObject newCustomer = Instantiate(customer);

            newCustomer.transform.position = transform.position;

            _autoTimer = 0.0f;
        }
        _autoTimer += Time.deltaTime;
    }

    private void Start()
    {
        _gameManager = GameManager.GetInstance();
    }

    public void SpawnCustomer(GameObject other)
    {
        GameObject newCustomer = Instantiate(other) as GameObject;

        newCustomer.transform.position = transform.position;

        _gameManager.AddCustomer(newCustomer.transform);
    }
}
