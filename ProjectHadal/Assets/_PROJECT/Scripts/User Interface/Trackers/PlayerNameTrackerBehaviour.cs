using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hadal.UI
{
    public class PlayerNameTrackerBehaviour : UITrackerBehaviour
    {
        [Header("Name Tracker Settings")]
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI DistanceText;

        [Header("Player Status Settings")]
        public Color playerDefaultColor = Color.white;
        public Color playerDownColor = Color.red;

        public float DistanceUpdateDelay = 1f;

        private void Start()
        {
            base.Start();
            InvokeRepeating(nameof(DistanceUpdater), 0f, DistanceUpdateDelay);
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

        public void SetDownSettings()
        {
            NameText.color = playerDownColor;
            DistanceText.color = playerDownColor;
        }
        
        public void SetDefaultSettings()
        {
            NameText.color = playerDefaultColor;
            DistanceText.color = playerDefaultColor;
        }
    }
}
