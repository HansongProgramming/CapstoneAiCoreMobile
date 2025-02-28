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
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private TextMeshProUGUI distanceTextPrefab;

    private bool _canPlaceSphere;
    private bool _canPlaceSquare;
    private GameObject _spherePreview;
    private GameObject _squarePreview;
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
        _spherePreview = Instantiate(spherePrefab);
        _squarePreview = Instantiate(squarePrefab);
        _squarePreview.SetActive(false);
        _spherePreview.SetActive(false);
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

    private void Update()
    {
        GetRaycastHitTransform();
        UpdateLinesAndDistances();
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

            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0; 
            cameraForward.Normalize();

            Quaternion lookRotation = Quaternion.LookRotation(cameraForward);

            _spherePreview.transform.position = _detectedPosition;
            _spherePreview.transform.rotation = _detectedQuaternion;

            _squarePreview.transform.position = _detectedPosition;
            _squarePreview.transform.rotation = Quaternion.Euler(
                _detectedQuaternion.eulerAngles.x,
                lookRotation.eulerAngles.y,  
                _detectedQuaternion.eulerAngles.z
            );
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

        Vector3 cameraForward = Camera.main.transform.forward;

        cameraForward.y = 0;
        cameraForward.Normalize();

        Quaternion lookRotation = Quaternion.LookRotation(cameraForward);

        point.transform.rotation = Quaternion.Euler(
            _detectedQuaternion.eulerAngles.x,
            lookRotation.eulerAngles.y, 
            _detectedQuaternion.eulerAngles.z
        );

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
        _spherePreview.SetActive(_canPlaceSphere);
    }

    public void SetCanAddSquare(bool canPlaceSquare)
    {
        _canPlaceSquare = canPlaceSquare;
        Button.gameObject.SetActive(!_canPlaceSquare);
        _squarePreview.SetActive(_canPlaceSquare);
    }
}
