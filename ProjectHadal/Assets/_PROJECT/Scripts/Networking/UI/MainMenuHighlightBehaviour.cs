using System.Collections;
using System.Collections.Generic;
using Hadal.Networking;
using Magthylius.Utilities;
using NaughtyAttributes;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.Networking.UI
{
    public class MainMenuHighlightBehaviour : MonoBehaviour
    {
        public Image image;
        public PlayerClassType ClassType;

        [SerializeField, ReadOnly] private bool selectedByOthers;

        public void Select(Color setColor, bool chosenByOthers)
        {
            selectedByOthers = chosenByOthers;
            image.enabled = true;
            image.color = setColor;
        }

        public void Deselect()
        {
            image.enabled = false;
            selectedByOthers = false;
            image.color = Color.white;
        }

        public bool IsSelectedByOthers => selectedByOthers;
    }
}
