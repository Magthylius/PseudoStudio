using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using NaughtyAttributes;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.Networking.UI
{
    public delegate void ClassEvent(PlayerClassType classType);
    public class MainMenuClassSelector : MonoBehaviour
    {
        public event ClassEvent ClassChangedEvent;
        
        public List<MainMenuHighlightBehaviour> ChosenHighlighters;
        
        //! Networked other player chosen classes, effectively NOT eligible player choices
        [SerializeField, ReadOnly] private List<PlayerClassType> chosenClassTypes = new List<PlayerClassType>();

        private PlayerClassType currentClassType = PlayerClassType.Invalid;

        private void Start()
        {
            NetworkEventManager.Instance.AddListener(ByteEvents.GAME_MENU_CLASS_CHOOSE, RE_PlayerChosenClass);
            NetworkEventManager.Instance.AddListener(ByteEvents.GAME_MENU_CLASS_UNCHOOSE, RE_PlayerUnchosenClass);
        }

        private void OnDestroy()
        {
            NetworkEventManager.Instance.RemoveListener(ByteEvents.GAME_MENU_CLASS_CHOOSE, RE_PlayerChosenClass);
            NetworkEventManager.Instance.RemoveListener(ByteEvents.GAME_MENU_CLASS_UNCHOOSE, RE_PlayerUnchosenClass);

            ClassChangedEvent = null;
        }

        /// <summary> Update class selectors based on already joined players </summary>
        public void UpdateNetworkSelector(PlayerClassType type, int playerIndex)
        {
            GetHighlighter(type).Select(NetworkEventManager.Instance.GetPlayerColor(playerIndex), true);
        }

        public void ChooseClass(PlayerClassType type)
        {
            if (chosenClassTypes.Contains(type) || currentClassType != PlayerClassType.Invalid) return;
            
            NetworkEventManager neManager = NetworkEventManager.Instance;

            int pIndex = neManager.GetCurrentPlayerIndex();
            object[] data = {type, pIndex};
            neManager.RaiseEvent(ByteEvents.GAME_MENU_CLASS_CHOOSE, data);

            currentClassType = type;
            
            Color pColor = neManager.GetCurrentPlayerColor();
            GetHighlighter(type).Select(pColor, false);
            
            //! Update room properties so that other players have the information
            neManager.UpdatePlayerClass(neManager.GetCurrentPlayerIndex(), type);
            
            ClassChangedEvent?.Invoke(currentClassType);
        }

        public void UnchooseClass(PlayerClassType type)
        {
            if (type != currentClassType) return;
            
            NetworkEventManager neManager = NetworkEventManager.Instance;
            
            neManager.RaiseEvent(ByteEvents.GAME_MENU_CLASS_UNCHOOSE, type);
            currentClassType = PlayerClassType.Invalid;
            GetHighlighter(type).Deselect();
            
            neManager.UpdatePlayerClass(neManager.GetCurrentPlayerIndex(), PlayerClassType.Invalid);
            ClassChangedEvent?.Invoke(currentClassType);
        }

        void RE_PlayerChosenClass(EventData data)
        {
            object[] newData = (object[])data.CustomData;
            PlayerClassType chosen = (PlayerClassType)newData[0];
            Color pColor = NetworkEventManager.Instance.GetPlayerColor((int) newData[1]);
            
            if (!chosenClassTypes.Contains(chosen))
            {
                chosenClassTypes.Add(chosen);
                GetHighlighter(chosen).Select(pColor, false);
            }
        }
        
        void RE_PlayerUnchosenClass(EventData data)
        {
            PlayerClassType unchosen = (PlayerClassType) data.CustomData;
            if (chosenClassTypes.Contains(unchosen))
            {
                chosenClassTypes.Remove(unchosen);
                GetHighlighter(unchosen).Deselect();
            }
        }
        
        public MainMenuHighlightBehaviour GetHighlighter(PlayerClassType type)
        {
            foreach (var highlighter in ChosenHighlighters)
            {
                if (highlighter.ClassType == type) return highlighter;
            }

            return null;
        }

        public PlayerClassType CurrentCurrentClass => currentClassType;
        public bool PlayerClassAvailable(PlayerClassType type) => !chosenClassTypes.Contains(type);
    }
}
