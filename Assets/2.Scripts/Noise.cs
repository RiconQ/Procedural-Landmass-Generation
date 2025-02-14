using UnityEngine;

public static class Noise
{
    /// <summary>
    /// float 2d array의 NoiseMap 생성 메서드
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="scale">non-integer를 위한 scale값. 0이하의 수 입력시 0.0001로 clamp</param>
    /// <param name="octaves"></param>
    /// <param name="persistance"></param>
    /// <param name="lacunarity"></param>
    /// <returns>float 2D array형식의 Perlin Noise</returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector3 offset)
    {
        var noiseMap = new float[mapWidth, mapHeight];

        //pseudo random number generator
        var prng = new System.Random(seed);
        var octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            var offsetX = prng.Next(-100000, 100000) + offset.x;
            var offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }


        //scale이 0보다 작거나 같다면 scale값 clamp
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        var maxNoiseHeight = float.MinValue;
        var minNoiseHeight = float.MaxValue;

        var halfWidth = mapWidth / 2f;
        var halfHieght = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                var amplitude = 1f;
                var frequency = 1f;
                var noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    //x, y값을 scale만큼 나눠서 non-integer value 구하기
                    //-> perlin noise는 정수값이 들어오면 항상 같은 값을 반환하기 때문
                    //frequency만큼 곱하여 주파수를 조절하고 octaveOffsets만큼 더해서 시드 반영
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHieght) / scale * frequency + octaveOffsets[i].y;

                    //단순히 perlinNoise를 사용하면 평균값이 0.5라서 너무 밝음
                    // *2-1을 하여 범위를 -1 ~ 1로 변환 ->평균이 0이됨
                    var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    //perlinValue에 amplitude(현재 옥타브의 강도)를 조절
                    //초반 옥타브는 amplitude가 강하고, 나중에는 작아져서 미세한 디테일만 추가됨
                    noiseHeight += perlinValue * amplitude;

                    // amplitude에 persistance(0~1)을 곱하여 다음 옥타브의 높이를 조절
                    // persistance가 작을수록 다음 옥타브의 높이가 낮아짐
                    // -> 큰 지형이 있고, 점점 작은 패턴이 추가되는 효과를 만듦
                    amplitude *= persistance;

                    // frequency에 lacunarity(1이상)을 곱하여 다음 옥타브의 frequency를 조절
                    // frequency가 커질수록 주파수가 증가하여 작고 세밀한 패턴이 추가됨
                    frequency *= lacunarity;
                }

                // 노이즈 최소, 최댓값을 갱신하여 정규화에 사용
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //min, max 사이의 값을 0~1범위로 정규화
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
