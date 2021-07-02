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
    public static Dictionary<Transform, string> OtherPlayerNames = new Dictionary<Transform, string>();

    public static void Reset()
    {
        Debug.LogWarning("what?");
        LocalPlayerUIManager = null;
        OtherPlayerNames = new Dictionary<Transform, string>();
        PlayerAddedEvent = null;
        PlayerRemovedEvent = null;
    }
    
    public static void AddPlayerTransform(Transform otherPlayer, string name)
    {
        if (!OtherPlayerNames.ContainsKey(otherPlayer))
        {
            //PlayerAddedEvent?.Invoke(otherPlayer, name);
            if (LocalPlayerUIManager) LocalPlayerUIManager.TrackPlayerName(otherPlayer, name);
            else Debug.LogWarning("UI Manager not init yet!");
            OtherPlayerNames.Add(otherPlayer, name);
        }
    }

    public static void RemovePlayerTransform(Transform otherPlayer, string name)
    {
        if (OtherPlayerNames.ContainsKey(otherPlayer))
        {
            //PlayerRemovedEvent?.Invoke(otherPlayer, name);
            
            //! need to untrack player
            OtherPlayerNames.Remove(otherPlayer);
        }
    }
}
