using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] Material _mat;
    [SerializeField] private Vector3Int _size = new Vector3Int(100,100,100);
    [SerializeField] private float _radius = 10f;
    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private ComputeShader _computeShaderTexture;
    [SerializeField] private ComputeShader _computeShaderEditTexture;
    [SerializeField] private ComputeShader _computeShaderEditTextureColor;
    [SerializeField] private float _seed = 0f;
    private float _mass;
    private float _gravity;
    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _rotationalVelocity;

    private GameObject chunkObj;

    //private float noiseHeightMultiplier = 20f;
    //private float noiseScale = 1f;

    private MC_Octree _octree;

    [SerializeField] private RenderTexture _renderTexture;
    private int _textureResolution;

    private int threadCount = 8;

    private string PlanetName = "Planet";


    void Start()
    {
        _radius = Random.Range(130f, 260f);
        _seed = Random.Range(-10000.0f, 10000.0f);
        //_maxLOD = Mathf.floor(_size / Helpers.minChunkSize);
        PlanetName += " "+transform.position.x.ToString();
        _position = transform.position;
        _textureResolution = _size.x;
        generateRenderTexture(_textureResolution);
        _mat = new Material(_mat);
        _mat.SetTexture("_Texture3D", _renderTexture);
        _mat.SetVector("_MinMaxTextureSize", new Vector2(-_size.x/2, _size.x/2));
         chunkObj = ObjectPool.SharedInstance.GetPooledObject();
        if (chunkObj != null) 
        {
            chunkObj.transform.parent = transform;
            chunkObj.SetActive(true);
        }
        _octree = chunkObj.GetComponent<MC_Octree>();            
        _octree.initiate(_position, _size, Helpers.getChunckRes(_size), _mat, this, _computeShader, 1, true);
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

    private void generateRenderTexture(int resolution)
    {
        Create3DTexture(ref _renderTexture, resolution, "Raw Density Texture");

        int textureSize = _renderTexture.width;

        _computeShaderTexture.SetTexture(0, "renderTexture", _renderTexture);
		_computeShaderTexture.SetInt("textureSize", textureSize);
		_computeShaderTexture.SetFloat("planetSize", _radius);
        _computeShaderTexture.SetFloat("seed", _seed);
        _computeShaderTexture.SetFloat("noiseScale", Random.Range(0.01f, 0.03f));

        _computeShaderTexture.Dispatch(0, textureSize/threadCount, textureSize/threadCount, textureSize/threadCount);

		// _computeShaderTexture.SetFloat("noiseHeightMultiplier", noiseHeightMultiplier);
		// _computeShaderTexture.SetFloat("noiseScale", noiseScale);
    }

    public RenderTexture getTexture()
    {
        return _renderTexture;
    }

    public int getTextureSize() 
    {
        return _textureResolution;
    }

    void Create3DTexture(ref RenderTexture texture, int size, string name)
	{
		//
		var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16_SFloat;
		if (texture == null || !texture.IsCreated() || texture.width != size || texture.height != size || texture.volumeDepth != size || texture.graphicsFormat != format)
		{
			//Debug.Log ("Create tex: update noise: " + updateNoise);
			if (texture != null)
			{
				texture.Release();
			}
			const int numBitsInDepthBuffer = 0;
			texture = new RenderTexture(size, size, numBitsInDepthBuffer);
			texture.graphicsFormat = format;
			texture.volumeDepth = size;
			texture.enableRandomWrite = true;
			texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;


			texture.Create();
		}
		texture.wrapMode = TextureWrapMode.Repeat;
		texture.filterMode = FilterMode.Bilinear;
		texture.name = name;
	}

    public string getName()
    {
        return PlanetName;
    }

    public void editTexture(Vector3 position, float radius, float strength, float color)
    {
        int textureSize = _renderTexture.width;
        position = position + new Vector3(textureSize/2, textureSize/2, textureSize/2);
        _computeShaderEditTexture.SetTexture(0, "renderTexture", _renderTexture);
        _computeShaderEditTexture.SetInt("textureSize", textureSize);
        _computeShaderEditTexture.SetFloat("radius", radius);
        _computeShaderEditTexture.SetFloat("strength", strength);
        _computeShaderEditTexture.SetFloat("color", color);
        _computeShaderEditTexture.SetVector("position", position);
        _computeShaderEditTexture.Dispatch(0, _textureResolution/threadCount, _textureResolution/threadCount, _textureResolution/threadCount);
    }

    public void editTextureColor(Vector3 position, float radius, float color) {
        int textureSize = _renderTexture.width;
        position = position + new Vector3(textureSize/2, textureSize/2, textureSize/2);
        _computeShaderEditTextureColor.SetTexture(0, "renderTexture", _renderTexture);
        _computeShaderEditTextureColor.SetInt("textureSize", textureSize);
        _computeShaderEditTextureColor.SetFloat("radius", radius);
        _computeShaderEditTextureColor.SetFloat("color", color);
        _computeShaderEditTextureColor.SetVector("position", position);
        _computeShaderEditTextureColor.Dispatch(0, _textureResolution/threadCount, _textureResolution/threadCount, _textureResolution/threadCount);
        _mat.SetTexture("_Texture3D", _renderTexture);
    }

    public Material getMaterial()
    {
        return _mat;
    }

    public void hide() {
        _renderTexture.Release();
        _seed = Random.Range(-10000.0f, 10000.0f);
        _radius = Random.Range(130f, 260f);
        generateRenderTexture(_textureResolution);
        chunkObj.GetComponent<MC_Octree>().redo();
        _mat.SetTexture("_Texture3D", _renderTexture);
    }
}
