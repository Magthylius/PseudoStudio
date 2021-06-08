using System.Collections.Generic;
using System.Linq;
using Hadal.Player;
using ICSharpCode.NRefactory.Ast;
using NaughtyAttributes;
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
    /// Used for pathing information
    /// </summary>
    public struct CavernPathData
    {
        public Queue<CavernHandler> pathingQueue;

        public CavernPathData(Queue<CavernHandler> newQueue = null)
        {
            pathingQueue = newQueue ?? new Queue<CavernHandler>();
        }

        public void Enqueue(CavernHandler queuedCavern) => pathingQueue.Enqueue(queuedCavern);
        public void Dequeue() => pathingQueue.Dequeue();
    }

    public delegate void CavernHandlerPlayerReturn(CavernPlayerData data);

    public delegate void CavernHandlerAIReturn(CavernHandler handler);

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
        Custom_Point
    }

    /// <summary>
    ///     Manages all the other handlers. Is a singleton.
    /// </summary>
    public class CavernManager : SingletonSoft<CavernManager>
    {
        [ReadOnly] public List<CavernHandler> handlerList = new List<CavernHandler>();

        public event CavernHandlerPlayerReturn PlayerEnterCavernEvent;
        public event CavernHandlerPlayerReturn PlayerLeftCavernEvent;
        public event CavernHandlerAIReturn AIEnterCavernEvent;
        public event CavernHandlerAIReturn AILeftCavernEvent;

        private void OnValidate()
        {
            handlerList.RemoveAll(wat => wat == null);
        }

        protected override void Awake()
        {
            base.Awake();
            GetHandlerOfAILocation = null;
        }

        private void Start()
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
            if (GetHandlerOfAILocation != handler)
                GetHandlerOfAILocation = handler;

            AIEnterCavernEvent?.Invoke(handler);
        }

        public void OnAILeaveCavern(CavernHandler handler)
        {
            if (GetHandlerOfAILocation == handler)
                GetHandlerOfAILocation = null;

            AILeftCavernEvent?.Invoke(handler);
        }

        /// <summary>
        ///     Gets most populated cavern.
        /// </summary>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetMostPopulatedCavern(bool tiedNumberRandomize = true)
        {
            var playerNum = 0;
            var tempCaverns = new List<CavernHandler>();

            foreach (var cavern in handlerList)
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
        ///     Gets the next suitable cavern based on adjacency.
        /// </summary>
        /// <param name="destinationCavern">Target destination cavern.</param>
        /// <param name="sourceCavern">Starting cavern to search.</param>
        /// <returns>A single cavern handler.</returns>
        public CavernHandler GetNextCavern(CavernHandler destinationCavern, CavernHandler sourceCavern,
            bool tiedNumberRandomize = true)
        {
            if (sourceCavern.ConnectedCavernContains(destinationCavern)) 
                return destinationCavern;
            var list = GetNextCaverns(destinationCavern, sourceCavern);
            if (tiedNumberRandomize)
                return list[Random.Range(0, list.Count)];
            return list[0];
        }

        /// <summary>
        ///     Gets the next suitable cavern based on adjacency.
        /// </summary>
        /// <param name="destinationCavern">Target destination cavern.</param>
        /// <param name="searchList">List of caverns to start search from.</param>
        /// <param name="tiedNumberRandomize">Allows randomize on tied player numbers.</param>
        /// <returns>A single cavern handler.</returns>
        public CavernHandler GetNextCavern(CavernHandler destinationCavern, List<CavernHandler> searchList,
            bool tiedNumberRandomize = true)
        {
            var list = GetNextCaverns(destinationCavern, searchList);
            if (tiedNumberRandomize)
                return list[Random.Range(0, list.Count)];
            return list[0];
        }

        /// <summary>
        ///     Gets a list suitable caverns based on adjacency.
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

            var list = new List<CavernHandler> {sourceCavern};
            if (destinationCavern == sourceCavern) return new List<CavernHandler> {destinationCavern};
            return GetNextCaverns(destinationCavern, list);
        }

        /// <summary>
        ///     Gets a list suitable caverns based on adjacency.
        /// </summary>
        /// <param name="destinationCavern">Target destination cavern.</param>
        /// <param name="searchList">List of caverns to start search from.</param>
        /// <returns>List of caverns that are equal distance to choose from.</returns>
        public List<CavernHandler> GetNextCaverns(CavernHandler destinationCavern, List<CavernHandler> searchList,
            List<CavernHandler> exclusionList = null, int loopcount = 0)
        {
            var researchList = new List<CavernHandler>();
            var returningList = new List<CavernHandler>();

            DebugPrintCavernList(searchList, "search list:");
            DebugPrintCavernList(exclusionList, "exclusion list:");
            foreach (var searchCavern in searchList)
            {
                foreach (var childCavern in searchCavern.ConnectedCaverns)
                {
                    //! ignore excluded caverns
                    if (exclusionList != null && exclusionList.Contains(childCavern)) continue;

                    if (childCavern == destinationCavern)
                        returningList.Add(searchCavern);
                    else if (returningList.Count < 1 && !searchList.Contains(childCavern) &&
                             !researchList.Contains(childCavern))
                        researchList.Add(childCavern);
                }
            }

            if (loopcount > 5) return null;

            if (returningList.Count > 0) return returningList;

            var newExclusionList = new List<CavernHandler>();
            if (exclusionList != null) newExclusionList.Union(exclusionList);
            newExclusionList.Union(searchList);
            loopcount++;
            return GetNextCaverns(destinationCavern, researchList, newExclusionList, loopcount);
        }

        public void SeedCavernHeuristics(CavernHandler sourceCavern, CavernHandler destinationCavern)
        {
            ResetCavernHeuristics();
            
            //sourceCavern.SetHeuristic((int.MaxValue));
            destinationCavern.SetHeuristic(0);
            Queue<CavernHandler> uncheckedCaverns = new Queue<CavernHandler>();
            
            foreach (var cavern in destinationCavern.ConnectedCaverns)
                uncheckedCaverns.Enqueue(cavern);

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

        public void ResetCavernHeuristics()
        {
            foreach(CavernHandler cavern in handlerList) 
                cavern.ResetHeuristic();
        }

        /// <summary>
        /// Gets the next best cavern based on player accounted heuristics. 
        /// </summary>
        /// <remarks>Heuristics are accounted by player number + distance to seeded destination</remarks>
        /// <param name="sourceCavern">Cavern to calculate from</param>
        /// <param name="randomizeOnTied">Randomizes return if there are tied results</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetNextBestCavern(CavernHandler sourceCavern, bool randomizeOnTied = true)
        {
            return GetNextBestCavern(sourceCavern.ConnectedCaverns, randomizeOnTied);
        }
            
        /// <summary>
        /// Gets the next best cavern based on player accounted heuristics. 
        /// </summary>
        /// <remarks>Heuristics are accounted by player number + distance to seeded destination</remarks>
        /// <param name="cavernChoices">Choices of cavern to choose</param>
        /// <param name="randomizeOnTied">Randomizes return if there are tied results</param>
        /// <returns>CavernHandler information</returns>
        public CavernHandler GetNextBestCavern(List<CavernHandler> cavernChoices, bool randomizeOnTied = true)
        {
            List<CavernHandler> bestCaverns = new List<CavernHandler>();
            int cheapestHeuristic = int.MaxValue;

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

            if (bestCaverns.Count == 1 || !randomizeOnTied)
                return bestCaverns[0];
            else
                return bestCaverns[Random.Range(0, bestCaverns.Count)];
        }

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
    }
}