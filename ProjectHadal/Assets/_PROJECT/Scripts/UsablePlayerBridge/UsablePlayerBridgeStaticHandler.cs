using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.UsablePlayerBridge;

//! For resetting static classes
namespace Hadal.UsablePlayerBridge
{
    public class UsablePlayerBridgeStaticHandler : MonoBehaviour, IStaticResetter
    {
        public void Start()
        {
            Reset(false);
            GameManager.Instance.SceneLoadedEvent += Reset;
        }
        
        public void Reset(bool booleanData)
        {
            UITrackerBridge.Reset();
        }

    }
}
