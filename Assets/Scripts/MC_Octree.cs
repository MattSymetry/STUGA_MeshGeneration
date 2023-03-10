using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MC_Octree : MonoBehaviour
{
    [SerializeField] public bool _Debug = false;
    [SerializeField] public Vector3Int _index = Vector3Int.zero;
    [SerializeField] public bool _onSur = true;
    private Material _mat;
    private Planet _planet;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;
    
    private Mesh _mesh;
    private Vector3 _position;
    private Vector3 _size;
    private Vector3Int _resolution;
    private Vector3 _ratioVec;
    private MC_Vertex[,,] _vertecies;

    private bool isDivided = false;
    private MC_Octree[] _chunks = new MC_Octree[8];

    public void initiate(Vector3 position, Vector3 size, Vector3Int resolution, Material mat, Planet planet)
    {
        _position = position;
        _size = size;
        _resolution = resolution;
        _ratioVec = new Vector3(_size.x / _resolution.x, _size.y / _resolution.y, _size.z / _resolution.z);
        _vertecies = new MC_Vertex[(_resolution.x+1),(_resolution.y+1),(_resolution.z+1)];
        _mat = mat;
        _meshRenderer.material = _mat;
        _planet = planet;

        transform.position = _position;

        EventManager.current.OctreeCreated(this);

        generateVertecies();
        generateMesh();
    }

    public void divide()
    {
        if(!isDivided)
        {
            for (int i = 0; i < 8; i++) {
                GameObject chunkObj = new GameObject("Chunk_"+i);
                chunkObj.transform.parent = transform;
                MC_Octree chunk = chunkObj.AddComponent<MC_Octree>();
                chunk.initiate((transform.position + Helpers.multiplyVecs(_size, Helpers.NeighbourTransforms[i])), _size/2, _resolution, _mat, _planet);
                _chunks[i] = chunk;
            }
            isDivided = true;
            _meshRenderer.enabled = false;
            _meshCollider.enabled = false;
        }
    }

    public void merge()
    {
        if(isDivided)
        {
            for (int i = 0; i < 8; i++) {
                Destroy(_chunks[i].gameObject);
            }
            isDivided = false;
            _meshRenderer.enabled = true;
            _meshCollider.enabled = true;
        }
    }

    private void Awake()
    {
        _meshFilter = transform.gameObject.AddComponent<MeshFilter>();
        _meshRenderer = transform.gameObject.AddComponent<MeshRenderer>();
        _meshCollider = transform.gameObject.AddComponent<MeshCollider>();
       
        _mesh = new Mesh {
			name = "Procedural Mesh"
		};
        _meshFilter.mesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
    }

// TODO only add vertecies that are on the surface
    private void generateVertecies()
    {
        for (int x = 0; x < _resolution.x+1; x++)
        {
            for (int y = 0; y < _resolution.y+1; y++)
            {
                for (int z = 0; z < _resolution.z+1; z++)
                {
                    Vector3 tmpPos = new Vector3(_ratioVec.x*(x-_resolution.x/2), _ratioVec.y*(y-_resolution.y/2), _ratioVec.z*(z-_resolution.z/2));
                    _vertecies[x,y,z] = new MC_Vertex(tmpPos, _planet.calcVert(tmpPos + transform.position));
                }
            }
        }
    }

    public bool getIsDivided()
    {
        return isDivided;
    }

    public float getSize()
    {
        return _size.x;
    }

    public void setVertexIsOnSurface(int x = -1, int y = -1, int z = -1, bool isOnSurface = true)
    {
        Vector3Int index = new Vector3Int(x, y, z);
        if(index.x == -1)
        {
            index = _index;
            isOnSurface = _onSur;
        }
        _vertecies[index.x, index.y, index.z].SetIsOnSurface(isOnSurface);
        generateMesh();
    }

    private void generateMesh() {
        _mesh.Clear();
        List<Vector3> vertecies = new List<Vector3>();
        List<int> triang = new List<int>();
        for(int x = 0; x < _resolution.x; x++)
        {
            for(int y = 0; y < _resolution.y; y++)
            {
                for(int z = 0; z < _resolution.z; z++)
                {
                    marchCube(new Vector3Int(x, y, z), ref vertecies, ref triang);
                }
            }
        }
        
        Vector3[] vertices = new Vector3[vertecies.Count];
        vertecies.CopyTo(vertices, 0);
        int[] triangles = new int[triang.Count];
        triang.CopyTo(triangles, 0);

        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _mesh.Optimize();
        _meshCollider.sharedMesh = _mesh;
    }

// TODO always calc 4 cubes at once to share edgeverts (one chunc is atleas 2x2x2 cubes)
    private void marchCube(Vector3Int index, ref List<Vector3> vertecies, ref List<int> triang)
    {
        MC_Vertex[] cube = new MC_Vertex[8];
        cube[0] = _vertecies[index.x, index.y, index.z];
        cube[1] = _vertecies[index.x+1, index.y, index.z];
        cube[2] = _vertecies[index.x, index.y+1, index.z];
        cube[3] = _vertecies[index.x+1, index.y+1, index.z];
        cube[4] = _vertecies[index.x, index.y, index.z+1];
        cube[5] = _vertecies[index.x+1, index.y, index.z+1];
        cube[6] = _vertecies[index.x, index.y+1, index.z+1];
        cube[7] = _vertecies[index.x+1, index.y+1, index.z+1];
        int cubeIndex = 0;

        // Calculate cube index
        if(cube[0].GetIsOnSurface()){cubeIndex += 1;}
        if(cube[1].GetIsOnSurface()){cubeIndex += 2;}
        if(cube[2].GetIsOnSurface()){cubeIndex += 4;}
        if(cube[3].GetIsOnSurface()){cubeIndex += 8;}
        if(cube[4].GetIsOnSurface()){cubeIndex += 16;}
        if(cube[5].GetIsOnSurface()){cubeIndex += 32;}
        if(cube[6].GetIsOnSurface()){cubeIndex += 64;}
        if(cube[7].GetIsOnSurface()){cubeIndex += 128;}

        // Cube is entirely in/out of the surface
        if(cubeIndex == 0 || cubeIndex == 255)
        {
            return;
        }
      
        // Find the vertices where the surface intersects the cube
        List<Vector3> edgeVertecies = new List<Vector3>();
        edgeVertecies.Add((cube[0].GetPosition() + cube[1].GetPosition()) / 2);
        edgeVertecies.Add((cube[1].GetPosition() + cube[3].GetPosition()) / 2);
        edgeVertecies.Add((cube[3].GetPosition() + cube[2].GetPosition()) / 2);
        edgeVertecies.Add((cube[2].GetPosition() + cube[0].GetPosition()) / 2);
        edgeVertecies.Add((cube[4].GetPosition() + cube[5].GetPosition()) / 2);
        edgeVertecies.Add((cube[5].GetPosition() + cube[7].GetPosition()) / 2);
        edgeVertecies.Add((cube[7].GetPosition() + cube[6].GetPosition()) / 2);
        edgeVertecies.Add((cube[6].GetPosition() + cube[4].GetPosition()) / 2);
        edgeVertecies.Add((cube[0].GetPosition() + cube[4].GetPosition()) / 2);
        edgeVertecies.Add((cube[1].GetPosition() + cube[5].GetPosition()) / 2);
        edgeVertecies.Add((cube[3].GetPosition() + cube[7].GetPosition()) / 2);
        edgeVertecies.Add((cube[2].GetPosition() + cube[6].GetPosition()) / 2);

        int[] triTable = MC_Edges.triangleTable[cubeIndex];
        foreach (int edgeIndex in triTable)
        {
            // Add vertecies
            if (edgeIndex == -1){return;}
            if(!vertecies.Contains(edgeVertecies[edgeIndex]))
            {
                vertecies.Add(edgeVertecies[edgeIndex]); // + transform.position
            }
            // Add triangles
            for(int i = 0; i < vertecies.Count; i++) 
            {
                if(vertecies[i] == edgeVertecies[edgeIndex]) // + transform.position
                {
                    triang.Add(i);
                    break;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if(!_Debug || _vertecies == null){return;}
        foreach(MC_Vertex vertex in _vertecies)
        {
            Gizmos.color = Color.red;
            if(vertex.GetIsOnSurface())
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawSphere(vertex.GetPosition() + transform.position, (float)0.1);
        }
    }

    void OnDestroy()
    {
        EventManager.current.OctreeDestroyed(this);
    }

}
