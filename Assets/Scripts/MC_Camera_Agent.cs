using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MC_Camera_Agent : MonoBehaviour
{
    private List<MC_Octree> _octrees = new List<MC_Octree>();
    private float currOctreeSize = 0f;
    private float currOctreeDistance = 0f;


    void Start()
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
            currOctreeSize = octree.getSize();
            currOctreeDistance = Vector3.Distance(octree.getAbsPosition(), transform.position);
            if (currOctreeSize/2 > Helpers.minChunkSize && currOctreeDistance < currOctreeSize && !octree.getIsDivided())
            {
                octree.divide();
                break;
            }
            else if (currOctreeDistance >  currOctreeSize && octree.getIsDivided())
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
        _octrees.Remove(octree);
    }
}
