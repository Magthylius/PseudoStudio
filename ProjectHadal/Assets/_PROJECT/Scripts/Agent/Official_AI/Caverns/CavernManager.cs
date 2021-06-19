using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hadal.Player;
using Kit;
using Tenshi.UnitySoku;
using UnityEngine;

//! C: Jon
namespace Hadal.AI.Caverns
{
    /// <summary>
    ///     For data passback through events back to manager
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

    /// <summary>
    ///     For data passback through events back to manager
    /// </summary>
    public struct TunnelPlayerData
    {
        public TunnelBehaviour Tunnel;
        public PlayerController Player;
        
        public TunnelPlayerData(TunnelBehaviour tunnel, PlayerController playerController)
        {
            Tunnel = tunnel;
            Player = playerController;
        }
    }
    
    public delegate void CavernHandlerPlayerReturn(CavernPlayerData data);
    public delegate void CavernHandlerAIReturn(CavernHandler handler);

    public delegate void TunnelBehaviourPlayerReturn(TunnelPlayerData data);
    public delegate void TunnelBehaviourAIReturn(TunnelBehaviour tunnel);

    /// <summary>
    ///     For cavern identification.
    /// </summary>
    public enum CavernTag
    {
        Invalid = 0,
        Starting,
        Lair,
        Hydrothermal,
        Bioluminescent,
        Staglamite,
        Middle,
        Custom_Point
    }

    /// <summary>
    ///     Manages all the other handlers. Is a singleton.
    /// </summary>
    public class CavernManager : SingletonSoft<CavernManager>
    {
        [Header("Cavern Handler List")]
        [NaughtyAttributes.ReadOnly] public List<CavernHandler> handlerList = new List<CavernHandler>();

        [Header("Settings")] 
        [SerializeField] private bool debugPlayerEvents = false;
        [SerializeField] private bool debugAIEvents = false;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask aiLayer;
        
        //! Events
        public event CavernHandlerPlayerReturn PlayerEnterCavernEvent;
        public event CavernHandlerPlayerReturn PlayerLeftCavernEvent;
        public event CavernHandlerAIReturn AIEnterCavernEvent;
        public event CavernHandlerAIReturn AILeftCavernEvent;
        public event TunnelBehaviourPlayerReturn PlayerEnterTunnelEvent;
        public event TunnelBehaviourPlayerReturn PlayerLeftTunnelEvent;
        public event TunnelBehaviourAIReturn AIEnterTunnelEvent;
        public event TunnelBehaviourAIReturn AILeftTunnelEvent;

        private void OnValidate()
        {
            handlerList.RemoveAll(cavern => cavern == null);
        }

        protected override void Awake()
        {
            base.Awake();
            GetHandlerOfAILocation = null;
            StartCoroutine(CheckCavernInitialization());
        }

        #region Event Handling

        public void OnPlayerEnterCavern(CavernPlayerData data)
        {
            if (debugPlayerEvents) print(data.Player.PlayerName + " entered " + data.Handler.CavernName);
            PlayerEnterCavernEvent?.Invoke(data);
        }

        public void OnPlayerLeftCavern(CavernPlayerData data)
        {
            if (debugPlayerEvents) print(data.Player.PlayerName + " left " + data.Handler.CavernName);
            PlayerLeftCavernEvent?.Invoke(data);
        }

        public void OnAIEnterCavern(CavernHandler handler)
        {
            if (debugAIEvents) print("AI entered " + handler.CavernName);
            
            if (GetHandlerOfAILocation != handler)
                GetHandlerOfAILocation = handler;

            AIEnterCavernEvent?.Invoke(handler);
        }

        public void OnAILeaveCavern(CavernHandler handler)
        {
            if (debugAIEvents) print("AI left " + handler.CavernName);
            
            if (GetHandlerOfAILocation == handler)
                GetHandlerOfAILocation = null;

            AILeftCavernEvent?.Invoke(handler);
        }

        public void OnPlayerEnterTunnel(TunnelPlayerData data)
        {
            if (debugPlayerEvents) print(data.Player.PlayerName + " entered tunnel " + data.Tunnel.name);
            PlayerEnterTunnelEvent?.Invoke(data);
        }

        public void OnPlayerLeftTunnel(TunnelPlayerData data)
        {
            if (debugPlayerEvents) print(data.Player.PlayerName + " left tunnel " + data.Tunnel.name);
            PlayerLeftTunnelEvent?.Invoke(data);
        }

        public void OnAIEnterTunnel(TunnelBehaviour tunnel)
        {
            if (debugAIEvents) print("AI entered " + tunnel.name);
            AIEnterTunnelEvent?.Invoke(tunnel);
        }

