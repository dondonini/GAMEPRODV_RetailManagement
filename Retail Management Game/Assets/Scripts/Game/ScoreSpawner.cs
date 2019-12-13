using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSpawner : MonoBehaviour
{
    public float force = 100.0f;
    [Range(0.0f, 0.1f)]
    public float directionVariation = 0.1f;

    //public GameObject test;


    //float timer = 0.0f;

    public GameObject SpawnObject(GameObject other)
    {
        GameObject newObject = Instantiate(other) as GameObject;

        newObject.transform.position = transform.position;

        Rigidbody rb = newObject.GetComponent<Rigidbody>();
        rb.AddForce(CalculatePushDirection() * force);

        return newObject;
    }

    Vector3 CalculatePushDirection()
    {
        // Add variation to throwing angle
        Vector3 direction = -Vector3.forward;
        direction.x = Random.Range(-directionVariation, directionVariation);
        direction.y = Random.Range(-directionVariation, directionVariation);

        Debug.DrawRay(transform.position, direction, Color.red, 1.0f);

        return transform.TransformVector(direction);
    }

    //private void Update()
    //{
    //    if (timer >= 0.1f)
    //    {
    //        SpawnObject(test);
    //        timer = 0.0f;
    //    }

    //    timer += Time.deltaTime;
    //}
}
