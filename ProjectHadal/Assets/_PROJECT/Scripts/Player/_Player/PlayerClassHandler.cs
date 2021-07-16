using Hadal.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Player
{
    public class PlayerClassHandler : MonoBehaviour
    {
        public static PlayerClassHandler Instance;
        [SerializeField] private PlayerClassData PlayerClass;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        [ContextMenu("Apply Class")]
        public void ApplyClass()
        {
            PlayerClass.SetUpUtility();
        }

        public void SetPlayerClass(PlayerClassData newPlayerClass)
        {
            PlayerClass = newPlayerClass;
        }
    }
}
