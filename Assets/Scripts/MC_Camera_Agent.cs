using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MC_Camera_Agent : MonoBehaviour
{
    private List<MC_Octree> _octrees = new List<MC_Octree>();
    private float currOctreeSize = 0f;
    private float currOctreeDistance = 0f;


    void Awake()
    {
        EventManager.current.onOctreeCreated += onOctreeCreated;
        EventManager.current.onOctreeDestroyed += onOctreeDestroyed;
    }

    void Update()
    {
        checkOctrees();
    }

    private void checkOctrees() 
    {
        foreach (MC_Octree octree in _octrees)
        {
            if (!octree.gameObject.activeSelf) continue;
            currOctreeSize = octree.getSize();
            currOctreeDistance = Vector3.Distance(octree.getAbsPosition(), transform.position);
            if (currOctreeSize > Helpers.minChunkSize && currOctreeDistance < currOctreeSize*2 && !octree.getIsDivided())
            {
                octree.divide();
                break;
            }
            else if (currOctreeDistance > currOctreeSize*2 && octree.getIsDivided())
            {
                octree.merge();
                break;
            }
        }
    }

    private void onOctreeCreated(MC_Octree octree)
    {
        _octrees.Add(octree);
    }

    private void onOctreeDestroyed(MC_Octree octree)
    {
        //_octrees.Remove(octree);
    }
}
