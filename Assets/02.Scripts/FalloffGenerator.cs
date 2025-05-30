using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size)
    {
        var map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                var x = i / (float)size * 2 - 1;
                var y = j / (float)size * 2 - 1;

                var value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                map[i, j] = Evaluate(value);
            }
        }
        return map;
    }

    private static float Evaluate(float value)
    {
        var a = 3f;
        var b = 2.2f;
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, b));
    }
}
