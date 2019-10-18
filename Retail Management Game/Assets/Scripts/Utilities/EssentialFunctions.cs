﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Where I keep all of my random yet useful functions
/// </summary>
public static class EssentialFunctions
{
    public static Bounds GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    public static Vector3[] GetAllCornersOfTransform(Transform t)
    {
        Vector3[] corners = new Vector3[8];

        /************************************************/
        // Top Vectors

        // Back Left
        corners[0] = t.position + (new Vector3(-t.localScale.x, t.localScale.y, t.localScale.z)     * 0.5f);

        // Back Right
        corners[1] = t.position + (new Vector3(t.localScale.x, t.localScale.y, t.localScale.z)      * 0.5f);

        // Front Left
        corners[2] = t.position + (new Vector3(-t.localScale.x, t.localScale.y, -t.localScale.z)    * 0.5f);

        // Front Right
        corners[3] = t.position + (new Vector3(t.localScale.x, t.localScale.y, -t.localScale.z)     * 0.5f);


        /************************************************/
        // Bottom Vectors

        // Back Left
        corners[4] = t.position + (new Vector3(-t.localScale.x, -t.localScale.y, t.localScale.z)    * 0.5f);

        // Back Right
        corners[5] = t.position + (new Vector3(t.localScale.x, -t.localScale.y, t.localScale.z)     * 0.5f);

        // Front Left
        corners[6] = t.position + (new Vector3(-t.localScale.x, -t.localScale.y, -t.localScale.z)   * 0.5f);

        // Front Right
        corners[7] = t.position + (new Vector3(t.localScale.x, -t.localScale.y, -t.localScale.z)    * 0.5f);

        return corners;
    }

    public static Component GetComponentMultiType(this GameObject source, Component[] components)
    {
        foreach (Component c in components)
        {
            Component result = source.GetComponentInParent(c.GetType());
            if (result)
            {
                return c;
            }
        }

        return null;
    }


    public static GameObject GetClosestInteractableInFOV(Transform transform, BoxCollider area, float FOV, float maxDistance, string[] tags = null, GameObject[] excludedGameObjects = null)
    {

        // Get all items in pickup area
        Collider[] inPickupArea = Physics.OverlapBox(
            area.transform.position,
            area.size,
            area.transform.rotation,
            LayerMask.GetMask("Interactive")
        );

        // Items in pickup area that is also in player view angle
        List<Transform> validItems = new List<Transform>();

        // Collect all items in pickup area and check if they are valid
        for (int cIndex = 0; cIndex < inPickupArea.Length; cIndex++)
        {
            Collider c = inPickupArea[cIndex];

            // Calculate angle of item in pickup area from player
            Vector3 targetDir = c.transform.position - transform.position;
            float targetAngleFromPlayer = Vector3.Angle(targetDir, transform.forward);
            float targetDistanceFromPlayer = Vector3.Distance(transform.position, c.transform.position);

            //Debug.Log("Item: " + c.gameObject + " Angle: " + targetAngleFromPlayer + " Distance: " + targetDistanceFromPlayer);


            // Item is in player view and is not too far
            if (Mathf.Abs(targetAngleFromPlayer) < FOV * 0.5f ||
                targetDistanceFromPlayer < maxDistance)
            {
                // Ignore excluded gameobjects
                bool excludedResult = false;
                if (excludedGameObjects != null)
                    for (int i = 0; i < excludedGameObjects.Length; i++)
                    {
                        if (!excludedGameObjects[i]) continue;

                        if (c.transform == excludedGameObjects[i].transform)
                        {
                            excludedResult = true;
                            continue;
                        }
                    }

                if (excludedResult) continue;

                Transform rootTransform = c.transform.root;

                bool tagResult = false;
                if (tags != null)
                    for (int t = 0; t < tags.Length; t++)
                    {
                        if (rootTransform.CompareTag(tags[t]))
                        {
                            tagResult = true;
                            continue;
                        }

                    }
                else
                {
                    validItems.Add(rootTransform);
                }

                if (tagResult) validItems.Add(rootTransform);
            }
        }

        // Stop code if there are no valid items in list
        if (validItems.Count == 0)
            return null;

        // Calculate the closest item from player
        Transform closestItem = validItems[0];
        float minDistance = Vector3.Distance(transform.position, validItems[0].position);

        // Continue scanning list if there are more than one item in the list
        if (validItems.Count > 1)
        {
            // Compare next item in the list to previously scanned closest item from player
            for (int i = 1; i < validItems.Count; i++)
            {
                float measuredDistance = Vector3.Distance(transform.position, validItems[i].position);

                if (measuredDistance < minDistance)
                {
                    closestItem = validItems[i];
                    minDistance = measuredDistance;
                }
            }
        }
        foreach (Collider c in inPickupArea)
        {
            
        }

        // Return the closest item
        return closestItem.gameObject;
    }

    /// <summary>
    /// Checks object if it is one of multiple tags.
    /// </summary>
    /// <param name="transform">Object to check.</param>
    /// <param name="tags">Tags to compare.</param>
    /// <returns>True if the tag of the object matches on of the tags.</returns>
    public static bool CompareTags(Transform transform, string[] tags)
    {
        for (int t = 0; t < tags.Length; t++)
        {
            if (transform.CompareTag(tags[t]))
            {
                return true;
            }
        }

        return false;
    }
}
