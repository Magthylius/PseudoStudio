using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.Networking.UI.MainMenu
{
    public class MainMenuDeployButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image centerImage;
        [SerializeField] private TextMeshProUGUI playerCounter;
        
        [Header("Settings")]
        [SerializeField] private Color unreadyColor;
        [SerializeField] private Color readyColor;
        [SerializeField] private string allPlayerReadyString;

        [Header("Effects")] 
        [SerializeField] private List<GameObject> effectList;
        [SerializeField] private List<GameObject> hideList;
        
        [Space(10f)]
        [SerializeField, ReadOnly] private int playersInRoom = 0;
        [SerializeField, ReadOnly] private int playersReady;

        private bool previousReadyState = false;
        
        private void OnEnable()
        {
            NetworkEventManager.Instance.JoinedRoomEvent += OnJoinedRoom;
            NetworkEventManager.Instance.LeftRoomEvent += OnLeftRoom;

            if (effectList.Count > 0)
            {
                foreach (GameObject obj in effectList)
                {
                    obj.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            NetworkEventManager.Instance.JoinedRoomEvent -= OnJoinedRoom;
            NetworkEventManager.Instance.LeftRoomEvent -= OnLeftRoom;
            
            //NetworkEventManager.Instance.RemoveListener(ByteEvents.GAME_MENU_CLASS_CHOOSE, RE_PlayerChosenClass);
            //NetworkEventManager.Instance.RemoveListener(ByteEvents.GAME_MENU_CLASS_UNCHOOSE, RE_PlayerUnchosenClass);
        }

        void OnJoinedRoom()
        {
            Debug.LogWarning($"Joined room");
            StartCoroutine(CheckPlayersReady());
        }

        void OnLeftRoom()
        {
            Debug.LogWarning($"Left room");
            StopCoroutine(CheckPlayersReady());
        }
        

        IEnumerator CheckPlayersReady()
        {
            while (true)
            {
                playersReady = NetworkEventManager.Instance.GetReadyPlayerCount();
                playersInRoom = NetworkEventManager.Instance.PlayerCount;
                UpdateCounter();

                if (AllPlayersReady && !previousReadyState)
                {
                    previousReadyState = true;
                    
                    centerImage.color = readyColor;
                    
                    if (effectList != null)
                    {
                        foreach (GameObject obj in effectList)
                        {
                            obj.SetActive(true);
                        }
                    }

                    if (hideList != null)
                    {
                        foreach (GameObject obj in hideList)
                        {
                            obj.SetActive(false);
                        }
                    }
                }
                else if (!AllPlayersReady && previousReadyState)
                {
                    previousReadyState = false;
                    
                    centerImage.color = unreadyColor;
                    if (effectList != null)
                    {
                        foreach (GameObject obj in effectList)
                        {
                            obj.SetActive(false);
                        }
                    }
                    
                    if (hideList != null)
                    {
                        foreach (GameObject obj in hideList)
                        {
                            obj.SetActive(true);
                        }
                    }
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }

        void UpdateCounter()
        {
            playerCounter.text = $"{playersReady} / {playersInRoom}";
        }

        public bool AllPlayersReady => playersReady >= playersInRoom;
    }

}