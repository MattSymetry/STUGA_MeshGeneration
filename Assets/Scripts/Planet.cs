using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] bool _Debug = true;
    [SerializeField] Material _mat;
    [SerializeField] private Vector3 _size = new Vector3(100,100,100);
    [SerializeField] private float _radius = 10f;
    private float _mass;
    private float _gravity;
    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _rotationalVelocity;

    private MC_Octree _octree;

    void Start()
    {
        _octree = gameObject.AddComponent<MC_Octree>();
        _octree.initiate(_position, _size, Helpers.getChunckRes(_size), _mat, this);
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

    public bool calcVert(Vector3 pos)
    {
        float dist = Vector3.Distance(pos, getPosition());
        return dist < getRadius();
    }
}
