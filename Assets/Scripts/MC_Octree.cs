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
    private Vector4[] _vert;

    private bool isDivided = false;
    private MC_Octree[] _chunks = new MC_Octree[8];

    private ComputeShader _computeShader;
    private int threadCount = 8;

    public void initiate(Vector3 position, Vector3 size, Vector3Int resolution, Material mat, Planet planet, ComputeShader shader)
    {
        _position = position;
        _size = size;
        _resolution = resolution;
        _ratioVec = new Vector3(_size.x / _resolution.x, _size.y / _resolution.y, _size.z / _resolution.z);
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

        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
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
                    _vert[x + y * (_resolution.x+1) + z * (_resolution.x+1) * (_resolution.y+1)] = new Vector4(tmpPos.x, tmpPos.y, tmpPos.z, _planet.calcVert(tmpPos + transform.position, _size.x/_resolution.x));
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

        var watch = System.Diagnostics.Stopwatch.StartNew();
        marchCubes();
        watch.Stop();
        Debug.Log("CS: "+watch.ElapsedMilliseconds);
    }

    private void marchCubes()
    {
        ComputeBuffer triangleBuffer = new ComputeBuffer(_vert.Length*5, sizeof (float)*3*3, ComputeBufferType.Append);
        ComputeBuffer pointsBuffer = new ComputeBuffer(_vert.Length, sizeof (float) * 4);
        ComputeBuffer triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);
        int threads = Mathf.CeilToInt ((_resolution.x+1) / (float) threadCount);
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