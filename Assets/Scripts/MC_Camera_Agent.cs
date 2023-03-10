using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MC_Camera_Agent : MonoBehaviour
{
    [SerializeField] private float _devideDistance = 20f;
    private List<MC_Octree> _octrees = new List<MC_Octree>();


    void Start()
    {
        EventManager.current.onOctreeCreated += onOctreeCreated;
        EventManager.current.onOctreeDestroyed += onOctreeDestroyed;
    }

    void Update()
    {
        foreach (MC_Octree octree in _octrees)
        {
            if (Vector3.Distance(octree.transform.position, transform.position) < octree.getSize() && !octree.getIsDivided())
            {
                Debug.Log("Divide");
                octree.divide();
                break;
            }
            else if (Vector3.Distance(octree.transform.position, transform.position) > octree.getSize() && octree.getIsDivided())
            {
                Debug.Log("Merge");
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
