using System;
using UnityEngine;

public static class Noise
{
    public enum ENormalizeMode
    {
        Local,
        Global
    }

    /// <summary>
    /// float 2d array�� NoiseMap ���� �޼���
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="scale">non-integer�� ���� scale��. 0������ �� �Է½� 0.0001�� clamp</param>
    /// <param name="octaves"></param>
    /// <param name="persistance"></param>
    /// <param name="lacunarity"></param>
    /// <returns>float 2D array������ Perlin Noise</returns>
    public static float[,] GenerateNoiseMap(
        int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector3 offset, ENormalizeMode normalizeMode)
    {
        var noiseMap = new float[mapWidth, mapHeight];

        //pseudo random number generator
        var prng = new System.Random(seed);
        var octaveOffsets = new Vector2[octaves];

        var maxPossibleHeight = 0f;
        var amplitude = 1f;
        var frequency = 1f;

        for (int i = 0; i < octaves; i++)
        {
            var offsetX = prng.Next(-100000, 100000) + offset.x;
            var offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        //scale�� 0���� �۰ų� ���ٸ� scale�� clamp
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        var maxLocalNoiseHeight = float.MinValue;
        var minLocalNoiseHeight = float.MaxValue;

        var halfWidth = mapWidth / 2f;
        var halfHieght = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1f;
                frequency = 1f;
                var noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    //x, y���� scale��ŭ ������ non-integer value ���ϱ�
                    //-> perlin noise�� �������� ������ �׻� ���� ���� ��ȯ�ϱ� ����
                    //frequency��ŭ ���Ͽ� ���ļ��� �����ϰ� octaveOffsets��ŭ ���ؼ� �õ� �ݿ�
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHieght + octaveOffsets[i].y) / scale * frequency;

                    //�ܼ��� perlinNoise�� ����ϸ� ��հ��� 0.5�� �ʹ� ����
                    // *2-1�� �Ͽ� ������ -1 ~ 1�� ��ȯ ->����� 0�̵�
                    var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    //perlinValue�� amplitude(���� ��Ÿ���� ����)�� ����
                    //�ʹ� ��Ÿ��� amplitude�� ���ϰ�, ���߿��� �۾����� �̼��� �����ϸ� �߰���
                    noiseHeight += perlinValue * amplitude;

                    // amplitude�� persistance(0~1)�� ���Ͽ� ���� ��Ÿ���� ���̸� ����
                    // persistance�� �������� ���� ��Ÿ���� ���̰� ������
                    // -> ū ������ �ְ�, ���� ���� ������ �߰��Ǵ� ȿ���� ����
                    amplitude *= persistance;

                    // frequency�� lacunarity(1�̻�)�� ���Ͽ� ���� ��Ÿ���� frequency�� ����
                    // frequency�� Ŀ������ ���ļ��� �����Ͽ� �۰� ������ ������ �߰���
                    frequency *= lacunarity;
                }

                // ������ �ּ�, �ִ��� �����Ͽ� ����ȭ�� ���
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == ENormalizeMode.Local)
                {
                    //min, max ������ ���� 0~1������ ����ȭ
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    var normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }
        return noiseMap;
    }
}
