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
        public static DebugCommand<int> C_AISetState;
        public static DebugCommand<float> C_AIStun;
        public static DebugCommand<int> C_AISetSlowStacks;
        public static DebugCommand C_AIClearDebuffs;

        //! Player
        public static DebugCommand<int> C_SetHp;
        public static DebugCommand<float> C_SetMaxSpeed;
        public static DebugCommand<float> C_SetAccel;
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
            C_AIStop = new DebugCommand("aistop", "Stops the creature movement", "aistop", () =>
            {
                AIBrain brain = FindObjectOfType<AIBrain>();
                brain.NavigationHandler.Disable();
            });
            C_AIMove = new DebugCommand("aimove", "Allows the creature to move", "aimove", () =>
            {
                FindObjectOfType<AIBrain>().NavigationHandler.Enable();
            });
            C_AISetSpeed = new DebugCommand<float>("aisetspeed", "Sets the speed multiplier of AI", "aisetspeed", (x) =>
            {
                FindObjectOfType<AIBrain>().NavigationHandler.SetDebugVelocityMultiplier(x);
            });
            C_AISetState = new DebugCommand<int>("aisetstate", "Forces the AI into (1)Anticipation, (2)Engagement, (3)Recovery, (4)Cooldown", "aisetstate", (x) =>
            {
                switch (x)
                {
                    case 1:
                        FindObjectOfType<AIBrain>().RuntimeData.SetBrainState(BrainState.Anticipation);
                        break;
                    case 2:
                        FindObjectOfType<AIBrain>().RuntimeData.SetBrainState(BrainState.Engagement);
                        break;
                    case 3:
                        FindObjectOfType<AIBrain>().RuntimeData.SetBrainState(BrainState.Recovery);
                        break;
                    case 4:
                        FindObjectOfType<AIBrain>().RuntimeData.SetBrainState(BrainState.Cooldown);
                        break;
                    default:
                        break;
                }
            });
            C_AIStun = new DebugCommand<float>("aistun", "Stuns the AI for the provided amount of (x) in seconds.", "aistun", x =>
            {
                FindObjectOfType<AIBrain>().HealthManager.TryStun(x);
            });
            C_AISetSlowStacks = new DebugCommand<int>("aisetslowstacks", "Sets the current slow stacks of the AI, which affects its overall movement speed.", "aisetslowstacks", x =>
            {
                FindObjectOfType<AIBrain>().HealthManager.SetSlowStacks(x);
            });
            C_AIClearDebuffs = new DebugCommand("aicleardebuffs", "Clears all negative effects on the AI.", "aicleardebuffs", () =>
            {
                AIBrain brain = FindObjectOfType<AIBrain>();
                brain.StopStun(); // stop stun
                brain.HealthManager.SetSlowStacks(0); //stop slow
            });

            //! Player
            C_SetHp = new DebugCommand<int>("sethp", "Sets the health of player", "sethp", (x) =>
            {
                var player = PlayerManager.Instance.LocalPlayerController;
                player.GetInfo.HealthManager.Debug_SetCurrentHealth(x);
            });
            C_SetMaxSpeed = new DebugCommand<float>("setmaxspeed", "Sets the max velocity of player", "setmaxspeed", (x) =>
            {
                var player = PlayerManager.Instance.LocalPlayerController;
                player.GetInfo.Mover.Speed.Max = x;
            });
            C_SetAccel = new DebugCommand<float>("setaccel", "Sets the acceleration of player", "setaccel", (x) =>
            {
                var player = PlayerManager.Instance.LocalPlayerController;
                player.GetInfo.Mover.Accel.MaxCummulation = x;
            });
            C_GodMode = new DebugCommand("god", "Turns the player into god", "god", () =>
            {
                var player = PlayerManager.Instance.LocalPlayerController;
                player.GetInfo.HealthManager.Debug_ToggleGodMode();
            });

            commandList = new List<object>
            {
                C_Help,
                C_Close,
                
                C_AIStop,
                C_AIMove,
                C_AISetSpeed,
                C_AISetState,
                C_AIStun,
                C_AISetSlowStacks,
                C_AIClearDebuffs,
                
                C_SetHp,
                C_SetMaxSpeed,
                C_SetAccel,
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
