using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
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

    public static Vector3 multiplyVecs(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static float minChunkSize = 32f;
    
    public static Vector3Int getChunckRes (Vector3 size)
    {
        return new Vector3Int(16,16,16);
    }
}
