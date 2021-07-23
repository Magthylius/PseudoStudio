using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Hadal.UI
{
    public class PointerEnterCapturer : MonoBehaviour, IPointerEnterHandler
    {
        public UnityEvent PointerEvent;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEvent.Invoke();
        }
    }
}
