using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Player
{
    public class PlayerDataHandler : MonoBehaviour
    {
        void Start()
        {
            LocalPlayerData.Reset();
            NetworkData.Reset();
        }

    }
}
