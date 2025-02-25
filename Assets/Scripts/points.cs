using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class points : MonoBehaviour
{
    public void PlacePoint([CanBeNull]ARTrackable trackableParent)
    {
        transform.SetParent(trackableParent.transform);
    }
}
