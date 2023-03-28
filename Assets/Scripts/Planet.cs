using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] Material _mat;
    [SerializeField] private Vector3 _size = new Vector3(100,100,100);
    [SerializeField] private float _radius = 10f;
    [SerializeField] private ComputeShader _computeShader;
    private float _mass;
    private float _gravity;
    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _rotationalVelocity;

    private MC_Octree _octree;

    void Start()
    {
        _octree = gameObject.AddComponent<MC_Octree>();
        _octree.initiate(_position, _size, Helpers.getChunckRes(_size), _mat, this, _computeShader);
    }

    public float getRadius()
    {
        return _radius;
    }

    public Vector3 getPosition()
    {
        _position = transform.position;
        return _position;
    }

    public float calcVert(Vector3 pos, float cubeSize)
    {
        float dist = Vector3.Distance(pos, getPosition());
        float offset = getRadius() - dist;
        return offset;
        if ((offset > -cubeSize && offset < 0f) || (offset < cubeSize && offset > 0f))
        {
            Debug.Log("offset: " + offset/cubeSize + " cubeSize: " + cubeSize);
            return offset/cubeSize;
        }
        else {
            return 0f;
        }
    }
}