        public void OnAILeftTunnel(TunnelBehaviour tunnel)
        {
            if (debugAIEvents) print("AI left " + tunnel.name);
            AILeftTunnelEvent?.Invoke(tunnel);
        }
        

        #endregion

        #region Cavern Handling

        [NaughtyAttributes.ReadOnly] public bool CavernsInitialized = false;
        IEnumerator CheckCavernInitialization()
        {
            while (!CavernsInitialized)
            {
                CavernsInitialized = true;
                foreach (var cavern in handlerList)
                {
                    if (!cavern.IsInitialized)
                    {
                        CavernsInitialized = false;
                        break;
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Seeds the cavern with heuristic values
        /// </summary>
        /// <param name="destinationCavern">End destination cavern</param>
        public void SeedCavernHeuristics(CavernHandler destinationCavern)
        {
            ResetCavernHeuristics();
            
            destinationCavern.SetHeuristic(0);
            Queue<CavernHandler> uncheckedCaverns = new Queue<CavernHandler>();

            foreach (var cavern in destinationCavern.ConnectedCaverns)
            {
                uncheckedCaverns.Enqueue(cavern);
            }

            int distHeuristic = 1;
            while (uncheckedCaverns.Any())
            {
                int queueCount = uncheckedCaverns.Count;
                for (int i = 0; i < queueCount; i++)
                {
                    CavernHandler currentCavern = uncheckedCaverns.Dequeue();

                    if (currentCavern.GetHeuristic < 0)
                    {
                        currentCavern.SetHeuristic(distHeuristic);
                        
                        //! Add unchecked children
                        foreach (var childCavern in currentCavern.ConnectedCaverns)
                        {
                            if (childCavern.GetHeuristic < 0) uncheckedCaverns.Enqueue(childCavern);
                        }
                    }
                }

                distHeuristic++;
            }

        }

        /// <summary>
        /// Resets heursitics of caverns
        /// </summary>
        public void ResetCavernHeuristics()
        {
            foreach(CavernHandler cavern in handlerList) 
                cavern.ResetHeuristic();
        }
        
        /// <summary>
        ///     Gets most populated cavern unsafely.
        /// </summary>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetMostPopulatedCavern(bool tiedNumberRandomize = true)
        {
            var playerNum = 0;
            var tempCaverns = new List<CavernHandler>();

            foreach (var cavern in handlerList)
                //! Must have a cavern somewhere with players
                if (cavern.GetPlayerCount > 0)
                {
                    //! Update higher playernum count
                    if (cavern.GetPlayerCount > playerNum)
                    {
                        playerNum = cavern.GetPlayerCount;
                        tempCaverns.Clear();
                        tempCaverns.Add(cavern);
                    }
                    //! Equal number players cavern
                    else if (cavern.GetPlayerCount == playerNum)
                    {
                        playerNum = cavern.GetPlayerCount;
                        tempCaverns.Add(cavern);
                    }
                }

            if (playerNum == 0) print("Cannot find any player!");

            if (tempCaverns.Count <= 0) return null;

            if (tempCaverns.Count == 1) return tempCaverns[0];

            if (tiedNumberRandomize)
                return tempCaverns[Random.Range(0, tempCaverns.Count)];
            return tempCaverns[0];
        }

        /// <summary>
        ///     Get least populated cavern with all handlers on manager.
        /// </summary>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetLeastPopulatedCavern(bool tiedNumberRandomize = true)
        {
            return GetLeastPopulatedCavern(handlerList, tiedNumberRandomize);
        }

        /// <summary>
        ///     Gets the least populated cavern given by a list of caverns.
        /// </summary>
        /// <param name="cavernList">List of caverns to query.</param>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetLeastPopulatedCavern(List<CavernHandler> cavernList, bool tiedNumberRandomize = true)
        {
            var candidateCaverns = new List<CavernHandler>();
            var playerMin = 0;
            var candidatesFound = false;
            do
            {
                candidateCaverns.Clear();
                foreach (var cavern in cavernList)
                    if (cavern.GetPlayerCount <= playerMin)
                    {
                        candidateCaverns.Add(cavern);
                        candidatesFound = true;
                    }
            } while (!candidatesFound);

            if (candidateCaverns.Count == 1) return candidateCaverns[0];

            if (tiedNumberRandomize)
                return candidateCaverns[Random.Range(0, candidateCaverns.Count)];
            return candidateCaverns[0];
        }
        

        /// <summary>
        /// Gets the next best cavern based on player accounted heuristics. 
        /// </summary>
        /// <remarks>Heuristics are accounted by player number + distance to seeded destination</remarks>
        /// <param name="sourceCavern">Cavern to calculate from</param>
        /// /// <param name="playerAccounted">Accounts heuristics with player count</param>
        /// <param name="randomizeOnTied">Randomizes return if there are tied results</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetNextBestCavern(CavernHandler sourceCavern, bool playerAccounted = true, bool randomizeOnTied = true)
        {
            return GetNextBestCavern(sourceCavern.ConnectedCaverns, playerAccounted, randomizeOnTied);
        }
            
        /// <summary>
        /// Gets the next best cavern based on player accounted heuristics. 
        /// </summary>
        /// <remarks>Heuristics are accounted by player number + distance to seeded destination</remarks>
        /// <param name="cavernChoices">Choices of cavern to choose</param>
        /// <param name="playerAccounted">Accounts heuristics with player count</param>
        /// <param name="randomizeOnTied">Randomizes return if there are tied results</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetNextBestCavern(List<CavernHandler> cavernChoices, bool playerAccounted = true, bool randomizeOnTied = true)
        {
            List<CavernHandler> bestCaverns = new List<CavernHandler>();
            int cheapestHeuristic = int.MaxValue;

            if (playerAccounted)
            {
                foreach (CavernHandler cavern in cavernChoices)
                {
                    if (cavern.GetPlayerAccountedHeuristic < cheapestHeuristic)
                    {
                        cheapestHeuristic = cavern.GetPlayerAccountedHeuristic;
                    }
                }

                foreach (CavernHandler cavern in cavernChoices)
                {
                    if (cavern.GetPlayerAccountedHeuristic == cheapestHeuristic) bestCaverns.Add(cavern);
                }
            }
            else
            {
                foreach (CavernHandler cavern in cavernChoices)
                {
                    if (cavern.GetHeuristic < cheapestHeuristic)
                    {
                        cheapestHeuristic = cavern.GetHeuristic;
                    }
                }

                foreach (CavernHandler cavern in cavernChoices)
                {
                    if (cavern.GetHeuristic == cheapestHeuristic) bestCaverns.Add(cavern);
                }

            }
            

            if (bestCaverns.Count == 1 || !randomizeOnTied)
                return bestCaverns[0];
            else
                return bestCaverns[Random.Range(0, bestCaverns.Count)];
        }
        

        #endregion
        private void DebugPrintCavernList(List<CavernHandler> cavernHandlerList, string prefix = "")
        {
            if (cavernHandlerList == null) return;

            var tags = "";
            foreach (var cavern in cavernHandlerList)
            {
                if (cavern == null) continue;
                tags += cavern.cavernTag + ", ";
            }

            print(prefix + " " + tags);
        }

        /// <summary>
        ///     Attempts to get an isolated player.
        /// </summary>
        /// <returns>An isolated player, or null if no one is isolated.</returns>
        public PlayerController GetIsolatedPlayer()
        {
            PlayerController isolatedPlayer = null;

            foreach (var handler in handlerList)
                if (handler.GetPlayerCount == 1)
                {
                    isolatedPlayer = handler.GetPlayersInCavern[0];
                    break;
                }

            return isolatedPlayer;
        }

        public CavernTag GetCavernTagOfAILocation()
        {
            if (GetHandlerOfAILocation == null)
                return CavernTag.Invalid;
            return GetHandlerOfAILocation.cavernTag;
        }

        public CavernHandler GetCavern(CavernTag tag)
        {
            foreach (var handler in handlerList)
                if (handler.cavernTag == tag)
                    return handler;

            return null;
        }

        public void InjectHandler(CavernHandler handler)
        {
            if (!handlerList.Contains(handler)) handlerList.Add(handler);
        }

        public List<CavernHandler> GetHandlerListExcludingAI()
        {
            return GetHandlerListExcluding(GetHandlerOfAILocation);
        }

        public List<CavernHandler> GetHandlerListExcluding(CavernHandler exludedCavern)
        {
            var newHandlerList = new List<CavernHandler>();
            newHandlerList = handlerList.ToList();
            newHandlerList.Remove(exludedCavern);
            return newHandlerList;
        }

        public CavernHandler GetHandlerOfAILocation { get; private set; }

        public CavernHandler GetHandlerOfTag(CavernTag tag)
        {
            return handlerList.Where(h => h.cavernTag == tag).SingleOrDefault();
        }

        //public int PlayerLayer => LayerMask.NameToLayer(playerLayer);
        //public int AILayer => LayerMask.NameToLayer(aiLayer);
        public bool PlayerLayerContains(int layer) => LayerMaskExtend.Contain(playerLayer, layer);
        public bool AILayerContains(int layer) => LayerMaskExtend.Contain(aiLayer, layer);
    }
}