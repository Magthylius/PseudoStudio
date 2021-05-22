using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleHandler : MonoBehaviour
{
    [Header("Primary Color")]
    public Color primaryColor;
    public bool overridePrimaryAlpha = true;

    [Header("Glow Color")]
    public Color glowColor;
    public bool overrideGlowAlpha = true;

    [SerializeField] List<Image> primaryComponents;
    [SerializeField] List<Image> glowComponents;

    void OnValidate()
    {
        foreach (Image img in primaryComponents)
        {
            if (!overridePrimaryAlpha)
            {
                img.color = primaryColor; 
            }
            else
            {
                Color newColor = primaryColor;
                newColor.a = img.color.a;
                img.color = newColor;
            }

            img.raycastTarget = false;
        }

        foreach (Image img in glowComponents)
        {
            if (!overrideGlowAlpha)
            {
                img.color = glowColor;   
            }
            else
            {
                Color newColor = glowColor;
                newColor.a = img.color.a;
                img.color = newColor;
            }

            img.raycastTarget = false;
        }
    }
}
