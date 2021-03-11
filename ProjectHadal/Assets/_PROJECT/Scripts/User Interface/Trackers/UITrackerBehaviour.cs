using UnityEngine;
using UnityEngine.UI;
using Magthylius.LerpFunctions;

//! C: Jon
/// <summary>
/// Parent class for UI Trackers
/// </summary>
namespace Hadal.UI
{
    public class UITrackerBehaviour : MonoBehaviour
    {
        public TrackerType Type;
        [SerializeField] bool startsEnabled = false;

        Image image;
        RectTransform rectTransform;
        Transform trackingTransform;
        Camera playerCamera;
        Transform playerTransform;

        FlexibleRect flexRect;

        void Start()
        {
            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();

            flexRect = new FlexibleRect(rectTransform);
            if (!startsEnabled) Disable();
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            //if (trackingTransform != null && playerCamera != null) flexRect.MoveTo(playerCamera.WorldToScreenPoint(trackingTransform.position));
            if (trackingTransform == null || playerCamera == null) return;

            float minX = image.GetPixelAdjustedRect().width * 0.5f;
            float minY = image.GetPixelAdjustedRect().height * 0.5f;

            float maxX = Screen.width - minX;
            float maxY = Screen.height - minY;

            Vector2 pos = playerCamera.WorldToScreenPoint(trackingTransform.position);

            //print(Vector3.Dot((trackingTransform.position - transform.position), playerTransform.forward));
            if (Vector3.Dot((trackingTransform.position - playerTransform.position), playerTransform.forward) < 0)
            {
                if (pos.x < Screen.width * 0.5f) pos.x = maxX;
                else pos.x = minX;
            }

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }

        public void InjectDependencies(Camera playerCamera, Transform playerTransform)
        {
            this.playerCamera = playerCamera;
            this.playerTransform = playerTransform;
        }
        public void TrackTransform(Transform transform)
        {
            Enable();
            trackingTransform = transform;
        }
        public void Untrack()
        {
            trackingTransform = null;
            flexRect.MoveTo(Vector2.zero);
            Disable();
        }
        public void Enable() => gameObject.SetActive(true);
        public void Disable() => gameObject.SetActive(false);
        public Transform TrackingTransform => trackingTransform;
    }
}
