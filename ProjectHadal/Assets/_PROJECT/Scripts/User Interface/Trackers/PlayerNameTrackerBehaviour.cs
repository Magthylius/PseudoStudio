using System;
using System.Collections;
using System.Collections.Generic;
using Hadal.Networking;
using Magthylius.LerpFunctions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class PlayerNameTrackerBehaviour : UITrackerBehaviour
    {
        [Header("Name Tracker Settings")]
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI DistanceText;
        
        [Header("Class Icon Settings")]
        public Image ClassIcon;
        public Image ClassIcon2;
        public Sprite HunterIcon;
        public Sprite MedicIcon;
        public Sprite TrackerIcon;
        public Sprite TrapperIcon;

        [Header("Player Status Settings")]
        public Color playerDefaultColor = Color.white;
        public Color playerDownColor = Color.red;

        public float DistanceUpdateDelay = 1f;

        [Header("Canvases")] 
        public CanvasGroup DistantCG;
        public CanvasGroup CloseCG;

        private CanvasGroupFader distantCGF;
        private CanvasGroupFader closeCGF;

        private void Start()
        {
            base.Start();
            InvokeRepeating(nameof(DistanceUpdater), 0f, DistanceUpdateDelay);

            distantCGF = new CanvasGroupFader(DistantCG, false, false, 0.00001f);
            closeCGF = new CanvasGroupFader(CloseCG, false, false, 0.00001f);
            distantCGF.SetOpaque();
            closeCGF.SetTransparent();
        }

        void LateUpdate()
        {
            //base.LateUpdate();
            
            if (!fadeWhenDistant) return;
            
            if (!IsValid())
            {
                Untrack();
                return;
            }
            
            distanceToTransform = Vector3.Distance(playerTransform.position, trackingTransform.position);
            if (distanceToTransform >= fadeOutDistance)
            {
                closeCGF.StartFadeOut();
                distantCGF.StartFadeIn();
            }
            else if (distanceToTransform <= fadeInDistance)
            {
                closeCGF.StartFadeIn();
                distantCGF.StartFadeOut();
            }
            
            closeCGF.Step(fadeSpeed * Time.deltaTime);
            distantCGF.Step(fadeSpeed * Time.deltaTime);
        }

        void DistanceUpdater()
        {
            UpdateDistance(distanceToTransform);
        }

        public void UpdateText(string name)
        {
            NameText.text = name;
        }

        public void UpdateDistance(float distance)
        {
            DistanceText.text = $"{(int)distance + " m"}";
        }

        public void UpdateIcon(PlayerClassType classType)
        {
            switch (classType)
            {
                case PlayerClassType.Harpooner:
                    ClassIcon.sprite = HunterIcon;
                    ClassIcon2.sprite = HunterIcon;
                    break;
                
                case PlayerClassType.Saviour:
                    ClassIcon.sprite = MedicIcon;
                    ClassIcon2.sprite = MedicIcon;
                    break;
                
                case PlayerClassType.Informer:
                    ClassIcon.sprite = TrackerIcon;
                    ClassIcon2.sprite = TrackerIcon;
                    break;
                
                case PlayerClassType.Trapper:
                    ClassIcon.sprite = TrapperIcon;
                    ClassIcon2.sprite = TrapperIcon;
                    break;
            }
        }

        public void SetDownSettings()
        {
            ClassIcon.color = playerDownColor;
            ClassIcon2.color = playerDownColor;
            NameText.color = playerDownColor;
            DistanceText.color = playerDownColor;
        }
        
        public void SetDefaultSettings()
        {
            ClassIcon.color = playerDefaultColor;
            ClassIcon2.color = playerDefaultColor;
            NameText.color = playerDefaultColor;
            DistanceText.color = playerDefaultColor;
        }
    }
}
