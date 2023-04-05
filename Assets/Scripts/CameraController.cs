using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private InputController _inputController;
    private Camera _camera;
    private List<Planet> _planets = new List<Planet>();
    private int _currentPlanet = 0;

[SerializeField]
    private float _cameraDistancePlanet = 0f;
    private float _cameraDistanceCenter = 0f;
    private float _minCameraDistance = 50f;
    private float _maxCameraDistance = 1000f;

    private float _zoomFactor = 2f;
    private float _dragFactor = 200f;

    private Vector2 _mouseDelta = Vector2.zero;
    private bool _mouseClicked = false;

    void Awake()
    {
        _inputController = new InputController();
        _inputController.FocusMode.Zoom.performed += ctx => zoom(ctx.ReadValue<float>());
        _inputController.FocusMode.MouseDelta.performed += ctx => mouseDelta(ctx.ReadValue<Vector2>());
        _inputController.FocusMode.LeftClick.performed += ctx => mouseClick(true);
        _inputController.FocusMode.LeftClick.canceled += ctx => mouseClick(false);
    }
    
    void Start()
    {
        _camera = GetComponent<Camera>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Planet"))
        {
            _planets.Add(obj.GetComponent<Planet>());
        }
        focusPlanet(_currentPlanet);
    }

    
    void Update()
    {
        
    }

    void zoom(float zoom)
    {
        if (zoom < 0 && _cameraDistancePlanet-zoom/_zoomFactor > _maxCameraDistance) zoom = (_maxCameraDistance-_cameraDistancePlanet)*-_zoomFactor;
        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, zoom/_zoomFactor+_minCameraDistance))
        {
            if (zoom > 0) zoom = (hit.distance - _minCameraDistance)*_zoomFactor;
        }
        _camera.transform.position += _camera.transform.forward * zoom/_zoomFactor;
        updateCameraDistance();
    }

    void mouseDelta(Vector2 delta)
    {
        if (!_mouseClicked) return;
        _mouseDelta = delta*_cameraDistancePlanet/_dragFactor;
        if (Vector3.Dot(_camera.transform.up, Vector3.down) > 0) _mouseDelta.x *= -1;
        _camera.transform.RotateAround(_planets[_currentPlanet].transform.position, Vector3.up, _mouseDelta.x);
        _camera.transform.RotateAround(_planets[_currentPlanet].transform.position, _camera.transform.right, -_mouseDelta.y);
        updateCameraDistance();
    }

    void mouseClick(bool click) 
    {
        _mouseClicked = click;
    }

    void focusPlanet(int planetIndex)
    {
        _currentPlanet = planetIndex;
        _camera.transform.position = _planets[_currentPlanet].transform.position + new Vector3(_planets[_currentPlanet].getRadius()*4f,0,0);
        _camera.transform.parent = _planets[_currentPlanet].transform;
        _camera.transform.LookAt(_planets[_currentPlanet].transform.position);
        _maxCameraDistance = _planets[_currentPlanet].getTextureSize()*2;
        updateCameraDistance();
    }

    void updateCameraDistance()
    {
        _cameraDistancePlanet = Vector3.Distance(_camera.transform.position, _planets[_currentPlanet].transform.position);
        _cameraDistanceCenter = Vector3.Distance(_camera.transform.position, Vector3.zero);
    }

    void OnEnable()
    {
        _inputController.Enable();
    }

    void OnDisable()
    {
        _inputController.Disable();
    }
}
