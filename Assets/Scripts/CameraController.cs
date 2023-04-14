using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using TMPro;

enum CameraControllerState
{
    Orbit,
    Fly,
    Rise,
    Lower
}

public class CameraController : MonoBehaviour
{
    private InputController _inputController;
    private Camera _camera;
    private List<Planet> _planets = new List<Planet>();
    private int _currentPlanet = 0;

    private float _cameraDistancePlanet = 0f;
    private float _cameraDistanceCenter = 0f;
    private float _minCameraDistance = 50f;
    private float _maxCameraDistance = 1000f;

    private float _zoomFactor = 2f;
    private float _dragFactor = 200f;

    private Vector2 _mouseDelta = Vector2.zero;
    private bool _mouseClicked = false;
    private CameraControllerState _state = CameraControllerState.Orbit;

    private List<Button> _interactionButtons = new List<Button>();

    [SerializeField] private TMP_Text _planetName;
    [SerializeField] private Slider _sizeSlider;
    [SerializeField] private GameObject _drawSphere;
    private DrawSphere _drawSphereScript;
    private Vector2 _mousePosition = Vector2.zero;

    private List<MC_Octree> _octrees = new List<MC_Octree>();

    void Awake()
    {
        _inputController = new InputController();
        _inputController.FocusMode.Zoom.performed += ctx => zoom(ctx.ReadValue<float>());
        _inputController.FocusMode.MouseDelta.performed += ctx => mouseDelta(ctx.ReadValue<Vector2>());
        _inputController.FocusMode.LeftClick.performed += ctx => mouseClick(true);
        _inputController.FocusMode.LeftClick.canceled += ctx => mouseClick(false);
        _inputController.FocusMode.MousePosition.performed += ctx => mousePosition(ctx.ReadValue<Vector2>());

        EventManager.current.onOctreeCreated_ALL += onOctreeCreated_ALL;
        EventManager.current.onOctreeDestroyed_ALL += onOctreeDestroyed_ALL;
    }
    
    void Start()
    {
        _drawSphere.SetActive(false);
        _drawSphereScript = _drawSphere.GetComponent<DrawSphere>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("InteractionButton"))
        {
            _interactionButtons.Add(obj.GetComponent<Button>());
        }
        _camera = GetComponent<Camera>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Planet"))
        {
            _planets.Add(obj.GetComponent<Planet>());
        }
        focusPlanet(_currentPlanet);
    }

    private void onOctreeCreated_ALL(MC_Octree octree)
    {
        _octrees.Add(octree);
    }

    private void onOctreeDestroyed_ALL(MC_Octree octree)
    {
        _octrees.Remove(octree);
    }

    
    void Update()
    {
        _drawSphere.SetActive(false);
        if (_state == CameraControllerState.Rise || _state == CameraControllerState.Lower) {
            Ray ray = _camera.ScreenPointToRay(_mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                _drawSphere.transform.localScale = new Vector3(_sizeSlider.value, _sizeSlider.value, _sizeSlider.value);
                _drawSphere.SetActive(true);
                _drawSphere.transform.position = hit.point;
            }
        }
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
        if (_state == CameraControllerState.Orbit) {
            _mouseDelta = delta*_cameraDistancePlanet/_dragFactor;
            if (Vector3.Dot(_camera.transform.up, Vector3.down) > 0) _mouseDelta.x *= -1;
            _camera.transform.RotateAround(_planets[_currentPlanet].transform.position, Vector3.up, _mouseDelta.x);
            _camera.transform.RotateAround(_planets[_currentPlanet].transform.position, _camera.transform.right, -_mouseDelta.y);
            updateCameraDistance();
        }
        
    }

    void mousePosition(Vector2 pos)
    {
        _mousePosition = pos;
    }

    void mouseClick(bool click) 
    {
        _mouseClicked = click;
        if (!click) return;
        if (_state == CameraControllerState.Rise && _drawSphere.activeInHierarchy) editPlanet(_drawSphere.transform.position, -1f);
        if (_state == CameraControllerState.Lower && _drawSphere.activeInHierarchy) editPlanet(_drawSphere.transform.position, 1f);

    }

    void editPlanet(Vector3 pos, float strength)
    {
        pos = pos - _planets[_currentPlanet].getPosition();
        _planets[_currentPlanet].editTexture(pos, _sizeSlider.value, strength);
		for (int i = 0; i < _octrees.Count; i++)
		{
			MC_Octree octree = _octrees[i];
			if (Helpers.SphereIntersectsBox(_drawSphere.transform.position, _drawSphere.transform.localScale.x, octree.getAbsPosition(), Vector3.one * octree.getSize()))
			{
				octree.updateMesh();
			}
		}
    }

    void focusPlanet(int planetIndex)
    {
        _currentPlanet = planetIndex;
        _camera.transform.parent = _planets[_currentPlanet].transform;
        _camera.transform.position = _planets[_currentPlanet].getPosition() + new Vector3(_planets[_currentPlanet].getRadius()*4f,0,0);
        _camera.transform.LookAt(_planets[_currentPlanet].getPosition());
        _maxCameraDistance = _planets[_currentPlanet].getTextureSize()*2;
        updateCameraDistance();
        _planetName.SetText(_planets[_currentPlanet].getName());
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

    public void OnButtonCam(Button btn) {
        _state = CameraControllerState.Orbit;
        enableAllBtns();
        btn.interactable = false;
    }

    public void OnButtonRise(Button btn) {
        _state = CameraControllerState.Rise;
        enableAllBtns();
        btn.interactable = false;
    }

    public void OnButtonLower(Button btn) {
        _state = CameraControllerState.Lower;
        enableAllBtns();
        btn.interactable = false;
    }

    private void enableAllBtns() {
        foreach (Button btn in _interactionButtons) {
            btn.interactable = true;
        }
    }
}
