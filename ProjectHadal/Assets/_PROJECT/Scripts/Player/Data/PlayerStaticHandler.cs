using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! For resetting static classes
namespace Hadal.Player
{
    public class PlayerStaticHandler : MonoBehaviour
    {
        void Start()
        {
            LocalPlayerData.Reset();
            NetworkData.Reset();
        }

    }
}
