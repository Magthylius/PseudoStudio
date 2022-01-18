using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Hadal.Networking.UI
{
    public class MainMenuChooseClass : MonoBehaviour
    {
        public MainMenuClassSelector Selector;
        public MainMenuHighlightBehaviour CorrespondingHighlight;
        public MainMenuIconBehaviour CorrespondingIconBehaviour;
        public PlayerClassType type;
        
        [SerializeField, ReadOnly] private bool selected = false;

        private void Start()
        {
            NetworkEventManager.Instance.LeftRoomAction += OnLeaveRoom;
        }

        public void OnDestroy()
        {
            NetworkEventManager.Instance.LeftRoomAction -= OnLeaveRoom;
        }

        public void ChooseClass()
        {
            if (CorrespondingHighlight.IsSelectedByOthers) return;
            
            PlayerClassType c = Selector.CurrentClassType;
            if (c == PlayerClassType.Invalid && Selector.PlayerClassAvailable(type))
            {
                Selector.ChooseClass(type);
                Selector.SetClassChooser(this);
                selected = true;
            }
        }

        public void UnchooseClass()
        {
            if (Selector.CurrentClassType == type)
            {
                Selector.UnchooseClass();
                Selector.SetClassChooser(null);
                selected = false;
            }
        }

        public void OnLeaveRoom()
        {
            selected = false;
        }

        public void ToggleButton()
        {
            if (!selected) ChooseClass();
            else UnchooseClass();
        }

        public bool SetSelectState(bool selectState) => selected = selectState;
    }
}
