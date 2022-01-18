using System;
using System.Collections;
using System.Collections.Generic;
using Magthylius.LerpFunctions;
using UnityEngine;

namespace Hadal.UI
{
    public class UIEffectsHandler : MonoBehaviour
    {
        private UIShootTracer ShootTracer;
        
        [Header("CenterUI movement effects")] 
        [SerializeField] private RectTransform centerUIRectTr;
        [SerializeField] private float scaleLerpSpeed = 0.5f;
        [SerializeField, MinMaxSlider(0f, 20f)] private Vector2 speedRef;
        [SerializeField, MinMaxSlider(0.1f, 1.0f)] private Vector2 minMaxScale;

        private Rigidbody playerRigidbody;

        [Header("Distance indicator")] 
        [SerializeField] private RectTransform distanceArrowLeft;
        [SerializeField] private RectTransform distanceArrowRight;
        [SerializeField] private float distLerpSpeed = 2f;
        [SerializeField, MinMaxSlider(0.1f, 500f)] private Vector2 minMaxDistance;
        [SerializeField, MinMaxSlider(20f, 500f)] private Vector2 minMaxDisplacement;

        private float previousDisplacement = 35f;
        private FlexibleRect arrowLeft;
        private FlexibleRect arrowRight;
        
        public void LateUpdate()
        {
            MovementUpdate();
            DistanceUpdate();
        }

        public void InjectDependencies(Rigidbody pRigidbody, UIShootTracer tracer)
        {
            playerRigidbody = pRigidbody;
            ShootTracer = tracer;

            arrowLeft = new FlexibleRect(distanceArrowLeft);
            arrowRight = new FlexibleRect(distanceArrowRight);
            
            previousDisplacement = minMaxDisplacement.x;
        }

        void MovementUpdate()
        {
            if (!playerRigidbody) return;
            
            float proportion = (playerRigidbody.velocity.magnitude - speedRef.x) / (speedRef.y - speedRef.x);
            float centerScale = Mathf.Lerp(minMaxScale.x, minMaxScale.y, proportion);
            centerUIRectTr.localScale = Vector3.Lerp(centerUIRectTr.localScale, new Vector3(centerScale, centerScale, centerScale), scaleLerpSpeed * Time.deltaTime);
        }

        void DistanceUpdate()
        {
            if (!ShootTracer) return;
            
            float proportion = (ShootTracer.HitDistance - minMaxDistance.x) / (minMaxDistance.y - minMaxDistance.x);
            float displacement = Mathf.Lerp(minMaxDisplacement.x, minMaxDisplacement.y, proportion);

            previousDisplacement = Mathf.Lerp(previousDisplacement, displacement, distLerpSpeed * Time.deltaTime);
            
            arrowLeft.MoveTo(new Vector2(-previousDisplacement, 0f));
            arrowRight.MoveTo(new Vector2(previousDisplacement, 0f));
        }
    }

}