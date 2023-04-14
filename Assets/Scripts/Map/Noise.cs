using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Noise
{
    public static float[,] GeneratePerlinNoise(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (var i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;    // divide by zero 예방
        }

        float maxNoiseHeight = float.MinValue;
        float minNoinHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (var y = 0; y < mapHeight; y++)
        {
            for (var x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (var i = 0; i < octaves; i++)
                { 
                    float xCoordinate = (x - halfWidth) / scale * frequency + octaveOffsets[i].x * frequency;
                    float yCoordinate = (y - halfHeight) / scale * frequency - octaveOffsets[i].y * frequency;

                    float noiseValue = Mathf.PerlinNoise(xCoordinate, yCoordinate) * 2 - 1;
                    noiseHeight += noiseValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // 구조물 느낌 나도록, 이상하면 뺄 예정
                if (noiseHeight > 0.4)
                    noiseHeight *= 3;

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoinHeight)
                {
                    minNoinHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        // 노이즈 값의 가장 큰 값과 낮은 값을 기준으로 0~1 정규화
        for (var y = 0; y < mapHeight; y++)
        {
            for (var x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoinHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    public static void MakeSeamlessNoiseHorizontally(float[,] noiseMap, int stitchWidth)
    {
        var mapWidth = noiseMap.GetLength(0);
        var mapHeight = noiseMap.GetLength(1);

        for (var x = 0; x < stitchWidth; x++)
        {
            var t = x / (float)stitchWidth; // 투명도 그라디언트
            for (var y = 0; y < mapHeight; y++)
            {
                var o = mapWidth - stitchWidth + x;
                noiseMap[o, y] = Mathf.Lerp(noiseMap[o, y], noiseMap[stitchWidth - x, y], t);
            }
        }
    }

    public static void MakeSeamlessNoiseVertically(float[,] noiseMap, int stitchHeight)
    {
        var mapWidth = noiseMap.GetLength(0);
        var mapHeight = noiseMap.GetLength(1);

        for (var y = 0; y < stitchHeight; y++)
        {
            var t = y / (float)stitchHeight;
            for (var x = 0; x < mapWidth; x++)
            {
                var o = mapHeight - stitchHeight + y;
                noiseMap[x, o] = Mathf.Lerp(noiseMap[x, o], noiseMap[x, stitchHeight - y], t);
            }
        }
    }

    public static float Get3DNoiseValue(float x, float y, float z, float scale, float seed, Vector3 Offset)
    {
        float xCoord = (x + seed + 0.1f) * scale + Offset.x;
        float yCoord = (y + seed + 0.1f) * scale + Offset.y;
        float zCoord = (z + seed + 0.1f) * scale + Offset.z;

        float XY = Mathf.PerlinNoise(xCoord, yCoord);
        float YZ = Mathf.PerlinNoise(yCoord, zCoord);
        float ZX = Mathf.PerlinNoise(zCoord, xCoord);

        float YX = Mathf.PerlinNoise(yCoord, xCoord);
        float ZY = Mathf.PerlinNoise(zCoord, yCoord);
        float XZ = Mathf.PerlinNoise(xCoord, zCoord);

        return (XY + YZ + ZX + YX + ZY + XZ) / 6f;
    }
}
