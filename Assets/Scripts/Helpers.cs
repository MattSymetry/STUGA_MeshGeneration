using System.Collections;
using System.Collections.Generic;
using static UnityEngine.Mathf;
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

    public static float minChunkSize = 16;
    
    public static Vector3Int getChunckRes (Vector3 size)
    {
        return new Vector3Int(16,16,16);
    }

    public static bool SphereIntersectsBox(Vector3 sphereCentre, float sphereRadius, Vector3 boxCentre, Vector3 boxSize)
	{
		float closestX = Clamp(sphereCentre.x, boxCentre.x - boxSize.x / 2, boxCentre.x + boxSize.x / 2);
		float closestY = Clamp(sphereCentre.y, boxCentre.y - boxSize.y / 2, boxCentre.y + boxSize.y / 2);
		float closestZ = Clamp(sphereCentre.z, boxCentre.z - boxSize.z / 2, boxCentre.z + boxSize.z / 2);

		float dx = closestX - sphereCentre.x;
		float dy = closestY - sphereCentre.y;
		float dz = closestZ - sphereCentre.z;

		float sqrDstToBox = dx * dx + dy * dy + dz * dz;
		return sqrDstToBox < sphereRadius * sphereRadius;
	}
}
