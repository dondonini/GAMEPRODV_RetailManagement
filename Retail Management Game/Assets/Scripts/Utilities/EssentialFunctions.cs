using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        //Debug.Log("There are " + inPickupArea.Length + " items in the pickup area.");

        // Collect all items in pickup area and check if they are valid
        foreach (Collider c in inPickupArea)
        {
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

        // Return the closest item
        return closestItem.gameObject;
    }

    //GameObject FindGameObjectsInParentsWithTag(Transform transform, string tag)
    //{
    //    if (transform.CompareTag(tag))
    //    {
    //        return transform.gameObject;
    //    }
    //    else
    //    {
    //        if (transform.parent = GameObject.)
    //        return FindGameObjectsInParentsWithTag(transform.parent, tag);
    //    }
    //}
}
