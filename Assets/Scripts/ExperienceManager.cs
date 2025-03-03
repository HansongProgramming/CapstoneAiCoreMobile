using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using System.Linq;
using static UnityEngine.UI.GridLayoutGroup;

public class ExperienceManager : MonoBehaviour
{
    [SerializeField] private Button Button;
    [SerializeField] private Button Button2;
    [SerializeField] private Button ScaleUp;
    [SerializeField] private Button ScaleDown;
    [SerializeField] private Button ScaleLeft;
    [SerializeField] private Button ScaleRight;
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
    private bool scaleleft = false;
    private bool scaleright = false;
    private GameObject _spherePreview;
    private GameObject _squarePreview;
    private GameObject convergenceLine;
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
        ScaleUp.gameObject.SetActive(false);
        ScaleDown.gameObject.SetActive(false);
        ScaleLeft.gameObject.SetActive(false);
        ScaleRight.gameObject.SetActive(false);
        _spherePreview = Instantiate(spherePrefab);
        _squarePreview = Instantiate(squarePrefab);
        _squarePreview.SetActive(false);
        _spherePreview.SetActive(false);
    }

    public void AdjustYPosition(float newY)
    {
        GameObject corner1 = _squarePreview.GetComponentsInChildren<Transform>()
                                   .FirstOrDefault(t => t.CompareTag("corner1"))?.gameObject;
        GameObject corner2 = _squarePreview.GetComponentsInChildren<Transform>()
                                   .FirstOrDefault(t => t.CompareTag("corner2"))?.gameObject;
        GameObject corner3 = _squarePreview.GetComponentsInChildren<Transform>()
                                   .FirstOrDefault(t => t.CompareTag("corner3"))?.gameObject;
        GameObject corner4 = _squarePreview.GetComponentsInChildren<Transform>()
                                   .FirstOrDefault(t => t.CompareTag("corner4"))?.gameObject;
        GameObject line1 = _squarePreview.GetComponentsInChildren<Transform>()
                                   .FirstOrDefault(t => t.CompareTag("line1"))?.gameObject;
        GameObject line2 = _squarePreview.GetComponentsInChildren<Transform>()
                           .FirstOrDefault(t => t.CompareTag("line2"))?.gameObject;
        GameObject line3 = _squarePreview.GetComponentsInChildren<Transform>()
                           .FirstOrDefault(t => t.CompareTag("line3"))?.gameObject;
        GameObject line4 = _squarePreview.GetComponentsInChildren<Transform>()
                           .FirstOrDefault(t => t.CompareTag("line4"))?.gameObject;
        if (corner1 != null && line1 != null)
        {
            Vector3 corner1pos = new Vector3(corner1.transform.position.x, corner1.transform.position.y - newY, corner1.transform.position.z);
            Vector3 corner2pos = new Vector3(corner2.transform.position.x, corner2.transform.position.y + newY, corner2.transform.position.z);
            Vector3 corner3pos = new Vector3(corner3.transform.position.x, corner3.transform.position.y - newY, corner3.transform.position.z);
            Vector3 corner4pos = new Vector3(corner4.transform.position.x, corner4.transform.position.y + newY, corner4.transform.position.z);

            corner1.transform.position = corner1pos;
            corner2.transform.position = corner2pos;
            corner3.transform.position = corner3pos;
            corner4.transform.position = corner4pos;
            line1.transform.position = corner1pos;
            line2.transform.position = corner3pos;
            line3.transform.position = corner3pos;
            line4.transform.position = corner4pos;
            line1.transform.localScale = new Vector3(line1.transform.localScale.x,line1.transform.localScale.y,line1.transform.localScale.z + newY * 2);
            line3.transform.localScale = new Vector3(line3.transform.localScale.x,line3.transform.localScale.y,line3.transform.localScale.z + newY * 2);
        }

        if (_squarePreview.activeSelf)
        {
            Vector3 previewPos = _squarePreview.transform.position;
            previewPos.y += newY;
            _squarePreview.transform.position = previewPos;
            rotateConvergenceLines();
        }
    }

    public void AdjustXPosition(float newX)
    {
        GameObject corner1 = _squarePreview.GetComponentsInChildren<Transform>()
                                    .FirstOrDefault(t => t.CompareTag("corner1"))?.gameObject;
        GameObject corner2 = _squarePreview.GetComponentsInChildren<Transform>()
                                   .FirstOrDefault(t => t.CompareTag("corner2"))?.gameObject;
        GameObject corner3 = _squarePreview.GetComponentsInChildren<Transform>()
                                   .FirstOrDefault(t => t.CompareTag("corner3"))?.gameObject;
        GameObject corner4 = _squarePreview.GetComponentsInChildren<Transform>()
                                   .FirstOrDefault(t => t.CompareTag("corner4"))?.gameObject;
        GameObject line1 = _squarePreview.GetComponentsInChildren<Transform>()
                           .FirstOrDefault(t => t.CompareTag("line1"))?.gameObject;
        GameObject line2 = _squarePreview.GetComponentsInChildren<Transform>()
                           .FirstOrDefault(t => t.CompareTag("line2"))?.gameObject;
        GameObject line3 = _squarePreview.GetComponentsInChildren<Transform>()
                           .FirstOrDefault(t => t.CompareTag("line3"))?.gameObject;
        GameObject line4 = _squarePreview.GetComponentsInChildren<Transform>()
                           .FirstOrDefault(t => t.CompareTag("line4"))?.gameObject;
        if (corner1 != null)
        {
            Vector3 corner1pos = new Vector3(corner1.transform.position.x, corner1.transform.position.y , corner1.transform.position.z - newX);
            Vector3 corner2pos = new Vector3(corner2.transform.position.x, corner2.transform.position.y , corner2.transform.position.z - newX);
            Vector3 corner3pos = new Vector3(corner3.transform.position.x, corner3.transform.position.y , corner3.transform.position.z + newX);
            Vector3 corner4pos = new Vector3(corner4.transform.position.x, corner4.transform.position.y , corner4.transform.position.z + newX);

            corner1.transform.position = corner1pos;
            corner2.transform.position = corner2pos;
            corner3.transform.position = corner3pos;
            corner4.transform.position = corner4pos;
            line1.transform.position = corner1pos;
            line2.transform.position = corner3pos;
            line3.transform.position = corner3pos;
            line4.transform.position = corner4pos;
            line2.transform.localScale = new Vector3(line2.transform.localScale.x, line2.transform.localScale.y, line2.transform.localScale.z + newX *2);
            line4.transform.localScale = new Vector3(line4.transform.localScale.x, line4.transform.localScale.y, line4.transform.localScale.z + newX *2);
        }

        if (_squarePreview.activeSelf)
        {
            Vector3 previewPos = _squarePreview.transform.position;
            previewPos.z += newX;
            _squarePreview.transform.position = previewPos;
            rotateConvergenceLines();
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
        InputHandler.OnTap -= SpawnSquare; 
        InputHandler.OnTap += SpawnSquare;
    }
    private void DisableSquarePlacement()
    {
        InputHandler.OnTap -= SpawnSquare;
    }
    private void OnDestroy()
    {
        InputHandler.OnTap -= SpawnSphere;
        InputHandler.OnTap -= SpawnSquare;
    }
    public void StartHold()
    {
        isHolding = true;
        DisableSquarePlacement();
    }
    public void StopHold()
    {
        isHolding = false;
        InputHandler.OnTap += SpawnSquare; 
    }
    public void startUp()
    {
        scaleup = true;
        DisableSquarePlacement();
    }
    public void stopUp()
    {
        scaleup = false;
        InputHandler.OnTap += SpawnSquare;
    }
    public void startDown()
    {
        scaledown = true;
        DisableSquarePlacement();
    }
    public void stopDown()
    {
        scaledown = false;
        InputHandler.OnTap += SpawnSquare;
    }
    public void startLeft()
    {
        scaleleft = true;
        DisableSquarePlacement();
    }
    public void stopLeft()
    {
        scaleleft = false;
        InputHandler.OnTap += SpawnSquare;
    }
    public void startRight()
    {
        scaleright = true;
        DisableSquarePlacement();
    }
    public void stopRight()
    {
        scaleright = false;
        InputHandler.OnTap += SpawnSquare;
    }
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
            if (scaleleft)
            {
                AdjustXPosition(0.05f * Time.deltaTime);
            }
            if (scaleright)
            {
                AdjustXPosition(-0.05f * Time.deltaTime);
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

    private float calculateImpactAngle(float length, float width)
    {
        if (width == 0)
        {
            Debug.LogError("Width is zero! Preventing division by zero.");
            return 0; // Or some default value
        }

        float ratio = length / width;
        ratio = Mathf.Clamp(ratio, -1f, 1f); // Ensure the value is within valid range

        float result = Mathf.Asin(ratio);
        return result * Mathf.Rad2Deg; // Convert from radians to degrees
    }


    private void rotateConvergenceLines()
    {
        GameObject line1 = _squarePreview.GetComponentsInChildren<Transform>()
                   .FirstOrDefault(t => t.CompareTag("line1"))?.gameObject;
        GameObject line2 = _squarePreview.GetComponentsInChildren<Transform>()
                           .FirstOrDefault(t => t.CompareTag("line2"))?.gameObject;
        convergenceLine = _squarePreview.GetComponentsInChildren<Transform>()
                   .FirstOrDefault(t => t.CompareTag("convergence"))?.gameObject;
        float Length = line2.transform.localScale.z;
        float width = line1.transform.localScale.z;
        float impactAngle = calculateImpactAngle(Length, width);
        Debug.Log(impactAngle);
        Quaternion currentRotation = convergenceLine.transform.rotation;
        convergenceLine.transform.localRotation = Quaternion.Euler(impactAngle, 0, 0);
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

        var point = Instantiate(_squarePreview);
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
        ScaleUp.gameObject.SetActive(_canPlaceSquare);
        ScaleDown.gameObject.SetActive(_canPlaceSquare);
        ScaleLeft.gameObject.SetActive(_canPlaceSquare);
        ScaleRight.gameObject.SetActive(_canPlaceSquare);
        label.gameObject.SetActive(_canPlaceSquare);
        rotationDropdown.gameObject.SetActive(!_canPlaceSquare);
        notice.gameObject.SetActive(_canPlaceSquare);
        rotationSlider.gameObject.SetActive(_canPlaceSquare);
        _squarePreview.SetActive(_canPlaceSquare);
    }
}
