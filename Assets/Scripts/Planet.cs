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
    private float _mass;
    private float _gravity;
    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _rotationalVelocity;

    private float noiseHeightMultiplier = 20f;
    private float noiseScale = 1f;

    private MC_Octree _octree;

    [SerializeField] private RenderTexture _renderTexture;
    private int _textureResolution;

    private int threadCount = 8;

    private string PlanetName = "Planet";

    void Start()
    {
        PlanetName += " "+transform.position.x.ToString();
        _position = transform.position;
        _textureResolution = _size.x;
        generateRenderTexture(_textureResolution);
        _mat.SetTexture("_Texture3D", _renderTexture);
        _mat.SetVector("_MinMaxTextureSize", new Vector2(-_size.x/2, _size.x/2));
        GameObject chunkObj = ObjectPool.SharedInstance.GetPooledObject();
        if (chunkObj != null) 
        {
            chunkObj.SetActive(true);
        }
        chunkObj.transform.parent = transform;
        _octree = chunkObj.GetComponent<MC_Octree>();            
        _octree.initiate(_position, _size, Helpers.getChunckRes(_size), _mat, this, _computeShader, 1);
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

    public void editTexture(Vector3 position, float radius, float strength)
    {
        int textureSize = _renderTexture.width;
        position = position + new Vector3(textureSize/2, textureSize/2, textureSize/2);
        Debug.Log(position);
        _computeShaderEditTexture.SetTexture(0, "renderTexture", _renderTexture);
        _computeShaderEditTexture.SetInt("textureSize", textureSize);
        _computeShaderEditTexture.SetFloat("radius", radius);
        _computeShaderEditTexture.SetFloat("strength", strength);
        _computeShaderEditTexture.SetVector("position", position);
        _computeShaderEditTexture.Dispatch(0, _textureResolution/threadCount, _textureResolution/threadCount, _textureResolution/threadCount);
        Debug.Log("END editTexture");
    }
}
