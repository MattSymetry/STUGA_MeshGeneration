using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_Chunk : MonoBehaviour
{
    [SerializeField] public bool _Debug = true;
    [SerializeField] public Vector3Int _index = Vector3Int.zero;
    [SerializeField] public bool _onSur = true;
    private Material _mat;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;
    
    private Mesh _mesh;
    private Vector3 _position;
    private Vector3 _size;
    private Vector3Int _resolution;
    private Vector3 _ratioVec;
    private MC_Vertex[,,] _vertecies;

    public void initiate(Vector3 position, Vector3 size, Vector3Int resolution, Material mat)
    {
        _position = position;
        _size = size;
        _resolution = resolution;
        _ratioVec = new Vector3(_size.x / _resolution.x, _size.y / _resolution.y, _size.z / _resolution.z);
        _vertecies = new MC_Vertex[(_resolution.x+1),(_resolution.y+1),(_resolution.z+1)];
        _mat = mat;
         _meshRenderer.material = _mat;

        transform.localPosition = _position;

        generateVertecies();
        generateMesh();
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

    private void generateVertecies()
    {
        for (int x = 0; x < _resolution.x+1; x++)
        {
            for (int y = 0; y < _resolution.y+1; y++)
            {
                for (int z = 0; z < _resolution.z+1; z++)
                {
                    _vertecies[x,y,z] = new MC_Vertex(new Vector3(_ratioVec.x*x, _ratioVec.y*y, _ratioVec.z*z), false);
                }
            }
        }
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
        Dictionary<int, Vector3> vertecies = new Dictionary<int, Vector3>();
        Dictionary<int, int> triang = new Dictionary<int, int>();
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
        vertecies.Values.CopyTo(vertices, 0);
        int[] triangles = new int[triang.Count];
        triang.Values.CopyTo(triangles, 0);

        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _mesh.Optimize();
        _meshCollider.sharedMesh = _mesh;
    }

    private void marchCube(Vector3Int index, ref Dictionary<int, Vector3> vertecies, ref Dictionary<int, int> triang)
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
        Dictionary<int, Vector3> edgeVertecies = new Dictionary<int, Vector3>();
        edgeVertecies.Add(0, (cube[0].GetPosition() + cube[1].GetPosition()) / 2);
        edgeVertecies.Add(1, (cube[1].GetPosition() + cube[3].GetPosition()) / 2);
        edgeVertecies.Add(2, (cube[3].GetPosition() + cube[2].GetPosition()) / 2);
        edgeVertecies.Add(3, (cube[2].GetPosition() + cube[0].GetPosition()) / 2);
        edgeVertecies.Add(4, (cube[4].GetPosition() + cube[5].GetPosition()) / 2);
        edgeVertecies.Add(5, (cube[5].GetPosition() + cube[7].GetPosition()) / 2);
        edgeVertecies.Add(6, (cube[7].GetPosition() + cube[6].GetPosition()) / 2);
        edgeVertecies.Add(7, (cube[6].GetPosition() + cube[4].GetPosition()) / 2);
        edgeVertecies.Add(8, (cube[0].GetPosition() + cube[4].GetPosition()) / 2);
        edgeVertecies.Add(9, (cube[1].GetPosition() + cube[5].GetPosition()) / 2);
        edgeVertecies.Add(10, (cube[3].GetPosition() + cube[7].GetPosition()) / 2);
        edgeVertecies.Add(11, (cube[2].GetPosition() + cube[6].GetPosition()) / 2);

        int[] triTable = MC_Edges.triangleTable[cubeIndex];
        foreach (int edgeIndex in triTable)
        {
            // Add vertecies
            if (edgeIndex == -1){return;}
            if(!vertecies.ContainsValue(edgeVertecies[edgeIndex]))
            {
                vertecies.Add(vertecies.Keys.Count , edgeVertecies[edgeIndex] + transform.position);
            }
            // Add triangles
            foreach (int key in vertecies.Keys)
            {
                if (vertecies[key] == edgeVertecies[edgeIndex] + transform.position)
                {
                    triang.Add(triang.Keys.Count, key);
                    break;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if(!_Debug){return;}
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

}
