using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! For resetting static classes
namespace Hadal.Player
{
    public class PlayerStaticHandler : MonoBehaviour, IStaticResetter
    {
        private void Start()
        {
            StaticClassManager.Instance.ResetEvent += Reset;
        }

        public void Reset()
        {
            LocalPlayerData.Reset();
            NetworkData.Reset();
        }

    }
}
