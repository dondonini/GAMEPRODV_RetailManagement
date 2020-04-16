using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        corners[0] = t.position + (new Vector3(-t.localScale.x, t.localScale.y, t.localScale.z) * 0.5f);

        // Back Right
        corners[1] = t.position + (new Vector3(t.localScale.x, t.localScale.y, t.localScale.z) * 0.5f);

        // Front Left
        corners[2] = t.position + (new Vector3(-t.localScale.x, t.localScale.y, -t.localScale.z) * 0.5f);

        // Front Right
        corners[3] = t.position + (new Vector3(t.localScale.x, t.localScale.y, -t.localScale.z) * 0.5f);


        /************************************************/
        // Bottom Vectors

        // Back Left
        corners[4] = t.position + (new Vector3(-t.localScale.x, -t.localScale.y, t.localScale.z) * 0.5f);

        // Back Right
        corners[5] = t.position + (new Vector3(t.localScale.x, -t.localScale.y, t.localScale.z) * 0.5f);

        // Front Left
        corners[6] = t.position + (new Vector3(-t.localScale.x, -t.localScale.y, -t.localScale.z) * 0.5f);

        // Front Right
        corners[7] = t.position + (new Vector3(t.localScale.x, -t.localScale.y, -t.localScale.z) * 0.5f);

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

    public static T GetRandomFromArray<T>(List<T> array)
    {
        return GetRandomFromArray(array.ToArray());
    }

    public static T GetRandomFromArray<T>(T[] array)
    {
        return array.Length == 1 ? array[0] : array[Random.Range(0, array.Length)];
    }

    public static float GetCurrentAnimationClipPosition(Animator animator)
    {
        AnimatorStateInfo animationState = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] animatorClips = animator.GetCurrentAnimatorClipInfo(0);

        return animatorClips[0].clip.length * animationState.normalizedTime;
    }

    public static float RotateTowardsTargetSmoothDamp(Transform transformToTurn, Transform target, ref Quaternion currentVelocity, float smoothTime)
    {
        return RotateTowardsTargetSmoothDamp(transformToTurn, target.position, ref currentVelocity, smoothTime);
    }

    /// <summary>
    /// Rotates subject towards the target using SmoothDamp
    /// </summary>
    /// <param name="transformToTurn">Subject to turn.</param>
    /// <param name="target">Target to turn subject towards.</param>
    /// <param name="currentVelocity">The current velocity, this value is modified by the function every time you call it.</param>
    /// <param name="smoothTime">Approximately the time it will take to reach the target. A smaller value will reach the target faster.</param>
    /// <returns>The delta angle between the direction the subject is facing to target position.</returns>
    public static float RotateTowardsTargetSmoothDamp(Transform transformToTurn, Vector3 target, ref Quaternion currentVelocity, float smoothTime)
    {
        // Calculate direction to target
        Vector3 targetRot = target - transformToTurn.position;
        targetRot.y = 0.0f;
        targetRot.Normalize();

        // SmoothDamp towards to target rotation
        transformToTurn.rotation =
            QuaternionUtil.SmoothDamp(
                transformToTurn.rotation,
                Quaternion.LookRotation(targetRot),
                ref currentVelocity,
                smoothTime
            );

        // Debug visuals
        Debug.DrawRay(transformToTurn.position, targetRot * 5.0f, Color.green);
        Debug.DrawRay(transformToTurn.position, transformToTurn.forward * 5.0f, Color.red);

        return Vector3.Angle(transformToTurn.forward, targetRot);
    }

    //*************************************************************************
    // GetRandomPositionIn___
    
    /*
     * Get random position in areas
     */
    
    public static Vector3 GetRandomPositionInTransform(Transform t)
    {
        return GetRandomPositionInZone(t.position, t.localScale);
    }

    public static Vector3 GetRandomPositionInBounds(Bounds b)
    {
        return GetRandomPositionInZone(b.center, b.size);
    }
    
    public static Vector3 GetRandomPositionInZone(Vector3 center, Vector3 size)
    {
        Vector3 sizeHalf = size * 0.5f;

        float xPos = center.x + Random.Range(-sizeHalf.x, sizeHalf.x);
        float yPos = center.y + Random.Range(-sizeHalf.y, sizeHalf.y);
        float zPos = center.z + Random.Range(-sizeHalf.z, sizeHalf.z);
        
        return new Vector3(xPos, yPos, zPos);
    }
}
