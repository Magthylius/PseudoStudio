using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI
{
    public class UIGenericGaugeHandler : MonoBehaviour
    {
        public List<UIFillerBehaviour> HandledFillers;

        public void LoadedGauge(int index)
        {
            HandledFillers[index].ToFilled();
        }
        
        public void UnloadedGauge(int index)
        {
            HandledFillers[index].ToHollow();
        }

        public void UpdateFullGauge(int index)
        {
            index = Mathf.Clamp(index, 0, HandledFillers.Count - 1);
            Debug.LogWarning($"index captured: {index}");

            for (int i = 0; i < HandledFillers.Count; i++)
            {
                if (i < index) HandledFillers[i].ToFilled();
                else HandledFillers[i].ToHollow();
            }
        }
    }

}