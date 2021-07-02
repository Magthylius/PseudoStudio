using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! For resetting static classes
namespace Hadal.Player
{
    public class PlayerStaticHandler : MonoBehaviour, IStaticResetter
    {
        public void Start()
        {
            Reset(false);
            GameManager.Instance.SceneLoadedEvent += Reset;
        }

        public void Reset(bool booleanData)
        {
            LocalPlayerData.Reset();
            NetworkData.Reset();
        }

    }
}
