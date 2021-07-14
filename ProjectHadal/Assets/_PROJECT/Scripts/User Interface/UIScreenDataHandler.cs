using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Hadal.UI;
using Magthylius.LerpFunctions;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenDataHandler : MonoBehaviour
{
    private UIManager playerUI;
    private Transform playerTransform;

    private bool initialized = false;
    
    private void FixedUpdate()
    {
        if (!initialized) return;

        UpdateHealth();
        UpdateDepth(-playerTransform.position.z);
        UpdateDistance(playerUI.ShootTracer.HitDistance);
    }

    public void InjectDependencies(UIManager ui, Transform pTransform)
    {
        playerUI = ui;
        playerTransform = pTransform;

        initialized = true;
    }

    [Header("Health")] public UIDataFormatBehaviour healthData;
    public float healthLerpSpeed = 5f;
    private float displayHealth = 100;
    private int targetHealth = 100;

    [Space(5f)] 
    public GameObject criticalText;
    public Image healthFill;
    public Image healthFrame;
    public int badHealthGate = 50;
    public Color badHealthColor;
    public int criticalHealthGate = 25;
    public Color criticalHealthColor;
    
    public void UpdateHealth()
    {
        displayHealth = Mathf.Lerp(displayHealth, targetHealth, healthLerpSpeed * Time.deltaTime);
        if (targetHealth == 0)
            healthData.UpdateTextNoSuffix("ERROR");
        else
            healthData.UpdateText(Mathf.RoundToInt(displayHealth));

        healthFill.fillAmount = displayHealth / 100f;
    }

    public void UpdateTargetHealth(int target)
    {
        targetHealth = target;

        if (targetHealth <= criticalHealthGate)
        {
            healthFill.color = criticalHealthColor;
            healthFrame.color = criticalHealthColor;
            criticalText.SetActive(true);
        }
        else if (targetHealth <= badHealthGate)
        {
            healthFill.color = badHealthColor;
            healthFrame.color = badHealthColor;
            criticalText.SetActive(false);
        }
        else
        {
            healthFill.color = Color.white;
            healthFrame.color = Color.white;
            criticalText.SetActive(false);
        }
        
    }

    [Header("Energy")]
    public UIDataFormatBehaviour energyData;
    public void UpdateEnergy()
    {
        
    }
    
    [Header("Distance")]
    public UIDataFormatBehaviour distanceData;
    public void UpdateDistance(float distance)
    {
        distanceData.UpdateText((int)distance);
    }
    
    [Header("Depth")]
    public UIDataFormatBehaviour depthData;
    public float initialDepth = 9274;
    public void UpdateDepth(float depth)
    {
        depthData.UpdateText((int)(depth + initialDepth));
    }
}
