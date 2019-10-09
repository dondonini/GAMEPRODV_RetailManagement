using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class EssentialFunctions
{
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
}
