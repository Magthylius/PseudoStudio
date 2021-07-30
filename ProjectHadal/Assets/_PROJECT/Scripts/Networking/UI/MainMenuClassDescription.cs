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
        [System.Serializable]
        public class ClassDescriptionBlock
        {
            public PlayerClassType ClassType;

            [Space(10f)] 
            public string ClassTitle;
            [TextArea] public string ClassDesc;
            
            [Space(10f)]
            public string PassiveTitle;
            [TextArea] public string PassiveDesc;
            
            [Space(10f)]
            public string ActiveTitle;
            [TextArea] public string ActiveDesc;
        }

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

        [Space(10f)] 
        [SerializeField] private List<ClassDescriptionBlock> ClassDescriptionBlocks;

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
            ClassDescriptionBlock block = FindDescriptionBlock(classType);

            if (block != null)
            {
                ClassTitle.text = block.ClassTitle;
                ClassDesc.text = block.ClassDesc;
                PassiveTitle.text = block.PassiveTitle;
                PassiveDesc.text = block.PassiveDesc;
                ActiveTitle.text = block.ActiveTitle;
                ActiveDesc.text = block.ActiveDesc;
            }

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

        ClassDescriptionBlock FindDescriptionBlock(PlayerClassType classType)
        {
            foreach (ClassDescriptionBlock block in ClassDescriptionBlocks)
            {
                if (block.ClassType == classType) return block;
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
