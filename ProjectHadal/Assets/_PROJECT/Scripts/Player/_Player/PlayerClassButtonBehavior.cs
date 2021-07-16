using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Player
{
    public class PlayerClassButtonBehavior : MonoBehaviour
    {
        [SerializeField] private PlayerClassData PlayerClass;

        public void SelectPlayerClass()
        {
            //PlayerClassManager.Instance.SetPlayerClass(PlayerClass);
        }
    }
}
