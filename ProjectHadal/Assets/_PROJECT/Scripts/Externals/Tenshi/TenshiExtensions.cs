using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tenshi
{
    public static class StringExtensions
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

        public static string Bold(this string target) => $"<b>{target}</b>";
        public static string Italic(this string target) => $"<i>{target}</i>";
        public static string Resize(this string target, int size) => $"<size={ClampTextSize(size)}>{target}</size>";
        public static string Recolour(this string target, Color colour) => $"<color=#{ColorUtility.ToHtmlStringRGBA(colour)}>{target}</color>";
        public static string SwitchMaterial(this string target, int index) => $"<material={index}>{target}</material>";
        public static string MakeQuad(this string target, int materialIndex, int textSize, Rect rectBounds) => $"<quad material={materialIndex} " +
            $"size={ClampTextSize(textSize)} x={rectBounds.x} y={rectBounds.y} width={rectBounds.width} height={rectBounds.height}>{target}</quad>";

        private static int ClampTextSize(int size)
        {
            if (size < 10) size = 10;
            else if (size > 30) size = 30;
            return size;
        }
    }

    public static class ConversionExtensions
    {
        public static byte AsByte(this object obj) => (byte) obj;
        public static bool AsBool(this object obj) => (bool) obj;
        public static int AsInt(this object obj) => (int) obj;
        public static uint AsUint(this object obj) => (uint) obj;
        public static float AsFloat(this object obj) => (float) obj;
        public static Vector3 AsVector3(this object obj) => (Vector3) obj;
        public static Quaternion AsQuaternion(this object obj) => (Quaternion) obj;
        public static object[] AsObjArray(this object obj) => (object[]) obj;

        public static int Round(this float number) => Mathf.RoundToInt(number);
        public static int Floor(this float number) => Mathf.FloorToInt(number);
        public static byte AsByte(this int number) => Convert.ToByte(number);
        public static byte AsByte(this bool statement) => Convert.ToByte(statement);
        public static int AsInt(this bool statement) => Convert.ToInt32(statement);
        public static uint AsUint(this bool statement) => Convert.ToUInt32(statement);
        public static uint AsUint(this float number) => Convert.ToUInt32(number);
        public static uint AsUint(this int number) => Convert.ToUInt32(number);
        public static long AsLong(this int number) => Convert.ToInt64(number);
        public static ulong AsUlong(this int number) => Convert.ToUInt64(number);
        public static float AsFloat(this bool statement) => statement.AsInt();
        public static float AsFloat(this int number) => number;
        public static float AsFloat(this double number) => Convert.ToSingle(number);
        public static double AsDouble(this long number) => Convert.ToDouble(number);
        public static double AsDouble(this ulong number) => Convert.ToDouble(number);
        public static bool AsBool(this byte bitSet) => Convert.ToBoolean(bitSet);
        public static bool AsBool(this uint number) => Convert.ToBoolean(number);
        public static string AsString(this int number) => Convert.ToString(number);
        public static T AsType<T>(this GameObject gameObject) where T : UnityEngine.Component => gameObject.GetComponent<T>();
        public static T AsType<T>(this T tee) where T : UnityEngine.Component => tee.GetComponent<T>();
        public static GameObject AsGObject(this UnityEngine.Object obj) => (GameObject) obj;
    }

    public static class FluentBool
    {
        public static bool Not(bool statement) => !statement;

        public static bool NotNull<T>(this T item) where T : class => item != null;
        public static bool IsNot<T>(this T item, T other) where T : IComparable<T> => !item.Equals(other);

        public static bool IsMoreThan(this float thisNum, float otherNum) => thisNum > otherNum;
        public static bool IsMoreOrEqualTo(this float thisNum, float otherNum) => thisNum >= otherNum;
        public static bool IsLessThan(this float thisNum, float otherNum) => thisNum < otherNum;
        public static bool IsLessOrEqualTo(this float thisNum, float otherNum) => thisNum <= otherNum;
        
        public static bool IsMoreThan(this int thisNum, int otherNum) => thisNum > otherNum;
        public static bool IsMoreOrEqualTo(this int thisNum, int otherNum) => thisNum >= otherNum;
        public static bool IsLessThan(this int thisNum, int otherNum) => thisNum < otherNum;
        public static bool IsLessOrEqualTo(this int thisNum, int otherNum) => thisNum <= otherNum;

        public static bool IsEmpty<T>(this IEnumerable<T> e) => e.Count() == 0;
        public static bool IsNotEmpty<T>(this IEnumerable<T> e) => e.Count() != 0;
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> e) => (e is null) ? e is null : e.IsEmpty();
    }

    public static class ClampExtensions
    {
        public static int Clamp(this int number, int min, int max) => Mathf.Clamp(number, min, max);
        public static float Clamp(this float number, float min, float max) => Mathf.Clamp(number, min, max);
        public static int Clamp0(this int number) => Mathf.Max(number, 0);
        public static float Clamp0(this float number) => Mathf.Max(number, 0.0f);
        public static float Clamp01(this float number) => Mathf.Clamp01(number);
        public static float DiffBetween(this float thisNum, float otherNum) => (thisNum - otherNum).Abs();
    }

    public static class MathExtensions
    {
        public static float ToDegrees(this float radian) => radian * Mathf.Rad2Deg;
        public static float ToRadians(this float degree) => degree * Mathf.Deg2Rad;
        public static float Abs(this float number) => Mathf.Abs(number);
        public static int Abs(this int number) => Mathf.Abs(number);
        public static float Pow(this float number, float power) => Mathf.Pow(number, power);
        public static float Pow(this int number, float power) => Mathf.Pow(number, power);
        public static float Sqr(this float number) => number * number;
        public static int Sqr(this int number) => number * number;
        
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
            angle = Vector3.Slerp(from, to, acceleration * deltaTime).x;
        }

        public static float NormalisedAngle(this float angle)
        {
            angle %= 360.0f;
            if (angle < 0.0f) angle += 360.0f;
            return angle;
        }

        public static float NormaliseValue(this int value, int min, int max)
        {
            return (value - min) / (max - min).AsFloat();
        }
        public static float NormaliseValue(this float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        public static bool IsEven(this int number) => number % 2 == 0;
        public static bool IsOdd(this int number) => number % 2 != 0;
        public static bool IsPrime(this int number)
        {
            if (number <= 1) return false;
            int i = -1 + 2;
            while(++i < number)
                if (number % i == 0)
                    return false;
            
            return true;
        }
    }

    public static class CollectionExtensions
    {
        public static T RandomElement<T>(this IEnumerable<T> e) => e.ElementAt(UnityEngine.Random.Range(0, e.Count()));
        public static T Requeue<T>(this Queue<T> q)
        {
            T obj = q.Dequeue();
            q.Enqueue(obj);
            return obj;
        }
    }

    public static class UnityExtensions
    {
        public static float DeltaTime(this float number) => number * Time.deltaTime;
        public static float UnscaledDeltaTime(this float number) => number * Time.unscaledDeltaTime;
        public static float FixedDeltaTime(this float number) => number * Time.fixedDeltaTime;

        public static void Print(this object item) => Debug.Log(item);

        public static int ToLayer(this LayerMask mask)
        {
            int index = 0;
            int layer = mask.value;
            while (layer > 0)
            {
                layer >>= 1;
                index++;
            }
            return index - 1;
        }

        public static bool IsAMatchingMask(this int layer, LayerMask includeLayers) => ((1 << layer) & includeLayers) != 0;
        public static bool IsNotAnIgnoredMask(this int layer, LayerMask ignoreLayers) => ((1 << layer) & ignoreLayers) == 0;
    }
}