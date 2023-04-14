using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawSphere : MonoBehaviour
{
    private List<MC_Octree> _octreeToRefresh = new List<MC_Octree>();

    private void OnCollisionEnter(Collision other) 
    {
        Debug.Log("Collision");
        _octreeToRefresh.Add(other.gameObject.GetComponent<MC_Octree>());
    }

    private void OnCollisionExit(Collision other) 
    {
        _octreeToRefresh.Remove(other.gameObject.GetComponent<MC_Octree>());
    }

    public List<MC_Octree> getOctrees() 
    {
        return _octreeToRefresh;
    }

}
