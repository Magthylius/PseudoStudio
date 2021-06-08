using System;
using System.Collections;
using System.Collections.Generic;
using Tenshi.UnitySoku;
using Hadal.Player;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

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

    /// <summary>
    /// Used to calculate cavern heuristics
    /// </summary>
    public struct CavernHeuristic
    {
        public int Cost;
        public CavernHandler Handler;

        public CavernHeuristic(CavernHandler handler)
        {
            Cost = 0;
            Handler = handler;
        }
    }

    public delegate void CavernHandlerPlayerReturn(CavernPlayerData data);
    public delegate void CavernHandlerAIReturn(CavernHandler handler);

    /// <summary>
    /// For cavern identification.
    /// </summary>
    public enum CavernTag
    {
        Invalid = 0,
        Starting,
        Lair,
        Hydrothermal,
        Bioluminescent,
        Staglamite,
        Custom_Point
    }

    /// <summary>
    /// Manages all the other handlers. Is a singleton.
    /// </summary>
    public class CavernManager : SingletonSoft<CavernManager>
    {
        [ReadOnly] public List<CavernHandler> handlerList = new List<CavernHandler>();
        CavernHandler aiAtHandler;

        public event CavernHandlerPlayerReturn PlayerEnterCavernEvent;
        public event CavernHandlerPlayerReturn PlayerLeftCavernEvent;
        public event CavernHandlerAIReturn AIEnterCavernEvent;
        public event CavernHandlerAIReturn AILeftCavernEvent;

        void OnValidate()
        {
           handlerList.RemoveAll(wat => wat == null);
        }

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
            PlayerEnterCavernEvent?.Invoke(data);
        }

        public void OnPlayerLeftCavern(CavernPlayerData data)
        {
            //return data;
            PlayerLeftCavernEvent?.Invoke(data);
        }

        public void OnAIEnterCavern(CavernHandler handler)
        {
            if (aiAtHandler != handler)
                aiAtHandler = handler;

            AIEnterCavernEvent?.Invoke(handler);
        }

        public void OnAILeaveCavern(CavernHandler handler)
        {
            if (aiAtHandler == handler)
                aiAtHandler = null;

            AILeftCavernEvent?.Invoke(handler);
        }

        /// <summary>
        /// Gets most populated cavern.
        /// </summary>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetMostPopulatedCavern(bool tiedNumberRandomize = true)
        {
            int playerNum = 0;
            List<CavernHandler> tempCaverns = new List<CavernHandler>();

            foreach (CavernHandler cavern in handlerList)
            {
                //! Must have a cavern somewhere with players
                if (playerNum > 0)
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
            }
            
            if (playerNum == 0) print("Cannot find any player!");
            
            if (tempCaverns.Count <= 0)
                return null;
            else if (tempCaverns.Count == 1)
                return tempCaverns[0];
            else
            {
                if (tiedNumberRandomize)
                    return tempCaverns[Random.Range(0, tempCaverns.Count)];
                else
                    return tempCaverns[0];
            }
        }

        /// <summary>
        /// Get least populated cavern with all handlers on manager.
        /// </summary>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetLeastPopulatedCavern(bool tiedNumberRandomize = true)
        {
            return GetLeastPopulatedCavern(handlerList, tiedNumberRandomize);
        }

        /// <summary>
        /// Gets the least populated cavern given by a list of caverns.
        /// </summary>
        /// <param name="cavernList">List of caverns to query.</param>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetLeastPopulatedCavern(List<CavernHandler> cavernList, bool tiedNumberRandomize = true)
        {
            List<CavernHandler> candidateCaverns = new List<CavernHandler>();
            int playerMin = 0;
            bool candidatesFound = false;
            do
            {
                candidateCaverns.Clear();
                foreach (CavernHandler cavern in cavernList)
                {
                    if (cavern.GetPlayerCount <= playerMin)
                    {
                        candidateCaverns.Add(cavern);
                        candidatesFound = true;
                    }
                }
            } while (!candidatesFound);

            if (candidateCaverns.Count == 1) return candidateCaverns[0];

            if (tiedNumberRandomize)
                return candidateCaverns[Random.Range(0, candidateCaverns.Count)];
            else
                return candidateCaverns[0];
        }

        /// <summary>
        /// Gets the next suitable cavern based on adjacency.
        /// </summary>
        /// <param name="destinationCavern">Target destination cavern.</param>
        /// <param name="sourceCavern">Starting cavern to search.</param>
        /// <returns>A single cavern handler.</returns>
        public CavernHandler GetNextCavern(CavernHandler destinationCavern, CavernHandler sourceCavern, bool tiedNumberRandomize = true)
        {
            List<CavernHandler> list = GetNextCaverns(destinationCavern, sourceCavern);
            if (tiedNumberRandomize)
                return list[Random.Range(0, list.Count)];
            else
                return list[0];
        }

        /// <summary>
        /// Gets the next suitable cavern based on adjacency.
        /// </summary>
        /// <param name="destinationCavern">Target destination cavern.</param>
        /// <param name="searchList">List of caverns to start search from.</param>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>A single cavern handler.</returns>
        public CavernHandler GetNextCavern(CavernHandler destinationCavern, List<CavernHandler> searchList, bool tiedNumberRandomize = true)
        {
            List<CavernHandler> list = GetNextCaverns(destinationCavern, searchList);
            if (tiedNumberRandomize)
                return list[Random.Range(0, list.Count)];
            else
                return list[0];
        }

        /// <summary>
        /// Gets a list suitable caverns based on adjacency.
        /// </summary>
        /// <param name="destinationCavern">Target destination cavern.</param>
        /// <param name="sourceCavern">Starting cavern to search.</param>
        /// <returns>List of caverns that are equal distance to choose from.</returns>
        public List<CavernHandler> GetNextCaverns(CavernHandler destinationCavern, CavernHandler sourceCavern)
        {
            if (sourceCavern == null)
            {
                Debug.LogError("Source cavern null!");
                return null;
            }
            
            List<CavernHandler> list = new List<CavernHandler> { sourceCavern };
            if (destinationCavern == sourceCavern) return list;
            return GetNextCaverns(destinationCavern, list);
        }

        /// <summary>
        /// Gets a list suitable caverns based on adjacency.
        /// </summary>
        /// <param name="destinationCavern">Target destination cavern.</param>
        /// <param name="searchList">List of caverns to start search from.</param>
        /// <returns>List of caverns that are equal distance to choose from.</returns>
        public List<CavernHandler> GetNextCaverns(CavernHandler destinationCavern, List<CavernHandler> searchList, List<CavernHandler> exclusionList = null, int loopcount = 0)
        {
            List<CavernHandler> researchList = new List<CavernHandler>();
            List<CavernHandler> returningList = new List<CavernHandler>();

            DebugPrintCavernList(searchList, "search list:");
            DebugPrintCavernList(exclusionList, "exclusion list:");
            foreach(CavernHandler searchCavern in searchList)
            {
                foreach (CavernHandler childCavern in searchCavern.ConnectedCaverns)
                {
                    //! ignore excluded caverns
                    if (exclusionList != null && exclusionList.Contains((childCavern))) continue;
                    
                    if (childCavern == destinationCavern)
                        returningList.Add(searchCavern);
                    else if (returningList.Count < 1 && !searchList.Contains(childCavern) && !researchList.Contains(childCavern))
                        researchList.Add(childCavern);
                }
            }

            if (loopcount > 5) return null;
            
            if (returningList.Count > 0) 
                return returningList;
            else
            {
                List<CavernHandler> newExclusionList = new List<CavernHandler>();
                if(exclusionList != null) newExclusionList.Union(exclusionList);
                newExclusionList.Union(searchList);
                loopcount++;
                return GetNextCaverns(destinationCavern, researchList, newExclusionList, loopcount);
            }
        }
        
        void DebugPrintCavernList(List<CavernHandler> cavernHandlerList, string prefix = "")
        {
            if (cavernHandlerList == null) return;
            
            string tags = "";
            foreach (CavernHandler cavern in cavernHandlerList)
            {
                if (cavern == null) continue;
                tags += cavern.cavernTag.ToString() + ", ";
            }
            print(prefix + " " + tags);
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

        public CavernHandler GetCavern(CavernTag tag)
        {
            foreach(CavernHandler handler in handlerList)
            {
                if (handler.cavernTag == tag) return handler;
            }

            return null;
        }

        public void InjectHandler(CavernHandler handler)
        {
            if (!handlerList.Contains(handler)) handlerList.Add(handler);
        }

        public List<CavernHandler> GetHandlerListExcludingAI() => GetHandlerListExcluding(GetHandlerOfAILocation);
        public List<CavernHandler> GetHandlerListExcluding(CavernHandler exludedCavern)
        {
            List<CavernHandler> newHandlerList = new List<CavernHandler>();
            newHandlerList = handlerList.ToList();
            newHandlerList.Remove(exludedCavern);
            return newHandlerList;
        }
        public CavernHandler GetHandlerOfAILocation => aiAtHandler;
        public CavernHandler GetHandlerOfTag(CavernTag tag) => handlerList.Where(h => h.cavernTag == tag).SingleOrDefault();
    }
}
