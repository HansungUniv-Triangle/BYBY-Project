using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer TextureRenderer;
   
    private Texture2D _texture;
    private Color[] _colorMap;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        var width = noiseMap.GetLength(0);
        var height = noiseMap.GetLength(1);

        _texture = new Texture2D(width, height);
        _colorMap = new Color[width * height];

        for(var y = 0; y < height; y++)
        {
            for(var x = 0; x < width; x++)
            {
                _colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }
        _texture.SetPixels(_colorMap);
        _texture.Apply();

        TextureRenderer.sharedMaterial.mainTexture = _texture;                   // editor에서도 텍스쳐를 확인할 수 있도록
        TextureRenderer.transform.localScale = new Vector3(width, 1, height);   // plane 크기와 map 크기 맞추기
    }
}
