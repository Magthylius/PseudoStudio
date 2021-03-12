using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hadal.UI
{
    public class PlayerNameTrackerBehaviour : UITrackerBehaviour
    {
        TextMeshProUGUI nameText;
        
        public void UpdateText(string name)
        {
            nameText.text = name;
        }

    }
}
