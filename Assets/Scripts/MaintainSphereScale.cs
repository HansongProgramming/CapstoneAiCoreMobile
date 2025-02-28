using UnityEngine;

public class MaintainSphereScale : MonoBehaviour
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = originalScale;
        Debug.Log("Maintaining scale for: " + gameObject.name);
    }

    void LateUpdate()
    {
        transform.localScale = originalScale;
    }

}
