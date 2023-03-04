using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_Vertex
{
    private Vector3 _position;
    private bool _isOnSurface;

    public MC_Vertex(Vector3 position, bool isOnSurface)
    {
        _position = position;
        _isOnSurface = isOnSurface;
    }

    public Vector3 GetPosition()
    {
        return _position;
    }

    public bool GetIsOnSurface()
    {
        return _isOnSurface;
    }

    public void SetIsOnSurface(bool isOnSurface)
    {
        _isOnSurface = isOnSurface;
    }
}
