namespace Hadal.Security
{
    public class BoolBitStorage
    {
        private uint _data;
        public BoolBitStorage() => Reset();
        public void Reset() => _data &= ~_data;
        public void SetBool(int location, bool flag)
            => _data = ((_data | (1.AsUint() << location)) * flag.AsUint()) + ((_data & ~(1.AsUint() << location)) * (!flag).AsUint());
        public bool GetBool(int location) => (_data & (1.AsUint() << location)).AsBool();
    }
}