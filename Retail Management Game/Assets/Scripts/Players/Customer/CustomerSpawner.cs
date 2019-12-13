using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] bool autoMode = false;
    [SerializeField] float autoSpawnRate = 5.0f;
    public GameObject customer = null;

    GameManager gameManager = null;

    float autoTimer = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (autoMode)
        {
            if (autoTimer >= autoSpawnRate)
            {
                GameObject newCustomer = Instantiate(customer) as GameObject;

                newCustomer.transform.position = transform.position;

                autoTimer = 0.0f;
            }
            autoTimer += Time.deltaTime;
        }
    }

    private void Start()
    {
        gameManager = GameManager.GetInstance();
    }

    public void SpawnCustomer(GameObject customer)
    {
        GameObject newCustomer = Instantiate(customer) as GameObject;

        newCustomer.transform.position = transform.position;

        gameManager.AddCustomer(newCustomer.transform);
    }
}
