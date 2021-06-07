using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.UnitySoku;
using Hadal.Player;
using System.Linq;
using NaughtyAttributes;

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

    public delegate void CavernHandlerReturn(CavernPlayerData data);
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
        /// <param name="searchList">List of caverns to start search from.</param>
        /// <returns>List of caverns that are equal distance to choose from.</returns>
        public List<CavernHandler> GetNextCavern(CavernHandler destinationCavern, List<CavernHandler> searchList)
        {
            List<CavernHandler> researchList = new List<CavernHandler>();
            List<CavernHandler> returningList = new List<CavernHandler>();

            foreach(CavernHandler searchCavern in searchList)
            {
                foreach (CavernHandler childCavern in searchCavern.connectedCaverns)
                {
                    if (childCavern == destinationCavern)
                        returningList.Add(searchCavern);
                    else if (!searchList.Contains(childCavern) && !researchList.Contains(childCavern))
                        researchList.Add(childCavern);
                }
            }

            if (returningList.Count > 0) return returningList;
            else return GetNextCavern(destinationCavern, researchList);
        }

        [Button ("TestGetNextCavern")]
        void TestGetNextCavern()
        {
            //print(GetCavern(CavernTag.Starting_Grounds).CalculateRelativeDistanceCost(GetCavern(CavernTag.Staglamite_Cavern)));
            List<CavernHandler> testList = new List<CavernHandler> { GetCavern(CavernTag.Starting) };
            print(GetNextCavern(GetCavern(CavernTag.Staglamite), testList).Count);
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

        public CavernHandler GetHandlerOfAILocation() => aiAtHandler;
        public CavernHandler GetHandlerOfTag(CavernTag tag) => handlerList.Where(h => h.cavernTag == tag).SingleOrDefault();
    }
}
