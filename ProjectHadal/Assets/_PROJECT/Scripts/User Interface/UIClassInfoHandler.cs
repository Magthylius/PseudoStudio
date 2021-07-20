using System.Collections;
using System.Collections.Generic;
using Hadal.Networking;
using TMPro;
using UnityEngine;

namespace Hadal.UI
{
    public class UIClassInfoHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI classInfoText;

        public void SetClass(PlayerClassType classType)
        {
            switch (classType)
            {
                case PlayerClassType.Harpooner:
                    classInfoText.text = $"HARPOONER";
                    break;
                case PlayerClassType.Informer:
                    classInfoText.text = $"INFORMER";
                    break;
                case PlayerClassType.Saviour:
                    classInfoText.text = $"SAVIOUR";
                    break;
                case PlayerClassType.Trapper:
                    classInfoText.text = $"Trapper";
                    break;
                default:
                    classInfoText.text = $"IMPOSTER";
                    break;
            }
        }
        
    }
}
