using System.Collections;
using System.Collections.Generic;
using Hadal.Networking;
using Hadal.Networking.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UIPauseInfoHandler : MonoBehaviour
    {
        [Header("Information")]
        [SerializeField] private List<PlayerClassInfo> ClassInfos;

        [Header("References")] 
        [SerializeField] private Image ClassIcon;
        [SerializeField] private TextMeshProUGUI PassiveTitle;
        [SerializeField] private TextMeshProUGUI PassiveDesc;
        [SerializeField] private TextMeshProUGUI ActiveTitle;
        [SerializeField] private TextMeshProUGUI ActiveDesc;

        public void InitInformation(PlayerClassType type)
        {
            Debug.LogWarning($"Pause UI type: {type}");
            PlayerClassInfo info = GetClassInfo(type);
            ClassIcon.sprite = info.ClassIcon;
            PassiveTitle.text = info.PassiveTitle;
            PassiveDesc.text = info.PassiveDesc;
            ActiveTitle.text = info.ActiveTitle;
            ActiveDesc.text = info.ActiveDesc;
        }
        
        PlayerClassInfo GetClassInfo(PlayerClassType classType)
        {
            foreach (PlayerClassInfo info in ClassInfos)
            {
                if (info.ClassType == classType) return info;
            }

            return null;
        }
    }
}
