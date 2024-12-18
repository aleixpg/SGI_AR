using UnityEngine;

public static class VectorExtensions
{
    public static bool IsZero(this Vector3 vector)
    {
        return vector == Vector3.zero;
    }
}
