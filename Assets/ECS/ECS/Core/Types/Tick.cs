namespace ME.ECS {

    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    using TType = System.UInt64;
    using TName = Tick;
    
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Tick : IComparable, IConvertible, IFormattable, IComparable<TName>, IEquatable<TName>, ISerializable {

        public static readonly Tick Zero = 0UL;
        public static readonly Tick One = 1UL;
        public static readonly Tick MinValue = 0UL;
        public static readonly Tick MaxValue = ulong.MaxValue;
        
        public readonly TType v; // Do not rename (binary serialization)

        private Tick(TType value) {

            this.v = value;

        }

        private Tick(SerializationInfo info, StreamingContext context) {
            
            this.v = info.GetUInt64("v");
            
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            
            info.AddValue("v", this.v);
            
        }

        // Compares this object to another object, returning an integer that
        // indicates the relationship. 
        // Returns :
        // 0 if the values are equal
        // Negative number if _value is less than value
        // Positive number if _value is more than value
        // null is considered to be less than any instance, hence returns positive number
        // If object is not of type Int32, this method throws an ArgumentException.
        // 
        public int CompareTo(Object value) {
            throw new AllocationException();
        }

        public int CompareTo(TName value) {
            // NOTE: Cannot use return (_value - value) as this causes a wrap
            // around in cases where _value - value > MaxValue.
            if (this.v < value.v) {
                return -1;
            }

            if (this.v > value.v) {
                return 1;
            }

            return 0;
        }

        public override bool Equals(Object obj) {
            throw new AllocationException();
        }

        public bool Equals(TName obj) {
            return this.v == obj.v;
        }

        // The absolute value of the int contained.
        public override int GetHashCode() {
            
            return (int)this.v ^ (int)(this.v >> 32);
            
        }

        public override String ToString() {
            return this.v.ToString();
        }

        // Parses an integer from a String. Returns false rather
        // than throwing exceptin if input is invalid
        // 
        public static bool TryParse(String s, out TName result) {
            if (s == null) {
                result = 0;
                return false;
            }

            return TName.TryParse(s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
        }

        // Parses an integer from a String in the given style. Returns false rather
        // than throwing exceptin if input is invalid
        // 
        public static bool TryParse(String s, NumberStyles style, IFormatProvider provider, out TName result) {
            if (s == null) {
                result = 0;
                return false;
            }

            var r = TType.TryParse(s, style, NumberFormatInfo.GetInstance(provider), out var res);
            result = res;
            return r;
        }

        public static TName operator -(TName value, TName value2) {

            return (TType)value - (TType)value2;

        }

        public static TName operator +(TName value, TName value2) {

            return (TType)value + (TType)value2;

        }

        public static TName operator %(TName value, TName value2) {

            return (TType)value % (TType)value2;

        }

        public static TName operator *(TName value, TName value2) {

            return (TType)value * (TType)value2;

        }

        public static TName operator /(TName value, TName value2) {

            return (TType)value / (TType)value2;

        }

        public static implicit operator TType(TName value) {

            return value.v;

        }
        
        public static implicit operator TName(TType value) {

            return new TName(value);

        }

        public static implicit operator int(TName value) {

            return (int)value.v;

        }
        
        public static implicit operator TName(int value) {

            return new TName((TType)value);

        }

        public static implicit operator long(TName value) {

            return (long)value.v;

        }
        
        public static implicit operator TName(long value) {

            return new TName((TType)value);

        }

        public static implicit operator double(TName value) {

            return (double)value.v;

        }
        
        public static implicit operator TName(double value) {

            return new TName((TType)value);

        }

        public static implicit operator uint(TName value) {

            return (uint)value.v;

        }
        
        public static implicit operator TName(uint value) {

            return new TName((TType)value);

        }

        public static implicit operator float(TName value) {

            return (float)value.v;

        }
        
        public static implicit operator TName(float value) {

            return new TName((TType)value);

        }

        //
        // IConvertible implementation
        // 

        public TypeCode GetTypeCode() {
            return TypeCode.UInt64;
        }

        string IFormattable.ToString(string format, IFormatProvider provider) {
            return this.v.ToString(format);
        }

        string IConvertible.ToString(IFormatProvider provider) {
            return this.v.ToString();
        }

        bool IConvertible.ToBoolean(IFormatProvider provider) {
            return Convert.ToBoolean(this.v);
        }

        char IConvertible.ToChar(IFormatProvider provider) {
            return Convert.ToChar(this.v);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider) {
            return Convert.ToSByte(this.v);
        }

        byte IConvertible.ToByte(IFormatProvider provider) {
            return Convert.ToByte(this.v);
        }

        short IConvertible.ToInt16(IFormatProvider provider) {
            return Convert.ToInt16(this.v);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider) {
            return Convert.ToUInt16(this.v);
        }

        int IConvertible.ToInt32(IFormatProvider provider) {
            return (int)this.v;
        }

        uint IConvertible.ToUInt32(IFormatProvider provider) {
            return Convert.ToUInt32(this.v);
        }

        long IConvertible.ToInt64(IFormatProvider provider) {
            return Convert.ToInt64(this.v);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider) {
            return Convert.ToUInt64(this.v);
        }

        float IConvertible.ToSingle(IFormatProvider provider) {
            return Convert.ToSingle(this.v);
        }

        double IConvertible.ToDouble(IFormatProvider provider) {
            return Convert.ToDouble(this.v);
        }

        Decimal IConvertible.ToDecimal(IFormatProvider provider) {
            return Convert.ToDecimal(this.v);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider) {
            throw new InvalidCastException("Invalid cast from " + nameof(TName) + " to DateTime");
        }

        Object IConvertible.ToType(Type type, IFormatProvider provider) {
            throw new InvalidCastException("Invalid cast from " + nameof(TName) + " to " + type);
        }

    }

}