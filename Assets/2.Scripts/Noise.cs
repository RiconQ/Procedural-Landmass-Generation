using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    /// <summary>
    /// float 2d array의 NoiseMap 생성 메서드
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="scale">non-integer를 위한 scale값. 0이하의 수 입력시 0.0001로 clamp</param>
    /// <returns>float 2D array형식의 Perlin Noise</returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
    {
        var noiseMap = new float[mapWidth, mapHeight];

        //scale이 0보다 작거나 같다면 scale값 clamp
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        for(int y  = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                //x, y값을 scale만큼 나눠서 non-integer value 구하기
                //-> perlin noise는 정수값이 들어오면 항상 같은 값을 반환하기 때문
                float sampleX = x / scale;
                float sampleY = y / scale;

                var perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[y, x] = perlinValue;
            }
        }

        return noiseMap;
    }
}
