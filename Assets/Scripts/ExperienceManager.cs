using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class ExperienceManager : MonoBehaviour
{
    [SerializeField] private Button sphereButton;
    [SerializeField] private Button squareButton;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject squarePreviewPrefab;
    [SerializeField] private Slider rotationSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider widthSlider;
    [SerializeField] private TextMeshProUGUI distanceTextPrefab;

    private bool _canPlaceObject;
    private bool _placingSquare;
    private GameObject _objectPreview;
    private Vector3 _detectedPosition;
    private Quaternion _detectedQuaternion;
    private ARTrackable _currentTrackable;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<TextMeshProUGUI> distanceTexts = new List<TextMeshProUGUI>();

    private void Start()
    {
        sphereButton.onClick.AddListener(() => StartPlacingObject(spherePrefab, false));
        squareButton.onClick.AddListener(() => StartPlacingObject(squarePreviewPrefab, true));
        InputHandler.OnTap += PlaceObject;
    }

    private void OnDestroy()
    {
        InputHandler.OnTap -= PlaceObject;
    }

    private void Update()
    {
        GetRaycastHitTransform();
        UpdateLinesAndDistances();
        UpdatePreviewTransform();
    }

    private void GetRaycastHitTransform()
    {
        var hits = new List<ARRaycastHit>();
        var middlescreen = new Vector2(Screen.width / 2, Screen.height / 2);
        if (arRaycastManager.Raycast(middlescreen, hits, TrackableType.PlaneWithinPolygon))
        {
            _detectedPosition = hits[0].pose.position;
            _detectedQuaternion = hits[0].pose.rotation;
        }
    }

    private void StartPlacingObject(GameObject prefab, bool isSquare)
    {
        _placingSquare = isSquare;
        _canPlaceObject = true;

        if (_objectPreview != null)
            Destroy(_objectPreview);

        _objectPreview = Instantiate(prefab);
        _objectPreview.SetActive(true);
        ToggleSliders(isSquare);
    }

    private void UpdatePreviewTransform()
    {
        if (_objectPreview == null) return;
        _objectPreview.transform.position = _detectedPosition;
        _objectPreview.transform.rotation = _detectedQuaternion * Quaternion.Euler(0, rotationSlider.value, 0);
        _objectPreview.transform.localScale = new Vector3(widthSlider.value, 1, heightSlider.value);
    }

    private void PlaceObject()
    {
        if (!_canPlaceObject) return;

        var placedObject = Instantiate(_objectPreview);
        spawnedObjects.Add(placedObject);

        if (_placingSquare)
        {
            GenerateSquareLines(placedObject);
        }
        else
        {
            if (spawnedObjects.Count > 1)
            {
                DrawLineBetweenLastTwoPoints();
            }
        }

        _canPlaceObject = false;
        ToggleSliders(false);
    }

    private void GenerateSquareLines(GameObject square)
    {
        Transform[] corners = square.GetComponentsInChildren<Transform>();
        for (int i = 1; i < corners.Length; i++)
        {
            if (i == corners.Length - 1) DrawLineBetweenPoints(corners[i], corners[1]);
            else DrawLineBetweenPoints(corners[i], corners[i + 1]);
        }
    }

    private void DrawLineBetweenLastTwoPoints()
    {
        if (spawnedObjects.Count < 2) return;
        DrawLineBetweenPoints(spawnedObjects[^2].transform, spawnedObjects[^1].transform);
    }

    private void DrawLineBetweenPoints(Transform start, Transform end)
    {
        GameObject newLineObj = Instantiate(linePrefab);
        LineRenderer line = newLineObj.GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, start.position);
        line.SetPosition(1, end.position);
        lines.Add(line);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            TextMeshProUGUI distanceText = Instantiate(distanceTextPrefab, canvas.transform);
            distanceTexts.Add(distanceText);
        }
    }

    private void UpdateLinesAndDistances()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (i >= spawnedObjects.Count - 1 || i >= distanceTexts.Count) continue;

            Vector3 start = spawnedObjects[i].transform.position;
            Vector3 end = spawnedObjects[i + 1].transform.position;

            lines[i].SetPosition(0, start);
            lines[i].SetPosition(1, end);

            float distance = Vector3.Distance(start, end) * 39.3701f;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint((start + end) / 2);

            distanceTexts[i].text = $"{distance:F2} inches";
            distanceTexts[i].transform.position = screenPosition;
        }
    }

    private void ToggleSliders(bool show)
    {
        rotationSlider.gameObject.SetActive(show);
        heightSlider.gameObject.SetActive(show);
        widthSlider.gameObject.SetActive(show);
    }
}
