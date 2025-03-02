using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class ExperienceManager : MonoBehaviour
{
    [SerializeField] private Button Button;
    [SerializeField] private Button Button2;
    [SerializeField] private Button ScaleUp;
    [SerializeField] private Button ScaleDown;
    [SerializeField] private Slider rotationSlider;
    [SerializeField] private TMP_Dropdown rotationDropdown;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private TextMeshProUGUI distanceTextPrefab;
    [SerializeField] private TextMeshProUGUI notice;
    [SerializeField] private TextMeshProUGUI label;

    private bool _canPlaceSphere;
    private bool _canPlaceSquare;
    private bool isHolding = false;
    private bool scaledown = false;
    private bool scaleup = false;
    private GameObject _spherePreview;
    private GameObject _squarePreview;
    public GameObject corner1;
    private GameObject corner2;
    private GameObject corner3;
    private GameObject corner4;
    private Vector3 _detectedPosition = new Vector3();
    private Quaternion _detectedQuaternion = Quaternion.identity;
    private ARTrackable _currentTrackable = null;

    private List<GameObject> spawnedSpheres = new List<GameObject>();
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<TextMeshProUGUI> distanceTexts = new List<TextMeshProUGUI>();

    private void Start()
    {
        Button.onClick.AddListener(SphereListener);
        Button2.onClick.AddListener(SquareListener);
        rotationSlider.onValueChanged.AddListener(UpdatePreviewRotation);
        rotationSlider.gameObject.SetActive(false);
        notice.gameObject.SetActive(false);
        label.gameObject.SetActive(false);

        _spherePreview = Instantiate(spherePrefab);
        _squarePreview = Instantiate(squarePrefab);
        _squarePreview.SetActive(false);
        _spherePreview.SetActive(false);
    }

    public void AdjustYPosition(float newY)
    {
        corner1 = GameObject.FindGameObjectWithTag("corner1");
        corner2 = GameObject.FindGameObjectWithTag("corner2");
        corner3 = GameObject.FindGameObjectWithTag("corner3");
        corner4 = GameObject.FindGameObjectWithTag("corner4");

        if (corner1 != null)
        {
            Vector3 corner1pos = new Vector3(corner1.transform.position.x, corner1.transform.position.y + newY, corner1.transform.position.z);
            Vector3 corner3pos = new Vector3(corner3.transform.position.x, corner3.transform.position.y + newY, corner3.transform.position.z);
            corner1.transform.position = corner1pos;
            corner3.transform.position = corner3pos;
        }
    }

    private void UpdatePreviewRotation(float value)
    {
        switch (rotationDropdown.value)
        {
            case 0: 
                _squarePreview.transform.rotation = Quaternion.Euler(
                    value, 
                    _squarePreview.transform.rotation.eulerAngles.y,
                    _squarePreview.transform.rotation.eulerAngles.z);
                break;

            case 1: 
                _squarePreview.transform.rotation = Quaternion.Euler(
                    _squarePreview.transform.rotation.eulerAngles.x,
                    value, 
                    _squarePreview.transform.rotation.eulerAngles.z);
                break;

            case 2: 
                _squarePreview.transform.rotation = Quaternion.Euler(
                    _squarePreview.transform.rotation.eulerAngles.x,
                    _squarePreview.transform.rotation.eulerAngles.y,
                    value); 
                break;
        }
    }

    private void SphereListener()
    {
        InputHandler.OnTap += SpawnSphere;
    }

    private void SquareListener()
    {
        InputHandler.OnTap += SpawnSquare;
    }

    private void OnDestroy()
    {
        InputHandler.OnTap -= SpawnSphere;
        InputHandler.OnTap -= SpawnSquare;
    }

    public void StartHold() => isHolding = true;
    public void StopHold() => isHolding = false;
    
    public void startUp() => scaleup = true;
    public void stopUp() => scaleup = false;

    public void startDown() => scaledown = true;
    public void stopDown() => scaledown = false;

    private void Update()
    {
        GetRaycastHitTransform();
        UpdateLinesAndDistances();

        if (isHolding)
        {
            if (scaleup)
            {
              AdjustYPosition(0.05f * Time.deltaTime);
            } 
            if (scaledown)
            {
              AdjustYPosition(-0.05f * Time.deltaTime);
            }
        }
    }

    private void GetRaycastHitTransform()
    {
        var hits = new List<ARRaycastHit>();
        var middleScreen = new Vector2(Screen.width / 2, Screen.height / 2);

        if (arRaycastManager.Raycast(middleScreen, hits, TrackableType.PlaneWithinPolygon))
        {
            _detectedPosition = hits[0].pose.position;
            _detectedQuaternion = hits[0].pose.rotation;
            _currentTrackable = hits[0].trackable;

            Vector3 surfaceNormal = hits[0].pose.up;
            Vector3 cameraForward = Camera.main.transform.forward;

            _spherePreview.transform.position = _detectedPosition;
            _spherePreview.transform.rotation = _detectedQuaternion;

            if (Mathf.Abs(surfaceNormal.y) > 0.7f)
            {
                cameraForward.y = 0;
                cameraForward.Normalize();

                Quaternion lookRotation = Quaternion.LookRotation(cameraForward);
                _squarePreview.transform.rotation = lookRotation;
            }
            else
            {
                _squarePreview.transform.rotation = _detectedQuaternion;
            }

            Vector3 previewEulerAngles = _squarePreview.transform.rotation.eulerAngles;

            switch (rotationDropdown.value)
            {
                case 0: 
                    previewEulerAngles.x += rotationSlider.value;
                    break;
                case 1: 
                    previewEulerAngles.y += rotationSlider.value;
                    break;
                case 2: 
                    previewEulerAngles.z += rotationSlider.value;
                    break;
            }

            _squarePreview.transform.rotation = Quaternion.Euler(previewEulerAngles);
            _squarePreview.transform.position = _detectedPosition;
        }
    }

    private void SpawnSphere()
    {
        if (!_canPlaceSphere) return;

        var point = Instantiate(spherePrefab);
        point.GetComponent<points>().PlacePoint(_currentTrackable);
        point.transform.position = _detectedPosition;
        point.transform.rotation = _detectedQuaternion;
        spawnedSpheres.Add(point);

        if (spawnedSpheres.Count > 1)
        {
            DrawLineBetweenLastTwoPoints();
        }

        SetCanAddSphere(false);
    }

    private void SpawnSquare()
    {
        if (!_canPlaceSquare) return;

        var point = Instantiate(squarePrefab);
        point.GetComponent<boxes>().PlaceBox(_currentTrackable);
        point.transform.position = _detectedPosition;

        Vector3 surfaceNormal = _detectedQuaternion * Vector3.up;
        Vector3 cameraForward = Camera.main.transform.forward;

        if (Mathf.Abs(surfaceNormal.y) > 0.7f)
        {
            cameraForward.y = 0;
            cameraForward.Normalize();

            Quaternion lookRotation = Quaternion.LookRotation(cameraForward);
            point.transform.rotation = lookRotation;
        }
        else
        {
            point.transform.rotation = _detectedQuaternion;
        }

        Vector3 spawnEulerAngles = point.transform.rotation.eulerAngles;

        switch (rotationDropdown.value)
        {
            case 0: 
                spawnEulerAngles.x += rotationSlider.value;
                break;
            case 1: 
                spawnEulerAngles.y += rotationSlider.value;
                break;
            case 2: 
                spawnEulerAngles.z += rotationSlider.value;
                break;
        }

        point.transform.rotation = Quaternion.Euler(spawnEulerAngles);
        SetCanAddSquare(false);
    }


    private void DrawLineBetweenLastTwoPoints()
    {
        int lastIndex = spawnedSpheres.Count - 1;
        if (lastIndex < 1) return;

        GameObject newLineObj = Instantiate(linePrefab);
        LineRenderer line = newLineObj.GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, spawnedSpheres[lastIndex - 1].transform.position);
        line.SetPosition(1, spawnedSpheres[lastIndex].transform.position);
        lines.Add(line);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene! Text will not be visible.");
            return;
        }

        TextMeshProUGUI distanceText = Instantiate(distanceTextPrefab, canvas.transform);
        distanceTexts.Add(distanceText);
    }

    private void UpdateLinesAndDistances()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (i >= spawnedSpheres.Count - 1 || i >= distanceTexts.Count) continue;

            Vector3 start = spawnedSpheres[i].transform.position;
            Vector3 end = spawnedSpheres[i + 1].transform.position;

            lines[i].SetPosition(0, start);
            lines[i].SetPosition(1, end);

            float distance = Vector3.Distance(start, end);
            float distanceInInches = distance * 39.3701f;

            Vector3 screenPosition = Camera.main.WorldToScreenPoint((start + end) / 2);

            distanceTexts[i].text = $"{distanceInInches:F2} inches";
            distanceTexts[i].transform.position = screenPosition;
        }
    }


    public void SetCanAddSphere(bool canPlaceSphere)
    {
        _canPlaceSphere = canPlaceSphere;
        Button.gameObject.SetActive(!_canPlaceSphere);
        rotationDropdown.gameObject.SetActive(!_canPlaceSphere);
        Button2.gameObject.SetActive(!_canPlaceSphere);
        notice.gameObject.SetActive(_canPlaceSphere);
        _spherePreview.SetActive(_canPlaceSphere);
    }

    public void SetCanAddSquare(bool canPlaceSquare)
    {
        _canPlaceSquare = canPlaceSquare;
        Button.gameObject.SetActive(!_canPlaceSquare);
        Button2.gameObject.SetActive(!_canPlaceSquare);
        label.gameObject.SetActive(_canPlaceSquare);
        rotationDropdown.gameObject.SetActive(!_canPlaceSquare);
        notice.gameObject.SetActive(_canPlaceSquare);
        rotationSlider.gameObject.SetActive(_canPlaceSquare);
        _squarePreview.SetActive(_canPlaceSquare);
    }
}
