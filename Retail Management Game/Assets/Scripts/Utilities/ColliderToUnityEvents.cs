using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnCollisionEnter_c : UnityEvent<Collision> { }
[System.Serializable]
public class OnCollisionExit_c : UnityEvent<Collision> { }
[System.Serializable]
public class OnCollisionStay_c : UnityEvent<Collision> { }

[RequireComponent(typeof(Collider))]
public class ColliderToUnityEvents : MonoBehaviour
{
    public OnCollisionEnter_c OnCollisionEnter_UE;
    public OnCollisionExit_c OnCollisionExit_UE;
    public OnCollisionStay_c OnCollisionStay_UE;

    private void Start()
    {
        if (OnCollisionEnter_UE == null)
            OnCollisionEnter_UE = new OnCollisionEnter_c();

        if (OnCollisionExit_UE == null)
            OnCollisionExit_UE = new OnCollisionExit_c();

        if (OnCollisionStay_UE == null)
            OnCollisionStay_UE = new OnCollisionStay_c();
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnter_UE.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExit_UE.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        OnCollisionStay_UE.Invoke(collision);
    }

    
}
