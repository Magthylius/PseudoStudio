using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;
using Hadal.Utility;
using Component = UnityEngine.Component;

//Created by Jet
namespace Hadal
{
    public static class TimerExten
    {
        public static Timer AttachTimer(this MonoBehaviour monoOwner, float duration,
            Action onComplete, Action<float> onUpdate = null, bool loop = false, bool shouldPersist = false)
            => Timer.Register(duration, onComplete, onUpdate, loop, monoOwner, shouldPersist);

        public static Timer AttachTimer(this MonoBehaviour monoOwner, Timer timer)
            => Timer.Register(timer.Duration, timer.OnCompleteEvent, timer.OnUpdateEvent, timer.Loop, monoOwner, timer.ShouldPersist);
    }

    public static class ConversionExten
    {
        public static int Round(this float number) => Mathf.RoundToInt(number);
        public static byte AsByte(this int number) => Convert.ToByte(number);
        public static byte AsByte(this bool statement) => Convert.ToByte(statement);
        public static int AsInt(this bool statement) => Convert.ToInt32(statement);
        public static uint AsUint(this bool statement) => Convert.ToUInt32(statement);
        public static uint AsUint(this float number) => Convert.ToUInt32(number);
        public static uint AsUint(this int number) => Convert.ToUInt32(number);
        public static float AsFloat(this bool statement) => statement.AsInt();
        public static float AsFloat(this int number) => number;
        public static bool AsBool(this byte bitSet) => Convert.ToBoolean(bitSet);
        public static bool AsBool(this uint number) => Convert.ToBoolean(number);
        public static T AsType<T>(this GameObject gameObject) where T : UnityEngine.Component => gameObject.GetComponent<T>();
        public static T AsType<T>(this T tee) where T : UnityEngine.Component => tee.GetComponent<T>();
        public static GameObject AsGObject(this UnityEngine.Object obj) => (GameObject) obj;
    }

    public static class FluentBool
    {
        public static bool Not(bool statement) => !statement;
    }

    public static class ClampExten
    {
        public static int Clamp0(this int number) => Mathf.Max(number, 0);
        public static float Clamp0(this float number) => Mathf.Max(number, 0.0f);
        public static float Clamp01(this float number) => Mathf.Clamp01(number);
        public static float DiffFrom(this float thisNum, float otherNum) => (thisNum - otherNum).Abs();
        
        public static bool IsGreaterThan(this float thisNum, float otherNum) => thisNum > otherNum;
        public static bool IsGreaterOrEqualTo(this float thisNum, float otherNum) => thisNum >= otherNum;
        public static bool IsLowerThan(this float thisNum, float otherNum) => thisNum < otherNum;
        public static bool IsLowerOrEqualTo(this float thisNum, float otherNum) => thisNum <= otherNum;
        
        public static bool IsGreaterThan(this int thisNum, int otherNum) => thisNum > otherNum;
        public static bool IsGreaterOrEqualTo(this int thisNum, int otherNum) => thisNum >= otherNum;
        public static bool IsLowerThan(this int thisNum, int otherNum) => thisNum < otherNum;
        public static bool IsLowerOrEqualTo(this int thisNum, int otherNum) => thisNum <= otherNum;
    }

    public static class VectorExten
    {
        public static Vector3 WithVectorForward(this Quaternion rotation) => rotation * Vector3.forward;
        public static Vector3 WithVectorRight(this Quaternion rotation) => rotation * Vector3.right;
        public static Vector3 WithVectorUp(this Quaternion rotation) => rotation * Vector3.up;
    }

    public static class StringExten
    {
        public static string NameOfClass<T>(this T type) where T : class => TypeDescriptor.GetClassName(type.GetType());

        public static string AddSpacesBeforeCapitalLetters(this string text, bool hasAcronym)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            Append(text[0]);
            for(int i = 1; i < text.Length; i++)
            {
                if (CanAddSpace()) Append(' ');
                Append(text[i]);

                #region Local Shorthands

                bool CanAddSpace() => CurrentIsUpperCase() && (PreviousIsLowerCase() || NotPartOfAcronym());

                bool PreviousIsLowerCase() => PreviousCharNotASpace() && !PreviousIsUpperCase();
                bool NotPartOfAcronym() => hasAcronym &&
                                           PreviousIsUpperCase() &&
                                           CurrentIsNotLastElement() &&
                                           !NextIsUpperCase();

                bool CurrentIsNotLastElement() => i < text.Length - 1;
                bool PreviousCharNotASpace() => text[i - 1] != ' ';
                bool CurrentIsUpperCase() => char.IsUpper(text[i]);
                bool PreviousIsUpperCase() => char.IsUpper(text[i - 1]);
                bool NextIsUpperCase() => char.IsUpper(text[i + 1]);

                #endregion
            }
            return newText.ToString();
            void Append(char c) => newText.Append(c);
        }
    }

    public static class MathExten
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
    }

    public static class UnityExten
    {
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
        public static UnityEngine.Object Instantiate0(this UnityEngine.Object prefab)
        {
            return UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
    }

    public static class UnityBuilderExten
    {
        public static T WithGObjectSetActive<T>(this T obj, bool state) where T : Component
        {
            obj.gameObject.SetActive(state);
            return obj;
        }
    }

    public static class CollectionsExten
    {
        public static bool IsEmpty<T>(this Queue<T> q) => q.Count == 0;
        public static bool IsEmpty<T>(this List<T> l) => l.Count == 0;
    }
}