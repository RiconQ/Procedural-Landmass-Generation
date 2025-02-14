using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private Renderer m_textureRender;

    /// <summary>
    /// 생성한 NoiseMap을 그리는 메서드
    /// </summary>
    /// <param name="noiseMap"></param>
    public void DrawNoiseMap(float[,] noiseMap)
    {
        var width = noiseMap.GetLength(0);
        var height = noiseMap.GetLength(1);

        var texture = new Texture2D(width, height);
        var colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        //texture의 각 픽셀들을 colorMap의 순서대로 할당
        texture.SetPixels(colorMap);
        texture.Apply();

        m_textureRender.sharedMaterial.mainTexture = texture;
        m_textureRender.transform.localScale = new Vector3(width, 1, height);
    }
}
