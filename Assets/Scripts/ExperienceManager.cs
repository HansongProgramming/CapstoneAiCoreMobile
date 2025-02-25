using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ExperienceManager : MonoBehaviour
{
    [SerializeField] private Button Button;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private GameObject spherePrefab;

    private bool _canPlaceSphere;
    private GameObject _spherePreview;
    private Vector3 _detectedPosition = new Vector3();
    private Quaternion _detectedQuaternion = Quaternion.identity;
    private ARTrackable _currentTrackable = null;

    private void Start()
    {
        InputHandler.OnTap += SpawnSphere;
    }

    private void OnDestroy()
    {
        InputHandler.OnTap -= SpawnSphere;
        _spherePreview = Instantiate(spherePrefab);
        SetCanAddSphere(true);
    }

    private void Update()
    {
        GetRaycastHitTransform();
    }

    private void GetRaycastHitTransform()
    {
        var hits = new List<ARRaycastHit>();
        var middlescreen = new Vector2(Screen.width / 2, Screen.height / 2);
        if (arRaycastManager.Raycast(middlescreen, hits, TrackableType.PlaneWithinPolygon))
        {
            _detectedPosition = hits[0].pose.position;
            _detectedQuaternion = hits[0].pose.rotation;
            _spherePreview.transform.position = _detectedPosition;
            _spherePreview.transform.rotation = _detectedQuaternion;
            _currentTrackable = hits[0].trackable;
        }
    }

    private void SpawnSphere()
    {
        if (!_canPlaceSphere) return;

        var point = Instantiate(spherePrefab);
        point.GetComponent<points>().PlacePoint(_currentTrackable);
        point.transform.position = _detectedPosition;
        point.transform.rotation = _detectedQuaternion;

        SetCanAddSphere(false);
    }

    public void SetCanAddSphere(bool canPlaceSphere)
    {
        _canPlaceSphere = canPlaceSphere;
        Button.gameObject.SetActive(!_canPlaceSphere);
        _spherePreview.SetActive(_canPlaceSphere);
    }
}
