using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hadal.PostProcess;
using NaughtyAttributes;

public class ReticleHandler : MonoBehaviour
{
    [Header("Primary Color")]
    [SerializeField] ReticleEmissiveSettings currentSettings;

    [Header("Glow Color")]
    public Color glowColor;
    public bool overrideGlowAlpha = true;

    [Header("Components")]
    [SerializeField] List<Image> glowComponents;
    [SerializeField] List<Image> primaryComponents;
    [SerializeField] List<Image> secondaryComponents;
    [SerializeField] List<Image> tertiaryComponents;

    void OnValidate()
    {
        RefreshColors();
    }

    [Button("Force Color Refresh")]
    void RefreshColors()
    {
        UpdateAllColors();

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

    void UpdateAllColors()
    {
        foreach (Image img in primaryComponents)
        {
            img.material.SetColor("_Color", currentSettings.primaryEmissiveColor);
            img.material.SetFloat("_Alpha", currentSettings.primaryEmissiveAlpha);
        }

        foreach (Image img in secondaryComponents)
        {
            img.material.SetColor("_Color", currentSettings.secondaryEmissiveColor);
            img.material.SetFloat("_Alpha", currentSettings.secondaryEmissiveAlpha);
        }

        foreach (Image img in tertiaryComponents)
        {
            img.material.SetColor("_Color", currentSettings.tertiaryEmissiveColor);
            img.material.SetFloat("_Alpha", currentSettings.tertiaryEmissiveAlpha);
        }
    }
}
