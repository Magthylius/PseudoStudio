using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI
{
    public class SonicDartTrackerBehaviour : UITrackerBehaviour
    {
        private static readonly int Started = Animator.StringToHash("Started");

        public void Activate()
        {
            GetComponent<Animator>().SetBool(Started, true);
        }
    }

}