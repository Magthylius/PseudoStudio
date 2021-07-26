using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.Networking.UI
{
    public class MainMenuIconBehaviour : MonoBehaviour
    {
        enum FadeMode
        {
            Normal,
            FadeLarge,
            FadeShrink
        }

        public PlayerClassType ClassType;
        public Image icon;
        public Color SelectableColor = Color.white;
        public Color UnselectableColor = Color.gray;
        public RectTransform iconRect;
        public float targetScale = 1.2f;
        public float lerpSpeed = 5f;

        private FadeMode mode;

        private void Start()
        {
            SetSelectable();
        }

        private void LateUpdate()
        {
            if (mode == FadeMode.FadeLarge)
            {
                if (Vector3.SqrMagnitude(TargetRectScale - iconRect.localScale) > 0.0001f)
                {
                    iconRect.localScale = Vector3.Lerp(iconRect.localScale, TargetRectScale, lerpSpeed * Time.deltaTime);
                }
                else
                {
                    iconRect.localScale = TargetRectScale;
                    mode = FadeMode.Normal;
                }
            }
            else if (mode == FadeMode.FadeShrink)
            {
                if (Vector3.SqrMagnitude(Vector3.one - TargetRectScale) > 0.0001f)
                {
                    iconRect.localScale = Vector3.Lerp(iconRect.localScale, Vector3.one, lerpSpeed * Time.deltaTime);
                }
                else
                {
                    iconRect.localScale = Vector3.one;
                    mode = FadeMode.Normal;
                }
            }
        }

        public void SetSelectable()
        {
            icon.color = SelectableColor;
        }

        public void SetUnselectable()
        {
            icon.color = UnselectableColor;
        }

        public void StartEnlarge() => mode = FadeMode.FadeLarge;
        public void StartShrink() => mode = FadeMode.FadeShrink;

        private Vector3 TargetRectScale => new Vector3(targetScale, targetScale, targetScale);
    }

}