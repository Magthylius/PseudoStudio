using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.UsablePlayerBridge;

//! For resetting static classes
namespace Hadal.UsablePlayerBridge
{
    public class UsablePlayerBridgeStaticHandler : MonoBehaviour, IStaticResetter
    {
        private void OnEnable()
        {
            StaticClassManager.Instance.ResetEvent += Reset;
        }
        
        private void OnDisable()
        {
            StaticClassManager.Instance.ResetEvent -= Reset;
        }

        public void Reset()
        {
            UITrackerBridge.Reset();
        }

    }
}
