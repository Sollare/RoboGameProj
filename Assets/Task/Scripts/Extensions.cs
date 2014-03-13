using UnityEngine;
using System.Collections;

public static class Extensions
{
    public static Transform FindChildInHierarchy(this Transform target, string name)
    {
        if (target.name == name) return target;

        for (int i = 0; i < target.childCount; ++i)
        {
            var result = FindChildInHierarchy(target.GetChild(i), name);

            if (result != null) return result;
        }

        return null;
    }
}