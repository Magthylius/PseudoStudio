using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Magthylius.LerpFunctions;
using Hadal.UI;

//! C: Jon
/// <summary>
/// Parent class for UI Trackers
/// </summary>
public class UITrackerBehaviour : MonoBehaviour
{
    public TrackerType Type;
    [SerializeField] bool startsEnabled = false;

    Image image;
    RectTransform rectTransform;
    Transform trackingTransform;
    Camera playerCamera;

    FlexibleRect flexRect;

    void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        flexRect = new FlexibleRect(rectTransform);
        if (!startsEnabled) Disable();
    }

    void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy) return;
        if (trackingTransform != null && playerCamera != null) flexRect.MoveTo(playerCamera.WorldToScreenPoint(trackingTransform.position));
    }

    public void InjectDependencies(Camera playerCamera) => this.playerCamera = playerCamera;
    public void TrackTransform(Transform transform) => trackingTransform = transform;
    public void Untrack()
    {
        trackingTransform = null;
        flexRect.MoveTo(Vector2.zero);
    }
    public void Enable() => gameObject.SetActive(true);
    public void Disable() => gameObject.SetActive(false);
}
