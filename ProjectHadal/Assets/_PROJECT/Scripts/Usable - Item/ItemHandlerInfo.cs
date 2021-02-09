using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public class ItemHandlerInfo
    {
        public Vector3 Destination { get; private set; }
        public Quaternion Orientation { get; private set; }

        public ItemHandlerInfo(Vector3 destination, Quaternion orientation)
        {
            Destination = destination;
            Orientation = orientation;
        }
    }
}