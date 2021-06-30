using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! For resetting static classes
public class UsablePlayerBridgeStaticHandler : MonoBehaviour
{
    void Start()
    {
        UITrackerBridge.Reset();
    }
    
}
