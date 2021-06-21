using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! Created by Jon
namespace Hadal.Player
{
    /// <summary>
    /// Data initialized through PlayerControllers
    /// </summary>
    public static class LocalPlayerData
    {
        /// <summary>
        /// PlayerController of local player.
        /// </summary>
        public static PlayerController PlayerController;
        
        /// <summary>
        /// View ID of local player. Returns -1 if PlayerController is null.
        /// </summary>
        public static int ViewID => PlayerController ? PlayerController.ViewID : -1;
    }
}
