using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

struct Triangle {
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public Vector3 this [int i] 
    {
        get {
            switch (i) {
                case 0:
                    return a;
                case 1:
                    return b;
                default:
                    return c;
            }
        }
    }
}

public class MC_Octree : MonoBehaviour
{
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
    private MC_Vertex[,,] _vertices;
    private Vector4[] _vert;

    private bool isDivided = false;
    private MC_Octree[] _chunks = new MC_Octree[8];

    private ComputeShader _computeShader;
    private bool shader = true;

    public void initiate(Vector3 position, Vector3 size, Vector3Int resolution, Material mat, Planet planet, ComputeShader shader)
    {
        _position = position;
        _size = size;
        _resolution = resolution;
        _ratioVec = new Vector3(_size.x / _resolution.x, _size.y / _resolution.y, _size.z / _resolution.z);
        _vertices = new MC_Vertex[(_resolution.x+1),(_resolution.y+1),(_resolution.z+1)];
        _vert = new Vector4[(_resolution.x+1)*(_resolution.y+1)*(_resolution.z+1)];
        _mat = mat;
        _meshRenderer.material = _mat;
        _planet = planet;

        transform.position = _position;

        EventManager.current.OctreeCreated(this);

        _computeShader = shader;

        generateVertices();
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
                chunk.initiate((transform.position + Helpers.multiplyVecs(_size, Helpers.NeighbourTransforms[i])), _size/2, _resolution, _mat, _planet, _computeShader);
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

// TODO only add vertices that are on the surface
    private void generateVertices()
    {
        for (int x = 0; x < _resolution.x+1; x++)
        {
            for (int y = 0; y < _resolution.y+1; y++)
            {
                for (int z = 0; z < _resolution.z+1; z++)
                {
                    Vector3 tmpPos = new Vector3(_ratioVec.x*(x-_resolution.x/2), _ratioVec.y*(y-_resolution.y/2), _ratioVec.z*(z-_resolution.z/2));
                    _vertices[x,y,z] = new MC_Vertex(tmpPos, _planet.calcVert(tmpPos + transform.position));
                    _vert[x + y * (_resolution.x+1) + z * (_resolution.x+1) * (_resolution.y+1)] = new Vector4(tmpPos.x, tmpPos.y, tmpPos.z, _planet.calcVertF(tmpPos + transform.position));
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

    private void generateMesh() {
        _mesh.Clear();
        if (shader){
            marchCubeCS();
            return;
        }
        List<Vector3> vertices = new List<Vector3>();
        List<int> triang = new List<int>();
        for(int x = 0; x < _resolution.x; x++)
        {
            for(int y = 0; y < _resolution.y; y++)
            {
                for(int z = 0; z < _resolution.z; z++)
                {
                    marchCube(new Vector3Int(x, y, z), ref vertices, ref triang);
                }
            }
        }
        
        Vector3[] verticess = new Vector3[vertices.Count];
        vertices.CopyTo(verticess, 0);
        int[] triangles = new int[triang.Count];
        triang.CopyTo(triangles, 0);

        _mesh.vertices = verticess;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _mesh.Optimize();
        _meshCollider.sharedMesh = _mesh;
    }

// TODO always calc 4 cubes at once to share edgeverts (one chunc is atleas 2x2x2 cubes)
    private void marchCube(Vector3Int index, ref List<Vector3> vertices, ref List<int> triang)
    {
        MC_Vertex[] cube = new MC_Vertex[8];
        cube[0] = _vertices[index.x, index.y, index.z];
        cube[1] = _vertices[index.x+1, index.y, index.z];
        cube[2] = _vertices[index.x, index.y+1, index.z];
        cube[3] = _vertices[index.x+1, index.y+1, index.z];
        cube[4] = _vertices[index.x, index.y, index.z+1];
        cube[5] = _vertices[index.x+1, index.y, index.z+1];
        cube[6] = _vertices[index.x, index.y+1, index.z+1];
        cube[7] = _vertices[index.x+1, index.y+1, index.z+1];
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
            // Add vertices
            if (edgeIndex == -1){return;}
            if(!vertices.Contains(edgeVertecies[edgeIndex]))
            {
                vertices.Add(edgeVertecies[edgeIndex]); // + transform.position
            }
            // Add triangles
            for(int i = 0; i < vertices.Count; i++) 
            {
                if(vertices[i] == edgeVertecies[edgeIndex]) // + transform.position
                {
                    triang.Add(i);
                    break;
                }
            }
        }
    }

    private void marchCubeCS()
    {
        // _vert = new Vector4[8];
        // for(int x = 0; x < 2; x++)
        // {
        //     for(int y = 0; y < 2; y++)
        //     {
        //         for(int z = 0; z < 2; z++)
        //         {
        //             _vert[z * 2 * 2 + y * 2 + x] = new Vector4(x, y, z, 0);
        //         }
        //     }
        // }
        // _vert[0].w = 1;
        // _vert = new Vector4[] {
        //     new Vector4(0,0,0,1),
        //     new Vector4(10,0,0,0),
        //     new Vector4(0,10,0,0),
        //     new Vector4(10,10,0,0),
        //     new Vector4(0,0,10,0),
        //     new Vector4(10,0,10,0),
        //     new Vector4(0,10,10,0),
        //     new Vector4(10,10,10,0)
        // };
        ComputeBuffer triangleBuffer = new ComputeBuffer(_vert.Length*5, sizeof (float)*3*3, ComputeBufferType.Append);
        ComputeBuffer pointsBuffer = new ComputeBuffer(_vert.Length, sizeof (float) * 4);
        ComputeBuffer triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);
        int threads = Mathf.CeilToInt ((_resolution.x+1) / (float) 8);
        pointsBuffer.SetData(_vert);

        triangleBuffer.SetCounterValue(0);
        _computeShader.SetBuffer(0, "points", pointsBuffer);
        _computeShader.SetBuffer(0, "triangles", triangleBuffer);
        _computeShader.SetInt("resolution", _resolution.x+1);
        _computeShader.Dispatch(0, threads,threads,threads);

        ComputeBuffer.CopyCount (triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData (triCountArray);
        int numTris = triCountArray[0];
        //Debug.Log("numTris: " + numTris);

        Triangle[] trisCS = new Triangle[numTris];
        triangleBuffer.GetData (trisCS, 0, 0, numTris);

        Vector3[] verts = new Vector3[numTris * 3];
        int[] tris = new int[numTris * 3];

        for (int i = 0; i < numTris; i++) {
            for (int j = 0; j < 3; j++) {
                tris[i * 3 + j] = i * 3 + j;
                verts[i * 3 + j] = trisCS[i][j];
            }
        }

        _mesh.vertices = verts;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _mesh.Optimize();
        _meshCollider.sharedMesh = _mesh;

        triangleBuffer.Release ();
        pointsBuffer.Release ();
        triCountBuffer.Release ();
    }

    void OnDestroy()
    {
        EventManager.current.OctreeDestroyed(this);
    }

}
