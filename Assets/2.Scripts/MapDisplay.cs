using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private Renderer m_textureRender;

    /// <summary>
    /// 2D texture�� �޾ƿ� material�� texture�� �����ϴ� �޼���
    /// </summary>
    public void DrawTexture(Texture2D texture)
    {
        m_textureRender.sharedMaterial.mainTexture = texture;
        m_textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
