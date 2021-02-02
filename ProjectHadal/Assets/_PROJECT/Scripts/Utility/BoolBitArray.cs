using System;
using System.Collections.Generic;

namespace Hadal.Security
{
    public class BoolBitArray
    {
        private byte _data;
        public BoolBitArray() => Reset();
        public void Reset() => _data = default;
        public bool this[int index]
        {
            get
            {
                CheckForOutOfBounds(index);
                return GetBool(index);
            }
            set
            {
                lock(this)
                {
                    CheckForOutOfBounds(index);
                    SetBool(index, value);
                }
            }
        }
        
        private static void CheckForOutOfBounds(int value)
        {
            if (value < byte.MaxValue) return;
            throw new Exception($"Index out of bounds, index must be less than {byte.MaxValue}.");
        }
        private void SetBool(int location, bool flag)
            => _data = (byte)(((_data | (1 << location)).AsByte() * flag.AsByte()) + ((_data & ~(1 << location)).AsByte() * (!flag).AsByte()));
        private bool GetBool(int location) => ((byte)(_data & (1 << location))).AsBool();
    }
    public class BoolBitDictionary<T> : Dictionary<string, T>
    {
        private BoolBitArray _array;

        public BoolBitDictionary()
        {

        }
    }
}