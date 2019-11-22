using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    [SerializeField] Vector3 rotationDirection = Vector3.zero;
    [SerializeField] float rotationSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationDirection * rotationSpeed);
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
