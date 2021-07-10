using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Hadal.UI;
using UnityEngine;

public class UIScreenDataHandler : MonoBehaviour
{
    private UIManager playerUI;
    private Transform playerTransform;

    private bool initialized = false;
    
    private void FixedUpdate()
    {
        if (!initialized) return;
        
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
    public void UpdateHealth()
    {
        
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
