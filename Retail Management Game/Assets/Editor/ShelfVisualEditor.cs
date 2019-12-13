using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShelfVisual))]
public class ShelfVisualEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShelfVisual myScript = (ShelfVisual)target;
        if (GUILayout.Button("Collect Visuals"))
        {
            int children = myScript.AddVisuals(myScript.transform);

            if (children == 0)
                Debug.Log("There are no visual children in this transform!");
        }
    }
}
