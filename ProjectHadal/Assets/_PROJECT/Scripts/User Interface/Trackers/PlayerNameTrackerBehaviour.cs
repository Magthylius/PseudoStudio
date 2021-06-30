using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hadal.UI
{
    public class PlayerNameTrackerBehaviour : UITrackerBehaviour
    {
        public TextMeshProUGUI NameText;
        
        public void UpdateText(string name)
        {
            NameText.text = name;
        }

    }
}
