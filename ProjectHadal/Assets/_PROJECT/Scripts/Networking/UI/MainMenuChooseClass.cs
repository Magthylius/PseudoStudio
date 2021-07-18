using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Networking.UI
{
    public class MainMenuChooseClass : MonoBehaviour
    {
        public MainMenuClassSelector Selector;
        public MainMenuHighlightBehaviour CorrespondingHighlight;
        public PlayerClassType type;
        
        private bool selected = false;

        public void ChooseClass()
        {
            if (CorrespondingHighlight.IsSelectedByOthers) return;
            
            PlayerClassType c = Selector.CurrentClassType;
            if (c == PlayerClassType.Invalid && Selector.PlayerClassAvailable(type))
            {
                Selector.ChooseClass(type);
                Selector.SetClassChooser(this);
                selected = true;
            }
        }

        public void UnchooseClass()
        {
            if (Selector.CurrentClassType == type)
            {
                Selector.UnchooseClass();
                Selector.SetClassChooser(null);
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
