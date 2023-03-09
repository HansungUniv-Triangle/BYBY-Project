using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBlockData
{
    public enum Direction
    {
        FORWARD,    // (0, 0, 1)
        RIGHT,      // (1, 0, 0)
        UP,         // (0, 1, 0)
        BACKWARD,   // (0, 0, -1)
        LEFT,       // (-1, 0, 0)
        DOWN        // (0, -1, 0)
    }

    public static readonly Vector3Int[] CheckDireactions = { 
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.up,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.down
    };

    public static readonly int[,] FaceNumber = {
        { 0, 1, 2, 3 }, // FORWARD
        { 5, 0, 3, 6 }, // RIGHT
        { 5, 4, 1, 0 }, // UP
        { 4, 5, 6, 7 }, // BACKWARD
        { 1, 4, 7, 2 }, // LEFT
        { 3, 2, 7, 6 }  // DOWN
    };

    public static readonly Vector3[] Vertices =
    {
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, +0.5f),
        new Vector3(+0.5f, -0.5f, +0.5f),
        new Vector3(-0.5f, +0.5f, -0.5f),
        new Vector3(+0.5f, +0.5f, -0.5f),
        new Vector3(+0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f)
    };

    public static readonly int[] Triangles = {
        0, 1, 3, 1, 2, 3
    };
}
