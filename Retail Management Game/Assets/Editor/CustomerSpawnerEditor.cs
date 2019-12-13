using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomerSpawner))]
public class CustomerSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CustomerSpawner myScript = (CustomerSpawner)target;
        if (GUILayout.Button("Force Spawn"))
        {
            if (myScript.customer)
                myScript.SpawnCustomer(myScript.customer);
            else
                Debug.Log("There is no customer set for force spawning.");
        }
    }
}
