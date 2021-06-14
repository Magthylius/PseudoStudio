using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hadal.AI;
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

        public static DebugCommand C_AIStop;
        public static DebugCommand C_AIMove;
        public List<object> commandList;
        
        #region Input system

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

        public void OnReturn(InputValue value)
        {
            if (showConsole)
            {
                HandleInput();
                input = "";
                showConsole = false;
                CloseConsole();
            }
        }

        #endregion

        private void Start()
        {
            AIBrain aiBrain = FindObjectOfType<AIBrain>();
            
            //! Implement commands here
            C_AIStop = new DebugCommand("AIStop", "Stops the creature movement", "AIStop", () =>
            {
                aiBrain.NavigationHandler.SetDebugVelocityMultiplier(0f);
            });
            
            C_AIMove = new DebugCommand("AIMove", "Allows the creature to move", "AIMove", () =>
            {
                aiBrain.NavigationHandler.ResetDebugVelocityMultiplier();
            });

            commandList = new List<object>
            {
                C_AIStop,
                C_AIMove
            };
        }

        private void OnGUI()
        {
            
            
            if (showConsole)
            {
                float y = 0f;
                GUI.Box(new Rect(0, y, Screen.width, 30f), "");
                GUI.backgroundColor = new Color(0f, 0f, 0f, 0f);

                GUI.SetNextControlName("console");
                input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);
                GUI.FocusControl("console");
            }
   
            //GUI.FocusControl("GUI Console");
        }

        void HandleInput()
        {
            foreach (var command in commandList)
            {
                DebugCommandBase commandBase = command as DebugCommandBase;

                if (input.Contains(commandBase.ID))
                {
                    if (command as DebugCommand != null)
                    {
                        (command as DebugCommand).Invoke();
                    }
                }
            }
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
