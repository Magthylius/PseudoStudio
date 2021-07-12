using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Hadal.UI;
using Magthylius.LerpFunctions;
using UnityEngine;

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

    [Header("Health")] 
    public UIDataFormatBehaviour healthData;
    public float healthLerpSpeed = 5f;
    private float displayHealth = 100;
    private int targetHealth = 100;
    public void UpdateHealth()
    {
        displayHealth = Mathf.Lerp(displayHealth, targetHealth, healthLerpSpeed * Time.deltaTime);
        if (targetHealth == 0)
            healthData.UpdateTextNoSuffix("ERROR");
        else
            healthData.UpdateText(Mathf.RoundToInt(displayHealth));
    }

    public void UpdateTargetHealth(int target)
    {
        targetHealth = target;
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
