using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    /// <summary>
    /// float 2d array�� NoiseMap ���� �޼���
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="scale">non-integer�� ���� scale��. 0������ �� �Է½� 0.0001�� clamp</param>
    /// <returns>float 2D array������ Perlin Noise</returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
    {
        var noiseMap = new float[mapWidth, mapHeight];

        //scale�� 0���� �۰ų� ���ٸ� scale�� clamp
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        for(int y  = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                //x, y���� scale��ŭ ������ non-integer value ���ϱ�
                //-> perlin noise�� �������� ������ �׻� ���� ���� ��ȯ�ϱ� ����
                float sampleX = x / scale;
                float sampleY = y / scale;

                var perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[y, x] = perlinValue;
            }
        }

        return noiseMap;
    }
}
