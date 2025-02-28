using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class boxes : MonoBehaviour
{
    public void PlaceBox([CanBeNull] ARTrackable trackableParent)
    {
        transform.SetParent(trackableParent.transform);
    }
}
