using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSpawner : MonoBehaviour
{
    public Vector3 forceDirection = new Vector3(0.0f, -1.0f, 0.0f);
    public float force = 100.0f;

    public GameObject test;


    float timer = 0.0f;
    private void OnValidate()
    {
        forceDirection = forceDirection.normalized;
    }

    public void SpawnObject(GameObject other)
    {
        GameObject newObject = Instantiate(other) as GameObject;

        newObject.transform.position = transform.position;

        Rigidbody rb = newObject.GetComponent<Rigidbody>();
        rb.AddForce(forceDirection * force);
    }

    private void Update()
    {
        if (timer >= 0.1f)
        {
            SpawnObject(test);
            timer = 0.0f;
        }

        timer += Time.deltaTime;
    }
}
