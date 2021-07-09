using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UIFillerBehaviour : MonoBehaviour
    {
        public Image FilledImage;
        public Image HollowImage;

        public void Disable() => gameObject.SetActive(false);
        public void Enable() => gameObject.SetActive(true);
        
        public void ToFilled()
        {
            FilledImage.gameObject.SetActive(true);
            HollowImage.gameObject.SetActive(false);
        }

        public void ToHollow()
        {
            FilledImage.gameObject.SetActive(false);
            HollowImage.gameObject.SetActive(true);
        }
    }
}
