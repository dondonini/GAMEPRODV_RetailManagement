using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] Camera cameraToLookAt = null;

    // Start is called before the first frame update
    void Start()
    {
        if (!cameraToLookAt)
            cameraToLookAt = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.forward = cameraToLookAt.transform.forward;
        //transform.LookAt(2 * transform.position - cameraToLookAt.transform.position);
    }
}
