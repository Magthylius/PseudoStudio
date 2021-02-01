using System;
using UnityEngine;
using Random = System.Random;

namespace Hadal.Security
{
    [Serializable]
    public struct SafeFloat
    {
        [SerializeField] private float _value;
        private float _encryptShift;

        public SafeFloat(float value = 0.0f)
        {
            var rand = new Random(new Random(746876180).Next());
            _encryptShift = rand.Next(-1000, 1000);
            _value = value + _encryptShift;
        }
        public void Dispose()
        {
            _value = 0.0f;
            _encryptShift = 0.0f;
        }
        private float GetValue => _value - _encryptShift;

        public override string ToString() => GetValue.ToString();
        public override bool Equals(object obj) => GetValue.Equals(obj);
        public override int GetHashCode() => GetValue.GetHashCode();
        public static SafeFloat operator +(SafeFloat f1, SafeFloat f2) => new SafeFloat(f1.GetValue + f2.GetValue);
        public static SafeFloat operator -(SafeFloat f1, SafeFloat f2) => new SafeFloat(f1.GetValue - f2.GetValue);
        public static SafeFloat operator *(SafeFloat f1, SafeFloat f2) => new SafeFloat(f1.GetValue * f2.GetValue);
        public static SafeFloat operator /(SafeFloat f1, SafeFloat f2) => new SafeFloat(f1.GetValue / f2.GetValue);
        public static implicit operator SafeFloat(float floatCast) => new SafeFloat(floatCast);
        public static implicit operator SafeFloat(int intCast) => new SafeFloat(intCast);
        public static implicit operator float(SafeFloat sFloatCast) => sFloatCast.GetValue;
    }

    [Serializable]
    public struct SafeBool
    {
        [SerializeField] private float _value;
        private float _encryptShift;

        public SafeBool(bool value = false)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            _encryptShift = rand.Next(-1000, 1000);
            _value = value.AsFloat() + _encryptShift;
        }
        private bool GetValue => (_value - _encryptShift) >= 1;

        public override string ToString() => GetValue.ToString();
        public override bool Equals(object obj) => GetValue.Equals(obj);
        public override int GetHashCode() => GetValue.GetHashCode();
        public static implicit operator bool(SafeBool sBool) => sBool.GetValue;
        public static implicit operator SafeBool(bool value) => new SafeBool(value);
    }

    [Serializable]
    public struct SafeInt
    {
        [SerializeField] private int _value;
        private int _encryptShift;

        public SafeInt(int value = 0)
        {
            var rand = new Random(new Random(946424821).Next());
            _encryptShift = rand.Next(-1000, 1000);
            _value = value + _encryptShift;
        }
        public void Dispose()
        {
            _value = 0;
            _encryptShift = 0;
        }
        private int GetValue => _value - _encryptShift;

        public override string ToString() => GetValue.ToString();
        public override bool Equals(object obj) => GetValue.Equals(obj);
        public override int GetHashCode() => GetValue.GetHashCode();
        public static SafeInt operator +(SafeInt i1, SafeInt i2) => new SafeInt(i1.GetValue + i2.GetValue);
        public static SafeInt operator -(SafeInt i1, SafeInt i2) => new SafeInt(i1.GetValue - i2.GetValue);
        public static SafeInt operator *(SafeInt i1, SafeInt i2) => new SafeInt(i1.GetValue * i2.GetValue);
        public static SafeInt operator /(SafeInt i1, SafeInt i2) => new SafeInt(i1.GetValue / i2.GetValue);
        public static implicit operator SafeInt(int intCast) => new SafeInt(intCast);
        public static implicit operator SafeInt(float floatCast) => new SafeInt(floatCast.Round());
        public static implicit operator int(SafeInt sIntCast) => sIntCast.GetValue;
    }

    [Serializable]
    public struct SafeString
    {
        [SerializeField] private string _str;
        private int _encryptShift;

        public SafeString(string value = default)
        {
            _str = string.Empty;
            var rand = new Random();
            _encryptShift = rand.Next(-1000, 1000);
            _str = SecureText(value, _encryptShift);
        }
        public void Dispose()
        {
            _str = string.Empty;
            _encryptShift = 0;
        }
        private string GetValue => UnsecureText(_str, _encryptShift);

        private static string SecureText(string str, int shift)
        {
            string sString = string.Empty;
            for(int i = 0; i < str.Length; i++) sString += (char)(str[i] + shift);
            return sString;
        }

        private static string UnsecureText(string str, int shift)
        {
            string uString = string.Empty;
            for (int i = 0; i < str.Length; i++) uString += (char)(str[i] - shift);
            return uString;
        }

        public override string ToString() => GetValue;
        public static implicit operator SafeString(string s) => new SafeString(s);
        public static implicit operator string(SafeString sString) => sString.GetValue;
    }
}