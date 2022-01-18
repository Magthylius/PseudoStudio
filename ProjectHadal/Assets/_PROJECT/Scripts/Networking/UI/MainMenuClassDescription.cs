using System;
using System.Collections;
using System.Collections.Generic;
using Magthylius.LerpFunctions;
using static Magthylius.Utilities.MathUtil;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Hadal.Networking.UI
{
    public class MainMenuClassDescription : MonoBehaviour
    {

        [Header("References")] 
        public CanvasGroup CanvasGroup;

        private CanvasGroupFader cgf;
        
        [Header("Text")]
        public TextMeshProUGUI ClassTitle;
        public TextMeshProUGUI ClassDesc;
        public TextMeshProUGUI PassiveTitle;
        public TextMeshProUGUI PassiveDesc;
        public TextMeshProUGUI ActiveTitle;
        public TextMeshProUGUI ActiveDesc;

        [Header("Settings")] 
        public string PassivePrefix = "PASSIVE: ";
        public string ActivePrefix = "ACTIVE: ";

        //[Space(10f)] 
        //[SerializeField] private List<ClassDescriptionBlock> ClassDescriptionBlocks;
        [SerializeField] private List<PlayerClassInfo> ClassInfos; 

        private void Start()
        {
            cgf = new CanvasGroupFader(CanvasGroup, true, false);
        }

        private void OnDisable()
        {
            if (cgf != null) cgf.SetTransparent();
        }

        public void UpdateDescriptions(PlayerClassType classType)
        {
            
            PlayerClassInfo info = GetClassInfo(classType);
            ClassTitle.text = info.ClassName;
            ClassDesc.text = info.ClassDesc;
            PassiveTitle.text = $"{PassivePrefix}{info.PassiveTitle}";
            PassiveDesc.text = $"{info.PassiveDesc}";
            ActiveTitle.text = $"{ActivePrefix}{info.ActiveTitle}";
            ActiveDesc.text = $"{info.ActiveDesc}";
            
            StartCoroutine(FadeCanvas());
            
            IEnumerator FadeCanvas()
            {
                cgf.StartFadeIn();
                while (cgf.IsFading)
                {
                    cgf.Step(5f * Time.deltaTime);
                    yield return null;
                }
            }
        }

        PlayerClassInfo GetClassInfo(PlayerClassType classType)
        {
            foreach (PlayerClassInfo info in ClassInfos)
            {
                if (info.ClassType == classType) return info;
            }

            return null;
        }
        

        public void Desc_Hunter() => UpdateDescriptions(PlayerClassType.Harpooner);
        public void Desc_Medic() => UpdateDescriptions(PlayerClassType.Saviour);
        public void Desc_Trapper() => UpdateDescriptions(PlayerClassType.Trapper);
        public void Desc_Tracker() => UpdateDescriptions(PlayerClassType.Informer);
        public void Desc_Invalids() => UpdateDescriptions(PlayerClassType.Invalid);
        
        [Button("Debug Harpooner Desc")]
        public void DebugDesc() => UpdateDescriptions(PlayerClassType.Harpooner);
    }
}
