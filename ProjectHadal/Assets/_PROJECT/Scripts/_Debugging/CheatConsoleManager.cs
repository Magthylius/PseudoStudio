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
        private bool showHelp = false;
        
        private string input;
        private Vector2 helpScroll;
        private PlayerController localPlayerController;
        
        public List<object> commandList;

        //! Console
        public static DebugCommand C_Help;
        public static DebugCommand C_Close;
        
        //! AI
        public static DebugCommand C_AIStop;
        public static DebugCommand C_AIMove;
        public static DebugCommand<float> C_AISetSpeed;
        
        //! Player
        public static DebugCommand<int> C_SetHp;
        public static DebugCommand<float> C_SetSpeed;
        public static DebugCommand<float> C_SetMaxSpeed;
        public static DebugCommand<float> C_SetAcce;
        public static DebugCommand C_GodMode;
        
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
                //CloseConsole();
            }
        }

        #endregion

        private void Start()
        {
            //AIBrain aiBrain = FindObjectOfType<AIBrain>();
            
            //! Implement commands here
            //! Console
            C_Help = new DebugCommand("help", "Toggles list of commands", "help", () =>
            {
                showHelp = !showHelp;
            });
            C_Close = new DebugCommand("close", "Closes this console", "close", () =>
            {
                showConsole = false;
                CloseConsole();
            });
            
            //! AI
            C_AIStop = new DebugCommand("AIStop", "Stops the creature movement", "AIStop", () =>
            {
                FindObjectOfType<AIBrain>().NavigationHandler.SetDebugVelocityMultiplier(0f);
            });
            C_AIMove = new DebugCommand("AIMove", "Allows the creature to move", "AIMove", () =>
            {
                FindObjectOfType<AIBrain>().NavigationHandler.ResetDebugVelocityMultiplier();
            });
            C_AISetSpeed = new DebugCommand<float>("AISetSpeed", "Sets the speed multiplier of AI", "AISetSpeed", (x) =>
            {
                FindObjectOfType<AIBrain>().NavigationHandler.SetDebugVelocityMultiplier(x);
            });
            
            //! Player
            C_SetHp = new DebugCommand<int>("SetHp", "Sets the health of player", "SetHp", (x) =>
            {
                //! TODO: bind to player health
            });
            C_SetSpeed = new DebugCommand<float>("SetSpeed", "Sets the speed multiplier of player", "SetSpeed", (x) =>
            {
                //! TODO: bind to player speed
            });
            C_SetMaxSpeed = new DebugCommand<float>("SetMaxSpeed", "Sets the max velocity of player", "SetMaxSpeed", (x) =>
            {
                //! TODO: bind to player max speed
            });
            C_SetAcce = new DebugCommand<float>("SetAcce", "Sets the acceleration of player", "SetAcce", (x) =>
            {
                //! TODO: bind to player acceleration
            });
            C_GodMode = new DebugCommand("God", "Turns the player into god", "God", () =>
            {
                //! TODO: bind to player health
            });

            commandList = new List<object>
            {
                C_Help,
                C_Close,
                C_AIStop,
                C_AIMove,
                C_AISetSpeed,
                C_SetHp,
                C_SetSpeed,
                C_SetMaxSpeed,
                C_SetAcce,
                C_GodMode
            };
        }

        private void OnGUI()
        {
            if (showConsole)
            {
                float y = 0f;
                
                if (showHelp)
                {
                    GUI.Box(new Rect(0f, y, Screen.width, 100), "");
                    Rect viewPort = new Rect(0f, 0f, Screen.width - 30f, 20f * commandList.Count);

                    helpScroll = GUI.BeginScrollView(new Rect(0f, y + 5f, Screen.width, 90f), helpScroll, viewPort);
                    for (int i = 0; i < commandList.Count; i++)
                    {
                        DebugCommandBase command = commandList[i] as DebugCommandBase;
                        string label = $"{command.Format} - {command.Desc}";
                        Rect labelRect = new Rect(5f, 20f * i, viewPort.width - 100f, 20f);
                        GUI.Label(labelRect, label);
                    }

                    GUI.EndScrollView();
                    y = 100f;
                }
                
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
            string[] properties = input.Split(' ');
            
            foreach (var command in commandList)
            {
                DebugCommandBase commandBase = command as DebugCommandBase;

                if (input.Contains(commandBase.ID))
                {
                    if (command as DebugCommand != null)
                    {
                        (command as DebugCommand).Invoke();
                    }
                    else if (command as DebugCommand<int> != null)
                    {
                        (command as DebugCommand<int>).Invoke(int.Parse(properties[1]));
                    }
                    else if (command as DebugCommand<float> != null)
                    {
                        (command as DebugCommand<float>).Invoke(float.Parse(properties[1]));
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
