using System;
using UnityEngine;

//Created by Jet
namespace Hadal
{
    public static class Extensions
    {
        public static float Delta(this float number) => number * Time.deltaTime;
        public static float Abs(this float number) => Mathf.Abs(number);
        public static void LerpSpeed(this ref float speed, in float directionalSpeed, in float acceleration, in float deltaTime)
        {
            speed = Mathf.Lerp(speed, directionalSpeed - float.Epsilon, acceleration * deltaTime);
        }
        public static void LerpAngle(this ref float angle, in float targetAngle, in float acceleration, in float deltaTime)
        {
            angle = Mathf.LerpAngle(angle, targetAngle - float.Epsilon, acceleration * deltaTime);
        }
        public static void SlerpAngle(this ref float angle, in float targetAngle, in float acceleration, in float deltaTime)
        {
            Vector3 to = new Vector2(targetAngle, targetAngle);
            Vector3 from = new Vector2(angle, angle);
            float x = Vector3.Slerp(from, to, acceleration * deltaTime).x;
            angle = x;
        }

        public static float NormalisedAngle(this float angle)
        {
            angle %= 360.0f;
            if (angle < 0.0f) angle += 360.0f;
            return angle;
        }

        public static int Round(this float number) => Mathf.RoundToInt(number);
        public static int AsInt(this bool statement) => Convert.ToInt32(statement);
        public static uint AsUint(this bool statement) => Convert.ToUInt32(statement);
        public static uint AsUint(this float number) => Convert.ToUInt32(number);
        public static uint AsUint(this int number) => Convert.ToUInt32(number);
        public static float AsFloat(this bool statement) => statement.AsInt();
        public static float AsFloat(this int number) => number;
        public static bool AsBool(this uint number) => Convert.ToBoolean(number);
        public static float Clamp0(this float number) => Mathf.Clamp(number, 0.0f, float.MaxValue);
        public static float Clamp01(this float number) => Mathf.Clamp01(number);
        public static float DiffFrom(this float thisNum, float otherNum) => (thisNum - otherNum).Abs();
        public static bool IsLowerThan(this float thisNum, float otherNum) => thisNum < otherNum;

        public static Vector3 WithVectorForward(this Quaternion rotation) => rotation * Vector3.forward;
        public static Vector3 WithVectorRight(this Quaternion rotation) => rotation * Vector3.right;
        public static Vector3 WithVectorUp(this Quaternion rotation) => rotation * Vector3.up;

        public static int MaskToLayer(this LayerMask mask)
        {
            int index = 0;
            int layer = mask.value;
            while(layer > 0)
            {
                layer >>= 1;
                index++;
            }
            return index - 1;
        }
    }
}