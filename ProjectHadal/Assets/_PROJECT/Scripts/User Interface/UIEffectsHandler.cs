using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI
{
    public class UIEffectsHandler : MonoBehaviour
    {
        [Header("CenterUI movement effects")] 
        [SerializeField] private RectTransform centerUIRectTr;
        [SerializeField] private float scaleLerpSpeed = 0.5f;
        [SerializeField, MinMaxSlider(0f, 20f)] private Vector2 speedRef;
        [SerializeField, MinMaxSlider(0.1f, 1.0f)] private Vector2 minMaxScale;

        private Rigidbody playerRigidbody;

        public void LateUpdate()
        {
            if (!playerRigidbody) return;

            float proportion = (playerRigidbody.velocity.magnitude - speedRef.x) / (speedRef.y - speedRef.x);
            float centerScale = Mathf.Lerp(minMaxScale.x, minMaxScale.y, proportion);
            centerUIRectTr.localScale = Vector3.Lerp(centerUIRectTr.localScale, new Vector3(centerScale, centerScale, centerScale), scaleLerpSpeed * Time.deltaTime);
        }

        public void InjectDependencies(Rigidbody pRigidbody)
        {
            playerRigidbody = pRigidbody;
        }
    }

}