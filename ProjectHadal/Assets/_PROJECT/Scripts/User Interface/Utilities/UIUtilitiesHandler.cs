using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIUtilitiesHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI utilitiesTMP;
    [SerializeField] bool toUpperLabel;

    public void UpdateCurrentUtilities(string utilityName)
    {
        if (toUpperLabel) utilityName = utilityName.ToUpper();
        utilitiesTMP.text = utilityName;
    }
}
