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

    public event Action<MC_Octree> onOctreeCreated_ALL;
    public void OctreeCreated_ALL(MC_Octree octree)
    {
        if (onOctreeCreated_ALL != null)
        {
            onOctreeCreated_ALL(octree);
        }
    }

    public event Action<MC_Octree> onOctreeDestroyed_ALL;
    public void OctreeDestroyed_ALL(MC_Octree octree)
    {
        if (onOctreeDestroyed_ALL != null)
        {
            onOctreeDestroyed_ALL(octree);
        }
    }
}
