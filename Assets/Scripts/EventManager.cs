using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static EventManager current;

    private void Awake()
    {
        current = this;
    }

    public event Action<MC_Octree> onOctreeCreated;
    public void OctreeCreated(MC_Octree octree)
    {
        if (onOctreeCreated != null)
        {
            onOctreeCreated(octree);
        }
    }

    public event Action<MC_Octree> onOctreeDestroyed;
    public void OctreeDestroyed(MC_Octree octree)
    {
        if (onOctreeDestroyed != null)
        {
            onOctreeDestroyed(octree);
        }
    }
}
