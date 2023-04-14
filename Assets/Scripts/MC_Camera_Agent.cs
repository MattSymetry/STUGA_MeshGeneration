using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System;

public class MC_Camera_Agent : MonoBehaviour
{
    private List<MC_Octree> _octrees = new List<MC_Octree>();
    private MC_Octree octree;
    private float currOctreeSize = 0f;
    private float currOctreeDistance = 0f;


    void Awake()
    {
        EventManager.current.onOctreeCreated_ALL += onOctreeCreated;
        EventManager.current.onOctreeDestroyed_ALL += onOctreeDestroyed;
    }

    void Update()
    {
        checkOctrees();
    }

    private void checkOctrees() 
    {
        for (int i = 0; i < _octrees.Count; i++)
        {
            octree = _octrees[i];
            if (!octree.gameObject.activeInHierarchy || !octree.hasMesh()) continue;
            currOctreeSize = octree.getSize();
            currOctreeDistance = Vector3.Distance(octree.getAbsPosition(), transform.position);
            if (currOctreeSize > Helpers.minChunkSize && currOctreeDistance < currOctreeSize*8 && !octree.getIsDivided())
            {
                octree.divide();
                break;
            }
            else if (currOctreeDistance > currOctreeSize*10 && octree.getIsDivided())
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
