using System;
using System.Collections;
using System.Collections.Generic;
using Magthylius.LerpFunctions;
using UnityEngine;
using UnityEngine.EventSystems;
using static Magthylius.Utilities.MathUtil;

namespace Hadal.Networking.UI
{
    public class MainMenuCreditBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public float sizeMultiplier = 1.5f;
        public float effectSpeed = 5f;
        public string portfolioLink;
        public CanvasGroup highlightCG;
        
        private RectTransform rtr;
        private CanvasGroupFader highlightCGF;
        private Vector2 originalSD;
        private Vector2 maxSD;

        private Vector2 targetSD;
        private bool allowEffect = false;

        private void Awake()
        {
            rtr = GetComponent<RectTransform>();
            originalSD = rtr.sizeDelta;
            maxSD = originalSD * sizeMultiplier;

            highlightCGF = new CanvasGroupFader(highlightCG, true, false);
            highlightCGF.SetTransparent();
        }

        private void LateUpdate()
        {
            if (allowEffect)
            {
                if (Tolerance(rtr.sizeDelta, targetSD, 0.001f))
                {
                    rtr.sizeDelta = targetSD;
                    allowEffect = false;
                }
                else
                {
                    rtr.sizeDelta = Vector2.Lerp(rtr.sizeDelta, targetSD, effectSpeed * Time.deltaTime);
                }
            }
            
            highlightCGF.Step(effectSpeed * Time.deltaTime);
        }
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetSD = maxSD;
            allowEffect = true;
            highlightCGF.StartFadeIn();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetSD = originalSD;
            allowEffect = true;
            highlightCGF.StartFadeOut();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(portfolioLink)) return;
            Application.OpenURL(portfolioLink);
        }
    }
}
