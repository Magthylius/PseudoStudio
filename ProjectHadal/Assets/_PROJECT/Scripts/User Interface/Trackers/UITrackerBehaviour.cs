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
        [SerializeField] bool screenBounded = true;
        [SerializeField] Vector3 positionOffset = Vector3.zero;

        Graphic graphic;
        RectTransform rectTransform;
        Transform trackingTransform;
        Camera playerCamera;
        Transform playerTransform;
        float scaleFactor;

        FlexibleRect flexRect;

        void Start()
        {
            graphic = GetComponent<Graphic>();
            rectTransform = GetComponent<RectTransform>();

            flexRect = new FlexibleRect(rectTransform);
            if (!startsEnabled) Disable();
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            //if (trackingTransform != null && playerCamera != null) flexRect.MoveTo(playerCamera.WorldToScreenPoint(trackingTransform.position));
            if (trackingTransform == null || playerCamera == null) return;

            float minX = graphic.GetPixelAdjustedRect().width * 0.5f;
            float minY = graphic.GetPixelAdjustedRect().height * 0.5f;

            float maxX = Screen.width - minX;
            float maxY = Screen.height - minY;

            Vector2 pos = playerCamera.WorldToScreenPoint(trackingTransform.position);
            pos.x -= Screen.width * 0.5f;
            pos.y -= Screen.height * 0.5f;
            print(pos);
            //print(trackingTransform.position);

            //! When tracker is behind player
            float dotProduct = Vector3.Dot((trackingTransform.position - playerTransform.position), playerTransform.forward);
            if (dotProduct < 0)
            {
                if (pos.x < Screen.width * 0.5f) pos.x = maxX;
                else pos.x = minX;
            }

            if (screenBounded)
            {
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);

                //transform.position = pos;
                rectTransform.anchoredPosition = pos;
            }
            else
            {
                //transform.position = pos;
                rectTransform.anchoredPosition = pos;
                //flexRect.MoveTo(pos);
                //transform.position *= Mathf.Sign(dotProduct);
            }
        }

        public void InjectDependencies(Camera playerCamera, Transform playerTransform, float scaleFactor)
        {
            this.playerCamera = playerCamera;
            this.playerTransform = playerTransform;

            if (scaleFactor == 0) this.scaleFactor = 1f;
            else this.scaleFactor = scaleFactor;
            
            print(scaleFactor);
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
