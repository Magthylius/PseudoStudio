using System.Collections;
using System.Collections.Generic;
using Hadal.Networking;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UIUtilitiesHandler : MonoBehaviour
    {
        public UIGenericGaugeHandler FlareGauge;
        
        public UIGenericGaugeHandler SaviourGauge;
        public UIGenericGaugeHandler HarpoonerGauge;
        public UIGenericGaugeHandler InformerGauge;
        public UIGenericGaugeHandler TrapperGauge;

        public Image TitleName;
        public Sprite HunterTitle;
        public Sprite TrackerTitle;
        public Sprite TrapperTitle;
        public Sprite MedicTitle;

        private UIGenericGaugeHandler currentUtilGauge;

        public void Initialize(PlayerClassType classType)
        {
            switch (classType)
            {
                case PlayerClassType.Saviour: 
                    SaviourGauge.gameObject.SetActive(true);
                    currentUtilGauge = SaviourGauge;
                    TitleName.sprite = MedicTitle;
                    break;
                
                case PlayerClassType.Harpooner: 
                    HarpoonerGauge.gameObject.SetActive(true);
                    currentUtilGauge = HarpoonerGauge;
                    TitleName.sprite = HunterTitle;
                    break;
                
                case PlayerClassType.Informer:
                    InformerGauge.gameObject.SetActive(true);
                    currentUtilGauge = InformerGauge;
                    TitleName.sprite = TrackerTitle;
                    break;
                
                case PlayerClassType.Trapper:
                    TrapperGauge.gameObject.SetActive(true);
                    currentUtilGauge = TrapperGauge;
                    TitleName.sprite = TrapperTitle;
                    break;
            }
        }

        public UIGenericGaugeHandler CurrentUtilGauge => currentUtilGauge;
    }
}
