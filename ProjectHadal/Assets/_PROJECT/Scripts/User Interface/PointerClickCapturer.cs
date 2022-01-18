using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Hadal.UI
{
    public class PointerClickCapturer : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent PointerEvent;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            PointerEvent.Invoke();
        }
    }
}
