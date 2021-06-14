using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Hadal.Player;
using Hadal.Networking;
using Hadal.UI;

namespace Hadal.Debugging
{
    public class CheatConsoleManager : MonoBehaviour
    {
        private bool showConsole = false;
        private string input;
        private PlayerController localPlayerController;

        public static DebugCommand C_StopCreature;
        public List<object> commandList;

        //! Called from Input system
        public void OnToggleDebug(InputValue value)
        {
            showConsole = !showConsole;
            
            if (showConsole)
            {
                OpenConsole();
            }
            else
            {
               CloseConsole();
            }
        }

        private void Awake()
        {
            //! Implement commands here
            /*C_StopCreature = new DebugCommand("StopCreature", "Stops the creature movement", "StopCreature") () =>
            {
                
            });*/
        }

        private void OnGUI()
        {
            if (!showConsole) return;
            
            GUI.SetNextControlName("GUI Console");
            float y = 0f;
            GUI.Box(new Rect(0, y, Screen.width, 30f), "");
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0f);

            input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);
            
            //GUI.FocusControl("GUI Console");
        }

        void OpenConsole()
        {
            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.Confined;

            if (ResolvePlayerController()) localPlayerController.UI.PNTR_Pause();
                
            GUI.FocusControl("GUI Console");
            input = "";
        }

        void CloseConsole()
        {
            Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
                
            if (ResolvePlayerController()) localPlayerController.UI.PNTR_Resume();
        }
        
        bool ResolvePlayerController()
        {
            if (localPlayerController != null) return true;
            
            localPlayerController = PlayerManager.Instance.GetController(NetworkEventManager.Instance.LocalPlayer);
            if (localPlayerController != null) return true;
            return false;
        }
    }
}
