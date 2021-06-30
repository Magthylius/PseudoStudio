using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.UI;

public static class UITrackerBridge
{
    public delegate void OtherPlayerAdded(Transform transform, string name);
    public static event OtherPlayerAdded PlayerAddedEvent;
    public static event OtherPlayerAdded PlayerRemovedEvent;
    
    public static UIManager LocalPlayerUIManager;
    public static List<Transform> OtherPlayerTransforms = new List<Transform>();

    public static void AddPlayerTransform(Transform otherPlayer, string name)
    {
        if (!OtherPlayerTransforms.Contains(otherPlayer))
        {
            //PlayerAddedEvent?.Invoke(otherPlayer, name);
            LocalPlayerUIManager.TrackPlayerName(otherPlayer, name);
            OtherPlayerTransforms.Add(otherPlayer);
        }
    }

    public static void RemovePlayerTransform(Transform otherPlayer, string name)
    {
        if (OtherPlayerTransforms.Contains(otherPlayer))
        {
            //PlayerRemovedEvent?.Invoke(otherPlayer, name);
            
            //! need to untrack player
            OtherPlayerTransforms.Remove(otherPlayer);
        }
    }
}
