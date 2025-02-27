using UnityEngine;
using UnityEngine.UI;

public class SquarePreview : MonoBehaviour
{
    public Transform corner1, corner2, corner3, corner4; // Corner points
    public LineRenderer line1, line2, line3, line4; // Lines

    public Slider rotationSlider;
    public Slider widthSlider;
    public Slider heightSlider;

    private float defaultWidth = 1f;  // Initial width
    private float defaultHeight = 1f; // Initial height

    private void Start()
    {
        // Set default values
        rotationSlider.value = 0f;
        widthSlider.value = defaultWidth;
        heightSlider.value = defaultHeight;

        // Add listeners to update square in real-time
        rotationSlider.onValueChanged.AddListener(UpdateRotation);
        widthSlider.onValueChanged.AddListener(UpdateSize);
        heightSlider.onValueChanged.AddListener(UpdateSize);
    }

    private void UpdateRotation(float value)
    {
        // Rotate the entire square preview around its center
        transform.rotation = Quaternion.Euler(0, value, 0);
    }

    private void UpdateSize(float _)
    {
        float width = widthSlider.value;
        float height = heightSlider.value;

        // Adjust corner positions based on new width & height
        corner1.localPosition = new Vector3(-width / 2, 0, height / 2);
        corner2.localPosition = new Vector3(width / 2, 0, height / 2);
        corner3.localPosition = new Vector3(width / 2, 0, -height / 2);
        corner4.localPosition = new Vector3(-width / 2, 0, -height / 2);

        // Update lines to match new corner positions
        UpdateLines();
    }

    private void UpdateLines()
    {
        line1.SetPositions(new Vector3[] { corner1.localPosition, corner2.localPosition });
        line2.SetPositions(new Vector3[] { corner2.localPosition, corner3.localPosition });
        line3.SetPositions(new Vector3[] { corner3.localPosition, corner4.localPosition });
        line4.SetPositions(new Vector3[] { corner4.localPosition, corner1.localPosition });
    }
}
