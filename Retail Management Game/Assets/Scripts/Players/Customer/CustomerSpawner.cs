using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    //[SerializeField] float spawnRate = 5.0f;
    //[SerializeField] GameObject customer = null;

    //float timer = 0.0f;

    //// Update is called once per frame
    //void Update()
    //{
    //    timer += Time.deltaTime;

    //    if (timer >= spawnRate)
    //    {
    //        GameObject newCustomer = Instantiate(customer) as GameObject;

    //        newCustomer.transform.position = transform.position;

    //        timer = 0.0f;
    //    }
    //}

    public void SpawnCustomer(GameObject customer)
    {
        GameObject newCustomer = Instantiate(customer) as GameObject;

        newCustomer.transform.position = transform.position;
    }
}
