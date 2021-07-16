using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Networking.UI
{
    public class MainMenuChooseClass : MonoBehaviour
    {
        public MainMenuClassSelector Selector;
        public PlayerClassType type;
        
        private bool selected = false;
        
        public void ChooseClass()
        {
            PlayerClassType c = Selector.CurrentChosenClass;
            if (c == PlayerClassType.Invalid && Selector.PlayerClassAvailable(type))
            {
                Selector.ChooseClass(type);
                selected = true;
            }
        }

        public void UnchooseClass()
        {
            if (Selector.CurrentChosenClass == type)
            {
                Selector.UnchooseClass(type);
                selected = false;
            }
        }

        public void ToggleButton()
        {
            if (!selected) ChooseClass();
            else UnchooseClass();
            
        }
    }
}
