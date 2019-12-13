using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    [SerializeField] bool rotateTimeUnscaled = false;
    [SerializeField] Vector3 rotationDirection = Vector3.zero;
    [SerializeField] float rotationSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rotationAmount = rotationDirection * rotationSpeed;

        if (rotateTimeUnscaled)
        {
            transform.Rotate(rotationAmount * Time.unscaledDeltaTime);
        }
        else
        {
            transform.Rotate(rotationAmount * Time.deltaTime);
        }
        
    }

    public Vector3 RotationDirection
    {
        get
        {
            return rotationDirection;
        }
        set
        {
            rotationDirection = value;
        }
    }

    public float RotationSpeed
    {
        get
        {
            return rotationSpeed;
        }
        set
        {
            rotationSpeed = value;
        }
    }
}
