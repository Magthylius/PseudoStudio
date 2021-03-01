using System;
using UnityEngine;

namespace Tenshi.SaveHigan
{
    public static class TenshiConverter
    {
        public static Color DeserialiseColour(SerialisableColour sCol)
        {
            return new Color(sCol.Colour[0], sCol.Colour[1], sCol.Colour[2], sCol.Colour[3]);
        }

        public static Vector3 DeserialiseVector(SerialisableVector sVec)
        {
            Vector3 newVector = default;
            if (sVec.Vector.Length == 2)
            {
                newVector = new Vector3(sVec.Vector[0], sVec.Vector[1], 0f);
                return newVector;
            }
            newVector = new Vector3(sVec.Vector[0], sVec.Vector[1], sVec.Vector[2]);
            return newVector;
        }

        public static Quaternion DeserialiseQuaternion(SerialisableQuaternion sQua)
        {
            return new Quaternion(sQua.Rotation[1], sQua.Rotation[2], sQua.Rotation[3], sQua.Rotation[0]);
        }

        public static Transform DeserialiseTransform(SerialisableTransform sTrans, ref Transform newTransform)
        {
            newTransform.localPosition = DeserialiseVector(sTrans.Position);
            newTransform.localRotation = DeserialiseQuaternion(sTrans.Rotation);
            newTransform.localScale = DeserialiseVector(sTrans.Scale);
            return newTransform;
        }

        public static Bounds DeserialiseBounds(SerialisableBounds sBou)
        {
            return new Bounds(DeserialiseVector(sBou.Centre), DeserialiseVector(sBou.Size));
        }
    }

    #region Tenshi Serialisables

    [Serializable]
    public struct SerialisableColour
    {
        public float[] Colour { get; private set; }

        public SerialisableColour(Color colour)
        {
            Colour = new float[4];

            Colour[0] = colour.r;
            Colour[1] = colour.g;
            Colour[2] = colour.b;
            Colour[3] = colour.a;
        }
    }

    [Serializable]
    public struct SerialisableVector
    {
        public float[] Vector { get; private set; }
        
        public SerialisableVector(Vector2 vector2)
        {
            Vector = new float[2];
            Vector[0] = vector2.x;
            Vector[1] = vector2.y;
        }
        public SerialisableVector(Vector3 vector3)
        {
            Vector = new float[3];
            Vector[0] = vector3.x;
            Vector[1] = vector3.y;
            Vector[2] = vector3.z;
        }
    }

    [Serializable]
    public struct SerialisableQuaternion
    {
        public float[] Rotation { get; private set; }

        public SerialisableQuaternion(Quaternion quaternion)
        {
            Rotation = new float[4];
            Rotation[0] = quaternion.w;
            Rotation[1] = quaternion.x;
            Rotation[2] = quaternion.y;
            Rotation[3] = quaternion.z;
        }
    }

    [Serializable]
    public struct SerialisableTransform
    {
        public SerialisableVector Position { get; private set; }
        public SerialisableQuaternion Rotation { get; private set; }
        public SerialisableVector Scale { get; private set; }

        public SerialisableTransform(Transform transform)
        {
            Position = new SerialisableVector(transform.localPosition);
            Rotation = new SerialisableQuaternion(transform.localRotation);
            Scale = new SerialisableVector(transform.localScale);
        }
    }

    [Serializable]
    public struct SerialisableBounds
    {
        public SerialisableVector Centre { get; private set; }
        public SerialisableVector Size { get; private set; }

        public SerialisableBounds(Bounds bounds)
        {
            Centre = new SerialisableVector(bounds.center);
            Size = new SerialisableVector(bounds.size);
        }
    }

    #endregion
}
