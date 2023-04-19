using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject _planetPrefab;
    [SerializeField] private GameObject _light;
    private GameObject _planet;
    private float _angle = 0f;
    private Vector3 _axis = new Vector3(0.2f,0.6f,1);
    void Start()
    {
        Time.timeScale = 1;
        _planet = Instantiate(_planetPrefab, new Vector3(0,0,0), Quaternion.identity);

    }

    // Update is called once per frame
    void Update()
    {
        if (_planet == null) return;
        _angle = Time.deltaTime*5;

        transform.RotateAround(_planet.transform.position, _axis, _angle);
        _light.transform.RotateAround(_planet.transform.position, _axis, _angle);
        
    }

    public void newPlanet() {
        _planet.GetComponent<Planet>().hide();
        //_planet = Instantiate(_planetPrefab, new Vector3(0,0,0), Quaternion.identity);
    }

    public void quit() {
        Application.Quit();
    }

    public void enterSolarSystem() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MeshTesting");
    }
}
