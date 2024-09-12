using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static Vector3 ToVector(this Color color)
    {
        return new Vector3(color.r, color.g, color.b);
    }
    public static Color ToColor(this Vector3 vector)
    {
        return new Color(vector.x, vector.y, vector.z);
    }
}