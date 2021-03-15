using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AI
{
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance;

        public Transform patrolPositionParent;

        Transform[] patrolPositions;

        void Awake()
        {
            if (Instance != null) Destroy(this);
            else Instance = this;
        }

        void Start()
        {
            patrolPositions = patrolPositionParent.GetComponentsInChildren<Transform>();
        }

        public Transform[] GetPositions() => patrolPositions;
    }
}
