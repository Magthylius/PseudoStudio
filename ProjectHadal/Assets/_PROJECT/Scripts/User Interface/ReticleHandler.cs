using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleHandler : MonoBehaviour
{
    public Color primaryColor;
    public Color glowColor;

    [SerializeField] List<Image> primaryComponents;
    [SerializeField] List<Image> glowComponents;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnValidate()
    {
        foreach (Image img in primaryComponents)
        {
            img.color = primaryColor;
            img.raycastTarget = false;
        }

        foreach (Image img in glowComponents)
        {
            img.color = glowColor;
            img.raycastTarget = false;
        }
    }
}
