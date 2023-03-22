using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
// blb, brb, tlb, trb, blf, brf, tlf, trf
    public static Vector3[] NeighbourTransforms = new Vector3[]
    {
        new Vector3((float)-0.25, (float)-0.25, (float)-0.25),
        new Vector3((float)0.25, (float)-0.25, (float)-0.25),
        new Vector3((float)-0.25, (float)0.25, (float)-0.25),
        new Vector3((float)0.25,(float) 0.25, (float)-0.25),
        new Vector3((float)-0.25, (float)-0.25, (float)0.25),
        new Vector3((float)0.25, (float)-0.25, (float)0.25),
        new Vector3((float)-0.25, (float)0.25, (float)0.25),
        new Vector3((float)0.25,(float)0.25, (float)0.25)
    };

// bottom, top, left, right, front, back
    public static int[,] Neighbours = new int[8,6]
    {
        {8, 2, 10, 1, 4, 13}, // 0
        {8, 3, 0, 11, 5, 13}, // 1
        {0, 9, 10, 3, 6, 13}, // 2
        {1, 9, 2, 11, 7, 13}, // 3
        {8, 6, 10, 5, 12, 0}, // 4
        {8, 7, 4, 11, 12, 1}, // 5
        {4, 9, 10, 7, 12, 2}, // 6
        {5, 9, 6, 11, 12, 3}  // 7
    };

    public static Vector3 multiplyVecs(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static float minChunkSize = 4;
    
    public static Vector3Int getChunckRes (Vector3 size)
    {
        return new Vector3Int((int)(size.x / 4), (int)(size.y / 4), (int)(size.z / 4));
    }
}
