using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.UnitySoku;
using Hadal.Player;
using System.Linq;

//! C: Jon
namespace Hadal.AI.Caverns
{
    /// <summary>
    /// For data passback through events back to manager
    /// </summary>
    public struct CavernPlayerData
    {
        public CavernHandler Handler;
        public PlayerController Player;

        public CavernPlayerData(CavernHandler cavernHandler, PlayerController playerController)
        {
            Handler = cavernHandler;
            Player = playerController;
        }
    }

    public delegate void CavernHandlerReturn(CavernPlayerData data);
    public delegate void CavernHandlerAIReturn(CavernHandler handler);

    /// <summary>
    /// For cavern identification.
    /// </summary>
    public enum CavernTag
    {
        Invalid = 0,
        Starting_Grounds,
        Lair_Grounds,
        Hydrothermal_Vents,
        Bioluminescent_Cavern,
        Staglamite_Cavern,
        Custom_Point
    }

    /// <summary>
    /// Manages all the other handlers. Is a singleton.
    /// </summary>
    public class CavernManager : SingletonSoft<CavernManager>
    {
        List<CavernHandler> handlerList = new List<CavernHandler>();
        CavernHandler aiAtHandler;

        protected override void Awake()
        {
            base.Awake();
            aiAtHandler = null;
        }

        void Start()
        {
            
        }

        public void OnPlayerEnterCavern(CavernPlayerData data)
        {
            //return data;
        }

        public void OnPlayerLeftCavern(CavernPlayerData data)
        {
            //return data;
        }

        public void OnAIEnterCavern(CavernHandler handler)
        {
            if (aiAtHandler != handler)
                aiAtHandler = handler;
        }

        public void OnAILeaveCavern(CavernHandler handler)
        {
            if (aiAtHandler == handler)
                aiAtHandler = null;
        }

        /// <summary>
        /// Attempts to get an isolated player.
        /// </summary>
        /// <returns>An isolated player, or null if no one is isolated.</returns>
        public PlayerController GetIsolatedPlayer()
        {
            PlayerController isolatedPlayer = null;

            foreach(CavernHandler handler in handlerList)
            {
                if (handler.GetPlayerCount == 1)
                {
                    isolatedPlayer = handler.GetPlayersInCavern[0];
                    break;
                }
            }

            return isolatedPlayer;
        }

        public CavernTag GetCavernTagOfAILocation()
        {
            if (aiAtHandler == null)
                return CavernTag.Invalid;
            return aiAtHandler.cavernTag;
        }

        public CavernHandler GetHandlerOfAILocation()
            => aiAtHandler;

        public void InjectHandler(CavernHandler handler) => handlerList.Add(handler);
        public CavernHandler GetHandlerOfTag(CavernTag tag) => handlerList.Where(h => h.cavernTag == tag).SingleOrDefault();
    }
}
