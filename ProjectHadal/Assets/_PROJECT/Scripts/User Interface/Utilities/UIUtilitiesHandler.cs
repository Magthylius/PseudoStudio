using System.Collections;
using System.Collections.Generic;
using Hadal.Networking;
using UnityEngine;
using TMPro;

namespace Hadal.UI
{
    public class UIUtilitiesHandler : MonoBehaviour
    {
        public UIGenericGaugeHandler FlareGauge;
        
        public UIGenericGaugeHandler SaviourGauge;
        public UIGenericGaugeHandler HarpoonerGauge;
        public UIGenericGaugeHandler InformerGauge;
        public UIGenericGaugeHandler TrapperGauge;

        private UIGenericGaugeHandler currentUtilGauge;

        public void Initialize(PlayerClassType classType)
        {
            switch (classType)
            {
                case PlayerClassType.Saviour: 
                    SaviourGauge.gameObject.SetActive(true);
                    currentUtilGauge = SaviourGauge;
                    break;
                
                case PlayerClassType.Harpooner: 
                    break;
                
                case PlayerClassType.Informer:
                    InformerGauge.gameObject.SetActive(true);
                    currentUtilGauge = InformerGauge;
                    break;
                
                case PlayerClassType.Trapper:
                    TrapperGauge.gameObject.SetActive(true);
                    currentUtilGauge = TrapperGauge;
                    break;
            }
        }

        public UIGenericGaugeHandler CurrentUtilGauge => currentUtilGauge;
    }
}
