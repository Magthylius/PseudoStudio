using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Networking.UI
{
    [CreateAssetMenu(menuName = "Player/Class Info")]
    public class PlayerClassInfo : ScriptableObject
    {
        public PlayerClassType ClassType;

        [Header("Images")] 
        public Sprite ClassIcon;
    
        [Header("Descriptions")]
        public string ClassName;
        [TextArea] public string ClassDesc;
            
        [Space(10f)]
        public string PassiveTitle;
        [TextArea] public string PassiveDesc;
            
        [Space(10f)]
        public string ActiveTitle;
        [TextArea] public string ActiveDesc;
    }
}
