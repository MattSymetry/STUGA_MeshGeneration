using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;

struct Vertex {
    public Vector3 position;
    public Vector3 normal;
    public int2 id;
}

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
    [SerializeField]private Vector3 _position;
    private Vector3 _size;
    private Vector3Int _resolution;

    private bool isDivided = false;
    private MC_Octree[] _chunks = new MC_Octree[8];
    private int hirarchyLevel = 0;

    private ComputeShader _computeShader;
    private int threadCount = 8;
    private bool useFlatShading = false;

    public void initiate(Vector3 position, Vector3 size, Vector3Int resolution, Material mat, Planet planet, ComputeShader shader, int hirarchyLevel = 0)
    {
        _position = position;
        _size = size;
        _resolution = resolution;
        _mat = mat;
        _meshRenderer.material = _mat;
        _planet = planet;
        this.hirarchyLevel = hirarchyLevel;

        //transform.position = _position;
        transform.localPosition = Vector3.zero;

        EventManager.current.OctreeCreated(this);

        _computeShader = shader;

        generateMesh();
    }

    public void divide()
    {
        if(!isDivided && ((_size.x/2) / _resolution.x) >= 1)
        {
            for (int i = 0; i < 8; i++) {
                GameObject chunkObj = new GameObject("Chunk_"+i);
                chunkObj.transform.parent = transform;
                MC_Octree chunk = chunkObj.AddComponent<MC_Octree>();
                chunk.initiate((transform.position + _position + Helpers.multiplyVecs(_size, Helpers.NeighbourTransforms[i])), _size/2, _resolution, _mat, _planet, _computeShader, hirarchyLevel+1);
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

    private async void marchCubes()
    {
        int numPoints = _resolution.x * _resolution.y * _resolution.z;
		int numVoxelsPerAxis = _resolution.x - 1;
		int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
		int maxTriangleCount = numVoxels * 5;
		int maxVertexCount = maxTriangleCount * 3;

        ComputeBuffer triangleBuffer = new ComputeBuffer(maxVertexCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vertex)), ComputeBufferType.Append);
        ComputeBuffer triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);
        Vertex[] vertexDataArray = new Vertex[maxVertexCount];
        int threads = Mathf.CeilToInt ((_resolution.x+1) / (float) threadCount);

        //Debug.Log("stepSize : "+(_size.x/(_resolution.x))+" pos: "+_position+" size: "+_size);

        _computeShader.SetTexture(0, "sampleTexture", _planet.getTexture());
        _computeShader.SetInt("textureSize", _planet.getTextureSize());
		_computeShader.SetInt("resolution", _resolution.x+1);
        _computeShader.SetFloat("stepSize", (_size.x/(_resolution.x)));
		triangleBuffer.SetCounterValue(0);
		_computeShader.SetBuffer(0, "triangles", triangleBuffer);
		_computeShader.SetVector("chunkPos", _position);
        _computeShader.SetVector("chunkSize", _size);
        _computeShader.SetInt("hirarchyLevel", hirarchyLevel);

		_computeShader.Dispatch(0, threads,threads,threads);


        int[] vertexCountData = new int[1];
		triCountBuffer.SetData(vertexCountData);
		ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);

		triCountBuffer.GetData(vertexCountData);

		int numVertices = vertexCountData[0] * 3;

		triangleBuffer.GetData(vertexDataArray, 0, 0, numVertices);

        // Fetch data
        Dictionary<int2, int> vertexIndexMap = new Dictionary<int2, int>();
		List<Vector3> processedVertices = new List<Vector3>();
		List<Vector3> processedNormals = new List<Vector3>();
		List<int> processedTriangles = new List<int>();

        int triangleIndex = 0;

        for (int i = 0; i < numVertices; i++)
		{
			Vertex data = vertexDataArray[i];

			int sharedVertexIndex;
			if (!useFlatShading && vertexIndexMap.TryGetValue(data.id, out sharedVertexIndex))
			{
				processedTriangles.Add(sharedVertexIndex);
			}
			else
			{
				if (!useFlatShading)
				{
					vertexIndexMap.Add(data.id, triangleIndex);
				}
				processedVertices.Add(data.position);
				processedNormals.Add(data.normal);
				processedTriangles.Add(triangleIndex);
				triangleIndex++;
			}
		}

        _mesh.SetVertices(processedVertices);
		_mesh.SetTriangles(processedTriangles, 0, true);

		if (useFlatShading)
		{
			_mesh.RecalculateNormals();
		}
		else
		{
			_mesh.SetNormals(processedNormals);
		}
        _mesh.RecalculateBounds();
        _mesh.Optimize();
        _meshCollider.sharedMesh = _mesh;

        triangleBuffer.Release ();
        triCountBuffer.Release ();
    }

    public Vector3 getAbsPosition()
    {
        return transform.position + _position;
    }

    void OnDestroy()
    {
        EventManager.current.OctreeDestroyed(this);
    }

}