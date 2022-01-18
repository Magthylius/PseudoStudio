using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.Networking.Hyperlinks
{
    public class HyperlinkButtonBehaviour : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] string urlLink;

        void OnValidate()
        {
            /*if (button == null) button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(URLOpen);
            }*/
        }

        public void URLOpen() => Application.OpenURL(urlLink);
    }
}
