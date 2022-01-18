using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hadal.UI
{
    public class UIDataFormatBehaviour : MonoBehaviour
    {
        public TextMeshProUGUI TMP;
        public string Suffix;

        public void UpdateText(object text)
        {
            TMP.text = text + Suffix;
        }

        public void UpdateTextNoSuffix(object text)
        {
            TMP.text = text.ToString();
        }
    }
}
