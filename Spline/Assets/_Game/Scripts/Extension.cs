using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WonnaMathf
{
    public static float WonnaLerp(float p0, float p1, float t)
    {
        return (1 - t) * p0 + t * p1;
    }

    public static Vector3 WonnaLerp(Vector3 p0, Vector3 p1, float t)
    {
        return new Vector3(
            WonnaLerp(p0.x, p1.x, t),
            WonnaLerp(p0.y, p1.y, t),
            WonnaLerp(p0.z, p1.z, t)
            );
    }

    public static float WonnaBinom(int upper, int lower)
    {
        if (lower == 0)
        {
            return 1;
        }
        if (upper - lower == 0)
        {
            return 1;
        }

        float denominator = 1; // payda

        for (int i = 1; i <= upper - lower; i++)
        {
            denominator *= i;
        }

        if (denominator == 0)
        {
            Debug.LogWarning("Payda sifir olamaz");
            return 1;
        }

        float numerator = 1; // pay
        
        for (int i = upper - lower; i <= upper; i++)
        {
            numerator *= i;
        }

        return numerator / denominator;
    }
}
