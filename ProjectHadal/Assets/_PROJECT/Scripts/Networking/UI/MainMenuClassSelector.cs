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

        private PlayerClassType _currentClassType = PlayerClassType.Invalid;
        private MainMenuChooseClass _currentClassChooser = null;

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
            if (type == PlayerClassType.Invalid) return;

            GetHighlighter(type).Select(NetworkEventManager.Instance.GetPlayerColor(playerIndex), true);
        }

        /// <summary> Update when someone leaves and needs colors to be updated </summary>
        public void ForceUpdateSlotColor(Dictionary<int, int> newIndices)
        {
            NetworkEventManager neManager = NetworkEventManager.Instance;

            Debug.LogWarning("Force slot color called");
            foreach (var pair in newIndices)
            {
                PlayerClassType type = (PlayerClassType) pair.Value;
                if (type != PlayerClassType.Invalid)
                {
                    GetHighlighter(type).Select(neManager.GetPlayerColor(pair.Key), type != _currentClassType);
                }
            }
        }

        public void ChooseClass(PlayerClassType type)
        {
            if (chosenClassTypes.Contains(type) || _currentClassType != PlayerClassType.Invalid) return;
            
            NetworkEventManager neManager = NetworkEventManager.Instance;

            int pIndex = neManager.GetCurrentPlayerIndex();
            object[] data = {type, pIndex};
            neManager.RaiseEvent(ByteEvents.GAME_MENU_CLASS_CHOOSE, data);

            _currentClassType = type;
            
            Color pColor = neManager.GetCurrentPlayerColor();
            GetHighlighter(type).Select(pColor, false);
            
            //! Update room properties so that other players have the information
            neManager.UpdatePlayerClass(neManager.GetCurrentPlayerIndex(), type);
            
            ClassChangedEvent?.Invoke(_currentClassType);
        }

        public void UnchooseClass()
        {
            if (_currentClassType == PlayerClassType.Invalid) return;
            
            NetworkEventManager neManager = NetworkEventManager.Instance;
            
            neManager.RaiseEvent(ByteEvents.GAME_MENU_CLASS_UNCHOOSE, _currentClassType);
            GetHighlighter(_currentClassType).Deselect();
            _currentClassType = PlayerClassType.Invalid;
            
            neManager.UpdatePlayerClass(neManager.GetCurrentPlayerIndex(), PlayerClassType.Invalid);
            ClassChangedEvent?.Invoke(_currentClassType);
        }

        public void FreeChosenClassButton()
        {
            if (_currentClassChooser != null) _currentClassChooser.UnchooseClass();
        }

        void RE_PlayerChosenClass(EventData data)
        {
            object[] newData = (object[])data.CustomData;
            PlayerClassType chosen = (PlayerClassType)newData[0];

            if (chosen == PlayerClassType.Invalid)
            {
                Debug.LogWarning($"Received player chosen class invalid! This should not happen");
                return;
            }
            
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
            
            if (unchosen == PlayerClassType.Invalid)
            {
                Debug.LogWarning($"Received player chosen class invalid! This should not happen");
                return;
            }
            
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

        public PlayerClassType CurrentClassType => _currentClassType;
        public bool PlayerClassAvailable(PlayerClassType type) => !chosenClassTypes.Contains(type);
        public void SetClassChooser(MainMenuChooseClass classChooser) => _currentClassChooser = classChooser;
    }
}
