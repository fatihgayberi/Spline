using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WonnaMathf
{
    private static float[] Factorial = new float[]{
        1.0f,
        1.0f,
        2.0f,
        6.0f,
        24.0f,
        120.0f,
        720.0f,
        5040.0f,
        40320.0f,
        362880.0f,
        3628800.0f,
        39916800.0f,
        479001600.0f,
        6227020800.0f,
        87178291200.0f,
        1307674368000.0f,
        20922789888000.0f,};

    public static float WonnaBinom(int upper, int lower)
    {
        float a1 = Factorial[upper];
        float a2 = Factorial[lower];
        float a3 = Factorial[upper - lower];

        return a1 / (a2 * a3);
    }

    public static float WonnaBernstein(int n, int v, float t)
    {
        return WonnaMathf.WonnaBinom(n, v) * Mathf.Pow(t, v) * Mathf.Pow(1 - t, (n - v));
    }
}
