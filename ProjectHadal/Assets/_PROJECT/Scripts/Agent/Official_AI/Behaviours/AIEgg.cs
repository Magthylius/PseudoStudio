using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.AI.States;

namespace Hadal.AI
{
    public class AIEgg : MonoBehaviour
    {
        int maxHealth;
        int curHealth;
        public delegate void MaxConfidenceOnEggDestroyed(bool isEggDestroyed);
        public event MaxConfidenceOnEggDestroyed eggDestroyedEvent;

        public void RaiseOnEggDestroyed()
        {
            if (eggDestroyedEvent != null)
                eggDestroyedEvent(false);
        }

        // Start is called before the first frame update
        void Start()
        {

            maxHealth = 40;
        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}
