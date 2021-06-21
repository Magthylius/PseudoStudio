using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! Created by Jon
namespace Hadal.Player
{
    public static class NetworkData
    {
        public static List<PlayerController> AllPlayers { get; private set; }
        public static int PlayerCount => AllPlayers.Count;

        /// <summary>
        /// Resets and initializes data
        /// </summary>
        public static void Reset() => AllPlayers = new List<PlayerController>();

        /// <summary>
        /// Adds new player if no duplicates are found.
        /// </summary>
        /// <param name="newPlayer">New player to add</param>
        public static void AddPlayer(PlayerController newPlayer)
        {
            AllPlayers ??= new List<PlayerController>();
            if (!AllPlayers.Contains(newPlayer)) AllPlayers.Add(newPlayer);
            else Debug.LogWarning("Player data already exist.");
        }

        /// <summary>
        /// Removes new player if found on data
        /// </summary>
        /// <param name="removedPlayer">Player to be removed</param>
        public static void RemovePlayer(PlayerController removedPlayer)
        {
            AllPlayers ??= new List<PlayerController>();
            if (AllPlayers.Contains(removedPlayer)) AllPlayers.Remove(removedPlayer);
            else Debug.LogWarning("Player data does not exist.");
        }

        /// <summary>
        /// Gets PlayerController with ViewID. Returns null if cannot be found.
        /// </summary>
        /// <param name="searchViewID">ViewID to search</param>
        /// <returns>Searched PlayerController</returns>
        public static PlayerController GetPlayerController(int searchViewID)
        {
            foreach (var controller in AllPlayers)
            {
                if (controller.ViewID == searchViewID)
                    return controller;
            }

            return null;
        }

        /// <summary>
        /// Gets PlayerController with PhotonRealtime Player. Returns null if cannot be found.
        /// </summary>
        /// <param name="photonPlayer">PhotonRealtime player query</param>
        /// <returns>Searched PlayerController</returns>
        public static PlayerController GetPlayerController(Photon.Realtime.Player photonPlayer)
        {
            foreach (var controller in AllPlayers)
            {
                if (controller.AttachedPlayer == photonPlayer)
                    return controller;
            }

            return null;
        }
        public static void Debug_PrintAllPlayers()
        {
            Debug.LogWarning(("Debug printing network data"));
            foreach (var player in AllPlayers)
            {
                Debug.LogWarning(player.ViewID);
            }
        }
    }
}
