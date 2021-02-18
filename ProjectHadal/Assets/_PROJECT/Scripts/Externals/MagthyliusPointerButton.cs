using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//! Version 1.0.0
[RequireComponent(typeof(EventTrigger), typeof(Image))]
public class MagthyliusPointerButton : MonoBehaviour
{
    [System.Serializable]
    public class UIEventContainer
    {
        [HideInInspector] public EventTriggerType eventType;
        public Color color;
        public UnityEvent eventDelegates;

        public UIEventContainer(EventTriggerType type)
        {
            eventType = type;
            eventDelegates = new UnityEvent();
            color = Color.white;
        }
    }

    EventTrigger et;
    Image img;

    [Range(0f, 1f)] public float colorFadeSpeed;

    [Header("Pointer events")]
    [SerializeField] UIEventContainer pointerEnterEvent = new UIEventContainer(EventTriggerType.PointerEnter);
    [Space(10f)]
    [SerializeField] UIEventContainer pointerExitEvent = new UIEventContainer(EventTriggerType.PointerExit);
    [Space(10f)]
    [SerializeField] UIEventContainer pointerUpEvent = new UIEventContainer(EventTriggerType.PointerUp);
    [Space(10f)]
    [SerializeField] UIEventContainer pointerDownEvent = new UIEventContainer(EventTriggerType.PointerDown);
    [Space(10f)]
    [SerializeField] UIEventContainer pointerClickedEvent = new UIEventContainer(EventTriggerType.PointerClick);

    List<UIEventContainer> eventList;
    bool allowLerp;
    Color targetColor;

    void Start()
    {
        et = GetComponent<EventTrigger>();
        img = GetComponent<Image>();

        eventList = new List<UIEventContainer> { pointerEnterEvent, pointerExitEvent, pointerUpEvent, pointerDownEvent, pointerClickedEvent };

        InjectEvents();
    }

    void FixedUpdate()
    {
        if (allowLerp)
        {
            img.color = Color.Lerp(img.color, targetColor, colorFadeSpeed);
            float colorCheck = color.r + color.g + color.b + color.a;
    
            if (colorCheck < 0.4)
            {
                img.color = targetColor;
                allowLerp = false;
            }
        }
    }

    #region Event Triggers
    UIEventContainer GetEvents(EventTriggerType type)
    {
        foreach (UIEventContainer uiEvent in eventList) if (uiEvent.eventType == type) return uiEvent;
        return null;
    }

    void InjectEvents()
    {
        foreach (UIEventContainer uiEvent in eventList)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = uiEvent.eventType;
            entry.callback.AddListener((data) => { InjectDelegates((PointerEventData)data, uiEvent.eventType); });
            et.triggers.Add(entry);
        }
    }

    void InjectDelegates(PointerEventData data, EventTriggerType type)
    {
        allowLerp = true;
        UIEventContainer e = GetEvents(type);
        if (e != null)
        {
            e.eventDelegates.Invoke();
            targetColor = e.color;
        }
    }
    #endregion

    #region Accessors
    public Image Image => img;
    public Color color => img.color;
    #endregion
}
