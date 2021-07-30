using System;
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
        [Header("Overall settings")]
        public TrackerType Type;
        [SerializeField] bool startsEnabled = false;
        [SerializeField] bool screenBounded = true;
        [SerializeField] Vector3 positionOffset = Vector3.zero;
        [SerializeField] private Graphic graphic;
        
        [Header("Fade Settings")]
        [SerializeField] protected bool fadeWhenDistant = false;
        [SerializeField] protected float fadeOutDistance = 100f;
        [SerializeField] protected float fadeInDistance = 100f;
        [SerializeField] protected float fadeSpeed = 2f;

        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        protected Transform trackingTransform;
        protected Camera playerCamera;
        protected Transform playerTransform;

        private FlexibleRect flexRect;
        private CanvasGroupFader cgf;

        //! UI Scaling
        private float resoScale;
        private float playerScale;
        
        //! Fading properties
        protected float distanceToTransform;

        public virtual void Start()
        {
            //graphic = GetComponent<Graphic>();
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            flexRect = new FlexibleRect(rectTransform);
            cgf = new CanvasGroupFader(canvasGroup, false, false, 0.01f);
            cgf.SetOpaque();

            //if (!startsEnabled) Disable();
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            //if (trackingTransform != null && playerCamera != null) flexRect.MoveTo(playerCamera.WorldToScreenPoint(trackingTransform.position));
            if (!IsValid())
            {
                Untrack();
                return;
            }

            float minX = graphic.GetPixelAdjustedRect().width * 0.5f;
            float minY = graphic.GetPixelAdjustedRect().height * 0.5f;

            float maxX = Screen.width - minX;
            float maxY = Screen.height - minY;

            Vector3 trackedPosition = trackingTransform.position;
            Vector2 pos = playerCamera.WorldToScreenPoint(trackedPosition + positionOffset);
            pos.x -= Screen.width * 0.5f;
            pos.y -= Screen.height * 0.5f;
            
            pos *= resoScale * playerScale;

            //! When tracker is behind player
            float dotProduct = Vector3.Dot(trackedPosition - playerTransform.position, playerTransform.forward);
            if (dotProduct < 0)
            {
                if (pos.x < Screen.width * 0.5f) pos.x = maxX;
                else pos.x = minX;
            }

            if (screenBounded)
            {
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                
                rectTransform.anchoredPosition = pos;
            }
            else
            {
                rectTransform.anchoredPosition = pos;
            }
        }
        
        public virtual void LateUpdate()
        {
            if (!fadeWhenDistant) return;
            
            if (!IsValid())
            {
                Untrack();
                return;
            }

            distanceToTransform = Vector3.Distance(playerTransform.position, trackingTransform.position);
            if (distanceToTransform >= fadeOutDistance)
                cgf.StartFadeOut();
            else if (distanceToTransform <= fadeInDistance)
                cgf.StartFadeIn();
            
            cgf.Step(fadeSpeed * Time.deltaTime);
        }

        public void InjectDependencies(Camera playerCamera, Transform playerTransform)
        {
            this.playerCamera = playerCamera;
            this.playerTransform = playerTransform;
            
            resoScale = 1f / (Screen.width / Screen.currentResolution.width);
            playerScale = 1f / PlayerScaleAverage();

            float PlayerScaleAverage()
            {
                float s = playerTransform.localScale.x + playerTransform.localScale.y + playerTransform.localScale.z;
                s /= 3;
                return s;
            }
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

        /// <summary>
        /// Checks if still valid to track. 
        /// </summary>
        /// <returns>True if valid, false if not</returns>
        protected bool IsValid()
        {
            if (trackingTransform == null || playerCamera == null)
            {
                Debug.LogWarning( "Track transform or player camera null!");
                return false;
            }

            return true;
        }
        
        public void EnableFadeEffects(float fadeOutDist, float fadeInDist)
        {
            fadeWhenDistant = true;
            fadeOutDistance = fadeOutDist;
            fadeInDistance = fadeInDist;
        }

        public void DisableFadeEffects() => fadeWhenDistant = false;
        
        public void Enable() => gameObject.SetActive(true);

        public void Disable()
        {
            //Debug.LogWarning("Disabled!");
            gameObject.SetActive(false);
        }
        public Transform TrackingTransform => trackingTransform;
    }
}
