using System;
using System.Collections;
using System.Collections.Generic;
using Magthylius.LerpFunctions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Hadal.Networking.UI
{
    public class MainMenuCreditReturn : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        //public List<GameObject> hiders;
        public CanvasGroup hiderCG;
        public UnityEvent ClickEvent;

        private CanvasGroupFader hiderCGF;

        private void Awake()
        {
            hiderCGF = new CanvasGroupFader(hiderCG, true, false);
        }

        private void OnEnable()
        {
            hiderCGF.SetTransparent();
        }

        private void OnDisable()
        {
            hiderCGF.SetTransparent();
        }

        private void LateUpdate()
        {
            hiderCGF.Step(10f * Time.deltaTime);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ClickEvent.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hiderCGF.StartFadeIn();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hiderCGF.StartFadeOut();
        }
    }
}
