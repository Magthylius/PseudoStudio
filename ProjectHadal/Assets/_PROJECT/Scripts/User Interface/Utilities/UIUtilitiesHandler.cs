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

        public void Initialize(PlayerClassType classType)
        {
            switch (classType)
            {
                case PlayerClassType.Saviour: 
                    SaviourGauge.gameObject.SetActive(true);
                    break;
                
                case PlayerClassType.Harpooner: break;
                
                case PlayerClassType.Informer:
                    InformerGauge.gameObject.SetActive(true);
                    break;
                
                case PlayerClassType.Trapper:
                    TrapperGauge.gameObject.SetActive(true);
                    break;
            }
        }
        
    }
}
