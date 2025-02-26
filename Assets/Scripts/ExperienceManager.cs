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
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private TextMeshProUGUI distanceTextPrefab;

    private bool _canPlaceSphere;
    private GameObject _spherePreview;
    private Vector3 _detectedPosition = new Vector3();
    private Quaternion _detectedQuaternion = Quaternion.identity;
    private ARTrackable _currentTrackable = null;

    private List<GameObject> spawnedSpheres = new List<GameObject>();
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<TextMeshProUGUI> distanceTexts = new List<TextMeshProUGUI>();

    private void Start()
    {
        InputHandler.OnTap += SpawnSphere;
        _spherePreview = Instantiate(spherePrefab);
        _spherePreview.SetActive(false);
    }

    private void OnDestroy()
    {
        InputHandler.OnTap -= SpawnSphere;
    }

    private void Update()
    {
        GetRaycastHitTransform();
        UpdateLinesAndDistances();
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
        spawnedSpheres.Add(point);

        if (spawnedSpheres.Count > 1)
        {
            DrawLineBetweenLastTwoPoints();
        }

        SetCanAddSphere(false);
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

        // Find a Canvas to attach the text to
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene! Text will not be visible.");
            return;
        }

        // Instantiate the text inside the Canvas
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

            // Convert world position to screen position
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
}
