using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

//! Version 1.1.0
[RequireComponent(typeof(EventTrigger), typeof(Image))]
public class MagthyliusPointerButton : MonoBehaviour
{
    [System.Serializable]
    public class UIEventContainer
    {
        [HideInInspector] public EventTriggerType eventType;
        public Color color;
        public GameObject triggerObject;
        public UnityEvent eventDelegates;

        public UIEventContainer(EventTriggerType type)
        {
            eventType = type;
            eventDelegates = new UnityEvent();
            color = Color.white;
            color.a = 0f;
        }
    }

    [Header("References")]
    public Image image;
    [Min(0f)] public float colorFadeSpeed;

    EventTrigger et;

    [Header("TMP Settings")]
    [SerializeField] string TMPText;
    [SerializeField] TextMeshProUGUI[] TMPObjects;

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

    bool allowColorLerp;
    Color targetColor;
    GameObject currentObject;

    //! External data
    bool isHovered = false;

    void OnValidate()
    {
        TMPObjects = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in TMPObjects) text.text = TMPText;
    }

    void Start()
    {
        et = GetComponent<EventTrigger>();

        if (image == null) image = GetComponent<Image>();

        eventList = new List<UIEventContainer> { pointerEnterEvent, pointerExitEvent, pointerUpEvent, pointerDownEvent, pointerClickedEvent };
        pointerEnterEvent.eventDelegates.AddListener(PointerEnteredHandling);
        pointerExitEvent.eventDelegates.AddListener(PointerExitHandling);

        InjectEvents();
    }

    void OnDestroy()
    {
        if (eventList != null)
        {
            foreach (UIEventContainer events in eventList)
            {
                events.eventDelegates.RemoveAllListeners();
            }
        }
        
    }

    void FixedUpdate()
    {
        if (allowColorLerp && image != null)
        {
            image.color = Color.Lerp(image.color, targetColor, colorFadeSpeed);
            float colorCheck = color.r + color.g + color.b + color.a;
    
            if (colorCheck < 0.4)
            {
                image.color = targetColor;
                allowColorLerp = false;
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
        allowColorLerp = true;
        UIEventContainer e = GetEvents(type);
        if (e != null)
        {
            e.eventDelegates.Invoke();
            targetColor = e.color;

            if (e.triggerObject != null)
            {
                if (currentObject != null) currentObject.SetActive(false);

                currentObject = e.triggerObject;
                currentObject.SetActive(true);
            }
        }
    }
    #endregion

    #region Data Handling
    void PointerEnteredHandling() => isHovered = true;
    void PointerExitHandling() => isHovered = false;
    #endregion

    #region Accessors
    public Color color => image.color;
    public bool IsHovered => isHovered;
    #endregion
}
