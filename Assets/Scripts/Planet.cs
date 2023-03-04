using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] bool _Debug = true;
    [SerializeField] Material _mat;
    private float _mass;
    private float _radius;
    private float _gravity;
    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _rotationalVelocity;

    private MC_Chunk[] _chunks;

    void Start()
    {
        _chunks = new MC_Chunk[1];
        GameObject gO = new GameObject("Chunk");
        _chunks[0] = gO.AddComponent<MC_Chunk>();
        _chunks[0].initiate(new Vector3(0, 0, 0), new Vector3(2, 2, 2), new Vector3Int(2, 2, 2), _mat);
    }

    void Update()
    {

    }
}
