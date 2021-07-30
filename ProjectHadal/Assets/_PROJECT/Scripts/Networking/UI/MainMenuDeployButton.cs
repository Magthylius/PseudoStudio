using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Hadal.Networking.UI.MainMenu
{
    public class MainMenuDeployButton : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private UnityEvent deployReadyAudio;
        [SerializeField] private ParticleSystem highlightParticleSystem;
        [SerializeField] private MagthyliusPointerButton highlightButton;
        [SerializeField] private Image centerImage;
        [SerializeField] private TextMeshProUGUI playerCounter;
        [SerializeField] private TextMeshProUGUI diveText;
        [SerializeField] private GameObject readyText;
        [SerializeField] private GameObject waitingText;
        
        [Header("Settings")]
        [SerializeField] private Color diveUnreadyColor;
        [SerializeField] private Color diveReadyColor;
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
            
            readyText.SetActive(false);
            waitingText.SetActive(false);
            diveText.color = diveUnreadyColor;
            highlightButton.DisallowDetection();
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
                    StartCoroutine(DelayButton());
                    previousReadyState = true;
                    
                    IEnumerator DelayButton()
                    {
                        yield return new WaitForSeconds(0.5f);
                        
                        deployReadyAudio.Invoke();

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

                        if (NetworkEventManager.Instance.IsMasterClient)
                        {
                            //Debug.LogWarning("Master clienmt");
                            readyText.SetActive(true);
                            highlightButton.AllowDetection();
                        }
                        else
                        {
                            //Debug.LogWarning(" not Master clienmt");
                            waitingText.SetActive(true);
                        }
                        diveText.color = diveReadyColor;
                        highlightParticleSystem.Emit(1);
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
                    
                    readyText.SetActive(false);
                    waitingText.SetActive(false);
                    
                    diveText.color = diveUnreadyColor;
                    highlightButton.DisallowDetection();
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