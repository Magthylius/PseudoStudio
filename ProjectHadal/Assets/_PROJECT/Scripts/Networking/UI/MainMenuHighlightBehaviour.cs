using System.Collections;
using System.Collections.Generic;
using Hadal.Networking;
using Magthylius.Utilities;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.Networking.UI
{
    public class MainMenuHighlightBehaviour : MonoBehaviour
    {
        public Image image;
        public PlayerClassType ClassType;

        public void Select(Color setColor)
        {
            image.enabled = true;
            image.color = setColor;
        }

        public void Deselect()
        {
            image.enabled = false;
            image.color = Color.white;
        }
    }
}
