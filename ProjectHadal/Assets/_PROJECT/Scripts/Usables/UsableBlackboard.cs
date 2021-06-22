using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Usables
{
    public static class UsableBlackboard
    {
        public static string[] PlayerLayers = {"Player", "PlayerGrabbed"};
        public static string[] AILayers = {"Monster"};
        public static string[] CollidableLayers = {"Default", "Obstacle"};

        static string _AIHitboxLayer = "Monster";
        public static string AIHitboxLayerName => _AIHitboxLayer;
        public static int AIHitboxLayerInt => LayerMask.NameToLayer(_AIHitboxLayer);

        public static bool InPlayerLayers(int layer) => InPlayerLayers(LayerMask.LayerToName(layer));
        public static bool InPlayerLayers(string layer)
        {
            foreach (var pLayer in PlayerLayers)
            {
                if (pLayer == layer) return true;
            }

            return false;
        }

        public static bool InAILayers(int layer) => InAILayers(LayerMask.LayerToName(layer));
        public static bool InAILayers(string layer)
        {
            foreach (var aiLayer in AILayers)
            {
                if (layer == aiLayer) return true;
            }

            return false;
        }

        public static bool InCollidableLayers(int layer) => InCollidableLayers(LayerMask.LayerToName(layer));
        public static bool InCollidableLayers(string layer)
        {
            foreach (var collidable in CollidableLayers)
            {
                if (layer == collidable) return true;
            }

            return false;
        }
    }
}
