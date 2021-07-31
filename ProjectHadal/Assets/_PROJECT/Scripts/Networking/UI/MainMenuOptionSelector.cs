using System.Collections;
using System.Collections.Generic;
using Magthylius.LerpFunctions;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.Networking.UI
{
    public class MainMenuOptionSelector : MonoBehaviour
    {
        public RectTransform SelectorRectTr;
        public RectTransform DefaultRectTr;
        public float moveSpeed = 5f;
        
        [Space(10f)]
        public Color DefaultColor = Color.black;
        public Color CancelColor = Color.red;

        private float targetX = 0f;
        private float targetY = 0f;
        
        private FlexibleRect selectorFR;
        private Vector2 targetPosition;
        
        void OnEnable()
        {
            selectorFR = new FlexibleRect(SelectorRectTr);
            targetX = FlexibleRect.GetCenterPos(SelectorRectTr).x;
            targetY = FlexibleRect.GetCenterPos(DefaultRectTr).y;
            targetPosition = new Vector2(targetX, targetY);
            selectorFR.MoveTo(targetPosition);
            
            StartCoroutine(Logic());
        }

        IEnumerator Logic()
        {
            while (true)
            {
                MoveTo(Vector2.Lerp(center, targetPosition, moveSpeed * Time.deltaTime));
                yield return null;
            }
        }

        public void PNTR_SelectorTarget(RectTransform rectTr)
        {
            targetY = FlexibleRect.GetCenterPos(rectTr).y;
            targetPosition  = new Vector2(targetX, targetY);
            selectorFR.ResizeY(rectTr.sizeDelta.y);
        }

        public void PNTR_SetDefaultColor() => SetColor(DefaultColor);
        public void PNTR_SetCancelColor() => SetColor(CancelColor);

        public void SetColor(Color color)
        {
            if (SelectorRectTr.TryGetComponent(out Image image))
            {
                image.color = color;
            }
        }
        
        void MoveTo(Vector2 targetPosition)
        {
            Vector2 diff = targetPosition - center;
            SelectorRectTr.offsetMax += diff;
            SelectorRectTr.offsetMin += diff;
        }
        
        Vector2 center => (SelectorRectTr.offsetMin  + SelectorRectTr.offsetMax) * 0.5f;
    }
}
