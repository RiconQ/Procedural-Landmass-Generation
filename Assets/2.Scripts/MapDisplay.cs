using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private Renderer m_textureRender;

    /// <summary>
    /// 2D texture를 받아와 material의 texture를 변경하는 메서드
    /// </summary>
    public void DrawTexture(Texture2D texture)
    {
        m_textureRender.sharedMaterial.mainTexture = texture;
        m_textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
