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
        // foreach (MC_Octree octree in _octrees)
        // {
        //     if (!octree.gameObject.activeInHierarchy) continue;
        //     currOctreeSize = octree.getSize();
        //     currOctreeDistance = Vector3.Distance(octree.getAbsPosition(), transform.position);
        //     if (currOctreeSize > Helpers.minChunkSize && currOctreeDistance < currOctreeSize && !octree.getIsDivided())
        //     {
        //         octree.divide();
        //         break;
        //     }
        //     else if (currOctreeDistance > currOctreeSize*4 && octree.getIsDivided())
        //     {
        //         octree.merge();
        //         break;
        //     }
        // }

        checkOctreeJob job = new checkOctreeJob{
            myoctrees = new NativeArray<MC_Octree_Struct>(_octrees.ToArray(), Allocator.Persistent),
            cameraPosition = transform.position
        };
        job.Schedule().Complete();
    }

    [BurstCompile(CompileSynchronously = true)]
    private struct checkOctreeJob : IJob
    {
        public NativeArray<MC_Octree_Struct> myoctrees;
        public Vector3 cameraPosition;

        public void Execute()
        {
            foreach (MC_Octree octree in octrees)
            {
                if (!octree.gameObject.activeInHierarchy) continue;
                float currOctreeSize = octree.getSize();
                float currOctreeDistance = Vector3.Distance(octree.getAbsPosition(), cameraPosition);
                if (currOctreeSize > Helpers.minChunkSize && currOctreeDistance < currOctreeSize && !octree.getIsDivided())
                {
                    octree.divide();
                    break;
                }
                else if (currOctreeDistance > currOctreeSize * 4 && octree.getIsDivided())
                {
                    octree.merge();
                    break;
                }
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
