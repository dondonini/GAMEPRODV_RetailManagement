using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CenterPivot : ScriptableWizard
{
    [MenuItem ("My Tools/Center Pivot to Children")]
    static void CenterPivotWizard()
    {
        if (Selection.objects.Length == 0)
        {
            Debug.Log("There is no object selected!");
            return;
        }

        if (Selection.objects.Length > 1)
        {
            Debug.Log("Too many objects selected! Only choose one!");
            return;
        }

        if (Selection.activeTransform.childCount == 0)
        {
            Debug.Log("The selected transform has no children!");
            return;
        }

        // Collect children in pivot
        List<Transform> children = new List<Transform>();

        Transform activeTransform = Selection.activeTransform;

        for (int c = 0; c < activeTransform.childCount; c++)
        {
            children.Add(activeTransform.GetChild(c));
        }

        // Get center pivot
        Vector3 centerPivot = EssentialFunctions.GetMaxBounds(activeTransform.gameObject).center;

        // Detach children from pivot
        foreach(Transform child in children)
        {
            child.SetParent(null);
        }

        // Move pivot to center
        activeTransform.position = centerPivot;

        // Bring back children into pivot
        foreach (Transform child in children)
        {
            child.SetParent(activeTransform);
        }

        Debug.Log("Pivot was moved successfully!");
    }
}
