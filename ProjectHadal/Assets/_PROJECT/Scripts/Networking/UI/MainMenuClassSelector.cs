using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Networking.UI
{
    public enum PlayerClassType
    {
        Harpooner,
        Saviour,
        Trapper,
        Informer
    }

    public delegate void ClassEvent(PlayerClassType classType);
    public class MainMenuClassSelector : MonoBehaviour
    {
        public event ClassEvent ClassChosenEvent;

        public void SelectHarpooner() => ClassChosenEvent?.Invoke(PlayerClassType.Harpooner);
        public void SelectSaviour() => ClassChosenEvent?.Invoke(PlayerClassType.Saviour);
        public void SelectTrapper() => ClassChosenEvent?.Invoke(PlayerClassType.Trapper);
        public void SelectInformer() => ClassChosenEvent?.Invoke(PlayerClassType.Informer);
    }
}
